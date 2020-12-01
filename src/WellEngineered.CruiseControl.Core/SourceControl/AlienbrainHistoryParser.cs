using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// 	
    /// </summary>
    public class AlienbrainHistoryParser : IHistoryParser
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public static readonly string FILE_REGEX = ".*|.*|.*|.*|.*|.*|.*|.*";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public static readonly char DELIMITER = '|';

        /// <summary>
        /// Parses the specified history.	
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification[] Parse(TextReader history, DateTime from, DateTime to)
		{
			string historyLog = history.ReadToEnd();

			Regex regex = new Regex(Alienbrain.NO_CHANGE);
			if (regex.Match(historyLog).Success == true)
			{
				return new Modification[0];
			}

			regex = new Regex(FILE_REGEX);
            var result = new List<Modification>();
			string oldfile = ",";

			for (Match match = regex.Match(historyLog); match.Success; match = match.NextMatch())
			{
				string[] modificationParams = this.AllModificationParams(match.Value);
				if (modificationParams.Length > 1)
				{
					string file = modificationParams[1];
					if (file != oldfile)
					{
						result.Add(this.ParseModification(modificationParams));
						oldfile = file;
					}
				}
			}
			return result.ToArray();
		}

		// strip carriage return, new line, and all leading and trailing characters from parameters
        /// <summary>
        /// Alls the modification params.	
        /// </summary>
        /// <param name="matchedLine">The matched line.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string[] AllModificationParams(string matchedLine)
		{
			matchedLine = matchedLine.Replace("\n",string.Empty);
			matchedLine = matchedLine.Replace("\r",string.Empty);
			string[] modificationParams = matchedLine.Split(DELIMITER);
			for (int ii = 0; ii < modificationParams.Length; ii++)
			{
				modificationParams[ii] = modificationParams[ii].Trim(' ');
			}
			return modificationParams;
		}

		// #CheckInComment#|#Name#|#DbPath#|#SCIT#|#Mime Type#|#LocalPath#|#Changed By#|#NxN_VersionNumber#
        /// <summary>
        /// Parses the modification.	
        /// </summary>
        /// <param name="modificationParams">The modification params.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification ParseModification(string[] modificationParams)
		{
			Modification modification = new Modification();
			modification.Comment = modificationParams[0];
			modification.FileName = modificationParams[1];
			modification.FolderName = "ab:/" + modificationParams[2].Replace("/" + modificationParams[1],string.Empty);
			modification.ModifiedTime = DateTime.FromFileTime(long.Parse(modificationParams[3], CultureInfo.CurrentCulture));
			modification.Type = modificationParams[4];
			modification.Url = modificationParams[5];
			modification.UserName = modificationParams[6];
			modification.Version = modificationParams[7];

			return modification;
		}
	}
}