using System;
using System.Collections.Generic;
using System.ComponentModel;

using WellEngineered.CruiseControl.Remote.Parameters;

namespace WellEngineered.CruiseControl.Remote.Monitor
{
    /// <summary>
    /// A project that is being monitored on a remote server.
    /// </summary>
    public class Project
        : INotifyPropertyChanged, IEquatable<Project>
    {
        #region Private fields
        private readonly CruiseServerClientBase client;
        private readonly Server server;
        private ProjectStatus project;
        private Exception exception;
        private Dictionary<string, ProjectBuild> builds = new Dictionary<string, ProjectBuild>();
        private bool buildsLoaded;
        private object lockObject = new object();
        private object snapshotLock = new object();
        private ProjectStatusSnapshot statusSnapshot;
        private DataBag data = new DataBag();
        private IEnumerable<ParameterBase> parameters;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new project.
        /// </summary>
        /// <param name="client">The underlying client.</param>
        /// <param name="server">The server this project belongs to.</param>
        /// <param name="project">The actual project details.</param>
        public Project(CruiseServerClientBase client,Server server,  ProjectStatus project)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (server == null) throw new ArgumentNullException("server");
            if (project == null) throw new ArgumentNullException("project");

            this.client = client;
            this.server = server;
            this.project = project;
        }
        #endregion

        #region Public properties
        #region Server
        /// <summary>
        /// The server this project belongs to.
        /// </summary>
        public Server Server
        {
            get { return this.server; }
        }
        #endregion

        #region Name
        /// <summary>
        /// The name of the project.
        /// </summary>
        public string Name
        {
            get { return this.InnerProject.Name; }
        }
        #endregion

        #region BuildStage
        /// <summary>
        /// The current build stage.
        /// </summary>
        public string BuildStage
        {
            get { return this.InnerProject.BuildStage; }
        }
        #endregion

        #region Status
        /// <summary>
        /// The current project status.
        /// </summary>
        public ProjectIntegratorState Status
        {
            get { return this.InnerProject.Status; }
        }
        #endregion

        #region BuildStatus
        /// <summary>
        /// The current build status.
        /// </summary>
        public IntegrationStatus BuildStatus
        {
            get { return this.InnerProject.BuildStatus; }
        }
        #endregion

        #region Activity
        /// <summary>
        /// The current activity.
        /// </summary>
        public ProjectActivity Activity
        {
            get { return this.InnerProject.Activity; }
        }
        #endregion

        #region Description
        /// <summary>
        /// The description of the project.
        /// </summary>
        public string Description
        {
            get { return this.InnerProject.Description; }
        }
        #endregion

        #region Category
        /// <summary>
        /// The project category.
        /// </summary>
        public string Category
        {
            get { return this.InnerProject.Category; }
        }
        #endregion

        #region BuildQueue
        /// <summary>
        /// The build queue this project belongs to.
        /// </summary>
        public BuildQueue BuildQueue
        {
            get { return this.server.FindBuildQueue(this.InnerProject.Queue); }
        }
        #endregion

        #region Queue
        /// <summary>
        /// The name of the queue this project belongs to.
        /// </summary>
        public string Queue
        {
            get { return this.InnerProject.Queue; }
        }
        #endregion

        #region QueuePriority
        /// <summary>
        /// The priority of the project within the queue.
        /// </summary>
        public int QueuePriority
        {
            get { return this.InnerProject.QueuePriority; }
        }
        #endregion

        #region WebURL
        /// <summary>
        /// The URL for the project.
        /// </summary>
        public string WebURL
        {
            get { return this.InnerProject.WebURL; }
        }
        #endregion

        #region LastBuildDate
        /// <summary>
        /// The date and time the project was last built.
        /// </summary>
        public DateTime LastBuildDate
        {
            get { return this.InnerProject.LastBuildDate; }
        }
        #endregion

        #region LastBuildLabel
        /// <summary>
        /// The last build label (independent of the outcome of the build).
        /// </summary>
        public string LastBuildLabel
        {
            get { return this.InnerProject.LastBuildLabel; }
        }
        #endregion

