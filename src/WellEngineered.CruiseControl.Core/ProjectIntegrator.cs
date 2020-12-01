using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using WellEngineered.CruiseControl.Core.Queues;
using WellEngineered.CruiseControl.Core.SourceControl;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;
using WellEngineered.CruiseControl.Remote.Events;

namespace WellEngineered.CruiseControl.Core
{
    /// <summary>
    /// An object responsible for the continuous integration of a single project.
    /// This integrator, when running, coordinates the top-level life cycle of
    /// a project's integration.
    /// <list type="1">
    ///		<item>The <see cref="ITrigger"/> instance is asked whether to build or not.</item>
    ///		<item>If a build is required, the <see cref="IIntegratable.Integrate(IntegrationRequest)"/>
    ///		is called.</item>
    /// </list>
    /// </summary>
    public class ProjectIntegrator : IProjectIntegrator, IDisposable, IIntegrationQueueNotifier
    {
        private readonly ITrigger trigger;
        private readonly IProject project;
        private readonly IIntegrationQueue integrationQueue;
        private Thread thread;
        private ProjectIntegratorState state = ProjectIntegratorState.Unknown;
        private int AmountOfSourceControlExceptions/* = 0*/;

        private bool runAndStop;
        private bool isRestarting/* = false*/;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectIntegrator" /> class.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="integrationQueue">The integration queue.</param>
        /// <remarks></remarks>
        public ProjectIntegrator(IProject project, IIntegrationQueue integrationQueue)
        {
            this.trigger = project.Triggers;
            this.project = project;
            this.integrationQueue = integrationQueue;
            this.runAndStop = false;
            // Make sure the project's directories exist.
            if (!Directory.Exists(project.WorkingDirectory))
                Directory.CreateDirectory(project.WorkingDirectory);
            if (!Directory.Exists(project.ArtifactDirectory))
                Directory.CreateDirectory(project.ArtifactDirectory);
        }

        /// <summary>
        /// Gets the name.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string Name
        {
            get { return this.project.Name; }
        }

        /// <summary>
        /// Gets the project.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IProject Project
        {
            get { return this.project; }
        }

        /// <summary>
        /// Gets the state.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public ProjectIntegratorState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// Gets the integration repository.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IIntegrationRepository IntegrationRepository
        {
            get { return this.project.IntegrationRepository; }
        }

        // TODO: should not start if stopping (ie. not stopped)
        /// <summary>
        /// Starts this instance.	
        /// </summary>
        /// <remarks></remarks>
        public void Start()
        {
            lock (this)
            {
                if (this.IsRunning)
                    return;

                //if stopping or currently within a restart, allow this run to occur, but then stop project when finished
                //this is to allow the server to restart when config has been update

                if (this.isRestarting || this.state == ProjectIntegratorState.Stopping)
                {
                    this.runAndStop = true;
                    this.isRestarting = false;
                }
                this.state = ProjectIntegratorState.Running;
            }

            // multiple thread instances cannot be created
            if (this.thread == null || this.thread.ThreadState == ThreadState.Stopped)
            {
                this.thread = new Thread(this.Run);
                this.thread.Name = this.project.Name;
            }

            // start thread if it's not running yet
            if (this.thread.ThreadState != ThreadState.Running)
            {
                this.thread.Start();
            }
        }

        /// <summary>
        /// Forces the build.	
        /// </summary>
        /// <param name="enforcerName">Name of the enforcer.</param>
        /// <param name="buildValues">The build values.</param>
        /// <remarks></remarks>
        public void ForceBuild(string enforcerName, Dictionary<string, string> buildValues)
        {
            if (this.State == ProjectIntegratorState.Stopping || this.State == ProjectIntegratorState.Stopped) throw new CruiseControlException("Project is stopping / stopped - unable to start integration");

            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} forced Build for project: {1}", enforcerName, this.project.Name));
            IntegrationRequest request = new IntegrationRequest(BuildCondition.ForceBuild, enforcerName, enforcerName);
            request.BuildValues = buildValues;
            this.AddToQueue(request);

