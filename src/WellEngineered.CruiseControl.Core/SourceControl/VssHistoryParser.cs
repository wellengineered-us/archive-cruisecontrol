using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// 	
    /// </summary>
	public class VssHistoryParser : IHistoryParser
	{
		private const string DELIMITER_VERSIONED_START = "*****************  ";
		private const string DELIMITER_VERSIONED_END = "  *****************";

		private const string DELIMITER_UNVERSIONED_START = "*****  ";
		private const string DELIMITER_UNVERSIONED_END = "  *****";

		private IVssLocale locale;

        /// <summary>
        /// Initializes a new instance of the <see cref="VssHistoryParser" /> class.	
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public VssHistoryParser(IVssLocale locale)
		{
			this.locale = locale;
		}

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
			string[] entries = this.ReadAllEntries(history);
			return this.ParseModifications(entries);
		}

        /// <summary>
        /// Parses the modifications.	
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public Modification[] ParseModifications(string[] entries)
		{
			// not every entry will yield a valid modification so we can't use
			// an array, but we can assume that most will so starting our 
			// list to be at least as big as the array will save
			// some resizing
            var modifications = new List<Modification>(entries.Length);

			foreach (string entry in entries)
			{
				VSSParser parser = VSSParserFactory.CreateParser(entry, this.locale);
				Modification mod = parser.Parse();
				if (mod != null)
					modifications.Add(mod);
			}

			return modifications.ToArray();
		}

        /// <summary>
        /// Reads all entries.	
        /// </summary>
        /// <param name="history">The history.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string[] ReadAllEntries(TextReader history)
		{
            var entries = new List<string>();
			string currentLine = history.ReadLine();
			while (this.IsEndOfFile(currentLine) == false)
			{
				if (this.IsEntryDelimiter(currentLine))
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(currentLine).Append("\n");
					currentLine = history.ReadLine();
					while (!this.IsEntryDelimiter(currentLine))
					{
						builder.Append(currentLine).Append("\n");
						currentLine = history.ReadLine();
					}
					entries.Add(builder.ToString());
				}
				else
				{
					currentLine = history.ReadLine();
				}
			}
			return entries.ToArray();
		}

        /// <summary>
        /// Determines whether [is entry delimiter] [the specified line].	
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool IsEntryDelimiter(string line)
		{
			return this.IsEndOfFile(line) || 
				(line.StartsWith(DELIMITER_UNVERSIONED_START) && line.EndsWith(DELIMITER_UNVERSIONED_END)) || 
				line.StartsWith(DELIMITER_VERSIONED_START) && line.EndsWith(DELIMITER_VERSIONED_END);
		}

		private bool IsEndOfFile(string line)
		{
			return line == null;
		}

        /// <summary>
        /// Gets or sets the locale.	
        /// </summary>
        /// <value>The locale.</value>
        /// <remarks></remarks>
		public IVssLocale Locale
		{
			get { return this.locale; }
			set { this.locale = value; }
		}
	}

	internal sealed class VSSParserFactory
	{
        private VSSParserFactory ()
        {}


		public static VSSParser CreateParser(string entry, IVssLocale locale)
		{
			string vssKeyworkdLine = ReadVSSKeywordLine(entry);
			if (vssKeyworkdLine.IndexOf(locale.CheckedInKeyword) > -1)
			{
				return new CheckInParser(entry, locale);
			}
			else if (vssKeyworkdLine.IndexOf(locale.AddedKeyword) > -1)
			{
				return new AddedParser(entry, locale);
			}
			else if (vssKeyworkdLine.IndexOf(locale.DeletedKeyword) > -1)
				return new DeletedParser(entry, locale);
			else if (vssKeyworkdLine.IndexOf(locale.DestroyedKeyword) > -1)
				return new DestroyedParser(entry, locale);

			return new NullParser(entry, locale);
		}

		private static string ReadVSSKeywordLine(string entry)
		{
			StringReader reader = new StringReader(entry);
			reader.ReadLine();
			reader.ReadLine();
			string nonCommentEntry = reader.ReadLine() + Environment.NewLine + reader.ReadLine();
			return nonCommentEntry;
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public abstract class VSSParser
	{
		private Regex REGEX_USER_DATE_LINE;
		private static readonly Regex REGEX_FILE_NAME = new Regex(@"\*+([\w\s\.-]+)", RegexOptions.Multiline);

        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		protected string entry;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		protected IVssLocale locale;

		internal const string DELIMITER_VERSIONED_START = "*****************  ";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSSParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
	    protected VSSParser(string entry, IVssLocale locale)
		{
			this.entry = entry.Replace(Convert.ToChar(160).ToString(CultureInfo.CurrentCulture),string.Empty);
			this.locale = locale;
			string regex = string.Format(CultureInfo.CurrentCulture, @"{0}:(.+){1}:(.+){2}:(.+)$", locale.UserKeyword, locale.DateKeyword, locale.TimeKeyword);
			this.REGEX_USER_DATE_LINE = new Regex(regex, RegexOptions.Multiline);
			this.locale = locale;
		}

        /// <summary>
        /// Parses this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public virtual Modification Parse()
		{
			Modification mod = new Modification();
			mod.Type = this.Keyword;
			this.ParseUsernameAndDate(mod);
			this.ParseComment(mod);
			mod.FileName = this.ParseFileName();
			mod.FolderName = this.ParseFolderName();
			return mod;
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public abstract string Keyword { get; }

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public abstract string ParseFileName();

        /// <summary>
        /// Parses the username and date.	
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <remarks></remarks>
		public void ParseUsernameAndDate(Modification mod)
		{
			Match match = this.REGEX_USER_DATE_LINE.Match(this.entry);
			if (! match.Success)
			{
				throw new CruiseControlException("Invalid data retrieved from VSS.  Unable to parse username and date from text. " + this.entry);
			}

			mod.UserName = match.Groups[1].Value.Trim();
			string date = match.Groups[2].Value.Trim();
			string time = match.Groups[3].Value.Trim();

			mod.ModifiedTime = this.locale.ParseDateTime(date, time);
		}

        /// <summary>
        /// Parses the comment.	
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <remarks></remarks>
		public void ParseComment(Modification mod)
		{
			string comment = this.locale.CommentKeyword + ":";
			int index = this.entry.IndexOf(comment);
			if (index > -1)
			{
				mod.Comment = this.entry.Substring(index + comment.Length).Trim();
			}
		}

        /// <summary>
        /// Parses the name of the folder.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public virtual string ParseFolderName()
		{
			string checkedin = this.locale.CheckedInKeyword;
			string comment = this.locale.CommentKeyword + ":";

			string folderName = null;
			int checkinIndex = this.entry.IndexOf(checkedin);
			if (checkinIndex > -1)
			{
				int startIndex = checkinIndex + checkedin.Length;
				int length = this.entry.Length - startIndex;
				int commentIndex = this.entry.IndexOf(comment);
				if (commentIndex > 0)
				{
					length = commentIndex - startIndex;
				}
				folderName = this.entry.Substring(startIndex, length).Trim();
			}
			return folderName;
		}

        /// <summary>
        /// Parses the file name other.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		protected string ParseFileNameOther()
		{
			try
			{
				int timeIndex = this.entry.IndexOf(this.locale.TimeKeyword + ":");
				int newlineIndex = this.entry.IndexOf("\n", timeIndex);

				int addedIndex = this.entry.IndexOf(this.Keyword, newlineIndex);
				string fileName = this.entry.Substring(newlineIndex, addedIndex - newlineIndex);
				return fileName.Trim();
			}
			catch (Exception e)
			{
				throw new CruiseControlException(string.Format(System.Globalization.CultureInfo.CurrentCulture,"ParseFileNameOther failed on string \"{0}\", type {1}", this.entry, this.Keyword), e);
			}
		}

        /// <summary>
        /// Parses the first name of the line.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public string ParseFirstLineName()
		{
			Match match = REGEX_FILE_NAME.Match(this.entry);
			return match.Groups[1].Value.Trim();
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public class CheckInParser : VSSParser
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public CheckInParser(string entry, IVssLocale locale) : base(entry, locale)
		{
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public override string Keyword
		{
			get { return this.locale.CheckedInKeyword; }
		}

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFileName()
		{
			return this.ParseFirstLineName();
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public class AddedParser : VSSParser
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="AddedParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public AddedParser(string entry, IVssLocale locale) : base(entry, locale)
		{
		}

        /// <summary>
        /// Parses this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override Modification Parse()
		{
			Modification mod = base.Parse();
			if (mod.FileName.StartsWith("$"))
				return null;
			else
				return mod;
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public override string Keyword
		{
			get { return this.locale.AddedKeyword; }
		}

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFileName()
		{
			return this.ParseFileNameOther();
		}

        /// <summary>
        /// Parses the name of the folder.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFolderName()
		{
			if (this.entry.StartsWith(DELIMITER_VERSIONED_START))
				return  "[projectRoot]";
			else
				return this.ParseFirstLineName();
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public class DeletedParser : VSSParser
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="DeletedParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public DeletedParser(string entry, IVssLocale locale) : base(entry, locale)
		{
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public override string Keyword
		{
			get { return this.locale.DeletedKeyword; }
		}

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFileName()
		{
			return this.ParseFileNameOther();
		}

        /// <summary>
        /// Parses the name of the folder.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFolderName()
		{
			if (this.entry.StartsWith(DELIMITER_VERSIONED_START))
				return  "[projectRoot]";
			else
				return this.ParseFirstLineName();
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public class DestroyedParser : VSSParser
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyedParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public DestroyedParser(string entry, IVssLocale locale) : base(entry, locale)
		{
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public override string Keyword
		{
			get { return this.locale.DestroyedKeyword; }
		}

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFileName()
		{
			return this.ParseFileNameOther();
		}

        /// <summary>
        /// Parses the name of the folder.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFolderName()
		{
			if (this.entry.StartsWith(DELIMITER_VERSIONED_START))
				return "[projectRoot]";
			else
				return this.ParseFirstLineName();
		}
	}

    /// <summary>
    /// 	
    /// </summary>
	public class NullParser : VSSParser
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="NullParser" /> class.	
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="locale">The locale.</param>
        /// <remarks></remarks>
		public NullParser(string entry, IVssLocale locale) : base(entry, locale)
		{
		}

        /// <summary>
        /// Gets the keyword.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public override string Keyword
		{
			get { return null; }
		}

        /// <summary>
        /// Parses this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override Modification Parse()
		{
			return null;
		}

        /// <summary>
        /// Parses the name of the file.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ParseFileName()
		{
			return null;
		}
	}
}