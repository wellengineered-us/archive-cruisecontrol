using System;
using System.Globalization;

using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
	/// Exposes project management functionality (start, stop, status) via remoting.  
	/// The CCTray is one such example of an application that may make use of this remote interface.
	/// </summary>
    [Obsolete("Use ICruiseServerClient instead")]
	public class CruiseManager : MarshalByRefObject, ICruiseManager
	{
		private readonly ICruiseServer cruiseServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CruiseManager" /> class.	
        /// </summary>
        /// <param name="cruiseServer">The cruise server.</param>
        /// <remarks></remarks>
        public CruiseManager(ICruiseServer cruiseServer)
		{
			this.cruiseServer = cruiseServer;
		}

        /// <summary>
        /// Gets the actual server.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public ICruiseServer ActualServer
        {
            get { return this.cruiseServer; }
        }

        /// <summary>
        /// Initializes the lifetime service.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override object InitializeLifetimeService()
		{
            return null;
		}

        /// <summary>
        /// Gets information about the last build status, current activity and project name.
        /// for all projects on a cruise server
        /// </summary>
        public ProjectStatus[] GetProjectStatus()
        {
            ProjectStatusResponse resp = this.cruiseServer.GetProjectStatus(this.GenerateServerRequest());
            this.ValidateResponse(resp);
            return resp.Projects.ToArray();
        }

        /// <summary>
        /// Forces a build for the named project.
        /// </summary>
        /// <param name="projectName">project to force</param>
        /// <param name="enforcerName">ID of trigger/action forcing the build</param>
        public void ForceBuild(string projectName, string enforcerName)
        {
            Response resp = this.cruiseServer.ForceBuild(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
        }

        /// <summary>
        /// Aborts the build.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="enforcerName">Name of the enforcer.</param>
        /// <remarks></remarks>
        public void AbortBuild(string projectName, string enforcerName)
		{
            Response resp = this.cruiseServer.AbortBuild(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Requests the specified project name.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="integrationRequest">The integration request.</param>
        /// <remarks></remarks>
		public void Request(string projectName, IntegrationRequest integrationRequest)
		{
            BuildIntegrationRequest request = new BuildIntegrationRequest(null, projectName);
            request.BuildCondition = integrationRequest.BuildCondition;
            Response resp = this.cruiseServer.ForceBuild(request);
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Starts the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
		public void Start(string project)
		{
            Response resp = this.cruiseServer.Start(this.GenerateProjectRequest(project));
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Stops the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
		public void Stop(string project)
		{
            Response resp = this.cruiseServer.Stop(this.GenerateProjectRequest(project));
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Sends the message.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="message">The message.</param>
        /// <remarks></remarks>
		public void SendMessage(string projectName, Message message)
		{
            MessageRequest request = new MessageRequest();
            request.ProjectName = projectName;
            request.Message = message.Text;
            request.Kind = message.Kind;
            Response resp = this.cruiseServer.SendMessage(request);
            this.ValidateResponse(resp);
        }

        /// <summary>
        /// Waits for exit.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <remarks></remarks>
        public void WaitForExit(string projectName)
        {
            Response resp = this.cruiseServer.WaitForExit(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Cancel a pending project integration request from the integration queue.
        /// </summary>
		public void CancelPendingRequest(string projectName)
		{
            Response resp = this.cruiseServer.CancelPendingRequest(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
		}
		
		/// <summary>
		/// Gets the projects and integration queues snapshot from this server.
		/// </summary>
        public CruiseServerSnapshot GetCruiseServerSnapshot()
		{
            SnapshotResponse resp = this.cruiseServer.GetCruiseServerSnapshot(this.GenerateServerRequest());
            this.ValidateResponse(resp);
            return resp.Snapshot;
		}

        /// <summary>
        /// Returns the name of the most recent build for the specified project
        /// </summary>
		public string GetLatestBuildName(string projectName)
		{
            DataResponse resp = this.cruiseServer.GetLatestBuildName(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Returns the names of all builds for the specified project, sorted s.t. the newest build is first in the array
        /// </summary>
		public string[] GetBuildNames(string projectName)
		{
            DataListResponse resp = this.cruiseServer.GetBuildNames(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data.ToArray();
		}

        /// <summary>
        /// Returns the names of the buildCount most recent builds for the specified project, sorted s.t. the newest build is first in the array
        /// </summary>
		public string[] GetMostRecentBuildNames(string projectName, int buildCount)
		{
            BuildListRequest request = new BuildListRequest(null, projectName);
            request.NumberOfBuilds = buildCount;
            DataListResponse resp = this.cruiseServer.GetMostRecentBuildNames(request);
            this.ValidateResponse(resp);
            return resp.Data.ToArray();
		}

        /// <summary>
        /// Returns the build log contents for requested project and build name
        /// </summary>
		public string GetLog(string projectName, string buildName)
		{
            BuildRequest request = new BuildRequest(null, projectName);
            request.BuildName = buildName;
            DataResponse resp = this.cruiseServer.GetLog(request);
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Returns a log of recent build server activity. How much information that is returned is configured on the build server.
        /// </summary>
		public string GetServerLog()
		{
            DataResponse resp = this.cruiseServer.GetServerLog(this.GenerateServerRequest());
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Returns a log of recent build server activity for a specific project. How much information that is returned is configured on the build server.
        /// </summary>
		public string GetServerLog(string projectName)
		{
            DataResponse resp = this.cruiseServer.GetServerLog(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
        }

        /// <summary>
        /// Returns the version of the server
        /// </summary>
        public string GetServerVersion()
        {
            DataResponse resp = this.cruiseServer.GetServerVersion(this.GenerateServerRequest());
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Adds a project to the server
        /// </summary>
		public void AddProject(string serializedProject)
		{
            ChangeConfigurationRequest request = new ChangeConfigurationRequest();
            request.ProjectDefinition = serializedProject;
            Response resp = this.cruiseServer.AddProject(request);
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Deletes the specified project from the server
        /// </summary>
		public void DeleteProject(string projectName, bool purgeWorkingDirectory, bool purgeArtifactDirectory, bool purgeSourceControlEnvironment)
		{
            ChangeConfigurationRequest request = new ChangeConfigurationRequest(null, projectName);
            request.PurgeWorkingDirectory = purgeWorkingDirectory;
            request.PurgeArtifactDirectory = purgeArtifactDirectory;
            request.PurgeSourceControlEnvironment = purgeSourceControlEnvironment;
            Response resp = this.cruiseServer.DeleteProject(request);
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Returns the serialized form of the requested project from the server
        /// </summary>
		public string GetProject(string projectName)
		{
            DataResponse resp = this.cruiseServer.GetProject(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Updates the selected project on the server
        /// </summary>
		public void UpdateProject(string projectName, string serializedProject)
		{
            ChangeConfigurationRequest request = new ChangeConfigurationRequest(null, projectName);
            request.ProjectDefinition = serializedProject;
            Response resp = this.cruiseServer.UpdateProject(request);
            this.ValidateResponse(resp);
		}

        /// <summary>
        /// Gets the external links.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public ExternalLink[] GetExternalLinks(string projectName)
		{
            ExternalLinksListResponse resp = this.cruiseServer.GetExternalLinks(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.ExternalLinks.ToArray();
		}

        /// <summary>
        /// Gets the artifact directory.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string GetArtifactDirectory(string projectName)
		{
            DataResponse resp = this.cruiseServer.GetArtifactDirectory(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Gets the statistics document.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public string GetStatisticsDocument(string projectName)
		{
            DataResponse resp = this.cruiseServer.GetStatisticsDocument(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Gets the modification history document.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetModificationHistoryDocument(string projectName)
        {
            DataResponse resp = this.cruiseServer.GetModificationHistoryDocument(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
        }

        /// <summary>
        /// Gets the RSS feed.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetRSSFeed(string projectName)
        {
            DataResponse resp = this.cruiseServer.GetRSSFeed(this.GenerateProjectRequest(projectName));
            this.ValidateResponse(resp);
            return resp.Data;
		}

        /// <summary>
        /// Retrieves the amount of free disk space.
        /// </summary>
        /// <returns></returns>
        public long GetFreeDiskSpace()
        {
            DataResponse resp = this.cruiseServer.GetFreeDiskSpace(this.GenerateServerRequest());
            this.ValidateResponse(resp);
            return Convert.ToInt64(resp.Data, CultureInfo.CurrentCulture);
        }

        #region RetrieveFileTransfer()
        /// <summary>
        /// Retrieve a file transfer object.
        /// </summary>
        /// <param name="project">The project to retrieve the file for.</param>
        /// <param name="fileName">The name of the file.</param>
        public virtual RemotingFileTransfer RetrieveFileTransfer(string project, string fileName)
        {
            var request = new FileTransferRequest();
            request.ProjectName = project;
            request.FileName = fileName;
            var response = this.cruiseServer.RetrieveFileTransfer(request);
            this.ValidateResponse(response);
            return response.FileTransfer as RemotingFileTransfer;
        }
		#endregion

        #region Helper methods - conversion from old to new
        #region GenerateServerRequest()
        /// <summary>
        /// Generate a server request.
        /// </summary>
        /// <returns></returns>
        private ServerRequest GenerateServerRequest()
        {
            ServerRequest request = new ServerRequest();
            return request;
        }
        #endregion

        #region GenerateProjectRequest()
        /// <summary>
        /// Generate a project request.
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        private ProjectRequest GenerateProjectRequest(string projectName)
        {
            ProjectRequest request = new ProjectRequest();
            request.ProjectName = projectName;
            return request;
        }
        #endregion

        #region ValidateResponse()
        /// <summary>
        /// Validate the response from the server.
        /// </summary>
        /// <param name="response"></param>
        private void ValidateResponse(Response response)
        {
            if (response.Result == ResponseResult.Failure)
        {
                string message = "Request request has failed on the remote server:" + Environment.NewLine +
                    response.ConcatenateErrors();
                throw new CruiseControlException(message);
        }
        }
        #endregion
        #endregion
    }
}
