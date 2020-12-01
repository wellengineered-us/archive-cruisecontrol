using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Security;
using WellEngineered.CruiseControl.Core.SourceControl;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
	/// <summary>
	/// Interface to which all projects must adhere, and via which all application
	/// code should interact with projects.
	/// </summary>
	public interface IProject : IIntegratable
	{
		/// <summary>
		/// The name of this project.
		/// </summary>
		string Name
		{
			get;
		}

        #region Links
        /// <summary>
        /// Link this project to other sites.
        /// </summary>
        NameValuePair[] LinkedSites { get; set; }
        #endregion

		/// <summary>
		/// An optional category that groups the project
		/// </summary>
		string Category
		{
			get;
		}

        #region ConfigurationXml
        /// <summary>
        /// Gets or sets the configuration XML.
        /// </summary>
        /// <value>The configuration XML.</value>
        string ConfigurationXml { get; }
        #endregion

        /// <summary>
        /// An optional description for the project.
        /// </summary>
        string Description { get; }

		/// <summary>
		/// A component to trigger integrations for this project.
		/// TODO: remove
		/// </summary>
		ITrigger Triggers 
		{
			get;
		}

		/// <summary>
		/// Where the results web page for this project can be found
		/// </summary>
		string WebURL 
		{ 
			get;
		}

		/// <summary>
		/// Gets the project's working directory, where the primary build and checkout happens
		/// </summary>
		string WorkingDirectory
		{
			get;
		}

		/// <summary>
		/// Gets the project's artifact directory, where build logs and distributables can be placed
		/// </summary>
		string ArtifactDirectory
		{
			get;
		}

		/// <summary>
		/// This method is called when the project is being deleted from the server. It allows resources to be cleaned up, SCM clients to be unregistered, etc.
		/// </summary>
		void Purge(bool purgeWorkingDirectory, bool purgeArtifactDirectory, bool purgeSourceControlEnvironment);

        /// <summary>
        /// Gets the external links.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		ExternalLink[] ExternalLinks { get; }

        /// <summary>
        /// Gets the statistics.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		string Statistics { get; }

        /// <summary>
        /// Gets the modification history.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        string ModificationHistory { get; }

        /// <summary>
        /// Gets the RSS feed.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        string RSSFeed { get; }

        /// <summary>
        /// Gets the integration repository.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		IIntegrationRepository IntegrationRepository { get; }

		/// <summary>
		/// Gets or sets the build queue this project will be added to when a start of the build is triggered.
		/// If no queue name specified, uses the project name.
		/// </summary>
		string QueueName { get; set; }

		/// <summary>
		/// Gets or sets the optional queue priority for when multiple projects share a queue. 
		/// A priority of zero (default) indicates a FIFO queue.
		/// An item with priority 1 will be inserted before an item of priority 2.
		/// </summary>
		int QueuePriority { get; }

        /// <summary>
        /// Initializes this instance.	
        /// </summary>
        /// <remarks></remarks>
		void Initialize();

        /// <summary>
        /// Creates the project status.	
        /// </summary>
        /// <param name="integrator">The integrator.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		ProjectStatus CreateProjectStatus(IProjectIntegrator integrator);
        /// <summary>
        /// Gets the current activity.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        ProjectActivity CurrentActivity { get; }

        /// <summary>
        /// Aborts the running build.	
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <remarks></remarks>
		void AbortRunningBuild(string userName);
		
        /// <summary>
        /// adds a message 
        /// </summary>
        /// <param name="message"></param>
		void AddMessage(Message message);

    	
		/// <summary>
		/// Notification that project should enter a pending state due to being queued.
		/// </summary>
		void NotifyPendingState();

		/// <summary>
		/// Notification of last project exiting the integration queue and hence can return to sleeping state.
		/// </summary>
		void NotifySleepingState();

        /// <summary>
        /// The associated security configuration.
        /// </summary>
        IProjectAuthorisation Security { get; }

        /// <summary>
        /// Maximum amount of sourcecontrol exceptions allowed, before stopping the project (if specified to do so).
        /// This equals to the amount of errors in GetModifications. 
        /// </summary>
        int MaxSourceControlRetries { get; }

        #region AskForForceBuildReason
        /// <summary>
        /// Should a comment be requested when a force build is triggered.
        /// </summary>
        DisplayLevel AskForForceBuildReason { get; }
        #endregion

        /// <summary>
        /// Stop the project when the MaxSourceControlRetries limit has been reached
        /// </summary>
        bool StopProjectOnReachingMaxSourceControlRetries { get; }

        /// <summary>
        /// What do do when an error occurs in the getmodifications stage of the source control 
        /// </summary>
        Common.SourceControlErrorHandlingPolicy SourceControlErrorHandling { get; }

        /// <summary>
        /// The initial start-up state to set.
        /// </summary>
        ProjectInitialState InitialState { get; }

        /// <summary>
        /// The start-up mode for this project.
        /// </summary>
        ProjectStartupMode StartupMode { get; }

        #region RetrievePackageList()
        /// <summary>
        /// Retrieves the latest list of packages.
        /// </summary>
        /// <returns></returns>
        List<PackageDetails> RetrievePackageList();

        /// <summary>
        /// Retrieves the list of packages for a build.
        /// </summary>
        /// <param name="buildName"></param>
        /// <returns></returns>
        List<PackageDetails> RetrievePackageList(string buildName);
        #endregion

        #region RetrieveBuildFinalStatus()
        /// <summary>
        /// Retrieves the final status of a build.
        /// </summary>
        /// <param name="buildName">Name of the build.</param>
        /// <returns>The final status if found; <c>null</c> otherwise.</returns>
        ItemStatus RetrieveBuildFinalStatus(string buildName);
        #endregion
    }
}
