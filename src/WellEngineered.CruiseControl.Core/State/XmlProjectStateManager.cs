using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core.State
{
    /// <summary>
    /// Records the state of a project.
    /// </summary>
    public class XmlProjectStateManager
        : IProjectStateManager
    {
        #region Fields
		private readonly IFileSystem fileSystem;
		private readonly IExecutionEnvironment executionEnvironment;

        private readonly string persistanceFileName;
        private Dictionary<string, bool> projectStates = null;
        private bool isLoading;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="XmlProjectStateManager"/> with the default path.
        /// </summary>
        public XmlProjectStateManager()
			: this(new SystemIoFileSystem(), new ExecutionEnvironment())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlProjectStateManager" /> class.	
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="executionEnvironment">The execution environment.</param>
        /// <remarks></remarks>
		public XmlProjectStateManager(IFileSystem fileSystem, IExecutionEnvironment executionEnvironment)
		{
			this.fileSystem = fileSystem;
			this.executionEnvironment = executionEnvironment;

			this.persistanceFileName = Path.Combine(this.executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server), "ProjectsState.xml");
			fileSystem.EnsureFolderExists(this.persistanceFileName);
		}
        #endregion

        /// <summary>
        /// Gets the name of the persistance file.	
        /// </summary>
        /// <value>The name of the persistance file.</value>
        /// <remarks></remarks>
		public string PersistanceFileName
		{
			get { return this.persistanceFileName; }
		}

        #region Public methods
        #region RecordProjectAsStopped()
        /// <summary>
        /// Records a project as stopped.
        /// </summary>
        /// <param name="projectName">The name of the project to record</param>
        public void RecordProjectAsStopped(string projectName)
        {
            this.ChangeProjectState(projectName, false);
        }
        #endregion

        #region RecordProjectAsStartable()
        /// <summary>
        /// Records a project as being able to start automatically.
        /// </summary>
        /// <param name="projectName">The name of the project to record</param>
        public void RecordProjectAsStartable(string projectName)
        {
            this.ChangeProjectState(projectName, true);
        }
        #endregion

        #region CheckIfProjectCanStart()
        /// <summary>
        /// Checks if a project can be started automatically.
        /// </summary>
        /// <param name="projectName">The name of the project to check.</param>
        /// <returns></returns>
        public bool CheckIfProjectCanStart(string projectName)
        {
            this.LoadProjectStates(false);
            if (this.projectStates.ContainsKey(projectName))
            {
                return this.projectStates[projectName];
            }
            else
            {
                return true;
            }
        }
        #endregion
        #endregion

        #region Private methods
        #region LoadProjectStates()
        /// <summary>
        /// Loads all the states from a persistance file.
        /// </summary>
        /// <param name="forceLoad"></param>
        private void LoadProjectStates(bool forceLoad)
        {
            if (forceLoad || (this.projectStates == null))
            {
                this.projectStates = new Dictionary<string, bool>();

                if (this.fileSystem.FileExists(this.persistanceFileName))
                {
                    this.isLoading = true;
                    try
                    {
                        var stateDocument = new XmlDocument();
                        using (var stream = this.fileSystem.OpenInputStream(this.persistanceFileName))
                        {
                            stateDocument.Load(stream);
                        }
                        foreach (XmlElement projectState in stateDocument.SelectNodes("/state/project"))
                        {
                            this.ChangeProjectState(projectState.InnerText, false);
                        }
                    }
                    finally
                    {
                        this.isLoading = false;
                    }
                }
            }
        }
        #endregion

        #region SaveProjectStates()
        /// <summary>
        /// Saves all the states to a persistance file.
        /// </summary>
        private void SaveProjectStates()
        {
            if (this.projectStates != null)
            {
                var stateDocument = new XmlDocument();
                var rootElement = stateDocument.CreateElement("state");
                stateDocument.AppendChild(rootElement);
                foreach (var projectName in this.projectStates.Keys)
                {
                    if (!this.projectStates[projectName])
                    {
                        var projectElement = stateDocument.CreateElement("project");
                        projectElement.InnerText = projectName;
                        rootElement.AppendChild(projectElement);
                    }
                }
                using (var stream = this.fileSystem.OpenOutputStream(this.persistanceFileName))
                {
                    var settings = new XmlWriterSettings
                    {
                        Encoding = Encoding.UTF8,
                        OmitXmlDeclaration = true,
                        Indent = false
                    };
                    using (var xmlWriter = XmlTextWriter.Create(stream, settings))
                    {
                        stateDocument.Save(xmlWriter);
                    }
                }
            }
        }
        #endregion

        #region ChangeProjectState()
        /// <summary>
        /// See if we need to change the state and if so, change it, then persist the states
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="newState"></param>
        private void ChangeProjectState(string projectName, bool newState)
        {
            this.LoadProjectStates(false);
            var saveStates = true;
            if (this.projectStates.ContainsKey(projectName))
            {
                if (this.projectStates[projectName] != newState)
                {
                    this.projectStates[projectName] = newState;
                }
                else
                {
                    saveStates = false;
                }
            }
            else
            {
                this.projectStates.Add(projectName, newState);
            }
            if (saveStates && !this.isLoading) this.SaveProjectStates();
        }
        #endregion
        #endregion
    }
}
