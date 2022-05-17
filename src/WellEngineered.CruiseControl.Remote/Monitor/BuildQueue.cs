using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WellEngineered.CruiseControl.Remote.Monitor
{
    /// <summary>
    /// A build queue that is being monitored on a remote server.
    /// </summary>
    public class BuildQueue
        : INotifyPropertyChanged
    {
        #region Private fields
        private readonly CruiseServerClientBase client;
        private readonly Server server;
        private Dictionary<string, BuildQueueRequest> requests = new Dictionary<string, BuildQueueRequest>();
        private QueueSnapshot buildQueue;
        private Exception exception;
        private object syncLock = new object();
        private DataBag data = new DataBag();
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new build queue.
        /// </summary>
        /// <param name="client">The underlying client.</param>
        /// <param name="server">The server this queue belongs to.</param>
        /// <param name="buildQueue">The actual build queue details.</param>
        public BuildQueue(CruiseServerClientBase client,Server server, QueueSnapshot buildQueue)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (server == null) throw new ArgumentNullException("server");
            if (buildQueue == null) throw new ArgumentNullException("buildQueue");

            this.client = client;
            this.server = server;
            this.buildQueue = buildQueue;
        }
        #endregion

        #region Public properties
        #region Server
        /// <summary>
        /// The server this build queue belongs to.
        /// </summary>
        public Server Server
        {
            get { return this.server; }
        }
        #endregion

        #region Name
        /// <summary>
        /// The name of the build queue.
        /// </summary>
        public string Name
        {
            get { return this.InnerBuildQueue.QueueName; }
        }
        #endregion

        #region Requests
        /// <summary>
        /// Any current or pending requests.
        /// </summary>
        public IEnumerable<BuildQueueRequest> Requests
        {
            get
            {
                lock (this.syncLock) { return this.requests.Values; }
            }
        }
        #endregion

        #region Exception
        /// <summary>
        /// Any server exception details.
        /// </summary>
        public Exception Exception
        {
            get { return this.exception; }
            set
            {
                if (((value != null) && (this.exception != null) && (value.Message != this.exception.Message)) ||
                    ((value == null) && (this.exception != null)) ||
                    ((value != null) && (this.exception == null)))
                {
                    this.exception = value;
                    this.FirePropertyChanged("Exception");
                }
            }
        }
        #endregion

        #region Data
        /// <summary>
        /// Gets the data bag.
        /// </summary>
        public DataBag Data
        {
            get { return this.data; }
        }
        #endregion
        #endregion

        #region Public methods
        #region Update()
        /// <summary>
        /// Updates the details on a build queue.
        /// </summary>
        /// <param name="value">The new build queue details.</param>
        public void Update(QueueSnapshot value)
        {
            // Validate the arguments
            if (value == null) throw new ArgumentNullException("value");

            // Find all the changed properties
            var changes = new List<string>();
            var newRequests = new List<BuildQueueRequest>();
            var oldRequests = new List<BuildQueueRequest>();

            lock (this.syncLock)
            {
                // Check for any request differences
                var requestValues = new Dictionary<BuildQueueRequest, QueuedRequestSnapshot>();
                var oldRequestNames = new List<string>(this.requests.Keys);
                foreach (var request in value.Requests)
                {
                    // Check if this request has already been loaded
                    var requestName = request.ProjectName;
                    if (oldRequestNames.Contains(requestName))
                    {
                        requestValues.Add(this.requests[requestName], request);
                        oldRequestNames.Remove(requestName);
                    }
                    else
                    {
                        // Otherwise this is a new request
                        var newRequest = new BuildQueueRequest(this.client, this, request);
                        newRequests.Add(newRequest);
                    }
                }

                // Store the old request
                foreach (var request in oldRequestNames)
                {
                    oldRequests.Add(this.requests[request]);
                }

                // Perform the actual update
                foreach (var request in oldRequestNames)
                {
                    this.requests.Remove(request);
                }
                foreach (var request in newRequests)
                {
                    if (!this.requests.ContainsKey(request.Name))
                    {
                        this.requests.Add(request.Name, request);
                    }
                }
                this.buildQueue = value;

                // Update all the requests
                foreach (var requestValue in requestValues)
                {
                    requestValue.Key.Update(requestValue.Value);
                }
            }

            // Tell any listeners about any changes
            foreach (var request in newRequests)
            {
                this.FireBuildQueueRequestAdded(request);
            }
            foreach (var request in oldRequests)
            {
                this.FireBuildQueueRequestRemoved(request);
            }
            foreach (var change in changes)
            {
                this.FirePropertyChanged(change);
            }
        }
        #endregion

        #region Equals()
        /// <summary>
        /// Compare if two queues are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as BuildQueue);
        }

        /// <summary>
        /// Compare if two queues are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool Equals(BuildQueue obj)
        {
            if (obj == null) return false;
            return obj.Server.Equals(this.Server) &&
                (obj.Name == this.Name);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Return the hash code for this queue.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.Server == null ? 0 : this.Server.GetHashCode()) +
                (this.Name ?? string.Empty).GetHashCode();
        }
        #endregion
        #endregion

        #region Public events
        #region BuildQueueRequestAdded
        /// <summary>
        /// A new request has been added.
        /// </summary>
        public event EventHandler<BuildQueueRequestChangedArgs> BuildQueueRequestAdded;
        #endregion

        #region BuildQueueRequestRemoved
        /// <summary>
        /// An existing request has been removed.
        /// </summary>
        public event EventHandler<BuildQueueRequestChangedArgs> BuildQueueRequestRemoved;
        #endregion

        #region PropertyChanged
        /// <summary>
        /// A property has been changed on this queue.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #endregion

        #region Protected properties
        #region InnerBuildQueue
        /// <summary>
        /// The underlying build queue status.
        /// </summary>
        protected QueueSnapshot InnerBuildQueue
        {
            get { return this.buildQueue; }
        }
        #endregion
        #endregion

        #region Protected methods
        #region FireBuildQueueRequestAdded()
        /// <summary>
        /// Fires the <see cref="BuildQueueRequestAdded"/> event.
        /// </summary>
        /// <param name="request">The request that was added.</param>
        protected void FireBuildQueueRequestAdded(BuildQueueRequest request)
        {
            if (this.BuildQueueRequestAdded != null)
            {
                var args = new BuildQueueRequestChangedArgs(request);
                this.BuildQueueRequestAdded(this, args);
            }
        }
        #endregion

        #region FireBuildQueueRequestRemoved()
        /// <summary>
        /// Fires the <see cref="BuildQueueRequestRemoved"/> event.
        /// </summary>
        /// <param name="request">The request that was removed.</param>
        protected void FireBuildQueueRequestRemoved(BuildQueueRequest request)
        {
            if (this.BuildQueueRequestRemoved != null)
            {
                var args = new BuildQueueRequestChangedArgs(request);
                this.BuildQueueRequestRemoved(this, args);
            }
        }
        #endregion

        #region FirePropertyChanged()
        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has changed.</param>
        protected void FirePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, args);
            }
        }
        #endregion
        #endregion
    }
}
