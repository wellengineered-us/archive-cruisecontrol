
namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public class ConsoleRunnerArguments
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const string DEFAULT_CONFIG_PATH = @"ccnet.config";
		private readonly bool useRemoting = false;
		private string project;
		private string configFile;
		private bool validateConfigOnly;
		private bool showHelp;
		private bool launchDebugger;
		private bool logging = true;
		private bool errorPause = true;

        /// <summary>
        /// Gets or sets the use remoting.	
        /// </summary>
        /// <value>The use remoting.</value>
        /// <remarks></remarks>
		public bool UseRemoting
		{
			get { return this.useRemoting; }
			//set { useRemoting = value; }
		}

        /// <summary>
        /// Gets or sets the project.	
        /// </summary>
        /// <value>The project.</value>
        /// <remarks></remarks>
		public string Project
		{
			get { return this.project; }
			set { this.project = value; }
		}

        /// <summary>
        /// Gets or sets the config file.	
        /// </summary>
        /// <value>The config file.</value>
        /// <remarks></remarks>
		public string ConfigFile
		{
			get
			{
				return (this.configFile == null) ? DEFAULT_CONFIG_PATH : this.configFile;
			}
			set { this.configFile = value; }
		}

        /// <summary>
        /// Gets or sets the validate config only.	
        /// </summary>
        /// <value>The validate config only.</value>
        /// <remarks></remarks>
	    public bool ValidateConfigOnly
	    {
            get { return this.validateConfigOnly; }
            set { this.validateConfigOnly = value; }
	    }

        /// <summary>
        /// Gets or sets the show help.	
        /// </summary>
        /// <value>The show help.</value>
        /// <remarks></remarks>
		public bool ShowHelp
		{
			get { return this.showHelp; }
			set { this.showHelp = value; }
		}

        /// <summary>
        /// Gets or sets the launch debugger.	
        /// </summary>
        /// <value>The launch debugger.</value>
        /// <remarks></remarks>
	    public bool LaunchDebugger
	    {
            get { return this.launchDebugger; }
            set { this.launchDebugger = value; }
	    }

        /// <summary>
        /// Gets or sets the logging.	
        /// </summary>
        /// <value>The logging.</value>
        /// <remarks></remarks>
        public bool Logging
        {
            get { return this.logging; }
            set { this.logging = value; }
        }

        /// <summary>
        /// Gets or sets the pause on error.	
        /// </summary>
        /// <value>The pause on error.</value>
        /// <remarks></remarks>
        public bool PauseOnError
        {
            get { return this.errorPause; }
            set { this.errorPause = value; }
        }
	}
}