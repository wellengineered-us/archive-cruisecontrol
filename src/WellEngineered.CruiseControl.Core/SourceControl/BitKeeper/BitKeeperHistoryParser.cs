using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.SourceControl.BitKeeper
{
    /// <summary>
    /// 	
    /// </summary>
	public class BitKeeperHistoryParser : IHistoryParser
	{
        private enum HistoryType
        {
            Unknown,
            Pre40NonVerbose,
            Pre40Verbose,
            Post40Verbose
        };

		/// <summary>
		/// This is the keyword that precedes a change set in the bk log information.
		/// </summary>
		private static readonly string BK_CHANGESET_LINE = "ChangeSet";

		private string currentLine = string.Empty;
        private HistoryType fileHistory = HistoryType.Unknown;

        /// <summary>
        /// Parses the specified bk log.	
        /// </summary>
        /// <param name="bkLog">The bk log.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification[] Parse(TextReader bkLog, DateTime from, DateTime to)
		{
			// Read to the first ChangeSet. The first entry in the log
			// information will begin with this line. If no ChangeSet file
			// lines are found then there is nothing to do.
			this.currentLine = this.ReadToNotPast(bkLog, BK_CHANGESET_LINE, null);
			this.fileHistory = this.DetermineHistoryType();

            var mods = new List<Modification>();
			while (this.currentLine != null)
			{
				// Parse the ChangeSet entry and read till next ChangeSet
				Modification mod;
                if (this.fileHistory == HistoryType.Pre40Verbose)
                    mod = this.ParsePre40VerboseEntry(bkLog);
                else if (this.fileHistory == HistoryType.Pre40NonVerbose)
                    mod = this.ParsePre40NonVerboseEntry(bkLog);
                else
                    mod = this.ParsePost40VerboseEntry(bkLog);

				// Add all the modifications to the local list.
				mods.Add(mod);

				// Read to the next non-blank line.
				this.currentLine = bkLog.ReadLine();
			}

			return mods.ToArray();
		}

		private Modification ParsePre40VerboseEntry(TextReader bkLog)
		{
			// Example: "ChangeSet\n1.201 05/09/08 14:52:49 user@host. +1 -0\nComments"
			Regex regex = new Regex(@"(?<version>[\d.]+)\s+(?<datetime>\d{2,4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2})\s+(?<username>\S+).*");

			this.currentLine = this.currentLine.TrimStart(new char[2] {' ', '\t'});
			string filename = this.ParseFileName(this.currentLine);
			string folder = this.ParseFolderName(this.currentLine);

			// Get the next line with change info
			this.currentLine = bkLog.ReadLine();

			return this.ParseModification(regex, filename, folder, bkLog);
		}

		private Modification ParsePre40NonVerboseEntry(TextReader bkLog)
		{
			// Example: "ChangeSet@1.6, 2005-10-06 12:58:40-07:00, user@host.(none)\n  Remove file in subdir."
			Regex regex = new Regex(@"ChangeSet@(?<version>[\d.]+),\s+(?<datetime>\d{2,4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}[-+]\d{2}:\d{2}),\s+(?<username>\S+).*");

			return this.ParseModification(regex, "ChangeSet",string.Empty, bkLog);
		}

        private Modification ParsePost40VerboseEntry(TextReader bkLog)
        {
            // Example: "ChangeSet\n1.201 05/09/08 14:52:49 user@host. +1 -0\nComments"
            Regex regex = new Regex(@"(?<filename>.+)@(?<version>[\d.]+),\s+(?<datetime>\d{2,4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}[-+]\d{2}:\d{2}),\s+(?<username>\S+).*");

            this.currentLine = this.currentLine.TrimStart(new char[2] { ' ', '\t' });

            Match match = regex.Match(this.currentLine);
            if (!match.Success)
                throw new CruiseControlException("Unable to parse line: " + this.currentLine);

            string filename = this.ParseFileName(match.Result("${filename}"));
            string folder = this.ParseFolderName(match.Result("${filename}"));


            return this.ParseModification(regex, filename, folder, bkLog);
        }

		private Modification ParseModification(Regex regex, string filename, string folder, TextReader bkLog)
		{
			Match match = regex.Match(this.currentLine);
			if (!match.Success)
				throw new CruiseControlException("Unable to parse line: " + this.currentLine);
	
			Modification mod = new Modification();
			mod.FileName = filename;
			mod.FolderName = folder;
			mod.ModifiedTime = this.ParseDate(match.Result("${datetime}"));
			mod.Type = "Modified";
			mod.UserName = match.Result("${username}");
			mod.Version = match.Result("${version}");
	
			// Read all lines of the comment and flatten them
			mod.Comment = this.ParseComment(bkLog);

			// Determine the modification type
			if (mod.FileName == "ChangeSet")
			{
				// Only ChangeSets should have a file name of ChangeSet
				mod.Type = "ChangeSet";
			}
			else if (mod.Comment.IndexOf("Delete: ") != -1
				&& mod.FolderName.IndexOf("BitKeeper/deleted") == 0)
			{
				string fullFilePath = mod.Comment.Substring(mod.Comment.IndexOf("Delete: ") + 8);

				// Deleted files have the name of the BitKeeper/deleted file,
				// but we would like the name of the original file that was deleted
				mod.Type = "Deleted";
				mod.FileName = this.ParseFileName(fullFilePath);
				mod.FolderName = this.ParseFolderName(fullFilePath);
			}
			else if (mod.Comment.IndexOf("BitKeeper file") != -1 || mod.Version == "1.0")
			{
				// Added files have a comment that starts with "BitKeeper file" in pre-4.0 versions
                // In post-4.0 versions, the only way to tell is by the revision number being "1.0"
				mod.Type = "Added";
			}

			else if (mod.Comment.IndexOf("Rename: ") != -1)
			{
				// Renamed files have a comment that starts with "Rename: "
				mod.Type = "Renamed";
			}
			return mod;
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

		private DateTime ParseDate(string date)
		{
			string sep = (this.fileHistory == HistoryType.Pre40Verbose) ? "/" : "-";

			// BK is funny - we can't guarantee that the year will be two or four digits,
			// so we have to check how many digits we got and deal with it
			int firstSep = date.IndexOf(sep);
			string dateFormat = (firstSep == 4) ? "yyyy" : "yy";
			dateFormat += string.Format(System.Globalization.CultureInfo.CurrentCulture,"'{0}'MM'{0}'dd HH:mm:ss", sep);
			if (this.fileHistory != HistoryType.Pre40Verbose)
				dateFormat += "zzz";

			return DateTime.ParseExact(date, dateFormat, DateTimeFormatInfo.InvariantInfo);
		}

		private string ParseComment(TextReader bkLog)
		{
			// All the text from now to the next blank line constitutes the comment
			string message = string.Empty;
			bool multiLine = false;

			// We don't trim the newly read line because blank comments lines
			// start with two or three spaces, while a blank line between changes
			// or changesets starts with no spaces.  Thus, we can handle blank comment
			// lines only if we treat the lines that start with spaces a "non-empty";
			// therefore, no trimming here!
			this.currentLine = bkLog.ReadLine();
			while (this.currentLine != null && this.currentLine.Length != 0)
			{
				if (multiLine)
				{
					message += Environment.NewLine;
				}
				else
				{
					multiLine = true;
				}
				message += this.currentLine;

				// Go to the next line.
				this.currentLine = bkLog.ReadLine();
			}
			return message;
		}

		/// <summary>
		/// Called on first ChangeSet line to determine if this is verbose or non-verbose output
		/// </summary>
		private HistoryType DetermineHistoryType()
		{
			if (this.currentLine == null)
				return HistoryType.Unknown;

            if (this.currentLine.StartsWith("ChangeSet@") && (this.currentLine.IndexOf("+") != -1))
                return HistoryType.Post40Verbose;
            if (this.currentLine.StartsWith("ChangeSet@"))
                return HistoryType.Pre40NonVerbose;
            return HistoryType.Pre40Verbose;
		}

		private string ParseFileName(string workingFileName)
		{
			int lastSlashIndex = workingFileName.LastIndexOf("/");
			return workingFileName.Substring(lastSlashIndex + 1);
		}

		private string ParseFolderName(string workingFileName)
		{
			int lastSlashIndex = workingFileName.LastIndexOf("/");
			string folderName = string.Empty;
			if (lastSlashIndex != -1)
			{
				folderName = workingFileName.Substring(0, lastSlashIndex);
			}
			return folderName;
		}
	}
}
