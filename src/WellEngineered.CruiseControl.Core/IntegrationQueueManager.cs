using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Queues;
using WellEngineered.CruiseControl.Core.State;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Events;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public class IntegrationQueueManager
        : IQueueManager
	{
		private readonly IProjectIntegratorListFactory projectIntegratorListFactory;
		private IProjectIntegratorList projectIntegrators;

		private readonly IntegrationQueueSet integrationQueues = new IntegrationQueueSet();
        private readonly IProjectStateManager stateManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationQueueManager" /> class.	
        /// </summary>
        /// <param name="projectIntegratorListFactory">The project integrator list factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="stateManager">The state manager.</param>
        /// <remarks></remarks>
		public IntegrationQueueManager(IProjectIntegratorListFactory projectIntegratorListFactory,
		                               IConfiguration configuration,
                                       IProjectStateManager stateManager)
		{
			this.projectIntegratorListFactory = projectIntegratorListFactory;
			this.Initialize(configuration);
            this.stateManager = stateManager;
		}

		/// <summary>
		/// Gets the projects and integration queues snapshot from this server.
		/// </summary>
        public CruiseServerSnapshot GetCruiseServerSnapshot()
		{
		    ProjectStatus[] projectStatuses = this.GetProjectStatuses();
		    QueueSetSnapshot queueSetSnapshot = this.integrationQueues.GetIntegrationQueueSnapshot();
            return new CruiseServerSnapshot(projectStatuses, queueSetSnapshot);
		}

        /// <summary>
        /// Starts all projects.	
        /// </summary>
        /// <remarks></remarks>
		public void StartAllProjects()
		{
			foreach (IProjectIntegrator integrator in this.projectIntegrators)
			{
                bool canStart = (integrator.Project == null) ||
                    ((integrator.Project.StartupMode == ProjectStartupMode.UseLastState) &&
                    this.stateManager.CheckIfProjectCanStart(integrator.Name)) ||
                    ((integrator.Project.StartupMode == ProjectStartupMode.UseInitialState) &&
                    (integrator.Project.InitialState == ProjectInitialState.Started));
                if (canStart) integrator.Start();
			}
		}

        /// <summary>
        /// Stops all projects.	
        /// </summary>
        /// <param name="restarting">The restarting.</param>
        /// <remarks></remarks>
        public void StopAllProjects(bool restarting)
		{
			foreach (IProjectIntegrator integrator in this.projectIntegrators)
			{
				integrator.Stop(restarting);
			}
			this.WaitForIntegratorsToExit();
			// We should clear the integration queue so the queues can be rebuilt when start again.
			this.integrationQueues.Clear();
		}

        /// <summary>
        /// Aborts this instance.	
        /// </summary>
        /// <remarks></remarks>
		public void Abort()
		{
			foreach (IProjectIntegrator integrator in this.projectIntegrators)
			{
				integrator.Abort();
			}
			this.WaitForIntegratorsToExit();
			// We should clear the integration queue so the queues can be rebuilt when start again.
			this.integrationQueues.Clear();
		}

        /// <summary>
        /// Gets the project statuses.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public ProjectStatus[] GetProjectStatuses()
		{
			ArrayList projectStatusList = new ArrayList();
			foreach (IProjectIntegrator integrator in this.projectIntegrators)
			{
				IProject project = integrator.Project;
				projectStatusList.Add(project.CreateProjectStatus(integrator));
			}
			return (ProjectStatus[]) projectStatusList.ToArray(typeof (ProjectStatus));
		}

        /// <summary>
        /// Gets the integrator.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public IProjectIntegrator GetIntegrator(string projectName)
		{
            if (string.IsNullOrEmpty(projectName))
                throw new NoSuchProjectException(projectName);

			IProjectIntegrator integrator = this.projectIntegrators[projectName];
			
            if (integrator == null)
                throw new NoSuchProjectException(projectName);

			return integrator;
		}

        /// <summary>
        /// Forces the build.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="enforcerName">Name of the enforcer.</param>
        /// <param name="buildValues">The build values.</param>
        /// <remarks></remarks>
        public void ForceBuild(string projectName, string enforcerName, Dictionary<string, string> buildValues)
		{
            this.GetIntegrator(projectName).ForceBuild(enforcerName, buildValues);
		}

        /// <summary>
        /// Waits for exit.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <remarks></remarks>
		public void WaitForExit(string projectName)
		{
			this.GetIntegrator(projectName).WaitForExit();
		}

        /// <summary>
        /// Requests the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="request">The request.</param>
        /// <remarks></remarks>
		public void Request(string project, IntegrationRequest request)
		{
			this.GetIntegrator(project).Request(request);
		}

        /// <summary>
        /// Cancels the pending request.	
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <remarks></remarks>
		public void CancelPendingRequest(string projectName)
		{
			this.GetIntegrator(projectName).CancelPendingRequest();
		}

        /// <summary>
        /// Stops the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
		public void Stop(string project)
		{
            this.stateManager.RecordProjectAsStopped(project);
			this.GetIntegrator(project).Stop(false);
		}

        /// <summary>
        /// Starts the specified project.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
		public void Start(string project)
		{
            this.stateManager.RecordProjectAsStartable(project);
			this.GetIntegrator(project).Start();
		}

        /// <summary>
        /// Restarts the specified configuration.	
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks></remarks>
		public void Restart(IConfiguration configuration)
		{
		    this.StopAllProjects(true);
			this.Initialize(configuration);
			this.StartAllProjects();
		}

		private void WaitForIntegratorsToExit()
		{
			foreach (IProjectIntegrator integrator in this.projectIntegrators)
			{
				integrator.WaitForExit();
			}
		}

		private void Initialize(IConfiguration configuration)
		{
			foreach (IProject project in configuration.Projects)
			{
				// Force the queue to be created if it does not exist already.
                IQueueConfiguration config = configuration.FindQueueConfiguration(project.QueueName);
				this.integrationQueues.Add(project.QueueName, config);
			}
			this.projectIntegrators = this.projectIntegratorListFactory.CreateProjectIntegrators(configuration.Projects, this.integrationQueues);

			if (this.projectIntegrators.Count == 0)
			{
				Log.Info("No projects found");
			}
		}

		/// <summary>
		/// Returns an array of the current queue names in usage.
		/// </summary>
		/// <returns>Array of current queue names in use.</returns>
		public string[] GetQueueNames()
		{
			return this.integrationQueues.GetQueueNames();
		}

        /// <summary>
        /// Associates the integration events.
        /// </summary>
        /// <param name="integrationStarted"></param>
        /// <param name="integrationCompleted"></param>
        public void AssociateIntegrationEvents(EventHandler<IntegrationStartedEventArgs> integrationStarted,
            EventHandler<IntegrationCompletedEventArgs> integrationCompleted)
        {
            foreach (IProjectIntegrator integrator in this.projectIntegrators)
            {
                integrator.IntegrationStarted += integrationStarted;
                integrator.IntegrationCompleted += integrationCompleted;
            }
        }
	}
}
