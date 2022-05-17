using System;
using System.Globalization;
using System.IO;
using System.Xml;

using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.Remote.Security;

namespace WellEngineered.CruiseControl.Core.Security.Auditing
{
    /// <summary>
    /// Sends audit logging information to a file. The information will be stored in an XML format.
    /// </summary>
    /// <remarks>
    /// The actual file will not be correct XML as it will not have a single root element - instead each line will be directly written to the
    /// file.
    /// </remarks>
    /// <version>1.5</version>
    /// <title>XML File Audit Logger</title>
    /// <example>
    /// <code>
    /// &lt;xmlFileAudit location="c:\Logs\ccnet_audit.log"/&gt;
    /// </code>
    /// </example>
    [ReflectorType("xmlFileAudit")]
    public class FileXmlLogger
        : AuditLoggerBase, IAuditLogger
    {
        private string auditFile = "SecurityAudit.xml";
		private readonly IExecutionEnvironment executionEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileXmlLogger" /> class.	
        /// </summary>
        /// <remarks></remarks>
		public FileXmlLogger() : this(new ExecutionEnvironment())
		{}

        /// <summary>
        /// Initializes a new instance of the <see cref="FileXmlLogger" /> class.	
        /// </summary>
        /// <param name="executionEnvironment">The execution environment.</param>
        /// <remarks></remarks>
		public FileXmlLogger(IExecutionEnvironment executionEnvironment)
		{
			this.executionEnvironment = executionEnvironment;
		}

        /// <summary>
        /// The location to log the audit events.
        /// </summary>
        /// <version>1.5</version>
        /// <default>SecurityAudit.xml</default>
        [ReflectorProperty("location", Required=false)]
        public string AuditFileLocation
        {
            get { return this.auditFile; }
            set { this.auditFile = value; }
        }

        /// <summary>
        /// Performs the actual logging of a security event
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="userName">The name of the user.</param>
        /// <param name="eventType">The type of event.</param>
        /// <param name="eventRight">The right of the event.</param>
        /// <param name="message">Any security message.</param>
        protected override void DoLogEvent(string projectName, string userName, SecurityEvent eventType, SecurityRight eventRight, string message)
        {
            // Generate the log entry
            XmlDocument auditXml = new XmlDocument();
            XmlElement xmlRoot = auditXml.CreateElement("event");
            auditXml.AppendChild(xmlRoot);
            this.AddXmlElement(auditXml, xmlRoot, "dateTime", DateTime.Now.ToString("o", CultureInfo.CurrentCulture));
            if (!string.IsNullOrEmpty(projectName)) this.AddXmlElement(auditXml, xmlRoot, "project", projectName);
            if (!string.IsNullOrEmpty(userName)) this.AddXmlElement(auditXml, xmlRoot, "user", userName);
            this.AddXmlElement(auditXml, xmlRoot, "type", eventType.ToString());
            if (eventRight != SecurityRight.Inherit) this.AddXmlElement(auditXml, xmlRoot, "outcome", eventRight.ToString());
            if (!string.IsNullOrEmpty(message)) this.AddXmlElement(auditXml, xmlRoot, "message", message);

            // Write the entry
			string auditLog = this.executionEnvironment.EnsurePathIsRooted(this.auditFile);
            lock (this)
            {
                File.AppendAllText(auditLog, auditXml.OuterXml.Replace(Environment.NewLine, " ") + Environment.NewLine);
            }
        }

        private void AddXmlElement(XmlDocument document, XmlElement parent, string name, string value)
        {
            XmlElement newNode = document.CreateElement(name);
            newNode.InnerText = value;
            parent.AppendChild(newNode);
        }
    }
}
