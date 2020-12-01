﻿using System;
using System.Globalization;

namespace WellEngineered.CruiseControl.Remote.Monitor
{
    /// <summary>
    /// Details on a build for a project.
    /// </summary>
    public class ProjectBuild
    {
        #region Private fields
        private readonly CruiseServerClientBase client;
        private readonly Project project;
        private readonly string name;
        private string log;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new project build entity.
        /// </summary>
        /// <param name="buildName">The name of the build.</param>
        /// <param name="project">The project this build belongs to.</param>
        /// <param name="client">The underlying client.</param>
        public ProjectBuild(string buildName, Project project, CruiseServerClientBase client)
        {
            this.name = buildName;
            this.project = project;
            this.client = client;

            // Parse the name for the details
            this.BuildDate = DateTime.ParseExact(this.name.Substring(3, 14), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            this.IsSuccessful = (this.name.Substring(17, 1) == "L");
            if (this.IsSuccessful)
            {
                var startPos = this.name.IndexOf("build.", StringComparison.OrdinalIgnoreCase) + 6;
                var endPos = this.name.LastIndexOf('.');
                this.Label = this.name.Substring(startPos, endPos - startPos);
            }
        }
        #endregion

        #region Public properties
        #region Name
        /// <summary>
        /// The name of the build.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }
        #endregion

        #region Log
        /// <summary>
        /// The log for the build.
        /// </summary>
        /// <remarks>
        /// This property uses lazy loading to retrieve the log from the server.
        /// </remarks>
        public string Log
        {
            get
            {
                if (string.IsNullOrEmpty(this.log))
                {
                    this.client.ProcessSingleAction<object>(o =>
                    {
                        this.log = this.client.GetLog(this.project.Name, this.name);
                    }, null);
                }
                return this.log;
            }
        }
        #endregion

        #region BuildDate
        /// <summary>
        /// The date and time of the build.
        /// </summary>
        public DateTime BuildDate { get; private set; }
        #endregion

        #region Label
        /// <summary>
        /// The label of the build.
        /// </summary>
        public string Label { get; private set; }
        #endregion

        #region IsSuccessful
        /// <summary>
        /// Was the build successful or not.
        /// </summary>
        public bool IsSuccessful { get; private set; }
        #endregion
        #endregion
    }
}
