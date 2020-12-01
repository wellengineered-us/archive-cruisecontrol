using System;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.SourceControl;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
    public class IntegrationResultManager : IIntegrationResultManager
    {
        private readonly Project project;
        private IIntegrationResult lastResult;
        private IIntegrationResult currentIntegration;
        private IntegrationSummary lastIntegration;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationResultManager" /> class.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <remarks></remarks>
        public IntegrationResultManager(Project project)
        {
            this.project = project;
        }

        /// <summary>
        /// Gets the last integration result.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IIntegrationResult LastIntegrationResult
        {
            get
            {
                // lazy loads because StateManager needs to be populated from configuration
                if (this.lastResult == null)
                {
                    this.lastResult = this.CurrentIntegration;
                }
                return this.lastResult;
            }
        }

        /// <summary>
        /// Gets the last integration.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IntegrationSummary LastIntegration
        {
            get
            {
                if (this.lastIntegration == null)
                {
                    this.lastIntegration = ConvertResultIntoSummary(this.LastIntegrationResult);
                }
                return this.lastIntegration;
            }
        }

        /// <summary>
        /// Gets the current integration.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IIntegrationResult CurrentIntegration
        {
            get
            {
                if (this.currentIntegration == null)
                {
                    if (this.project.StateManager.HasPreviousState(this.project.Name))
                        this.currentIntegration = this.project.StateManager.LoadState(this.project.Name);
                    else
                        this.currentIntegration = IntegrationResult.CreateInitialIntegrationResult(this.project.Name, this.project.WorkingDirectory, this.project.ArtifactDirectory);
                }
                return this.currentIntegration;
            }
        }

        /// <summary>
        /// Starts the new integration.	
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public IIntegrationResult StartNewIntegration(IntegrationRequest request)
        {
            IntegrationResult newResult = new IntegrationResult(this.project.Name, this.project.WorkingDirectory, this.project.ArtifactDirectory, request, this.LastIntegration);
            newResult.ArtifactDirectory = this.project.ArtifactDirectory;
            newResult.ProjectUrl = this.project.WebURL;
            NameValuePair.Copy(this.LastIntegrationResult.SourceControlData, newResult.SourceControlData);

            return this.currentIntegration = newResult;
        }

        /// <summary>
        /// Finishes the integration.	
        /// </summary>
        /// <remarks></remarks>
        public void FinishIntegration()
        {
            try
            {
                // Put the failed tasks into the appropriate list.
                // Must be done here as it is before the build log is dumped
                var failedTasks = new List<string>();
                this.project.FindFailedTasks(failedTasks);
                this.currentIntegration.FailureTasks.AddRange(failedTasks);

                // Save users who may have broken integration so we can email them until it's fixed
                if (this.currentIntegration.Status == IntegrationStatus.Failure)
                {
                    // Build is broken - add any users who contributed modifications to the existing list of users
                    // who have contributed modifications to failing builds.
                    foreach (Modification modification in this.currentIntegration.Modifications)
                    {
                        if (!this.currentIntegration.FailureUsers.Contains(modification.UserName))
                            this.currentIntegration.FailureUsers.Add(modification.UserName);
                    }
                }
                this.project.StateManager.SaveState(this.currentIntegration);
            }
            catch (Exception ex)
            {
                // swallow exception???
                Log.Error("Unable to save integration result: " + ex);
            }
            this.lastResult = this.currentIntegration;
            this.lastIntegration = ConvertResultIntoSummary(this.currentIntegration);
        }

        private static IntegrationSummary ConvertResultIntoSummary(IIntegrationResult integration)
        {
            string lastSuccessfulIntegrationLabel = (integration.Succeeded) ? integration.Label : integration.LastSuccessfulIntegrationLabel;
            IntegrationSummary newSummary = new IntegrationSummary(integration.Status, integration.Label, lastSuccessfulIntegrationLabel, integration.StartTime);
            newSummary.FailureUsers = integration.FailureUsers;
            newSummary.FailureTasks = integration.FailureTasks;
            return newSummary;
        }
    }
}