            this.Start();
        }

        /// <summary>
        /// Aborts the build.	
        /// </summary>
        /// <param name="enforcerName">Name of the enforcer.</param>
        /// <remarks></remarks>
        public void AbortBuild(string enforcerName)
        {
            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"{0} aborted the running Build for project: {1}", enforcerName, this.project.Name));
            this.project.AbortRunningBuild(enforcerName);
        }

        /// <summary>
        /// Requests the specified request.	
        /// </summary>
        /// <param name="request">The request.</param>
        /// <remarks></remarks>
        public void Request(IntegrationRequest request)
        {
            if (this.State == ProjectIntegratorState.Stopping || this.State == ProjectIntegratorState.Stopped) throw new CruiseControlException("Project is stopping / stopped - unable to start integration");

            this.AddToQueue(request);
            this.Start();
        }

        /// <summary>
        /// Cancels the pending request.	
        /// </summary>
        /// <remarks></remarks>
        public void CancelPendingRequest()
        {
            this.integrationQueue.RemovePendingRequest(this.project);
        }

        /// <summary>
        /// Main integration loop, intended to be run in its own thread.
        /// </summary>
        private void Run()
        {
            Log.Info("Starting integrator for project: " + this.project.Name);
            try
            {
                // loop, until the integrator is stopped
                while (this.IsRunning)
                {
                    try
                    {
                        bool ran = this.Integrate();
                        if (ran && this.runAndStop)
                        {
                            this.state = ProjectIntegratorState.Stopping;
                            this.runAndStop = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                    // sleep for a short while, to avoid hammering CPU
                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException)
            {
                // suppress logging of ThreadAbortException
                Thread.ResetAbort();
            }
            finally
            {
                this.Stopped();
            }
        }

        private bool Integrate()
        {
            bool ran=false;

            while (this.integrationQueue.IsBlocked)
                Thread.Sleep(200);

            IntegrationRequest ir = this.integrationQueue.GetNextRequest(this.project);
            if (ir != null && this.IsRunning)
            {
                IDisposable queueLock;
                if (!this.integrationQueue.TryLock(out queueLock))
                    return false;

                using (queueLock)
                {
                    // Check to see if this integration request should proceed - Extension point
                    IntegrationStartedEventArgs.EventResult eventResult = this.FireIntegrationStarted(ir);
                    switch (eventResult)
                    {
                        case IntegrationStartedEventArgs.EventResult.Continue:
                            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: '{0}' is first in queue: '{1}' and shall start integration.",
                                                   this.project.Name, this.project.QueueName));

                            IntegrationStatus status = IntegrationStatus.Unknown;
                            IIntegrationResult result = new IntegrationResult();
                            ran = true;

                            try
                            {
                                ir.PublishOnSourceControlException = (this.AmountOfSourceControlExceptions == this.project.MaxSourceControlRetries)
                                                                      || (this.project.SourceControlErrorHandling == Common.SourceControlErrorHandlingPolicy.ReportEveryFailure);
                                result = this.project.Integrate(ir);
                                if (result != null) status = result.Status;
                            }
                            catch
                            {
                                status = IntegrationStatus.Exception;
                                throw;
                            }
                            finally
                            {
                                this.RemoveCompletedRequestFromQueue();

                                // Tell any extensions that an integration has completed
                                this.FireIntegrationCompleted(ir, status);

                                // handle post build : check what to do if source control errors occured
                                if (result != null)
                                {
                                    if (result.SourceControlError != null)
                                    {
                                        this.AmountOfSourceControlExceptions++;
                                    }
                                    else
                                    {
                                        this.AmountOfSourceControlExceptions = 0;
                                    }
                                }

                                if ((this.AmountOfSourceControlExceptions > this.project.MaxSourceControlRetries)
                                    && (this.project.SourceControlErrorHandling == Common.SourceControlErrorHandlingPolicy.ReportOnEveryRetryAmount))
                                {
                                    this.AmountOfSourceControlExceptions = 0;
                                }


                                if ((this.AmountOfSourceControlExceptions > this.project.MaxSourceControlRetries)
                                    && this.project.StopProjectOnReachingMaxSourceControlRetries)
                                {
                                    this.Stopped();
                                }
                            }
                            break;

                        case IntegrationStartedEventArgs.EventResult.Delay:
                            // Log that the request has been cancelled and delay until the request is cleared - otherwise 
                            // stuck in an endless loop until the extensions allow the request through
                            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"An external extension has delayed an integration - project '{0}' on queue '{1}'",
                                this.project.Name,
                                this.project.QueueName));
                            while (this.FireIntegrationStarted(ir) == IntegrationStartedEventArgs.EventResult.Delay)
                            {
                                Thread.Sleep(1000);
                            }
                            break;

                        case IntegrationStartedEventArgs.EventResult.Cancel:
                            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"An external extension has cancelled an integration - project '{0}' on queue '{1}'",
                                this.project.Name,
                                this.project.QueueName));
                            this.RemoveCompletedRequestFromQueue();
                            this.FireIntegrationCompleted(ir, IntegrationStatus.Cancelled);
                            break;
                    }
                }
            }
            else
            {
                this.PollTriggers();
                // If a build is queued for this project we need to hang around until either:
                // - the build gets started by reaching it's turn on the queue
                // - the build gets cancelled from the queue
                // - the thread gets killed
                // However, if the queue is blocked, do not hang around - we need to exit, so that we can come back to the queue
                // after the lock has been released (otherwise we could get stuck here forever
                while (this.IsRunning && this.integrationQueue.HasItemPendingOnQueue(this.project) && !this.integrationQueue.IsBlocked)
                {
                    Thread.Sleep(200);
                }
            }

            return ran;
        }

        private void PollTriggers()
        {
            IntegrationRequest triggeredRequest = this.trigger.Fire();
            if (triggeredRequest != null)
            {
                this.AddToQueue(triggeredRequest);
            }
        }

        private void AddToQueue(IntegrationRequest request)
        {
            this.integrationQueue.Enqueue(new IntegrationQueueItem(this.project, request, this));
        }

        private void RemoveCompletedRequestFromQueue()
        {
            // Free up the queue to kick off the next integration in it if any.
            this.integrationQueue.Dequeue();
        }

        private void Stopped()
        {
            // the state was set to 'Stopping', so set it to 'Stopped'
            this.state = ProjectIntegratorState.Stopped;
            this.thread = null;
            // Ensure that any queued integrations are cleared for this project.
            this.integrationQueue.RemoveProject(this.project);
            Log.Info("Integrator for project: " + this.project.Name + " is now stopped.");
        }

        /// <summary>
        /// Gets a value indicating whether this project integrator is running
        /// and will continue to run.  If the state is Stopping, this returns false.
        /// </summary>
        public bool IsRunning
        {
            get { return this.state == ProjectIntegratorState.Running; }
        }

        /// <summary>
        /// Sets the state to <see cref="ProjectIntegratorState.Stopping"/>, telling the project to
        /// stop at the next possible point in time.
        /// </summary>
        public void Stop(bool restarting)
        {
            if (this.IsRunning || this.state == ProjectIntegratorState.Unknown)
            {
                this.isRestarting = restarting;
                Log.Info("Stopping integrator for project: " + this.project.Name);
                this.state = ProjectIntegratorState.Stopping;
            }
        }

        /// <summary>
        /// Asynchronously abort project by aborting the project thread.  This needs to be followed by a call to WaitForExit 
        /// to ensure that the abort has completed.
        /// </summary>
        public void Abort()
        {
            if (this.thread != null)
            {
                Log.Info("Aborting integrator for project: " + this.project.Name);
                //thread.Abort();
            }
        }

        /// <summary>
        /// Waits for exit.	
        /// </summary>
        /// <remarks></remarks>
        public void WaitForExit()
        {
            if (this.thread != null && this.thread.IsAlive)
            {
                if (this.State != ProjectIntegratorState.Stopping)
                {
                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"WaitForExit requested for non stopping project '{0}' - stopping project", this.Name));
                    this.Stop(false);
                }

                try
                {
                    this.thread.Join();
                }
                catch (NullReferenceException)
                {
                    // if the process stops quickly, the reference to thread may be disposed by the time we get here,
                    // therefore ignore any NullReferenceExceptions
                }
            }
        }

        /// <summary>
        /// Ensure that the integrator's thread is aborted when this object is disposed.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Abort();
        }

        /// <summary>
        /// Notification of entering the integration queue.
        /// </summary>
        public void NotifyEnteringIntegrationQueue()
        {
            if (!this.integrationQueue.HasItemOnQueue(this.project))
            {
                // We only set pending if first request on queue, as do not want to overwrite a Building state.
                this.project.NotifyPendingState();
            }
        }

        /// <summary>
        /// Notification of exiting the integration queue. This could be due to a single project completing,
        /// a pending integration being cancelled or due to all projects being removed from the queue.
        /// </summary>
        public void NotifyExitingIntegrationQueue(bool isPendingItemCancelled)
        {
            if (isPendingItemCancelled)
            {
                // User has cancelled a build request that has not yet started.
                if (this.integrationQueue.GetNextRequest(this.project) == null)
                {
                    // We cancelled the only request for this project in the queue
                    this.project.NotifySleepingState();
                }
                else
                {
                    // We cancelled the pending request but there is one still building
                    // We do not touch the state as will leave project in "Building" state.
                }
            }
            else
            {
                // The project at the front of the queue has completed.
                if (!this.integrationQueue.HasItemPendingOnQueue(this.project))
                {
                    // Nothing is pending on the queue for this project.
                    this.project.NotifySleepingState();
                }
                else
                {
                    // State should go to pending as we still have an item on the queue
                    this.project.NotifyPendingState();
                }
            }
            this.trigger.IntegrationCompleted();
        }

        #region Integration events
        /// <summary>
        /// A project integrator is starting an integration.
        /// </summary>
        public event EventHandler<IntegrationStartedEventArgs> IntegrationStarted;

        /// <summary>
        /// A project integrator has completed an integration.
        /// </summary>
        public event EventHandler<IntegrationCompletedEventArgs> IntegrationCompleted;
        #endregion

        #region Integration event firers
        /// <summary>
        /// Fires the IntegrationStarted event.
        /// </summary>
        /// <param name="request">The integration request.</param>
        protected virtual IntegrationStartedEventArgs.EventResult FireIntegrationStarted(IntegrationRequest request)
        {
            IntegrationStartedEventArgs.EventResult result = IntegrationStartedEventArgs.EventResult.Continue;
            if (this.IntegrationStarted != null)
            {
                IntegrationStartedEventArgs args = new IntegrationStartedEventArgs(request,
                    this.project.Name);
                this.IntegrationStarted(this, args);
                result = args.Result;
            }
            return result;
        }

        /// <summary>
        /// Fires the IntegrationCompleted event.
        /// </summary>
        /// <param name="request">The integration request.</param>
        /// <param name="status">The outcome of the integration.</param>
        protected virtual void FireIntegrationCompleted(IntegrationRequest request, IntegrationStatus status)
        {
            if (this.IntegrationCompleted != null)
            {
                IntegrationCompletedEventArgs args = new IntegrationCompletedEventArgs(request,
                    this.project.Name,
                    status);
                this.IntegrationCompleted(this, args);
            }
        }
        #endregion
    }
}
