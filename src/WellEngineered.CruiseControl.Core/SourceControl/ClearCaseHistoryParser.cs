using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
	/// <summary>
	/// Provides for parsing output from a ClearCase cleartool.exe lshist command.
	/// </summary>
	/// <remarks>
	/// Written by Garrett M. Smith (gsmith@thoughtworks.com).
	/// Parsing logic inspired from (but improved upon) CruiseControl for Java.
	/// </remarks>
	public class ClearCaseHistoryParser : IHistoryParser
	{
		
		/// <summary>
		/// Unlikely combination of characters to separate fields
		/// </summary>
		public readonly static string DELIMITER = "#~#";
		
		/// <summary>
		/// Unlikely combination of characters to indicate end of one line in query.
		/// Carriage return (\n) may be used in comments and so is not available to us.
		/// </summary>
		public readonly static string END_OF_LINE_DELIMITER = "@#@#@#@#@#@#@#@#@#@#@#@";

        /// <summary>
        /// Parses the specified history.	
        /// </summary>
        /// <param name="history">The history.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification[] Parse( TextReader history, DateTime from, DateTime to )
		{
			return this.ParseStream( history );
		}

        /// <summary>
        /// Assigns the file info.	
        /// </summary>
        /// <param name="modification">The modification.</param>
        /// <param name="file">The file.</param>
        /// <remarks></remarks>
		public void AssignFileInfo( Modification modification, string file )
		{
			int separatorLocation = file.LastIndexOf( Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture) );
			if ( separatorLocation > - 1 )
			{
				modification.FolderName = file.Substring( 0, separatorLocation );
				modification.FileName = file.Substring( separatorLocation + 1 );
			}
			else
			{
				modification.FolderName = string.Empty;
				modification.FileName = file;
			}
		}

        /// <summary>
        /// Assigns the modification time.	
        /// </summary>
        /// <param name="modification">The modification.</param>
        /// <param name="time">The time.</param>
        /// <remarks></remarks>
		public void AssignModificationTime( Modification modification, string time )
		{
			try
			{
				modification.ModifiedTime = DateTime.Parse( time, CultureInfo.CurrentCulture );
			}
			catch ( FormatException )
			{
				modification.ModifiedTime = DateTime.MinValue;
			}
		}

        /// <summary>
        /// Creates the new modification.	
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="time">The time.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="modificationType">Type of the modification.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="change">The change.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification CreateNewModification(
			string userName,
			string time,
			string elementName,
			string modificationType,
			string comment,
			string change )
		{
			Modification modification = new Modification();
			modification.ChangeNumber = change;
			modification.UserName = userName;
			modification.Type = modificationType;
			modification.Comment = ( (comment != null && comment.Length == 0) ? null : comment );
			this.AssignFileInfo( modification, elementName );
			this.AssignModificationTime( modification, time );
			return modification;
		}

        /// <summary>
        /// Parses the entry.	
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification ParseEntry( string line )
		{
			string[] tokens = this.TokenizeEntry( line );
			if ( tokens == null
				 || tokens.Length != 8 )
			{
				return null;
			}
			// A branch event shouldn't be considered a modification
			string modificationType = tokens[4].Trim().ToLower();
			if ( modificationType == "mkbranch" || modificationType == "rmbranch" )
			{
				return null;
			}
			return this.CreateNewModification(
				tokens[0].Trim(),
				tokens[1].Trim(),
				tokens[2].Trim(),
				tokens[4].Trim(),
				tokens[7].Trim(),
				tokens[5].Trim() );
		}

		internal Modification[] ParseStream( TextReader reader )
		{
            var modifications = new List<Modification>();
			string nextLine;
			while ( ( nextLine = reader.ReadLine() ) != null )
			{
				string line = null;

				if (nextLine.IndexOf(END_OF_LINE_DELIMITER) == -1)
				{
					line = this.AccumulateMultiLineEntry(nextLine, reader);
				}
				else
				{
					line = nextLine;
				}

				int lineIndex = line.IndexOf( END_OF_LINE_DELIMITER );
				Modification modification = this.ParseEntry( line.Substring( 0, lineIndex ) );	

				if ( modification != null )
				{
					modifications.Add( modification );
				}
			}
			return modifications.ToArray();
		}

		private string AccumulateMultiLineEntry(string nextLine, TextReader reader)
		{
			string followingLine;
			while (nextLine.IndexOf(END_OF_LINE_DELIMITER) == -1 && (followingLine = reader.ReadLine()) != null)
			{
				nextLine += followingLine;
			}
			return nextLine;
		}


        /// <summary>
        /// Tokenizes the entry.	
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string[] TokenizeEntry( string line )
		{
            var items = new List<string>();
			int firstDelimiter = -1;
			int entryIndex = 0;
			int secondDelimiter = line.IndexOf( DELIMITER, entryIndex );
			while ( secondDelimiter != -1 )
			{
				items.Add( line.Substring( entryIndex, secondDelimiter - entryIndex ) );

				firstDelimiter = secondDelimiter;
				entryIndex = firstDelimiter + DELIMITER.Length;
				secondDelimiter = line.IndexOf( DELIMITER, entryIndex );
			}
			items.Add( line.Substring( entryIndex, line.Length - entryIndex ) );
			return items.ToArray();
		}
	}
}