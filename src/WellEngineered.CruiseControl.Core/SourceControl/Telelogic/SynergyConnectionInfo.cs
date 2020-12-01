

using System;
using System.Globalization;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.Core.SourceControl.Telelogic
{
	/// <summary>
	/// A CM Synergy client session.
	/// </summary>
    /// <title>Synergy Client Session</title>
    /// <version>1.0</version>
    /// <example>
    /// <code>
    /// &lt;connection&gt;
    /// &lt;host&gt;myserver&lt;/host&gt;
    /// &lt;database&gt;\\myserver\share\mydatabase&lt;/database&gt;
    /// &lt;!-- store values in an environmental variable--&gt;
    /// &lt;username&gt;%CCM_USER%&lt;/username&gt;
    /// &lt;password&gt;%CCM_PWD%&lt;/password&gt;
    /// &lt;role&gt;build_mgr&lt;/role&gt;
    /// &lt;homeDirectory&gt;D:\cmsynergy\%CCM_USER%&lt;/homeDirectory&gt;
    /// &lt;clientDatabaseDirectory&gt;D:\cmsynergy\uidb&lt;/clientDatabaseDirectory&gt;
    /// &lt;polling&gt;true&lt;/polling&gt;
    /// &lt;timeout&gt;3600&lt;/timeout&gt;
    /// &lt;/connection&gt;
    /// </code>
    /// </example>
	[ReflectorType("synergyConnection")]
	public class SynergyConnectionInfo
	{
		private string username;
		private string password;
		private string homeDirectory;
		private string clientDatabaseDirectory;
		private string executable;
		private string workingDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynergyConnectionInfo" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public SynergyConnectionInfo()
		{
			this.Executable = "ccm.exe";
			this.Timeout = 3600;
			this.Host = "localhost";
			this.Database = null;
			this.Username = "%USERNAME%";
			this.Password = String.Empty;
			this.Role = "build_mgr";
			this.HomeDirectory = @"%SystemDrive%\cmsynergy\%USERNAME%";
			this.ClientDatabaseDirectory = @"%SystemDrive%\cmsynergy\uidb";
			this.WorkingDirectory = @"%ProgramFiles%\Telelogic\CM Synergy 6.3\bin";

			this.Reset();
		}

		/// <summary>
		/// The identitifer for the Synergy client side process.
		/// Required to have multiple Synergy processes.
		/// </summary>
		/// <example>
		/// <c>COMPUTERNAME:1234:127.0.0.1</c>
		/// </example>
		/// <value>
		/// Defaults to <see langword="null" />.
		/// </value>
        /// <version>1.0</version>
        /// <default>None</default>
		public string SessionId;

		/// <summary>
		/// The executable filename/path for the CM Synergy command line interface.
		/// </summary>
        /// <remarks>
        /// Can include environmental variables to be replaced.
        /// </remarks>
		/// <value>
		/// Defaults to <c>ccm.exe</c>.
		/// </value>
        /// <version>1.0</version>
        /// <default>ccm.exe</default>
        [ReflectorProperty("executable")]
		public string Executable
		{
			get { return this.executable; }
			set { this.executable = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// The directory to execute all CM Synergy commands from.
		/// </summary>
        /// <remarks>
        /// Can include environmental variables to be replaced.
        /// </remarks>
		/// <value>
		/// Defaults to <c>%PROGRAMFILES%\Telelogic\CM Synergy 6.3\bin</c>
		/// </value>
        /// <version>1.0</version>
        /// <default>%PROGRAMFILES%\Telelogic\CM Synergy 6.3\bin</default>
        [ReflectorProperty("workingDirectory", Required = false)]
		public string WorkingDirectory
		{
			get { return this.workingDirectory; }
			set { this.workingDirectory = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// Hostname of the Synergy server
		/// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("host")]
        public string Host { get; set; }

		/// <summary>
		/// Network path to the Synergy database instance
		/// </summary>
        /// <version>1.0</version>
        /// <default>n/a</default>
        [ReflectorProperty("database")]
        public string Database { get; set; }

		/// <summary>
		/// The configured database delimiter for object and project specifications.
		/// </summary>
		/// <value>
		///     Defaults to <c>-</c>.
		/// </value>
		public char Delimiter;

		/// <summary>
		///     Extracts the name of the database from the <see cref="Database"/> full
		///     physical path.
		/// </summary>
		public string DatabaseName
		{
			get
			{
				string databaseName;

				// HACK: assume the database name always matches the directory name for the path
				//       to the database.
				databaseName = Path.GetFileName(this.Database);
				if (databaseName.Length == 0)
				{
					// System.IO.Path parses "\\server\share\folder\" as dir,
					// but parses            "\\server\share\folder"  as a file
					databaseName = Directory.GetParent(this.Database).Name;
				}

				return (databaseName);
			}
		}

		/// <summary>
		/// Poll the server every minute when the <c>ccm_admin</c> has protected the database for the purpose of issuing backup commands.
		/// </summary>
		/// <remarks>
		/// This is useful if a long runing inadventently enters the scheduled time window for routine downtime, generally for server maintenance jobs like backups.
		/// </remarks>
		/// <value>
		///     Defaults to <see langword="false" />
		/// </value>
        /// <version>1.0</version>
        /// <default>false</default>
        [ReflectorProperty("polling", Required = false)]
        public bool PollingEnabled { get; set; }

		/// <summary>
		/// The username for the Synergy session. Can include environmental variables to be replaced.
		/// </summary>
		/// <value>
		/// Defaults to <c>("%USERNAME%")</c>.
		/// </value>
        /// <version>1.0</version>
        /// <default>%USERNAME%</default>
        [ReflectorProperty("username", Required = false)]
		public string Username
		{
			get { return this.username; }
			set { this.username = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// The Synergy password for the associate <see cref="Username"/> value.
		/// </summary>
		/// <value>
		/// Defaults to <see cref="String.Empty"/>.
		/// </value>
        /// <version>1.0</version>
        /// <default>None</default>
        [ReflectorProperty("password", Required = false)]
		public string Password
		{
			get { return this.password; }
			set { this.password = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// The role to use for the Synergy session.
		/// </summary>
		/// <value>
		/// Defaults to <c>build_mgr</c>.
		/// </value>
        /// <version>1.0</version>
        /// <default>build_mgr</default>
        [ReflectorProperty("role", Required = false)]
        public string Role { get; set; }

		/// <summary>
		/// The full physical path of the home directory for the associated Username on the client machine. Can include environmental variables to be replaced.
		/// </summary>
		/// <remarks>
		/// This role must have sufficient permissions to modify task folders, change reconfigure properties, and create baselines.
		/// </remarks>
		/// <value>Defaults to <c>%SystemDrive%\cmsynergy\%USERNAME%</c>.</value>
        /// <version>1.0</version>
        /// <default>%SystemDrive%\cmsynergy\%USERNAME%</default>
        [ReflectorProperty("homeDirectory", Required = false)]
		public string HomeDirectory
		{
			get { return this.homeDirectory; }
			set { this.homeDirectory = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// Path for the remote client session to copy database information to. Can include environmental variables to be replaced.
		/// </summary>
		/// <value>Defaults to <c>%SystemDrive%\cmsynergy\uidb</c>.</value>
        /// <version>1.0</version>
        /// <default>%SystemDrive%\cmsynergy\uidb</default>
        [ReflectorProperty("clientDatabaseDirectory", Required = false)]
		public string ClientDatabaseDirectory
		{
			get { return this.clientDatabaseDirectory; }
			set { this.clientDatabaseDirectory = Environment.ExpandEnvironmentVariables(value); }
		}

		/// <summary>
		/// Timeout in seconds for all Synergy commands.
		/// </summary>
		/// <value>Defaults to <c>3600</c> seconds (one hour).</value>
        /// <version>1.0</version>
        /// <default>3600</default>
        [ReflectorProperty("timeout", Required = false)]
        public int Timeout { get; set; }

        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public IFormatProvider FormatProvider = CultureInfo.CurrentCulture;
		
		/// <summary>
		///     Resets session variables back to default values.
		///     Useful for when a connection is closed or reestablished.
		/// </summary>
		public void Reset()
		{
			this.SessionId = null;
			this.Delimiter = '-';
		}
	}
}