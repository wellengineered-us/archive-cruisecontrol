using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Xml;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A client connection to an old (pre-1.5) version of CruiseControl.NET via HTTP.
    /// </summary>
    public class CruiseServerHttpClient
        : CruiseServerClientBase
    {
        #region Private fields
        private readonly string serverUri;
        private string targetServer;
        private WebClient client;
        private IWebFunctions webFunctions;

        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new <see cref="CruiseServerRemotingClient"/>.
        /// </summary>
        /// <param name="serverUri">The address of the server.</param>
        public CruiseServerHttpClient(string serverUri)
            : this(serverUri, new WebClient())
        {
        }

        /// <summary>
        /// Initialises a new <see cref="CruiseServerRemotingClient"/>.
        /// </summary>
        /// <param name="serverUri">The address of the server.</param>
        /// <param name="client">The <see cref="WebClient"/> to use.</param>
        public CruiseServerHttpClient(string serverUri, WebClient client)
        {
            this.serverUri = serverUri.EndsWith("/", StringComparison.CurrentCulture) ? serverUri.Substring(0, serverUri.Length - 1) : serverUri;
            this.client = client;
            this.webFunctions = new DefaultWebFunctions();
        }
        #endregion

        #region Public properties
        #region TargetServer
        /// <summary>
        /// The server that will be targeted by all messages.
        /// </summary>
        public override string TargetServer
        {
            get { return string.IsNullOrEmpty(this.targetServer) ? "local" : this.targetServer; }
            set { this.targetServer = value; }
        }
        #endregion

        #region IsBusy
        /// <summary>
        /// Is this client busy performing an operation.
        /// </summary>
        public override bool IsBusy
        {
            get { return false; }
        }
        #endregion

        #region Address
        /// <summary>
        /// The address of the client.
        /// </summary>
        public override string Address
        {
            get { return this.serverUri; }
        }
        #endregion
        #endregion

        #region Public methods
        #region GetProjectStatus()
        /// <summary>
        /// Gets information about the last build status, current activity and project name.
        /// for all projects on a cruise server
        /// </summary>
        public override ProjectStatus[] GetProjectStatus()
        {
            try
            {
                // Retrieve the XML from the server
                var url = this.GenerateUrl("XmlStatusReport.aspx");
                var uri = new Uri(url);
                this.webFunctions.SetCredentials(this.client, uri, false);
                string response;
                try
                {
                    response = this.client.DownloadString(url);
                }
                catch (WebException error)
                {
                    if (error.Message.Contains("(403) Forbidden"))
                    {
                        // Jenkins doesn't give a challenge for HTTP Authentication
                        // So we need to force an Authorization header
                        this.webFunctions.SetCredentials(this.client, uri, true);
                        response = this.client.DownloadString(url);
                    }
                    else
                    {
                        throw;
                    }
                }
                if (string.IsNullOrEmpty(response)) throw new CommunicationsException("No data retrieved");

                // Load the XML and parse it
                var document = new XmlDocument();
                document.LoadXml(response);
                var projects = this.ParseProjects(document.SelectNodes("/Projects/Project"));
                return projects.ToArray();
            }
            catch (Exception error)
            {
                throw new CommunicationsException("Unable to retrieve project status from the remote server", error);
            }
        }
        #endregion

        #region ForceBuild()
        /// <summary>
        /// Forces a build for the named project.
        /// </summary>
        /// <param name="projectName">project to force</param>
        public override void ForceBuild(string projectName)
        {
            this.SendButtonPush("ForceBuild", projectName);
        }

        /// <summary>
        /// Forces a build for a named project.
        /// </summary>
        /// <param name="projectName">The project to force.</param>
        /// <param name="parameters">The parameters to pass into the project (these are ignored).</param>
        public override void ForceBuild(string projectName, List<NameValuePair> parameters)
        {
            this.ForceBuild(projectName);
        }
        #endregion

        #region AbortBuild()
        /// <summary>
        /// Attempts to abort a current project build.
        /// </summary>
        /// <param name="projectName">The name of the project to abort.</param>
        public override void AbortBuild(string projectName)
        {
            this.SendButtonPush("AbortBuild", projectName);
        }
        #endregion

        #region Request()
        /// <summary>
        /// Sends a build request to the server.
        /// </summary>
        /// <param name="projectName">The name of the project to use.</param>
        /// <param name="integrationRequest"></param>
        public override void Request(string projectName, IntegrationRequest integrationRequest)
        {
            this.ForceBuild(projectName);
        }
        #endregion

        #region StartProject()
        /// <summary>
        /// Attempts to start a project.
        /// </summary>
        /// <param name="project"></param>
        public override void StartProject(string project)
        {
            this.SendButtonPush("StartBuild", project);
        }
        #endregion

        #region StopProject()
        /// <summary>
        /// Stop project.
        /// </summary>
        /// <param name="project"></param>
        public override void StopProject(string project)
        {
            this.SendButtonPush("StopBuild", project);
        }
        #endregion

        #region GetCruiseServerSnapshot()
        /// <summary>
        /// Gets the projects and integration queues snapshot from this server.
        /// </summary>
        public override CruiseServerSnapshot GetCruiseServerSnapshot()
        {
            try
            {
                string response;

                try
                {
                    // Retrieve the XML from the server - 1.3 or later
                    var url = this.GenerateUrl("XmlServerReport.aspx");
                    this.webFunctions.SetCredentials(this.client, new Uri(url), false);
                    response = this.client.DownloadString(url);
                }
                catch (Exception)
                {
                    // Retrieve the XML from the server - earlier than 1.3
                    var url = this.GenerateUrl("XmlStatusReport.aspx");
                    response = this.client.DownloadString(url);
                }
                if (string.IsNullOrEmpty(response)) throw new CommunicationsException("No data retrieved");

                // Load the XML and parse it
                var document = new XmlDocument();
                document.LoadXml(response);

                var snapshot = new CruiseServerSnapshot();
                if (document.DocumentElement.Name == "CruiseControl")
                {
                    snapshot.ProjectStatuses = this.ParseProjects(document.SelectNodes("/CruiseControl/Projects/Project")).ToArray();

                    // Add all the queues
                    this.ParseQueues(document, snapshot);
                }
                else
                {
                    snapshot.ProjectStatuses = this.ParseProjects(document.SelectNodes("/Projects/Project")).ToArray();
                }
                return snapshot;
            }
            catch (Exception error)
            {
                throw new CommunicationsException("Unable to retrieve project status from the remote server", error);
            }
        }
        #endregion

        #region GetServerVersion()
        /// <summary>
        /// Returns the version of the server
        /// </summary>
        public override string GetServerVersion()
        {
            // We can't actually get the server version, just attempt to get the statuses , if this
            // passes returns a made-up version, otherwise there will be an exception thrown
            this.GetProjectStatus();
            return "0.0.0.0";
        }
        #endregion
        #endregion

        #region Private methods
        #region GenerateUrl()
        /// <summary>
        /// Generate a URL to the server.
        /// </summary>
        /// <param name="pageUrl"></param>
        /// <returns></returns>
        private string GenerateUrl(string pageUrl)
        {
            // Assumption - if a URI contains a "." then it already has a page in it
            var lastSlash = this.serverUri.LastIndexOf('/');
            if (this.serverUri.IndexOf('.', lastSlash) > 0)
            {
                return this.serverUri;
            }
            else
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0}/{1}", this.serverUri, pageUrl);
            }
        }
        #endregion

        #region ParseProjects()
        /// <summary>
        /// Parse an array of project definitions.
        /// </summary>
        /// <param name="projectNodes"></param>
        /// <returns></returns>
        private List<ProjectStatus> ParseProjects(XmlNodeList projectNodes)
        {
            var projects = new List<ProjectStatus>();

            foreach (XmlElement node in projectNodes)
            {
                var project = new ProjectStatus{
                    Activity = new ProjectActivity(this.RetrieveAttributeValue(node, "activity", "Unknown")),
                    BuildStage = node.GetAttribute("BuildStage"),
                    BuildStatus = this.RetrieveAttributeValue(node, "lastBuildStatus", IntegrationStatus.Unknown),
                    Category = node.GetAttribute("category"),
                    LastBuildDate = this.RetrieveAttributeValue(node, "lastBuildTime", DateTime.MinValue),
                    LastBuildLabel = node.GetAttribute("lastBuildLabel"),
                    Name = node.GetAttribute("name"),
                    NextBuildTime = this.RetrieveAttributeValue(node, "nextBuildTime", DateTime.MaxValue),
                    ServerName = node.GetAttribute("serverName"),
                    Status = this.RetrieveAttributeValue(node, "status", ProjectIntegratorState.Unknown),
                    WebURL = node.GetAttribute("webUrl")
                };
                projects.Add(project);
            }

            return projects;
        }
        #endregion

        #region ParseQueues()
        /// <summary>
        /// Parse the queue information.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="snapshot"></param>
        private void ParseQueues(XmlDocument document, CruiseServerSnapshot snapshot)
        {
            foreach (XmlElement queueSnapshotEl in document.SelectNodes("/CruiseControl/Queues/Queue"))
            {
                // Retrieve the queue details
                var queueSnapshot = new QueueSnapshot
                {
                    QueueName = this.RetrieveAttributeValue(queueSnapshotEl, "name", string.Empty)
                };
                snapshot.QueueSetSnapshot.Queues.Add(queueSnapshot);

                // Retrieve the requests
                foreach (XmlElement requestEl in queueSnapshotEl.SelectNodes("Request"))
                {
                    var request = new QueuedRequestSnapshot
                    {
                        Activity = new ProjectActivity(this.RetrieveAttributeValue(requestEl, "activity", "Unknown")),
                        ProjectName = this.RetrieveAttributeValue(requestEl, "projectName", string.Empty)
                    };
                    queueSnapshot.Requests.Add(request);
                }
            }
        }
        #endregion

        #region RetrieveAttributeValue()
        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private TEnum RetrieveAttributeValue<TEnum>(XmlElement element, string attributeName, TEnum defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string RetrieveAttributeValue(XmlElement element, string attributeName, string defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            if (string.IsNullOrEmpty(value)) value = defaultValue;
            return value;
        }

        /// <summary>
        /// Retrieves an attribute value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private DateTime RetrieveAttributeValue(XmlElement element, string attributeName, DateTime defaultValue)
        {
            var value = element.GetAttribute(attributeName);
            var dateValue = string.IsNullOrEmpty(value)
                ? defaultValue
                : DateTime.Parse(value, CultureInfo.CurrentCulture);
            return dateValue;
        }
        #endregion

        #region SendButtonPush()
        /// <summary>
        /// Sends a button push command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="project">The project the command is for.</param>
        private void SendButtonPush(string command, string project)
        {
            var url = this.GenerateUrl("ViewFarmReport.aspx");
            var values = new NameValueCollection();
            values.Add(command, "true");
            values.Add("projectName", project);
            values.Add("serverName", this.TargetServer);
            try
            {
                this.webFunctions.SetCredentials(this.client, new Uri(url), false);
                this.client.UploadValues(url, values);
            }
            catch (Exception error)
            {
                throw new CommunicationsException(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} failed: {1}", command, error.Message),
                    error);
            }
        }
        #endregion
        #endregion
    }
}
