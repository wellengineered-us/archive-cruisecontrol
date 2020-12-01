using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Tasks
{
	/// <summary>
	/// <para>
	/// Most complex build processes use <link>NAnt Task</link> or <link>MSBuild Task</link> to script the build. However, for simple
	/// projects that just need to build a Visual Studio.NET solution, the Visual Studio task &lt;devenv&gt; provides an easier method.
	/// </para>
	/// </summary>
	/// <title>Visual Studio Task</title>
	/// <version>1.0</version>
	/// <remarks>
	/// <para>
	/// If executable and version are not specified, CC.NET will search the registry for VS.NET 2010, 2008, 2005, 2003, and 2002 in that order.
	/// If you need to use a specific version when a newer version is installed, you should specify the version property to identify it,
	/// or specify the executable property to point to the location of correct version of devenv.com.
	/// </para>
	/// <para type="warning">
	/// This task requires you to have Visual Studio .NET installed on your integration server.
	/// </para>
	/// <para>
	/// Often programmers like to use a centralised project to build an entire software system. They define specific dependencies and the
	/// build order on that specific project to reproduce the behaviours of an nmake build.
	/// </para>
	/// <includePage>Integration_Properties</includePage>
	/// </remarks>
	/// <example>
	/// <code title="Minimalist example">
	/// &lt;devenv&gt;
	/// &lt;solutionfile&gt;src\MyProject.sln&lt;/solutionfile&gt;
	/// &lt;configuration&gt;Debug&lt;/configuration&gt;
	/// &lt;/devenv&gt;
	/// </code>
	/// <code title="Full example">
	/// &lt;devenv&gt;
	/// &lt;solutionfile&gt;src\MyProject.sln&lt;/solutionfile&gt;
	/// &lt;configuration&gt;Debug&lt;/configuration&gt;
	/// &lt;buildtype&gt;Build&lt;/buildtype&gt;
	/// &lt;project&gt;MyProject&lt;/project&gt;
	/// &lt;executable&gt;c:\program files\Microsoft Visual Studio .NET\Common7\IDE\devenv.com&lt;/executable&gt;
	/// &lt;buildTimeoutSeconds&gt;600&lt;/buildTimeoutSeconds&gt;
	/// &lt;version&gt;VS2002&lt;/version&gt;
	/// &lt;/devenv&gt;
	/// </code>
	/// </example>    
	[ReflectorType("devenv")]
	public class DevenvTask
				: TaskBase
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string LogFilename = "devenv-results-{0}.xml";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public readonly Guid LogFileId = Guid.NewGuid();
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS2010_REGISTRY_PATH = @"Software\Microsoft\VisualStudio\10.0";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS2008_REGISTRY_PATH = @"Software\Microsoft\VisualStudio\9.0";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS2005_REGISTRY_PATH = @"Software\Microsoft\VisualStudio\8.0";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS2003_REGISTRY_PATH = @"Software\Microsoft\VisualStudio\7.1";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS2002_REGISTRY_PATH = @"Software\Microsoft\VisualStudio\7.0";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string VS_REGISTRY_KEY = @"InstallDir";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string DEVENV_EXE = "devenv.com";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int DEFAULT_BUILD_TIMEOUT = 600;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string DEFAULT_BUILDTYPE = "rebuild";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string DEFAULT_PROJECT = "";
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const ProcessPriorityClass DEFAULT_PRIORITY = ProcessPriorityClass.Normal;

		private readonly IRegistry registry;
		private readonly ProcessExecutor executor;
		private string executable;
		private string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevenvTask" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public DevenvTask() :
			this(new Registry(), new ProcessExecutor()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevenvTask" /> class.	
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="executor">The executor.</param>
        /// <remarks></remarks>
		public DevenvTask(IRegistry registry, ProcessExecutor executor)
		{
			this.registry = registry;
			this.executor = executor;
			this.BuildTimeoutSeconds = DEFAULT_BUILD_TIMEOUT;
			this.BuildType = DEFAULT_BUILDTYPE;
			this.Project = DEFAULT_PROJECT;
			this.Priority = DEFAULT_PRIORITY;
		}

		private readonly string[] ExpectedVisualStudioVersions =
			new string[]
				{
					"10.0", "9.0", "8.0", "7.1", "7.0",
					"VS2010", "VS2008", "VS2005", "VS2003", "VS2002"
				};

		private readonly string[] RegistryScanOrder =
			new string[]
				{
					VS2010_REGISTRY_PATH, VS2008_REGISTRY_PATH, VS2005_REGISTRY_PATH, VS2003_REGISTRY_PATH, VS2002_REGISTRY_PATH
				};

		/// <summary>
		/// The version of Visual Studio.
		/// </summary>
		/// <version>1.0</version>
		/// <default>See below</default>
		/// <values>
		/// <value>VS2002</value>
		/// <value>VS2003</value>
		/// <value>VS2005</value>
		/// <value>VS2008</value>
		/// <value>VS2010</value>
		/// <value>7.0</value>
		/// <value>7.1</value>
		/// <value>8.0</value>
		/// <value>9.0</value>
		/// <value>10.0</value>
		/// </values>
		[ReflectorProperty("version", Required = false)]
		public string Version
		{
			get { return this.version; }

			set
			{
				if (Array.IndexOf(this.ExpectedVisualStudioVersions, value) == -1)
					throw new CruiseControlException("Invalid value for Version, expected one of: " +
						StringUtil.Join(", ", this.ExpectedVisualStudioVersions));

				this.version = value;
			}
		}

		/// <summary>
		/// The path to devenv.com.
		/// </summary>
		/// <version>1.0</version>
		/// <default>See below</default>
		[ReflectorProperty("executable", Required = false)]
		public string Executable
		{
			get
			{
				if (this.executable == null)
					this.executable = this.ReadDevenvExecutableFromRegistry();

				return this.executable;
			}
			set { this.executable = value; }
		}

		/// <summary>
		/// Get the name of the Visual Studio executable for the highest version installed on this machine.
		/// </summary>
		/// <returns>The fully-qualified pathname of the executable.</returns>
		private string ReadDevenvExecutableFromRegistry()
		{
			// If null, scan for any version.
			if (this.Version == null)
				return Path.Combine(this.ScanForRegistryForVersion(), DEVENV_EXE);

			string path;

			switch (this.Version)
			{
				case "VS2010":
				case "10.0":
					path = this.registry.GetExpectedLocalMachineSubKeyValue(VS2010_REGISTRY_PATH, VS_REGISTRY_KEY);
					break;
				case "VS2008":
				case "9.0":
					path = this.registry.GetExpectedLocalMachineSubKeyValue(VS2008_REGISTRY_PATH, VS_REGISTRY_KEY);
					break;
				case "VS2005":
				case "8.0":
					path = this.registry.GetExpectedLocalMachineSubKeyValue(VS2005_REGISTRY_PATH, VS_REGISTRY_KEY);
					break;
				case "VS2003":
				case "7.1":
					path = this.registry.GetExpectedLocalMachineSubKeyValue(VS2003_REGISTRY_PATH, VS_REGISTRY_KEY);
					break;
				case "VS2002":
				case "7.0":
					path = this.registry.GetExpectedLocalMachineSubKeyValue(VS2002_REGISTRY_PATH, VS_REGISTRY_KEY);
					break;
				default:
					throw new CruiseControlException("Unknown version of Visual Studio.");
			}

			return Path.Combine(path, DEVENV_EXE);
		}

		private string ScanForRegistryForVersion()
		{
			foreach (string x in this.RegistryScanOrder)
			{
				string path = this.registry.GetLocalMachineSubKeyValue(x, VS_REGISTRY_KEY);
				if (path != null)
					return path;
			}

			throw new CruiseControlException("Unknown version of Visual Studio, or no version found.");
		}

		/// <summary>
		/// The path of the solution file to build. If relative, it is relative to the Project Working Directory. 
		/// </summary>
		/// <default>n/a</default>
		/// <version>1.0</version>
		[ReflectorProperty("solutionfile")]
		public string SolutionFile { get; set; }

		/// <summary>
		/// The solution configuration to use (not case sensitive). 
		/// </summary>
		/// <default>n/a</default>
		/// <version>1.0</version>
		[ReflectorProperty("configuration")]
		public string Configuration { get; set; }

		/// <summary>
		/// Number of seconds to wait before assuming that the process has hung and should be killed. 
		/// </summary>
		/// <default>600 (10 minutes)</default>
		/// <version>1.0</version>
		[ReflectorProperty("buildTimeoutSeconds", Required = false)]
		public int BuildTimeoutSeconds { get; set; }

		/// <summary>
		/// The type of build.
		/// </summary>
		/// <version>1.0</version>
		/// <default>rebuild</default>
		/// <values>
		/// <value>Rebuild</value>
		/// <value>Build</value>
		/// <value>Clean</value>
		/// </values>
		[ReflectorProperty("buildtype", Required = false)]
		public string BuildType { get; set; }

		/// <summary>
		/// A specific project in the solution, if you only want to build one project (not case sensitive). 
		/// </summary>
		/// <version>1.0</version>
		/// <default>All projects</default>
		[ReflectorProperty("project", Required = false)]
		public string Project { get; set; }

		/// <summary>
		/// The priority class of the spawned process.
		/// </summary>
		/// <version>1.5</version>
		/// <default>Normal</default>
		[ReflectorProperty("priority", Required = false)]
		public ProcessPriorityClass Priority { get; set; }

        /// <summary>
        /// Executes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		protected override bool Execute(IIntegrationResult result)
		{
			result.BuildProgressInformation.SignalStartRunTask(!string.IsNullOrEmpty(this.Description) ? this.Description : string.Format(System.Globalization.CultureInfo.CurrentCulture,"Executing Devenv :{0}", this.GetArguments(result)));
			ProcessResult processResult = this.TryToRun(result);

			// rei added 30.5.2010, merge devenv output to task result 
			string buildOutputFile = this.DevEnvOutputFile(result);
			if (File.Exists(buildOutputFile))
				result.AddTaskResult(new FileTaskResult(buildOutputFile) { WrapInCData=true} );

			result.AddTaskResult(new DevenvTaskResult(processResult));
			Log.Info("Devenv build complete.  Status: " + result.Status);

			if (processResult.TimedOut)
				throw new BuilderException(this, string.Format(System.Globalization.CultureInfo.CurrentCulture,"Devenv process timed out after {0} seconds.", this.BuildTimeoutSeconds));

			return !processResult.Failed;
		}

		private ProcessResult TryToRun(IIntegrationResult result)
		{
			ProcessInfo processInfo = new ProcessInfo(this.Executable, this.GetArguments(result), result.WorkingDirectory, this.Priority);
			processInfo.TimeOut = this.BuildTimeoutSeconds * 1000;
			IDictionary properties = result.IntegrationProperties;

            // pass user defined the environment variables
            foreach (EnvironmentVariable item in this.EnvironmentVariables)
                processInfo.EnvironmentVariables[item.name] = item.value;

			// Pass the integration environment variables to devenv.
			foreach (string key in properties.Keys)
			{
				processInfo.EnvironmentVariables[key] = StringUtil.IntegrationPropertyToString(properties[key]);
			}

			Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Starting build: {0} {1}", processInfo.FileName, processInfo.PublicArguments));
			try
			{
				return this.executor.Execute(processInfo);
			}
			catch (IOException ex)
			{
				string message = string.Format(System.Globalization.CultureInfo.CurrentCulture,"Unable to launch the devenv process.  Please verify that you can invoke this command from the command line: {0} {1}", processInfo.FileName, processInfo.PublicArguments);
				throw new BuilderException(this, message, ex);
			}
		}

		private string GetArguments(IIntegrationResult result)
		{
			StringBuilder sb = new StringBuilder();

			if (this.SolutionFile.StartsWith("\""))
				sb.Append(this.SolutionFile);
			else
				sb.AppendFormat(CultureInfo.CurrentCulture, "\"{0}\"", this.SolutionFile);

			sb.AppendFormat(CultureInfo.CurrentCulture, " /{0}", this.BuildType);

			if (this.Configuration.StartsWith("\""))
				sb.AppendFormat(CultureInfo.CurrentCulture, " {0}", this.Configuration);
			else
				sb.AppendFormat(CultureInfo.CurrentCulture, " \"{0}\"", this.Configuration);

			if (!string.IsNullOrEmpty(this.Project))
			{
				if (this.Project.StartsWith("\""))
					sb.AppendFormat(CultureInfo.CurrentCulture, " /project {0}", this.Project);
				else
					sb.AppendFormat(CultureInfo.CurrentCulture, " /project \"{0}\"", this.Project);
			}

			// always create an out file, will be merged into build log later
			sb.AppendFormat(CultureInfo.CurrentCulture, " /out \"{0}\"", this.DevEnvOutputFile(result));

			return sb.ToString();
		}


		private string DevEnvOutputFile(IIntegrationResult result)
		{
			return Path.Combine(result.ArtifactDirectory, string.Format(CultureInfo.CurrentCulture, LogFilename, this.LogFileId));
		}

	}
}
