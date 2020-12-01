using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.SourceControl
{
    /// <summary>
    /// CruiseControl.NET supports integrating with the PVCS Source Control system via the pcli client.
    /// </summary>
    /// <title>PVCS Source Control Block</title>
    /// <version>1.0</version>
    /// <key name="type">
    /// <description>The type of source control block.</description>
    /// <value>pvcs</value>
    /// </key>
    /// <example>
    /// <code>
    /// &lt;sourcecontrol type="pvcs"&gt;
    /// &lt;executable&gt;c:\pvcs\pvcs.exe&lt;/executable&gt;
    /// &lt;project&gt;ccnet&lt;/project&gt;
    /// &lt;subproject&gt;ccnet1.0&lt;/subproject&gt;
    /// &lt;/sourcecontrol&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>
    /// Contributed by James Bolles.
    /// </para>
    /// </remarks>
    [ReflectorType("pvcs")]
	public class Pvcs : ProcessSourceControl
	{
		private const string DELETE_LABEL_TEMPLATE =
			@"run -y -xe""{0}"" -xo""{1}"" DeleteLabel -pr""{2}"" {3} {4} {5} {6}";

		private const string APPLY_LABEL_TEMPLATE =
			@"Vcs -q -xo""{0}"" -xe""{1}"" {2} -v""{3}"" ""@{4}""";

		private const string VLOG_INSTRUCTIONS_TEMPLATE =
			@"run -xe""{0}"" -xo""{1}"" -q vlog -pr""{2}"" {3} {4} -ds""{5}"" -de""{6}"" {7}";

		private const string VLOG_LABEL_INSTRUCTIONS_TEMPLATE =
			@"run -xe""{0}"" -xo""{1}"" -q vlog -pr""{2}"" {3} {4} -r""{5}"" {6}";

		private const string GET_INSTRUCTIONS_TEMPLATE =
			@"run -y -xe""{0}"" -xo""{1}"" -q Get -pr""{2}"" {3} {4} -sp""{5}"" {6} {7} ";

		private const string INDIVIDUAL_GET_REVISION_TEMPLATE =
			@"-r{0} ""{1}{2}\{3}""(""{4}"") ";

		private const string INDIVIDUAL_LABEL_REVISION_TEMPLATE =
			@"{0} ""{1}{2}\{3}"" ";

		private TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
		private string baseLabelName =string.Empty;
		private Modification[] modifications = null;
		private Modification[] baseModifications = null;
		private string errorFile =string.Empty;
		private string logFile =string.Empty;
		private string tempFile =string.Empty;
		private string tempLabel =string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pvcs" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public Pvcs() : this(new PvcsHistoryParser(), new ProcessExecutor())
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="Pvcs" /> class.	
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="executor">The executor.</param>
        /// <remarks></remarks>
        public Pvcs(IHistoryParser parser, ProcessExecutor executor)
            : base(parser, executor)
        {
            this.Executable = "pcli.exe";
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.WorkingDirectory = string.Empty;
            this.Workspace = "/@/RootWorkspace";
            this.Recursive = true;
            this.LabelOnSuccess = false;
            this.AutoGetSource = true;
            this.ManuallyAdjustForDaylightSavings = false;
        }

        /// <summary>
        /// The PVCS client executable.
        /// </summary>
        /// <version>1.0</version>
        /// <default>pcli.exe</default>
        [ReflectorProperty("executable")]
        public string Executable { get; set; }

        /// <summary>
        /// The location of the PVCS project database.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("project")]
        public string Project { get; set; }

        /// <summary>
        /// One ore more projects in PVCS that you wish to monitor. As long as each subproject is separated with a space
        /// and a "/", you can monitor more than one subproject at a time.
        /// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("subproject")]
        public string Subproject { get; set; }

        /// <summary>
        /// Username for the user account to use to connect to PVCS.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("username", Required = false)]
        public string Username { get; set; }

        /// <summary>
        /// Password for the PVCS user account.
        /// </summary>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("password", Required = false)]
        public string Password { get; set; }

        /// <summary>
        /// The local directory containing the source from the repository. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>Project Working Directory</default>
        [ReflectorProperty("workingdirectory", Required = false)]
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The workspace to use.
        /// </summary>
        /// <version>1.0</version>
        /// <default>/@/RootWorkspace</default>
        [ReflectorProperty("workspace", Required = false)]
        public string Workspace { get; set; }

        /// <summary>
        /// Whether to monitor all subfolders of the specified subproject.
        /// </summary>
        /// <version>1.0</version>
        /// <default>true</default>
        [ReflectorProperty("recursive", Required = false)]
        public bool Recursive { get; set; }

        /// <summary>
        /// Whether or not to apply a label to the repository after each successful build. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>false</default>
        [ReflectorProperty("labelOnSuccess", Required = false)]
        public bool LabelOnSuccess { get; set; }

        /// <summary>
        /// Specifies whether the CCNet should take responsibility for retrieving the current version of the source from
        /// the repository.
        /// </summary>
        /// <version>1.0</version>
        /// <default>true</default>
        [ReflectorProperty("autoGetSource", Required = false)]
        public bool AutoGetSource { get; set; }

        /// <summary>
        /// In PVCS 7.5.1, the client does not automatically adjust dates to accommodate daylight savings time. Setting
        /// this flag to true will make CCNet compensate for it.
        /// </summary>
        /// <version>1.2.2</version>
        /// <default>false</default>
        [ReflectorProperty("manuallyAdjustForDaylightSavings", Required = false)]
        public bool ManuallyAdjustForDaylightSavings { get; set; }

		//TODO: Support Promotion Groups -- [ReflectorProperty("isPromotionGroup", Required=false)]
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public bool IsPromotionGroup/* = false*/;

        /// <summary>
        /// The label to use as your code-base. If this is specified, this label will be called to get all code
        /// associated with it when a get is done. When the build is successful, the good code will have this base label
        /// associated with it in turn promoting it into the label. Label to apply to repository. If a value is
        /// specified, labelOnSuccess will automatically be set to true. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>none</default>
        [ReflectorProperty("labelOrPromotionName", Required = false)]
		public string LabelOrPromotionName
		{
			get { return this.baseLabelName; }
			set
			{
				this.baseLabelName = value;
                this.LabelOnSuccess = !string.IsNullOrEmpty(this.baseLabelName);
			}
		}

        /// <summary>
        /// Sets the current time zone.	
        /// </summary>
        /// <value>The current time zone.</value>
        /// <remarks></remarks>
		public TimeZone CurrentTimeZone
		{
			set { this.currentTimeZone = value; }
		}

        /// <summary>
        /// Gets the error file.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string ErrorFile
		{
			get { return this.errorFile = this.TempFileNameIfBlank(this.errorFile); }
		}

        /// <summary>
        /// Gets the log file.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string LogFile
		{
			get { return this.logFile = this.TempFileNameIfBlank(this.logFile); }
		}

        /// <summary>
        /// Gets the temp file.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string TempFile
		{
			get { return this.tempFile = this.TempFileNameIfBlank(this.tempFile); }
		}

        /// <summary>
        /// Labels the or promotion input.	
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string LabelOrPromotionInput(string label)
		{
			return (label.Length == 0) ?string.Empty : (this.IsPromotionGroup == false ? "-v" : "-g") + label;
		}

        /// <summary>
        /// Gets the modifications.	
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public override Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			this.baseModifications = null;

			using (TextReader reader = this.ExecuteVLog(from.StartTime, to.StartTime))
			{
				this.modifications = this.ParseModifications(reader, from.StartTime, to.StartTime);
			}
            base.FillIssueUrl(this.modifications);
			return this.modifications;
		}

		private string GetRecursiveValue()
		{
			return this.Recursive ? "-z" : string.Empty;
		}

		private TextReader ExecuteVLog(DateTime from, DateTime to)
		{
			// required due to DayLightSavings bug in PVCS 7.5.1
			if (this.ManuallyAdjustForDaylightSavings)
			{
				from = this.AdjustForDayLightSavingsBug(from);
				to = this.AdjustForDayLightSavingsBug(to);				
			}
			this.Execute(this.CreatePcliContentsForCreatingVLog(GetDateString(from), GetDateString(to)));
			return this.GetTextReader(this.LogFile);
		}

        /// <summary>
        /// Generate a userid-login option if needed.
        /// </summary>
        /// <param name="doubleQuotes">If true, wrap the entire option in double-quotes ('"').</param>
        /// <returns>The option, possibly <see cref="String.Empty"/>.</returns>
        /// <remarks>
        /// PVCS allows users to have no password, so we have three different choices (five if you count
        /// the double quotes):
        /// <list type="ul">
        /// <item>(nothing)</item>
        /// <item> -id"username" </item>
        /// <item> -id"username":"password" </item>
        /// <item> "-id"username"" </item>
        /// <item> "-id"username":"password"" </item>
        /// </list>
        /// </remarks>
		public string GetLogin(bool doubleQuotes)
		{
            if (this.Username.Length == 0)
                return string.Empty;
            string quotes = doubleQuotes ? "\"\"" : string.Empty;
            if (this.Password.Length == 0)
				return string.Format(System.Globalization.CultureInfo.CurrentCulture," {1}-id\"{0}\"{1} ", this.Username, quotes);
            return string.Format(System.Globalization.CultureInfo.CurrentCulture," {2}-id\"{0}\":\"{1}\"{2} ", this.Username, this.Password, quotes);
        }

		private void ExecuteNonPvcsFunction(string content)
		{
			string filename = Path.GetTempFileName();
			filename = filename.Substring(0, filename.Length - 3) + "cmd";
			try
			{
				this.CreatePVCSInstructionFile(filename, content);
				string arguments = string.Format(CultureInfo.CurrentCulture, @"/c ""{0}""", filename);
				this.Execute(this.CreatePVCSProcessInfo("cmd.exe", arguments));
			}
			finally
			{
				if (File.Exists(filename))
					File.Delete(filename);
			}
		}

		private void Execute(string pcliContent)
		{
			this.Execute(this.CreatePVCSProcessInfo(this.Executable, pcliContent));
		}

		private void CreatePVCSInstructionFile(string filename, string content)
		{
			using (StreamWriter stream = File.CreateText(filename))
			{
				stream.Write(content);
			}
		}

		private ProcessInfo CreatePVCSProcessInfo(string executable, string arguments)
		{
			return new ProcessInfo(executable, arguments);
		}

        /// <summary>
        /// Creates the pcli contents for get.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreatePcliContentsForGet()
		{
			return string.Format(CultureInfo.CurrentCulture, GET_INSTRUCTIONS_TEMPLATE, this.ErrorFile, this.LogFile, this.Project, this.GetLogin(false), this.GetRecursiveValue(), this.Workspace, this.LabelOrPromotionInput(this.tempLabel), this.Subproject);
		}

        /// <summary>
        /// Creates the pcli contents for creating V log.	
        /// </summary>
        /// <param name="beforedate">The beforedate.</param>
        /// <param name="afterdate">The afterdate.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreatePcliContentsForCreatingVLog(string beforedate, string afterdate)
		{
			return string.Format(CultureInfo.CurrentCulture, VLOG_INSTRUCTIONS_TEMPLATE, this.ErrorFile, this.LogFile, this.Project, this.GetLogin(false), this.GetRecursiveValue(), beforedate, afterdate, this.Subproject);
		}

        /// <summary>
        /// Creates the pcli contents for creating vlog by label.	
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreatePcliContentsForCreatingVlogByLabel(string label)
		{
			return string.Format(CultureInfo.CurrentCulture, VLOG_LABEL_INSTRUCTIONS_TEMPLATE, this.ErrorFile, this.LogFile, this.Project, this.GetLogin(false), this.GetRecursiveValue(), label, this.Subproject);
		}

        /// <summary>
        /// Creates the pcli contents for deleting label.	
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreatePcliContentsForDeletingLabel(string label)
		{
			return string.Format(CultureInfo.CurrentCulture, DELETE_LABEL_TEMPLATE, this.ErrorFile, this.LogFile, this.Project, this.GetLogin(false), this.GetRecursiveValue(), this.LabelOrPromotionInput(label), this.Subproject);
		}

        /// <summary>
        /// Creates the pcli contents for labeling.	
        /// </summary>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreatePcliContentsForLabeling(string label)
		{
			return string.Format(CultureInfo.CurrentCulture, APPLY_LABEL_TEMPLATE, this.LogFile, this.ErrorFile, this.GetLogin(false), label, this.TempFile);
		}

        /// <summary>
        /// Creates the individual label string.	
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <param name="label">The label.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreateIndividualLabelString(Modification mod, string label)
		{
			return string.Format(CultureInfo.CurrentCulture, INDIVIDUAL_LABEL_REVISION_TEMPLATE, this.GetVersion(mod, label), this.GetUncPathPrefix(mod), mod.FolderName, mod.FileName);
		}

        /// <summary>
        /// Creates the individual get string.	
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <param name="fileLocation">The file location.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string CreateIndividualGetString(Modification mod, string fileLocation)
		{
			return string.Format(CultureInfo.CurrentCulture, INDIVIDUAL_GET_REVISION_TEMPLATE, this.GetVersion(mod,string.Empty), this.GetUncPathPrefix(mod), mod.FolderName, mod.FileName, fileLocation);
		}

		private string GetUncPathPrefix(Modification mod)
		{
			//Add extract \ for UNC paths
			return mod.FolderName.StartsWith("\\") ? @"\" :string.Empty;
		}

		private string GetVersion(Modification mod, string label)
		{
			return ((label == null || label.Length == 0) ? (mod.Version == null ? "1.0" : mod.Version) : (this.LabelOrPromotionInput(label)));
		}

		private TextReader GetTextReader(string path)
		{
			FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			return new StreamReader(stream);
		}

        /// <summary>
        /// Adjusts for day light savings bug.	
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public DateTime AdjustForDayLightSavingsBug(DateTime date)
		{
			if (this.currentTimeZone.IsDaylightSavingTime(DateTime.Now))
			{
				TimeSpan anHour = new TimeSpan(1, 0, 0);
				return date.Subtract(anHour);
			}
			return date;
		}

		private string TempFileNameIfBlank(string file)
		{
            return string.IsNullOrEmpty(file) ? Path.GetTempFileName() : file;
		}

		#region GetSource Logic

        /// <summary>
        /// Gets the source.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <remarks></remarks>
		public override void GetSource(IIntegrationResult result)
		{
            result.BuildProgressInformation.SignalStartRunTask("Getting source from Pvcs");

			// If no changes or no Auto Get Source, exit
			if (result.Modifications.Length < 1 || !this.AutoGetSource)
				return;

			// Commented out until TemporaryLabeller logic can be worked out.
			// Execute( CreatePcliContentsForGet() );

			//Ensure that the working directory and Modifications are not empty
			this.WorkingDirectory = (this.WorkingDirectory.Length < 3) ? result.WorkingDirectory : this.WorkingDirectory; // why checking for length < 3?

			//Ensure that the Modifications are set locally
			this.modifications = result.Modifications;

			if (this.LabelOnSuccess && this.LabelOrPromotionName.Length > 0)
				this.DetermineMaxRevisions(this.LabelOrPromotionName);

			// Write the revision to pull from Version Manager and close the file
			StringDictionary createFolders = new StringDictionary();
			using (TextWriter stream = File.CreateText(this.TempFile))
			{
				foreach (Modification mod in this.modifications)
				{
					string fileLoc = this.DetermineFileLocation(mod.FolderName);

					if (!createFolders.ContainsKey(fileLoc))
						createFolders.Add(fileLoc, fileLoc);

					stream.WriteLine(this.CreateIndividualGetString(mod, fileLoc));

				}
			}
			this.ExecutePvcsGet(createFolders);
		}

		private string DetermineFileLocation(string folderName)
		{
			// Determine the Local File Location for when the Version Manager Get logic
			// Gets the revision to be built
			string folder = Path.GetFullPath(folderName).ToLower();
			string archive = this.Project.ToLower() + @"\archives";
			if (folder.IndexOf(archive) < 0)
			{
				return this.WorkingDirectory + folder;
			}
			else
			{
				return folder.Replace(archive, this.WorkingDirectory);
			}
		}

		private void ExecutePvcsGet(StringDictionary folders)
		{
			StringBuilder content = new StringBuilder();
			content.Append("@echo off \r\necho Create all necessary folders first\r\n");
			foreach (string key in folders.Keys)
			{
				content.AppendFormat(CultureInfo.CurrentCulture, "IF NOT EXIST \"{0}\" mkdir \"{0}\\\" >NUL \r\n", folders[key]);
			}

			content.Append("\r\necho Get all of the files by version number and archive location \r\n");
			// Build the Get Command based on the revisions -- Executable.Substring(0,Executable.LastIndexOf("\\"))
			content.AppendFormat("\"{0}\" -W -Y -xo\"{1}\" -xe\"{2}\" @\"{3}\"\r{4}", this.GetExeFilename(), this.LogFile, this.ErrorFile, this.TempFile, Environment.NewLine);

			// Allow us to use same logic for Executing files
			this.ExecuteNonPvcsFunction(content.ToString());
		}

        /// <summary>
        /// Gets the exe filename.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public string GetExeFilename()
		{
			string dir = Path.GetDirectoryName(this.Executable);
			return Path.Combine(dir, "Get.exe");
		}

		#endregion

		#region LabelSourceControl Logic

        /// <summary>
        /// Labels the source control.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <remarks></remarks>
		public override void LabelSourceControl(IIntegrationResult result)
		{
			// If no changes or LabelOnSuccess is false exit
			if (result.Modifications.Length < 1 || this.LabelOnSuccess == false || ! result.Succeeded) //|| _temporaryLabel.Length == 0
				return;

			this.modifications = result.Modifications;

			// Ensure the Label Or Promotion Name exist
			if (this.LabelOrPromotionName.Length > 0)
				this.LabelSourceControl(string.Empty, this.LabelOrPromotionName, result.ProjectName);
			// This allows for the labeller concept to support absolute labelling
			if (result.Label != this.LabelOrPromotionName)
				this.LabelSourceControl(this.LabelOrPromotionName, result.Label, result.ProjectName);
		}

		private void LabelSourceControl(string oldLabel, string newLabel, string project)
		{
			if (oldLabel.Length > 0)
			{
				Log.Info("Copying PVCS Label " + oldLabel + " to " + newLabel);

				// Determine what revisions between old label and new label
				// Get assigned the new label
				this.DetermineMaxRevisions(oldLabel);
			}
			using (TextWriter stream = File.CreateText(this.TempFile))
			{
				foreach (Modification mod in this.modifications)
				{
					stream.WriteLine(this.CreateIndividualLabelString(mod, (oldLabel.Length > 0 ? newLabel :string.Empty)));
				}
			}
			Log.Info("Applying PVCS Label " + newLabel + " on Project " + project);
			// Allow us to use same logic for Executing files
			this.ExecuteNonPvcsFunction(this.CreatePcliContentsForLabeling(newLabel));
		}

		private void DetermineMaxRevisions(string oldLabel)
		{
			// Only Execute this one-time during this process
			// until the GetModifications is run again.
			if (this.baseModifications == null)
			{
				Log.Info("Determine Revisions based on Promotion Group/Label : " + oldLabel);
				// Execute new VLog Session to get revision for old Label
				this.Execute(this.CreatePcliContentsForCreatingVlogByLabel(oldLabel));

				using (TextReader reader = this.GetTextReader(this.LogFile))
				{
					this.baseModifications = this.historyParser.Parse(reader, DateTime.Now, DateTime.Now);
				}
			}

            var allMods = new List<Modification>();
			foreach (Modification mod in this.baseModifications)
			{
				allMods.Add(mod);
			}
			foreach (Modification mod in this.modifications)
			{
				allMods.Add(mod);
			}

			// Only Modifications that need stamp should be generated
			this.modifications = PvcsHistoryParser.AnalyzeModifications(allMods);
		}

		#endregion

        /// <summary>
        /// Gets the date string.	
        /// </summary>
        /// <param name="dateToConvert">The date to convert.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public static string GetDateString(DateTime dateToConvert)
		{
			return GetDateString(dateToConvert, CultureInfo.CurrentCulture.DateTimeFormat);
		}

        /// <summary>
        /// Gets the date string.	
        /// </summary>
        /// <param name="dateToConvert">The date to convert.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public static string GetDateString(DateTime dateToConvert, DateTimeFormatInfo format)
		{
			string pattern = string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} {1}", format.ShortDatePattern, format.ShortTimePattern);
			return dateToConvert.ToString(pattern, format);
		}

        /// <summary>
        /// Gets the date.	
        /// </summary>
        /// <param name="dateToParse">The date to parse.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public static DateTime GetDate(string dateToParse)
		{
			return GetDate(dateToParse, CultureInfo.CurrentCulture);
		}

        /// <summary>
        /// Gets the date.	
        /// </summary>
        /// <param name="dateToParse">The date to parse.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public static DateTime GetDate(string dateToParse, IFormatProvider format)
		{
			try
			{
				return DateTime.Parse(dateToParse, format);
			}
			catch (Exception ex)
			{
				throw new CruiseControlException("Unable to parse: " + dateToParse, ex);
			}
		}
	}
}