        #region LastSuccessfulBuildLabel
        /// <summary>
        /// The last successful build label.
        /// </summary>
        public string LastSuccessfulBuildLabel
        {
            get { return this.InnerProject.LastSuccessfulBuildLabel; }
        }
        #endregion

        #region NextBuildTime
        /// <summary>
        /// The date and time of the next build check.
        /// </summary>
        public DateTime NextBuildTime
        {
            get { return this.InnerProject.NextBuildTime; }
        }
        #endregion

        #region Messages
        /// <summary>
        /// Any associated messages for the project.
        /// </summary>
        public IEnumerable<Message> Messages
        {
            get { return this.InnerProject.Messages; }
        }
        #endregion

        #region Builds
        /// <summary>
        /// The builds for this project.
        /// </summary>
        public IEnumerable<ProjectBuild> Builds
        {
            get
            {
                if (!this.buildsLoaded) this.LoadBuilds(this.InnerProject);
                return this.builds.Values;
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
        /// Updates the details on a project.
        /// </summary>
        /// <param name="value">The new project details.</param>
        public void Update(ProjectStatus value)
        {
            // Validate the arguments
            if (value == null) throw new ArgumentNullException("value");

            // Find all the changed properties
            var changes = new List<string>();
            if (this.project.Activity != value.Activity) changes.Add("Activity");
            if (this.project.BuildStage != value.BuildStage) changes.Add("BuildStage");
            if (this.project.BuildStatus != value.BuildStatus) changes.Add("BuildStatus");
            if (this.project.Category != value.Category) changes.Add("Category");
            if (this.project.Description != value.Description) changes.Add("Description");
            if (this.project.LastBuildDate != value.LastBuildDate) changes.Add("LastBuildDate");
            if (this.project.LastBuildLabel != value.LastBuildLabel) changes.Add("LastBuildLabel");
            if (this.project.LastSuccessfulBuildLabel != value.LastSuccessfulBuildLabel) changes.Add("LastSuccessfulBuildLabel");
            if (this.project.NextBuildTime != value.NextBuildTime) changes.Add("NextBuildTime");
            if (this.project.Queue != value.Queue) changes.Add("Queue");
            if (this.project.QueuePriority != value.QueuePriority) changes.Add("QueuePriority");
            if (this.project.Status != value.Status) changes.Add("Status");
            if (this.project.WebURL != value.WebURL) changes.Add("WebURL");
            if (this.project.Messages.Length != value.Messages.Length)
            {
                changes.Add("Messages");
            }
            else
            {
                var messageChanged = false;
                for (var loop = 0; loop < this.project.Messages.Length; loop++)
                {
                    messageChanged = (this.project.Messages[loop].Text != value.Messages[loop].Text);
                    if (messageChanged) break;
                }
                if (messageChanged) changes.Add("Messages");
            }

            // Update the builds
            this.LoadBuilds(value);

            // Make the actual change
            this.project = value;
            this.statusSnapshot = null;

            // Fire any change notifications
            foreach (var change in changes)
            {
                this.FirePropertyChanged(change);
            }
            this.FireUpdated();
        }
        #endregion

        #region ForceBuild()
        /// <summary>
        /// Sends a force build request to the remote server.
        /// </summary>
        public void ForceBuild()
        {
            this.client.ProcessSingleAction(p =>
            {
                this.client.ForceBuild(p.Name);
            }, this.InnerProject);
        }

        /// <summary>
        /// Sends a force build request to the remote server.
        /// </summary>
        /// <param name="parameters">The parameters for the build.</param>
        public void ForceBuild(List<NameValuePair> parameters)
        {
            this.client.ProcessSingleAction(p =>
            {
                this.client.ForceBuild(p.Name, parameters);
            }, this.InnerProject);
        }
        #endregion

        #region AbortBuild()
        /// <summary>
        /// Sends an abort build request to the remote server.
        /// </summary>
        public void AbortBuild()
        {
            this.client.ProcessSingleAction(p =>
            {
                this.client.AbortBuild(p.Name);
            }, this.InnerProject);
        }
        #endregion

        #region Start()
        /// <summary>
        /// Sends a start project request to the remote server.
        /// </summary>
        public void Start()
        {
            this.client.ProcessSingleAction(p =>
            {
                this.client.StartProject(p.Name);
            }, this.InnerProject);
        }
        #endregion

        #region Stop()
        /// <summary>
        /// Sends a stop project request to the remote server.
        /// </summary>
        public void Stop()
        {
            this.client.ProcessSingleAction(p =>
            {
                this.client.StopProject(p.Name);
            }, this.InnerProject);
        }
        #endregion

        #region RetrieveCurrentStatus()
        /// <summary>
        /// Retrieves the current snapshot of the status.
        /// </summary>
        /// <returns>The current status snapshot.</returns>
        public ProjectStatusSnapshot RetrieveCurrentStatus()
        {
            if (this.statusSnapshot == null)
            {
                lock (this.snapshotLock)
                {
                    if (this.statusSnapshot == null)
                    {
                        this.client.ProcessSingleAction(p =>
                        {
                            this.statusSnapshot = this.client.TakeStatusSnapshot(p.Name);
                        }, this.InnerProject);
                    }
                }
            }
            return this.statusSnapshot;
        }
        #endregion

        #region RetrieveParameters()
        /// <summary>
        /// Retrieve the parameters for this project.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterBase> RetrieveParameters()
        {
            if (this.parameters == null)
            {
                this.parameters = this.client.ListBuildParameters(this.project.Name);
            }
            return this.parameters;
        }
        #endregion

        #region Equals()
        /// <summary>
        /// Compare if two projects are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Project);
        }

        /// <summary>
        /// Compare if two projects are the same.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual bool Equals(Project obj)
        {
            if (obj == null) return false;
            return obj.Server.Equals(this.Server) &&
                (obj.Name == this.Name);
        }
        #endregion

        #region GetHashCode()
        /// <summary>
        /// Return the hash code for this project.
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
        #region PropertyChanged
        /// <summary>
        /// A property has been changed on this project.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Updated
        /// <summary>
        /// The project has been updated.
        /// </summary>
        /// <remarks>
        /// <see cref="PropertyChanged"/> will be fired when any properties change, this event will be fired whether or
        /// not any properties change.
        /// </remarks>
        public event EventHandler Updated;
        #endregion
        #endregion

        #region Protected properties
        #region InnerProject
        /// <summary>
        /// The underlying project status.
        /// </summary>
        protected ProjectStatus InnerProject
        {
            get { return this.project; }
        }
        #endregion
        #endregion

        #region Protected methods
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

        #region FireUpdated()
        /// <summary>
        /// Fire the <see cref="Updated"/> event.
        /// </summary>
        protected void FireUpdated()
        {
            if (this.Updated != null)
            {
                this.Updated(this, EventArgs.Empty);
            }
        }
        #endregion

        #region LoadBuilds()
        /// <summary>
        /// Load the builds for the project.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void LoadBuilds(ProjectStatus value)
        {
            lock (this.lockObject)
            {
                this.buildsLoaded = true;
                if (this.builds.Count == 0)
                {
                    // This is the first load - so load all of the builds
                    string[] buildNames = { };
                    try
                    {
                        this.client.ProcessSingleAction(p =>
                        {
                            buildNames = this.client.GetBuildNames(p.Name);
                        }, this.InnerProject);
                    }
                    catch
                    { 
                        // Ignore any errors - just means that no builds will be loaded
                    }
                    foreach (var buildName in buildNames ?? new string[0])
                    {
                        this.builds.Add(buildName, new ProjectBuild(buildName, this, this.client));
                    }
                }
                else
                {
                    if (this.project.LastBuildDate != value.LastBuildDate)
                    {
                        // Last build date has changed, therefore there will be one or more builds to load
                        string[] buildNames = { };
                        try
                        {
                            this.client.ProcessSingleAction(p =>
                            {
                                // Cannot pass in a date, only a number, so guessing there will be no more then 
                                // 10 builds since the last build
                                buildNames = this.client.GetMostRecentBuildNames(p.Name, 10);
                            }, this.InnerProject);
                        }
                        catch
                        {
                            // Ignore any errors - just means that no builds will be loaded
                        }
                        foreach (var buildName in buildNames)
                        {
                            if (!this.builds.ContainsKey(buildName))
                            {
                                this.builds.Add(buildName, new ProjectBuild(buildName, this, this.client));
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
