using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// 	
    /// </summary>
	public class CvsHistoryParser : IHistoryParser
	{
		/// <summary>
		///  This line delimits seperate files in the CVS log information.
		///  </summary>
		private static readonly string CVS_FILE_DELIM =
			"=============================================================================";

		/// <summary>
		/// This line delimits the different revisions of a file in the CVS log information.
		/// </summary>
		private static readonly string CvsModificationDelimiter = "----------------------------";

		/// <summary>
		/// This is the keyword that precedes the name of the RCS filename in the CVS log information.
		/// </summary>
		private static readonly string CVS_RCSFILE_LINE = "RCS file: ";

		/// <summary>
		/// This is the keyword that precedes the timestamp of a file revision in the CVS log information.
		/// </summary>
		private static readonly string CVS_REVISION_DATE = "date:";

		/// <summary>
		/// This is a state keyword which indicates that a revision to a file was not
		/// relevant to the current branch, or the revision consisted of a deletion
		/// of the file (removal from branch..).
		/// </summary>
		private static readonly string CVS_REVISION_DEAD = "dead";

		private string currentLine;

        /// <summary>
        /// Parses the specified CVS log.	
        /// </summary>
        /// <param name="cvsLog">The CVS log.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification[] Parse(TextReader cvsLog, DateTime from, DateTime to)
		{
            var mods = new List<Modification>();

			// Read to the first RCS file name. The first entry in the log
			// information will begin with this line. A CVS_FILE_DELIMITER is NOT
			// present. If no RCS file lines are found then there is nothing to do.			
			while ((this.currentLine = this.ReadToNotPast(cvsLog, CVS_RCSFILE_LINE, null)) != null)
			{
				// Parse the single file entry, which may include several modifications.
				var entryList = this.ParseFileEntry(this.currentLine, cvsLog);

				//Add all the modifications to the local list.
				mods.AddRange(entryList);
			}
			return mods.ToArray();
		}

        private List<Modification> ParseFileEntry(string rcsFileLine, TextReader cvsLog)
		{
            var mods = new List<Modification>();

			string rcsFile = this.ParseFileNameAndPath(rcsFileLine);
			string fileName = this.ParseFileName(rcsFile);
			string folderName = this.ParseFolderName(rcsFile);

			this.currentLine = this.ReadToNotPast(cvsLog, CvsModificationDelimiter, CVS_FILE_DELIM);
			while (this.currentLine != null && !this.currentLine.StartsWith(CVS_FILE_DELIM))
			{
				Modification mod = this.ParseModification(cvsLog, folderName, fileName);
				if (IsFileAddedOnBranch(mod)) continue;
				mods.Add(mod);
			}
			return mods;
		}

		private readonly Regex rcsfileRegex = new Regex(@"^RCS file:\s+(.+),v\s*$");

		private string ParseFileNameAndPath(string rcsFileLine)
		{
			return this.rcsfileRegex.Match(rcsFileLine).Groups[1].Value;
		}

		private string ReadToNotPast(TextReader reader, string startsWith, string notPast)
		{
			this.currentLine = reader.ReadLine();
			while (this.currentLine != null && !this.currentLine.StartsWith(startsWith))
			{
				if ((notPast != null) && this.currentLine.StartsWith(notPast))
				{
					return null;
				}
				this.currentLine = reader.ReadLine();
			}
			return this.currentLine;
		}

		private Modification ParseModification(TextReader reader, string folderName, string fileName)
		{
			this.currentLine = reader.ReadLine();
			Modification modification = new Modification();
			if (this.currentLine.StartsWith("revision"))
			{
				modification.Version = this.currentLine.Substring("revision".Length).Trim();
				this.currentLine = reader.ReadLine();
			}
			if (this.currentLine.StartsWith(CVS_REVISION_DATE))
			{
				this.ParseDateLine(modification, this.currentLine);
				modification.FileName = fileName;
				modification.FolderName = folderName;

				this.currentLine = reader.ReadLine();
				modification.Comment = this.ParseComment(reader);
			}
			return modification;
		}

		private string ParseComment(TextReader cvsLog)
		{
			// All the text from now to the next revision delimiter or working
			// file delimiter constitutes the comment.
			StringBuilder message = new StringBuilder();
			while (this.currentLine != null && !this.currentLine.StartsWith(CVS_FILE_DELIM)
				&& !this.currentLine.StartsWith(CvsModificationDelimiter))
			{
				if (message.Length > 0)
				{
					message.Append(Environment.NewLine);
				}
				message.Append(this.currentLine);

				//Go to the next line.
				this.currentLine = cvsLog.ReadLine();
			}
			return message.ToString();
		}

		private string ParseFileName(string rcsFilePath)
		{
			int lastSlashIndex = rcsFilePath.LastIndexOf("/");
			return rcsFilePath.Substring(lastSlashIndex + 1);
		}

		/// <summary>
		/// Strip the filename, Attic folder (if the file has been deleted) and repository folder prefix to get folder name.
		/// </summary>
		private string ParseFolderName(string rcsFilePath)
		{
			return StripAtticFolder(StripFilename(rcsFilePath));
		}

		private static string StripFilename(string rcsFilePath)
		{
			int lastSlashIndex = rcsFilePath.LastIndexOf("/");
			if (lastSlashIndex != -1)
			{
				return rcsFilePath.Substring(0, lastSlashIndex);
			}
			return string.Empty;
		}

		private static string StripAtticFolder(string rcsFilePath)
		{
			if (rcsFilePath.EndsWith("Attic"))
				rcsFilePath = rcsFilePath.Substring(0, rcsFilePath.LastIndexOf("/"));
			return rcsFilePath;
		}

		private readonly Regex dateLineRegex = new Regex(@"date:\s+(?<date>\S+)\s+(?<time>\S+)\s*(?<timezone>\S*);\s+author:\s+(?<author>.*);\s+state:\s+(?<state>\S*);(\s+lines:\s+\+(?<line1>\d+)\s+-(?<line2>\d+))?");

		private void ParseDateLine(Modification modification, string dateLine)
		{
			Match match = this.dateLineRegex.Match(dateLine);
			try
			{
				modification.ModifiedTime = this.ParseModifiedTime(match.Groups["date"].Value, match.Groups["time"].Value, match.Groups["timezone"].Value);
				modification.UserName = match.Groups["author"].Value;
				modification.Type = this.ParseType(match.Groups["state"].Value, match.Groups["line1"].Value);
			}
			catch (Exception ex)
			{
				throw new CruiseControlException("Unable to parse CVS date line: " + dateLine, ex);
			}
		}

		private DateTime ParseModifiedTime(string dateStamp, string timeStamp, string timezone)
		{
            if(String.IsNullOrEmpty(timezone))
                timezone = "+0";
			string dateTimeString = string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} {1} {2}", dateStamp, timeStamp, timezone);
			return DateTime.Parse(dateTimeString, DateTimeFormatInfo.GetInstance(CultureInfo.InvariantCulture));
		}

		private string ParseType(string stateKeyword, string line1)
		{
			if (StringUtil.EqualsIgnoreCase(stateKeyword, CVS_REVISION_DEAD))
			{
				return "deleted";
			}
            else if (string.IsNullOrEmpty(line1) || Convert.ToInt32(line1, CultureInfo.CurrentCulture) == 0)
			{
				return "added";
			}
			else
			{
				return "modified";
			}
		}

		private static bool IsFileAddedOnBranch(Modification mod)
		{
			return mod.Type == "deleted" && mod.Version == "1.1";
		}
	}
}
