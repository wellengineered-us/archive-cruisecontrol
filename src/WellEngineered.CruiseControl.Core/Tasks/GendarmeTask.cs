using System.Diagnostics;
using System.Globalization;
using System.IO;

using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks
{
    /// <summary>
    /// <para>
    /// Gendarme is a extensible rule-based tool to find problems in .NET applications and libraries. Gendarme inspects programs and libraries
    /// that contain code in ECMA CIL format (Mono and .NET) and looks for common problems with the code, problems that compiler do not
    /// typically check or have not historically checked. Website: http://mono-project.com/Gendarme
    /// </para>
    /// <para type="tip">
    /// See <link>using WellEngineered.CruiseControl.NET with Gendarme</link> for more details.
    /// </para>
    /// </summary>
    /// <title>Gendarme Task</title>
    /// <version>1.4.3</version>
    /// <example>
    /// <code title="Minimalist example">
    /// &lt;gendarme&gt;
    /// &lt;assemblies&gt;
    /// &lt;assemblyMatch expr='*.dll' /&gt;
    /// &lt;assemblyMatch expr='*.exe' /&gt;
    /// &lt;/assemblies&gt;
    /// &lt;/gendarme&gt;
    /// </code>
    /// <code title="Full example">
    /// &lt;gendarme&gt;
    /// &lt;executable&gt;Tools\gendarme.exe&lt;/executable&gt;
    /// &lt;baseDirectory&gt;C:\Build\Project1\Bin\Debug\&lt;/baseDirectory&gt;
    /// &lt;configFile&gt;rules.xml&lt;/configFile&gt;
    /// &lt;ruleSet&gt;*&lt;/ruleSet&gt;
    /// &lt;ignoreFile&gt;C:\Build\Project1\gendarme.ignore.list.txt&lt;/ignoreFile&gt;
    /// &lt;limit&gt;200&lt;/limit&gt;
    /// &lt;severity&gt;medium+&lt;/severity&gt;
    /// &lt;confidence&gt;normal+&lt;/confidence&gt;
    /// &lt;quiet&gt;FALSE&lt;/quiet&gt;
    /// &lt;verbose&gt;TRUE&lt;/verbose&gt;
    /// &lt;failBuildOnFoundDefects&gt;TRUE&lt;/failBuildOnFoundDefects&gt;
    /// &lt;verifyTimeoutSeconds&gt;600&lt;/verifyTimeoutSeconds&gt;
    /// &lt;assemblyListFile&gt;C:\Build\Project1\gendarme.assembly.list.txt&lt;/assemblyListFile&gt;
    /// &lt;description&gt;Test description&lt;/description&gt;
    /// &lt;/gendarme&gt;
    /// </code>
    /// </example>
	[ReflectorType("gendarme")]
	public class GendarmeTask : BaseExecutableTask
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string defaultExecutable = "gendarme";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string logFilename = "gendarme-results.xml";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int defaultLimit = -1;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const bool defaultQuiet = false;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const bool defaultVerbose = false;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const bool defaultFailBuildOnFoundDefects = false;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int defaultVerifyTimeout = 0;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public const ProcessPriorityClass defaultPriority = ProcessPriorityClass.Normal;

        private readonly IFileDirectoryDeleter fileDirectoryDeleter = new IoService();

        /// <summary>
        /// Initializes a new instance of the <see cref="GendarmeTask" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public GendarmeTask(): 
			this(new ProcessExecutor()){}

        /// <summary>
        /// Initializes a new instance of the <see cref="GendarmeTask" /> class.	
        /// </summary>
        /// <param name="executor">The executor.</param>
        /// <remarks></remarks>
		public GendarmeTask(ProcessExecutor executor)
		{
			this.executor = executor;
            this.Assemblies = new AssemblyMatch[0];
            this.AssemblyListFile = string.Empty;
            this.VerifyTimeoutSeconds = defaultVerifyTimeout;
            this.Executable = defaultExecutable;
            this.ConfiguredBaseDirectory = string.Empty;
            this.Priority = defaultPriority;
            this.ConfigFile = string.Empty;
            this.RuleSet = string.Empty;
            this.IgnoreFile = string.Empty;
            this.Limit = defaultLimit;
            this.Severity = string.Empty;
            this.Confidence = string.Empty;
            this.Quiet = defaultQuiet;
            this.Verbose = defaultVerbose;
            this.FailBuildOnFoundDefects = defaultFailBuildOnFoundDefects;
		}

		#region public properties

		/// <summary>
        /// The location of the Gendarme executable.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>gendarme</default>
        [ReflectorProperty("executable", Required = false)]
        public string Executable { get; set; }

		/// <summary>
        /// The directory to run Gendarme in.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>Project Working Directory</default>
        [ReflectorProperty("baseDirectory", Required = false)]
        public string ConfiguredBaseDirectory { get; set; }

		/// <summary>
        /// The priority class of the spawned process.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Normal</default>
        [ReflectorProperty("priority", Required = false)]
        public ProcessPriorityClass Priority { get; set; }

		/// <summary>
        /// Specify the configuration file.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>rules.xml</default>
        /// <remarks>
        /// <b>Maps to "--config configfile"</b>
        /// </remarks>
        [ReflectorProperty("configFile", Required = false)]
        public string ConfigFile { get; set; }

		/// <summary>
		/// Specify the set of rules to verify.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>*</default>
        /// <remarks>
        /// <b>Maps to "--set ruleset"</b>
        /// </remarks>
        [ReflectorProperty("ruleSet", Required = false)]
        public string RuleSet { get; set; }

		/// <summary>
		/// Do not report the known defects that are part of the specified file.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>None</default>
        /// <remarks>
        /// <b>Maps to "--ignore ignore-file"</b>
        /// </remarks>
        [ReflectorProperty("ignoreFile", Required = false)]
        public string IgnoreFile { get; set; }

		/// <summary>
		/// Stop reporting after N defects are found.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>-1</default>
        /// <remarks>
        /// <b>Maps to "--limit N"</b>
        /// </remarks>
        [ReflectorProperty("limit", Required = false)]
        public int Limit { get; set; }

		/// <summary>
		/// Filter the reported defects to include the specified severity levels.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>Medium+</default>
        /// <remarks>
        /// <b>Maps to "--severity [all | audit[+] | low[+|-] | medium[+|-] | high[+|-] | critical[-]],..."</b>
        /// </remarks>
        [ReflectorProperty("severity", Required = false)]
        public string Severity { get; set; }

		/// <summary>
		/// Filter the reported defects to include the specified confidence levels.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>normal+</default>
        /// <remarks>
        /// <b>"--confidence [all | low[+] | normal[+|-] | high[+|-] | total[-]],..."</b>
        /// </remarks>
        [ReflectorProperty("confidence", Required = false)]
        public string Confidence { get; set; }

		/// <summary>
		/// If true, display minimal output (results) from the runner.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>false</default>
        [ReflectorProperty("quiet", Required = false)]
        public bool Quiet { get; set; }

		/// <summary>
		/// Enable debugging output.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>false</default>
        [ReflectorProperty("verbose", Required = false)]
        public bool Verbose { get; set; }

		/// <summary>
		/// Specify whenver the build should fail if some defects are found by Gendarme.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>false</default>
        [ReflectorProperty("failBuildOnFoundDefects", Required = false)]
        public bool FailBuildOnFoundDefects { get; set; }

		/// <summary>
		/// Specify the assemblies to verify. You can specify multiple filenames, including masks (? and *).
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>None</default>
        [ReflectorProperty("assemblies", Required = false)]
        public AssemblyMatch[] Assemblies { get; set; }

		/// <summary>
		/// Specify a file that contains the assemblies to verify. You can specify multiple filenames, including masks (? and *), one by line.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>None</default>
        [ReflectorProperty("assemblyListFile", Required = false)]
        public string AssemblyListFile { get; set; }

		/// <summary>
		/// The maximum number of seconds that the build may take.  If the build process takes longer than this period, it will be killed.  Specify this value as zero to disable process timeouts.
		/// </summary>
        /// <version>1.4.3</version>
        /// <default>0</default>
        [ReflectorProperty("verifyTimeoutSeconds", Required = false)]
        public int VerifyTimeoutSeconds { get; set; }
		#endregion

		#region BaseExecutableTask overrides

        /// <summary>
        /// Gets the process filename.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override string GetProcessFilename()
		{
			return this.Executable;
		}

        /// <summary>
        /// Gets the process arguments.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override string GetProcessArguments(IIntegrationResult result)
		{
			ProcessArgumentBuilder buffer = new ProcessArgumentBuilder();
			buffer.AppendIf(!string.IsNullOrEmpty(this.ConfigFile), "--config {0}", StringUtil.AutoDoubleQuoteString(this.ConfigFile));
			buffer.AppendIf(!string.IsNullOrEmpty(this.RuleSet), "--set {0}", this.RuleSet);
			buffer.AppendIf(!string.IsNullOrEmpty(this.IgnoreFile), "--ignore {0}", StringUtil.AutoDoubleQuoteString(this.IgnoreFile));
			buffer.AppendIf(this.Limit > 0, "--limit {0}", this.Limit.ToString(CultureInfo.CurrentCulture));
			buffer.AppendIf(!string.IsNullOrEmpty(this.Severity), "--severity {0}", this.Severity);
			buffer.AppendIf(!string.IsNullOrEmpty(this.Confidence), "--confidence {0}", this.Confidence);
			buffer.AppendIf(this.Quiet, "--quiet");
			buffer.AppendIf(this.Verbose, "--verbose");

			// append output xml file
			buffer.AppendArgument("--xml {0}", StringUtil.AutoDoubleQuoteString(GetGendarmeOutputFile(result)));

			// append assembly list or list file
			this.CreateAssemblyList(buffer);

			return buffer.ToString();
		}

        /// <summary>
        /// Gets the process base directory.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override string GetProcessBaseDirectory(IIntegrationResult result)
		{
			return result.BaseFromWorkingDirectory(this.ConfiguredBaseDirectory);
		}

        /// <summary>
        /// Gets the process timeout.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override int GetProcessTimeout()
		{
			return this.VerifyTimeoutSeconds * 1000;
		}

        /// <summary>
        /// Gets the process priority class.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override ProcessPriorityClass GetProcessPriorityClass()
        {
            return this.Priority;
        }

        /// <summary>
        /// Executes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
				protected override bool Execute(IIntegrationResult result)
				{
					string gendarmeOutputFile = GetGendarmeOutputFile(result);
					//delete old nant output logfile, if exist
					this.fileDirectoryDeleter.DeleteIncludingReadOnlyObjects(gendarmeOutputFile);

					result.BuildProgressInformation.SignalStartRunTask(!string.IsNullOrEmpty(this.Description) ? this.Description :
			"Executing Gendarme to verifiy assemblies.");

					var info = this.CreateProcessInfo(result);
					ProcessResult processResult = this.TryToRun(this.CreateProcessInfo(result), result);

					if (File.Exists(gendarmeOutputFile))
					{
						result.AddTaskResult(new FileTaskResult(gendarmeOutputFile));
					}

					result.AddTaskResult(new ProcessTaskResult(processResult, true));

					if (processResult.TimedOut)
						result.AddTaskResult(MakeTimeoutBuildResult(info));

					return processResult.Succeeded;
				}

		/// <summary>
		/// Gendarme returns the following codes:
		/// - 0 for success
		/// - 1 if some defects are found
		/// - 2 if some parameters are bad
		/// - 3 if a problem is related to the xml configuration file
		/// - 4 if an uncaught exception occured
		/// </summary>
		/// <returns>Defects should not break the build, so return an array of 0 and 1.</returns>
		protected override int[] GetProcessSuccessCodes()
		{
			if (this.FailBuildOnFoundDefects)
				return new int[] {0};

			return new int[] {0, 1};
		}

		#endregion

		#region private methods

		private static string GetGendarmeOutputFile(IIntegrationResult result)
		{
			return Path.Combine(result.ArtifactDirectory, logFilename);
		}

		private void CreateAssemblyList(ProcessArgumentBuilder buffer)
		{
			if (string.IsNullOrEmpty(this.AssemblyListFile) && (this.Assemblies == null || this.Assemblies.Length == 0))
				throw new ConfigurationException("[GendarmeTask] Neither 'assemblyListFile' nor 'assemblies' are specified. Please specify one of them.");

			// append the assembly list file if set
			if (!string.IsNullOrEmpty(this.AssemblyListFile))
				buffer.AppendArgument(string.Concat("@", StringUtil.AutoDoubleQuoteString(this.AssemblyListFile)));

			// build the assembly list by the assembly match collection
			foreach (AssemblyMatch asm in this.Assemblies)
				buffer.AppendArgument(asm.Expression);
		}

		#endregion
	}
}
