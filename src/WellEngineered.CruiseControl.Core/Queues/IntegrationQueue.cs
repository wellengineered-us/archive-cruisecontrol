using System;
using System.Collections;
using System.Collections.Generic;

using WellEngineered.CruiseControl.Core.Config;
using WellEngineered.CruiseControl.Core.Util;
using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Queues
{
	/// <summary>
	/// Implementation of a named integration queue.
	/// The currently integrating project in this queue will be at queue position zero.
	/// </summary>
	public class IntegrationQueue : ArrayList, IIntegrationQueue
	{
        // TryLockInUse serialized to prevent two integration queues from
        // partially locking their LockQueues, running into each other, and
        // aborting; that could cause a functional deadlock.
        private static object blockingLockObject = new object();
        
        private readonly string name;
        private readonly IQueueConfiguration configuration;
        private readonly List<string> blockingQueueNames;
        private readonly IntegrationQueueSet parentQueueSet;
        private bool inUse/* = false*/;

        private static readonly object queueLockSync = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationQueue" /> class.	
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="parentQueueSet">The parent queue set.</param>
        /// <remarks></remarks>
		public IntegrationQueue(string name, IQueueConfiguration configuration, IntegrationQueueSet parentQueueSet)
		{
			this.name = name;
            this.configuration = configuration;
            this.parentQueueSet = parentQueueSet;

            this.blockingQueueNames = new List<string>();
		}

        /// <summary>
        /// Gets the name.	
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
		public string Name
		{
			get { return this.name; }
		}

        /// <summary>
        /// Is this Queue locked by another (N) Queue(s)?
        /// </summary>
        public virtual bool IsBlocked
        {
            get
            {
                lock (queueLockSync) 
                { 
                    return this.blockingQueueNames.Count != 0; 
                }
            }
        }
        
        /// <summary>
        /// The configuration settings for this queue.
        /// </summary>
        public virtual IQueueConfiguration Configuration
        {
            get { return this.configuration; }
        }

		/// <summary>
		/// Add a project integration request be added to the integration queue.
		/// If no requests are on that queue already the integration is just kicked off immediately.
		/// If the request is a force build and an integration is already on the queue for that project
		/// then the queue request is ignored as it is redundant.
		/// </summary>
		/// <param name="integrationQueueItem">The integration queue item.</param>
		public void Enqueue(IIntegrationQueueItem integrationQueueItem)
		{
			lock (this)
			{
				if (this.Count == 0)
				{
					// We can start integration straight away as first in first served
					this.AddToQueue(integrationQueueItem);
				}
				else
				{
					// We need to see if we already have a integration request for this project on the queue
					// If so then we will ignore the latest request.
					// Note we start at queue position 1 since position 0 is currently integrating.

					int? foundIndex = null;
                    bool addItem = true;
                    IIntegrationQueueItem foundItem = null;

					for (int index = 1; index < this.Count; index++)
					{
						IIntegrationQueueItem queuedItem = this.GetIntegrationQueueItem(index);
						if (queuedItem.Project == integrationQueueItem.Project)
						{
                            foundItem = queuedItem;
                            foundIndex = index;
                            break;

						}
					}

					if (foundIndex != null)
 					{
                        switch (this.configuration.HandlingMode)
                        {
                            case QueueDuplicateHandlingMode.UseFirst:
                                // Only use the first item in the queue - if a newer item is added it will be ignored
                                Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} - cancelling new request", integrationQueueItem.Project.Name, this.Name));
                                addItem = false;
                                break;

                            case QueueDuplicateHandlingMode.ApplyForceBuildsReAdd:
                                // If a force build is added to the queue, it will remove an existing non-force build and add the new request to the end of the queue
                                if (foundItem.IntegrationRequest.BuildCondition >= integrationQueueItem.IntegrationRequest.BuildCondition)
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} - cancelling new request", integrationQueueItem.Project.Name, this.Name));
                                    addItem = false;
                                }
                                else
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} with lower prority - cancelling existing request", integrationQueueItem.Project.Name, this.Name));
                                    lock (this)
                                    {
                                        this.NotifyExitingQueueAndRemoveItem(foundIndex.Value, foundItem, true);
                                    }
                                }
                                break;

                            case QueueDuplicateHandlingMode.ApplyForceBuildsReAddTop:
                                // If a force build is added to the queue, it will remove an existing non-force build and add the new request to the beginning of the queue
                                addItem = false;
                                if (foundItem.IntegrationRequest.BuildCondition >= integrationQueueItem.IntegrationRequest.BuildCondition)
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} - cancelling new request", integrationQueueItem.Project.Name, this.Name));
                                }
                                else
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} with lower prority - cancelling existing request", integrationQueueItem.Project.Name, this.Name));
                                    lock (this)
                                    {
                                        this.NotifyExitingQueueAndRemoveItem(foundIndex.Value, foundItem, true);
                                        // Add project to the queue directly after the currently building one.
                                        this.AddToQueue(integrationQueueItem, 1);
                                    }
                                }
                                break;

                            case QueueDuplicateHandlingMode.ApplyForceBuildsReplace:
                                // If a force build is added to the queue, it will replace an existing non-forc build
                                addItem = false;
                                if (foundItem.IntegrationRequest.BuildCondition >= integrationQueueItem.IntegrationRequest.BuildCondition)
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} - cancelling new request", integrationQueueItem.Project.Name, this.Name));
                                }
                                else
                                {
                                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: {0} already on queue: {1} with lower prority - replacing existing request at position {2}", integrationQueueItem.Project.Name, this.Name, foundIndex));
                                    lock (this)
                                    {
                                        this.NotifyExitingQueueAndRemoveItem(foundIndex.Value, foundItem, true);
                                        this.AddToQueue(integrationQueueItem, foundIndex);
                                    }
                                }
                                break;
                            default:
                                throw new ConfigurationException("Unknown handling mode for duplicates: " + this.configuration.HandlingMode);
                        }
 					}

                    if (addItem)
                    {
                        lock (this)
                        {
                            this.AddToQueue(integrationQueueItem);
                        }
                    }
				}
			}
		}

		private IIntegrationQueueItem GetIntegrationQueueItem(int index)
		{
			return this[index] as IIntegrationQueueItem;
		}

		/// <summary>
		/// Releases the next integration request on the queue to start it's integration.
		/// </summary>
		public void Dequeue()
		{
			lock (this)
			{
				if (this.Count > 0)
				{
					// The first item in the queue has now been integrated so discard it.
					IIntegrationQueueItem integrationQueueItem = (IIntegrationQueueItem) this[0];
					this.NotifyExitingQueueAndRemoveItem(0, integrationQueueItem, false);
				}
			}
		}

		/// <summary>
		/// Removes a pending integration request (i.e. one that has not yet started) for this
		/// project from the queue if it is available.
		/// </summary>
		/// <param name="project">The project to have pending items removed from the queue.</param>
		public void RemovePendingRequest(IProject project)
		{
			lock (this)
			{
				bool considerFirstQueueItem = false;
				this.RemoveProjectItems(project, considerFirstQueueItem);
			}
		}

		/// <summary>
		/// Removes all queued integrations for this project. To be invoked when "stopping"
		/// a project.
		/// </summary>
		/// <param name="project">The project to be removed.</param>
		public void RemoveProject(IProject project)
		{
			lock (this)
			{
				bool considerFirstQueueItem = true;
				this.RemoveProjectItems(project, considerFirstQueueItem);
			}
		}

		/// <summary>
		/// Returns an array of the current queued integrations on the queue.
		/// </summary>
		/// <returns>Array of current queued integrations on the queue.</returns>
		public IIntegrationQueueItem[] GetQueuedIntegrations()
		{
			return (IIntegrationQueueItem[]) this.ToArray(typeof (IIntegrationQueueItem));
		}

        /// <summary>
        /// Gets the next request.	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public IntegrationRequest GetNextRequest(IProject project)
		{
            lock (this)
            {
                if (this.Count == 0) return null;

                if (this.IsBlocked) return null;

                IIntegrationQueueItem item = this.GetIntegrationQueueItem(0);

                if (item != null && item.Project == project)
                    return item.IntegrationRequest;

                return null;
            }
		}

        /// <summary>
        /// Determines whether [has item on queue] [the specified project].	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool HasItemOnQueue(IProject project)
		{
			return this.HasItemOnQueue(project, /* pendingItemsOnly*/ false);
		}

        /// <summary>
        /// Determines whether [has item pending on queue] [the specified project].	
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        /// <remarks></remarks>
		public bool HasItemPendingOnQueue(IProject project)
		{
			return this.HasItemOnQueue(project, /* pendingItemsOnly*/ true);
		}

		private bool HasItemOnQueue(IProject project, bool pendingItemsOnly)
		{
			lock (this)
			{
				int startIndex = pendingItemsOnly ? 1 : 0;
				if (this.Count > startIndex)
				{
					for	(int index = startIndex; index < this.Count; index++)
					{
						IIntegrationQueueItem queuedIntegrationQueueItem = this[index] as IIntegrationQueueItem;
						if ((queuedIntegrationQueueItem != null) && (queuedIntegrationQueueItem.Project == project))
							return true;
					}
				}
				return false;
			}
		}

        private void AddToQueue(IIntegrationQueueItem integrationQueueItem)
        {
            this.AddToQueue(integrationQueueItem, null);
        }

		private void AddToQueue(IIntegrationQueueItem integrationQueueItem, int? queuePosition)
		{
            if (!queuePosition.HasValue)
            {
                queuePosition = this.GetPrioritisedQueuePosition(integrationQueueItem.Project.QueuePriority);
                Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Project: '{0}' is added to queue: '{1}' in position {2}. Requestsource : {3} ({4})",
                                       integrationQueueItem.Project.Name, this.Name, queuePosition, integrationQueueItem.IntegrationRequest.Source,integrationQueueItem.IntegrationRequest.UserName));
            }
			integrationQueueItem.IntegrationQueueNotifier.NotifyEnteringIntegrationQueue();
			this.Insert(queuePosition.Value, integrationQueueItem);
		}

		private int GetPrioritisedQueuePosition(int insertingItemPriority)
		{
			// Assume the back of the queue will be where we insert it.
			int targetQueuePosition = this.Count;

			// Items with priority zero always get added to the end of the queue, as will anything if the
			// queue only has one item in it as we assume that item is integrating already and cannot be moved.
			if (insertingItemPriority != 0 && this.Count > 1)
			{
				for (int index = 1; index < this.Count; index++)
				{
					IIntegrationQueueItem queuedIntegrationQueueItem = this[index] as IIntegrationQueueItem;
					if (queuedIntegrationQueueItem != null)
					{
						int compareQueuePosition = queuedIntegrationQueueItem.Project.QueuePriority;
						// If two items have the same priority it will be FIFO order for that priority
						if (compareQueuePosition == 0 || compareQueuePosition > insertingItemPriority)
						{
							targetQueuePosition = index;
							break;
						}
					}
				}
			}
			return targetQueuePosition;
		}

		private void RemoveProjectItems(IProject project, bool considerFirstQueueItem)
		{
			// Note we are also potentially removing the item at index[0] as this method should
			// only be called when the thread performing the build has been stopped.
			int startQueueIndex = considerFirstQueueItem ? 0 : 1;
			for (int index = this.Count - 1; index >= startQueueIndex; index--)
			{
				IIntegrationQueueItem integrationQueueItem = (IIntegrationQueueItem) this[index];
				if (integrationQueueItem.Project.Equals(project))
				{
					Log.Info("Project: " + integrationQueueItem.Project.Name + " removed from queue: " + this.Name);
					bool isPendingItemCancelled = index > 0;
					this.NotifyExitingQueueAndRemoveItem(index, integrationQueueItem, isPendingItemCancelled);
				}
			}
		}

		private void NotifyExitingQueueAndRemoveItem(int index, IIntegrationQueueItem integrationQueueItem, bool isPendingItemCancelled)
		{
			integrationQueueItem.IntegrationQueueNotifier.NotifyExitingIntegrationQueue(isPendingItemCancelled);
            this.RemoveAt(index);
		}

        private IEnumerable<IIntegrationQueue> LockQueues
        {
            get
            {
                if (!string.IsNullOrEmpty(this.configuration.LockQueueNames) && this.parentQueueSet != null)
                {
                    string[] queues = this.configuration.LockQueueNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> actualQueues = new List<string>(this.parentQueueSet.GetQueueNames());

                    for (int i = 0; i < queues.Length; i++)
                    {
                        string queueToLock = queues[i].Trim();
                        if (actualQueues.Contains(queueToLock))
                            yield return this.parentQueueSet[queueToLock];
                        else
                            Log.Warning(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Unknown queue found: '{0}'", queueToLock));
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to acquire a lock on the queue to mark it as in-use.
        /// </summary>
        /// <param name="lockObject">If locking the queue for use was
        /// successful (returned true), lockObject is an IDisposable that
        /// will discard the lock when disposed.</param>
        /// <returns>True if the queue is now marked as in-use, false if the
        /// queue could not be marked as in-use due to being blocked (or
        /// one of its lockqueues was in-use).</returns>
        public bool TryLock(out IDisposable lockObject)
        {
            Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' is attempting to be in-use, trying to lock related queues", this.Name));

            lockObject = null;
            lock (blockingLockObject)
            {
                if (this.IsBlocked)
                {
                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' is locked and cannot be in-use", this.Name));
                    return false;
                }

                IList<IIntegrationQueue> lockedQueues = new List<IIntegrationQueue>();
                bool failed = false;

                foreach (IIntegrationQueue queue in this.LockQueues)
                {
                    if (queue.BlockQueue(this))
                    {
                        Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' has acquired a lock against queue '{1}'", this.Name, queue.Name));
                        lockedQueues.Add(queue);
                    }
                    else
                    {
                        Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' has FAILED to acquire a lock against queue '{1}'", this.Name, queue.Name));
                        failed = true;
                        break;
                    }
                }

                if (failed)
                {
                    foreach (IIntegrationQueue queue in lockedQueues)
                    {
                        Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' has released a lock against queue '{1}'", this.Name, queue.Name));
                        queue.UnblockQueue(this);
                        return false;
                    }
                }

                lockObject = new LockHolder(this, lockedQueues);
                this.inUse = true;
                return true;
            }
        }

        private sealed class LockHolder : IDisposable
        {
            private IntegrationQueue lockingQueue;
            private IList<IIntegrationQueue> lockedQueues;

            public LockHolder(IntegrationQueue lockingQueue, IList<IIntegrationQueue> lockedQueues)
            {
                this.lockingQueue = lockingQueue;
                this.lockedQueues = lockedQueues;
            }

            public void Dispose()
            {
                foreach (IIntegrationQueue queue in this.lockedQueues)
                {
                    Log.Info(string.Format(System.Globalization.CultureInfo.CurrentCulture,"Queue: '{0}' has released a lock against queue '{1}'", this.lockingQueue.Name, queue.Name));
                    queue.UnblockQueue(this.lockingQueue);
                }
                this.lockingQueue.inUse = false;
            }
        }


        /// <summary>
        /// Lock this queue, based upon a request from another queue.
        /// Acquires a fresh lock for the queue making the request (assuming none exists).
        /// </summary>
        /// <param name="requestingQueue">Queue requesting that a lock be taken out</param>
        public bool BlockQueue(IIntegrationQueue requestingQueue)
        {
            if (this.inUse)
                return false;

            lock (queueLockSync)
            {
                if (!this.blockingQueueNames.Contains(requestingQueue.Name))
                {
                    this.blockingQueueNames.Add(requestingQueue.Name);
                }
            }

            return true;
        }

        /// <summary>
        /// Unlock this queue, based upon a request from another queue.
        /// Releases any locks currently held by the queue making the request.
        /// </summary>
        /// <param name="requestingQueue">Queue requesting that a lock be released</param>
        public void UnblockQueue(IIntegrationQueue requestingQueue)
        {
            lock (queueLockSync)
            {
                this.blockingQueueNames.Remove(requestingQueue.Name);
            }
        }
    }
}
