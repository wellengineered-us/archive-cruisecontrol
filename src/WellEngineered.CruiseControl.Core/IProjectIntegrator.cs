using System;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Events;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// 	
    /// </summary>
	public interface IProjectIntegrator
	{
        /// <summary>
        /// Gets the project.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		IProject Project { get; }

        /// <summary>
        /// Gets the name.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		string Name { get; }

		/// <summary>
		/// Starts the integration of this project on a separate thread.  If
		/// this integrator has already started, this method causes no action.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the integration of this project.
		/// </summary>
        void Stop(bool restarting);

		/// <summary>
		/// Waits for the project integrator thread to exit, and joins with it.
		/// </summary>
		void WaitForExit();

		/// <summary>
		/// Aborts the integrator thread immediately.
		/// </summary>
		void Abort();

		/// <summary>
		/// Gets a value indicating whether this project integrator is currently
		/// running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Gets a value indicating the project integrator's current state.
		/// </summary>
		ProjectIntegratorState State { get; }

        /// <summary>
        /// Gets the integration repository.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		IIntegrationRepository IntegrationRepository { get; }

		/// <summary>
		/// For invocation by a force build publisher or having the exe config running a project
		/// when CC.Net first starts.
		/// </summary>
        /// <param name="enforcerName">ID of program/person forcing the build</param>
        /// <param name="buildValues"></param>
        void ForceBuild(string enforcerName, Dictionary<string, string> buildValues);
		
		/// <summary>
		/// Aborts the build of the selected project.
		/// </summary>
		void AbortBuild(string enforcerName);

		/// <summary>
		/// For "Force" requests such as by CCTray or the Web GUI.
		/// </summary>
		/// <param name="request">Request contains the source such as the user id.</param>
		void Request(IntegrationRequest request);

		/// <summary>
		/// Cancel a pending project integration request from the integration queue.
		/// </summary>
		void CancelPendingRequest();

        #region Integration events
        /// <summary>
        /// A project integrator is starting an integration.
        /// </summary>
        event EventHandler<IntegrationStartedEventArgs> IntegrationStarted;

        /// <summary>
        /// A project integrator has completed an integration.
        /// </summary>
        event EventHandler<IntegrationCompletedEventArgs> IntegrationCompleted;
        #endregion
    }
}
