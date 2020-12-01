using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Administration
{
    /// <summary>
    /// Defines a manifest for a plug-in package.
    /// </summary>
    [XmlRoot("package")]
    public class PackageManifest
        : IComparable
    {
        #region Private fields
        private string name;
        private string description;
        private string fileName;
        private bool isInstalled;
        private PackageType type;
        private List<FileLocation> fileLocations = new List<FileLocation>();
        private List<ConfigurationSetting> configSettings = new List<ConfigurationSetting>();
        private string group;
        #endregion

        #region Public properties
        #region Name
        /// <summary>
        /// The name of the package.
        /// </summary>
        [XmlElement("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        #endregion

        #region Description
        /// <summary>
        /// The description of the package.
        /// </summary>
        [XmlElement("description")]
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }
        #endregion

        #region Type
        /// <summary>
        /// The type of the package.
        /// </summary>
        [XmlElement("type")]
        public PackageType Type
        {
            get { return this.type; }
            set { this.type = value; }
        }
        #endregion

        #region Group
        /// <summary>
        /// The description of the package.
        /// </summary>
        [XmlElement("group")]
        public string Group
        {
            get { return this.@group; }
            set { this.@group = value; }
        }
        #endregion


        #region FileName
        /// <summary>
        /// The filename of the package.
        /// </summary>
        [XmlIgnore]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }
        #endregion

        #region IsInstalled
        /// <summary>
        /// Whether this package has been installed or not.
        /// </summary>
        [XmlIgnore]
        public bool IsInstalled
        {
            get { return this.isInstalled; }
            set { this.isInstalled = value; }
        }
        #endregion

        #region FileLocations
        /// <summary>
        /// The files to export and their locations.
        /// </summary>
        [XmlArray("folders")]
        [XmlArrayItem("folder")]
        public List<FileLocation> FileLocations
        {
            get { return this.fileLocations; }
        }
        #endregion

        #region ConfigurationSettings
        /// <summary>
        /// The configuration settings to apply.
        /// </summary>
        [XmlArray("configuration")]
        [XmlArrayItem("setting")]
        public List<ConfigurationSetting> ConfigurationSettings
        {
            get { return this.configSettings; }
        }
        #endregion
        #endregion

        #region Public methods
        #region ToString()
        /// <summary>
        /// Returns the name of the package.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.name;
        }
        #endregion

        #region CompareTo()
        /// <summary>
        /// Compare this to another package.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is PackageManifest)
            {
                return string.Compare(this.name, (obj as PackageManifest).name);
            }
            else
            {
                return 0;
            }
        }
        #endregion
        #endregion
    }
}
