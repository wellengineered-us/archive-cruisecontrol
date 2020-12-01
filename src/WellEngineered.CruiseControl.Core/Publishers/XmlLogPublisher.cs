using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.Core.Tasks;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Publishers
{
    /// <summary>
    /// <para>
    /// The Xml Log Publisher is used to create the log files used by the CruiseControl.NET Web Dashboard, so if you don't define an 
    /// &lt;xmllogger /&gt; section the Dashboard will not function correctly.
    /// </para>
    /// <para type="warning">
    /// You should place the &lt;xmllogger /&gt; in the &lt;publishers /&gt; section, after any <link>File Merge Task</link>s, in your 
    /// <link>Project Configuration Block</link>.
    /// </para>
    /// </summary>
    /// <title>XML Log Publisher</title>
    /// <version>1.0</version>
    /// <remarks>
    /// <para type="info">
    /// XML Log Publisher used to support the 'mergeFiles' option. This functionality is now removed and you should use <link>File Merge
    /// Task</link> instead.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code title="Minimalist example">
    /// &lt;xmllogger /&gt;
    /// </code>
    /// <code title="Full example">
    /// &lt;xmllogger logDir="c:\myproject\buildlogs" /&gt;
    /// </code>
    /// </example>
    [ReflectorType("xmllogger")]
    public class XmlLogPublisher
        : TaskBase, IMergeTask
    {
        /// <summary>
        /// 	
        /// </summary>
        /// <remarks></remarks>
		public static readonly string DEFAULT_LOG_SUBDIRECTORY = "buildlogs";

        /// <summary>
        /// The directory to save log files to. If relative, then relative to the Project Artifact Directory.
        /// </summary>
        /// <version>1.0</version>
        /// <default>buildlogs</default>
        [ReflectorProperty("logDir", Required = false)]
        public string ConfiguredLogDirectory { get; set; }

		// This is only public because of a nasty hack which I (MR) put in the code. To be made private later...
        /// <summary>
        /// Logs the directory.	
        /// </summary>
        /// <param name="artifactDirectory">The artifact directory.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string LogDirectory(string artifactDirectory)
		{
            if (string.IsNullOrEmpty(this.ConfiguredLogDirectory))
			{
				return Path.Combine(artifactDirectory, DEFAULT_LOG_SUBDIRECTORY);
			}
			else if (Path.IsPathRooted(this.ConfiguredLogDirectory))
			{
				return this.ConfiguredLogDirectory;
			}
			else
			{
				return Path.Combine(artifactDirectory, this.ConfiguredLogDirectory);
			}
		}

        /// <summary>
        /// Executes the specified result.	
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override bool Execute(IIntegrationResult result)
        {         
            // only deal with known integration status
            if (result.Status == IntegrationStatus.Unknown)
                return true;

            result.BuildLogDirectory = this.LogDirectory(result.ArtifactDirectory);

            using (XmlIntegrationResultWriter integrationWriter = new XmlIntegrationResultWriter(this.CreateWriter(this.LogDirectory(result.ArtifactDirectory), this.GetFilename(result))))
            {
				integrationWriter.Formatting = Formatting.Indented;
				integrationWriter.Write(result);
            }

            return true;
        }

        private TextWriter CreateWriter(string dirname, string filename)
        {
            // create directory if necessary
            if (!Directory.Exists(dirname))
                Directory.CreateDirectory(dirname);

            string path = Path.Combine(dirname, filename);

			// create XmlWriter using UTF8 encoding
			return new StreamWriter(path);
        }

        private string GetFilename(IIntegrationResult result)
        {
            return StringUtil.RemoveInvalidCharactersFromFileName(new LogFile(result).Filename);
        }
    }
}