using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
	public class ProcessInfo
	{
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int DefaultTimeout = 120000;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public const int InfiniteTimeout = 0;

        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public const ProcessPriorityClass DEFAULT_PRIORITY = ProcessPriorityClass.Normal;
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
        public ProcessPriorityClass Priority;

        private readonly PrivateArguments arguments;
		private readonly ProcessStartInfo startInfo = new ProcessStartInfo();
		private string standardInputContent;
		private int timeout = DefaultTimeout;

        private readonly int[] successExitCodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfo" /> class.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <remarks></remarks>
	    public ProcessInfo(string filename) : 
            this(filename, null){}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfo" /> class.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <remarks></remarks>
		public ProcessInfo(string filename, PrivateArguments arguments) : 
            this(filename, arguments, null){}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfo" /> class.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <remarks></remarks>
        public ProcessInfo(string filename, PrivateArguments arguments, string workingDirectory) : 
            this(filename, arguments, workingDirectory, DEFAULT_PRIORITY){}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfo" /> class.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="priority">The priority.</param>
        /// <remarks></remarks>
        public ProcessInfo(string filename, PrivateArguments arguments, string workingDirectory, ProcessPriorityClass priority) :
            this(filename, arguments, workingDirectory, priority, null){}

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessInfo" /> class.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workingDirectory">The working directory.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="successExitCodes">The success exit codes.</param>
        /// <remarks></remarks>
        public ProcessInfo(string filename, PrivateArguments arguments, string workingDirectory, ProcessPriorityClass priority, int[] successExitCodes)
		{
            this.arguments = arguments;
            this.Priority = priority;
			this.startInfo.FileName = StringUtil.StripQuotes(filename);
			this.startInfo.Arguments = arguments == null ? null : arguments.ToString(SecureDataMode.Private);
			this.startInfo.WorkingDirectory = StringUtil.StripQuotes(workingDirectory);
			this.startInfo.UseShellExecute = false;
			this.startInfo.CreateNoWindow = true;
			this.startInfo.RedirectStandardOutput = true;
			this.startInfo.RedirectStandardError = true;
			this.startInfo.RedirectStandardInput = false;
			this.RepathExecutableIfItIsInWorkingDirectory();
            this.successExitCodes = successExitCodes ?? new int[] { 0 };
		}

		private void RepathExecutableIfItIsInWorkingDirectory()
		{
			if (this.WorkingDirectory == null) 
				return;

			string executableInWorkingDirectory = Path.Combine(this.WorkingDirectory, this.FileName);
			if (File.Exists(executableInWorkingDirectory))
				this.startInfo.FileName = executableInWorkingDirectory;
		}

        /// <summary>
        /// Gets the environment variables.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public StringDictionary EnvironmentVariables
		{
			get { return this.startInfo.EnvironmentVariables; }
		}

        /// <summary>
        /// Processes the successful.	
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool ProcessSuccessful(int exitCode)
		{
			return Array.IndexOf(this.successExitCodes, exitCode) > -1;
		}

        /// <summary>
        /// Gets the name of the file.	
        /// </summary>
        /// <value>The name of the file.</value>
        /// <remarks></remarks>
		public string FileName
		{
			get { return this.startInfo.FileName; }
		}

        /// <summary>
        /// Gets the arguments.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string Arguments
		{
            get { return this.startInfo.Arguments; }
		}

        /// <summary>
        /// Gets the public arguments.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string PublicArguments
        {
            get
            {
                if (this.arguments == null)
                {
                    return null;
                }
                else
                {
                    return this.arguments.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the working directory.	
        /// </summary>
        /// <value>The working directory.</value>
        /// <remarks></remarks>
        public string WorkingDirectory
		{
			get { return this.startInfo.WorkingDirectory; }
			set { this.startInfo.WorkingDirectory = value; }
		}

        /// <summary>
        /// Gets or sets the content of the standard input.	
        /// </summary>
        /// <value>The content of the standard input.</value>
        /// <remarks></remarks>
		public string StandardInputContent
		{
			get { return this.standardInputContent; }
			set
			{
				this.startInfo.RedirectStandardInput = true;
				this.startInfo.UseShellExecute = false;
				this.standardInputContent = value;
			}
		}

        /// <summary>
        /// Gets or sets the time out.	
        /// </summary>
        /// <value>The time out.</value>
        /// <remarks></remarks>
		public int TimeOut
		{
			get { return this.timeout; }
			set { this.timeout = (value == InfiniteTimeout) ? 0x7fffffff : value; }
		}

        /// <summary>
        /// Gets or sets the stream encoding.	
        /// </summary>
        /// <value>The stream encoding.</value>
        /// <remarks></remarks>
	    public Encoding StreamEncoding
	    {
	        get
	        {
	            return this.startInfo.StandardOutputEncoding;
	        }
	        set
	        {
	             this.startInfo.StandardOutputEncoding = value;
	             this.startInfo.StandardErrorEncoding = value;
	        }
	    }

        /// <summary>
        /// Creates the process.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
	    public Process CreateProcess()
		{
            // if WorkingDirectory is filled in, check that it exists
			if (!string.IsNullOrEmpty(this.WorkingDirectory) && !Directory.Exists(this.WorkingDirectory)) 
				throw new DirectoryNotFoundException("Directory does not exist: " + this.WorkingDirectory);

			Process process = new Process();
			process.StartInfo = this.startInfo;
			return process;
		}

        /// <summary>
        /// Equalses the specified obj.	
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public override bool Equals(object obj)
		{
			ProcessInfo otherProcessInfo = obj as ProcessInfo;
			if (otherProcessInfo == null)
				return false;

			return (this.FileName == otherProcessInfo.FileName
				&& this.Arguments == otherProcessInfo.Arguments
				&& this.WorkingDirectory == otherProcessInfo.WorkingDirectory
				&& this.TimeOut == otherProcessInfo.TimeOut
				&& this.StandardInputContent == otherProcessInfo.StandardInputContent);
		}

        /// <summary>
        /// Gets the hash code.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

        /// <summary>
        /// Toes the string.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.CurrentCulture, "FileName: [{0}] -- Arguments: [{1}] -- WorkingDirectory: [{2}] -- StandardInputContent: [{3}] -- Timeout: [{4}]",
			    this.FileName, this.Arguments, this.WorkingDirectory, this.StandardInputContent, this.TimeOut);
		}
	}
}
