

using System.Diagnostics;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.Tasks
{
	/// <summary>
    /// <para>
    /// Executes Rake.
    /// </para>
    /// </summary>
    /// <title> Rake Task </title>
    /// <version>1.4</version>
    /// <example>
    /// <code title="Minimalist example">
    /// &lt;rake /&gt;
    /// </code>
    /// <code title="Full example">
    /// &lt;rake&gt;
    /// &lt;executable&gt;c:\ruby\bin\rake.bat&lt;/executable&gt;
    /// &lt;baseDirectory&gt;c:\fromcvs\myrepo\myproject&lt;/baseDirectory&gt;
    /// &lt;buildArgs&gt;additional-argument&lt;/buildArgs&gt;
    /// &lt;rakefile&gt;Rakefile&lt;/rakefile&gt;
    /// &lt;targetList&gt;
    /// &lt;target&gt;build&lt;/target&gt;
    /// &lt;/targetList&gt;
    /// &lt;buildTimeoutSeconds&gt;1200&lt;/buildTimeoutSeconds&gt;
    /// &lt;quiet&gt;false&lt;/quiet&gt;
    /// &lt;silent&gt;false&lt;/silent&gt;
    /// &lt;trace&gt;true&lt;/trace&gt;
    /// &lt;/rake&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <heading>Accessing CruiseControl.NET build labels in Rake</heading>
    /// <para>
    /// CCNet will pass the current build label to Rake via the environment variable CCNetLabel. This means that you can access this 
    /// variable too. For example, archive the build results in a folder with the same name as the build label (this is what we do on
    /// CCNetLive  using NAnt. Here's some example Rakefile demonstrating how to do this:
    /// </para>
    /// <code type="none">
    /// #!ruby
    /// require 'rake'
    /// 
    /// task :default =&gt; [:deploy]
    /// 
    /// task :deploy do
    /// 	publishdir="C:/download-area/CCNet-Builds/#{ENV['CCNetLabel']}"
    /// 	mkdir_p publishdir
    /// 	FileList['dist/*'].each do |file|
    /// 		cp file, publishdir
    /// 	end
    /// end
    /// </code>
    /// <para>
    /// See <link>Integration Properties</link> for the values that are passed to the task.
    /// </para>
    /// </remarks>
    [ReflectorType("rake")]
	public class RakeTask
        : BaseExecutableTask
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int DefaultBuildTimeout = 600;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string DefaultExecutable = @"rake";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public const ProcessPriorityClass DefaultPriority = ProcessPriorityClass.Normal;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RakeTask"/> class.
        /// </summary>
        public RakeTask()
            : this(new ProcessExecutor()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RakeTask"/> class.
        /// </summary>
        /// <param name="executor">The executor.</param>
        public RakeTask(ProcessExecutor executor)
        {
            this.executor = executor;
            this.BuildArgs = string.Empty;
            this.BaseDirectory = string.Empty;
            this.BuildTimeoutSeconds = RakeTask.DefaultBuildTimeout;
            this.Executable = RakeTask.DefaultExecutable;
            this.Priority = RakeTask.DefaultPriority;
            this.Rakefile = string.Empty;
            this.Targets = new string[0];
        }
        #endregion

        /// <summary>
        /// Any arguments to pass through to Rake (e.g to specify build properties).
        /// </summary>
        /// <default>None</default>
        /// <version>1.4</version>
        [ReflectorProperty("buildArgs", Required = false)]
        public string BuildArgs { get; set; }

        /// <summary>
        /// The directory to run the Rake process in. If relative, is a subdirectory of the Project Working Directory.
        /// </summary>
        /// <default>Project Working Directory</default>
        /// <version>1.4</version>
        [ReflectorProperty("baseDirectory", Required = false)]
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Number of seconds to wait before assuming that the process has hung and should be killed. 
        /// </summary>
        /// <default>600</default>
        /// <version>1.4</version>
        [ReflectorProperty("buildTimeoutSeconds", Required = false)]
        public int BuildTimeoutSeconds { get; set; }

        /// <summary>
        /// Do not log messages to standard output.
        /// </summary>
        /// <default>false</default>
        /// <version>1.4</version>
        [ReflectorProperty("quiet", Required = false)]
        public bool Quiet { get; set; }

        /// <summary>
        /// The path of the version of Rake you want to run. If this is relative, then must be relative to either (a) the base directory, 
        /// (b) the CCNet Server application, or (c) if the path doesn't contain any directory details then can be available in the system 
        /// or application's 'path' environment variable.
        /// </summary>
        /// <default>c:\ruby\bin\rake.bat</default>
        /// <version>1.4</version>
        [ReflectorProperty("executable", Required = false)]
        public string Executable { get; set; }

        /// <summary>
        /// The priority class of the spawned process.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Normal</default>
        [ReflectorProperty("priority", Required = false)]
        public ProcessPriorityClass Priority { get; set; }

        /// <summary>
        /// The name of the Rakefile to run, relative to the baseDirectory. 
        /// </summary>
        /// <default>None</default>
        /// <version>1.4</version>
        /// <remarks>
        /// If no rake file is specified Rake will use the default build file in the working directory.
        /// </remarks>
        [ReflectorProperty("rakefile", Required = false)]
        public string Rakefile { get; set; }

        /// <summary>
        /// Like quiet but also suppresses the 'in directory' announcement. 
        /// </summary>
        /// <default>false</default>
        /// <version>1.4</version>
        [ReflectorProperty("silent", Required = false)]
        public bool Silent { get; set; }

        /// <summary>
        /// A list of targets to be called. CruiseControl.NET does not call Rake once for each target, it uses the Rake feature of
        /// being able to specify multiple targets. 
        /// </summary>
        /// <remarks>
        /// If no targets are defined Rake will use the default target.
        /// </remarks>
        /// <default>None</default>
        /// <version>1.4</version>
        [ReflectorProperty("targetList", Required = false)]
        public string[] Targets { get; set; }

        /// <summary>
        /// Turns on invoke/execute tracing and enables full backtrace.
        /// </summary>
        /// <default>false</default>
        /// <version>1.4</version>
        [ReflectorProperty("trace", Required = false)]
        public bool Trace { get; set; }

        /// <summary>
        /// Executes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
				protected override bool Execute(IIntegrationResult result)
				{
					ProcessInfo processInfo = this.CreateProcessInfo(result);
					result.BuildProgressInformation.SignalStartRunTask(!string.IsNullOrEmpty(this.Description) ? this.Description : string.Format(System.Globalization.CultureInfo.CurrentCulture, "Executing Rake: {0}", processInfo.PublicArguments));
					ProcessResult processResult = this.TryToRun(processInfo, result);

					if (!StringUtil.IsWhitespace(processResult.StandardOutput) || !StringUtil.IsWhitespace(processResult.StandardError))
					{
						// The executable produced some output.  We need to transform it into an XML build report 
						// fragment so the rest of CC.Net can process it.
						ProcessResult newResult = new ProcessResult(
							StringUtil.MakeBuildResult(processResult.StandardOutput, string.Empty),
							StringUtil.MakeBuildResult(processResult.StandardError, "Error"),
							processResult.ExitCode,
							processResult.TimedOut,
							processResult.Failed);

						processResult = newResult;
					}

					result.AddTaskResult(new ProcessTaskResult(processResult));

					if (processResult.TimedOut)
						result.AddTaskResult(MakeTimeoutBuildResult(processInfo));

					return processResult.Succeeded;
				}

        /// <summary>
        /// Gets the process arguments.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override string GetProcessArguments(IIntegrationResult result)
		{
			ProcessArgumentBuilder args = new ProcessArgumentBuilder();
			args.AddArgument("--rakefile", this.Rakefile);

			if (this.Silent)
				args.AddArgument("--silent");
			else if (this.Quiet)
				args.AddArgument("--quiet");

			if (this.Trace)
				args.AddArgument("--trace");

			args.AppendArgument(this.BuildArgs);

			foreach (string t in this.Targets)
				args.AppendArgument(t);

			return args.ToString();
		}

        /// <summary>
        /// Gets the process base directory.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override string GetProcessBaseDirectory(IIntegrationResult result)
		{
			return result.BaseFromWorkingDirectory(this.BaseDirectory);
		}

        /// <summary>
        /// Gets the process timeout.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override int GetProcessTimeout()
		{
			return this.BuildTimeoutSeconds*1000;
		}

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
        /// Gets the process priority class.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override ProcessPriorityClass GetProcessPriorityClass()
        {
            return this.Priority;
        }

        /// <summary>
        /// Gets or sets the targets for presentation.	
        /// </summary>
        /// <value>The targets for presentation.</value>
        /// <remarks></remarks>
		public string TargetsForPresentation
		{
			get
			{
				return StringUtil.ArrayToNewLineSeparatedString(this.Targets);
			}
			set
			{
				this.Targets = StringUtil.NewLineSeparatedStringToArray(value);
			}
		}
	}
}
