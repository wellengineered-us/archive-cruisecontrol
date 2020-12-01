using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote
{
	/// <summary>
    /// Details on a package.
    /// </summary>
    [XmlRoot("package")]
    [Serializable]
    public class PackageDetails
    {
        #region Private fields
        private string name;
        private string buildLabel;
        private DateTime dateTime;
        private int numberOfFiles;
        private long size;
        private string fileName;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new blank <see cref="PackageDetails"/>.
        /// </summary>
        public PackageDetails()
        {
        }

        /// <summary>
        /// Initialise a new <see cref="PackageDetails"/> with a package.
        /// </summary>
        /// <param name="package">The location of the package.</param>
        public PackageDetails(string package)
        {
            this.fileName = package;
        }
        #endregion

        #region Public properties
        #region Name
        /// <summary>
        /// The name of the package.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        #endregion

        #region BuildLabel
        /// <summary>
        /// The label of the build this package is for.
        /// </summary>
        [XmlElement("buildLabel")]
        public string BuildLabel
        {
            get { return this.buildLabel; }
            set { this.buildLabel = value; }
        }
        #endregion

        #region DateTime
        /// <summary>
        /// The date and time the package was generated.
        /// </summary>
        [XmlAttribute("dateTime")]
        public DateTime DateTime
        {
            get { return this.dateTime; }
            set { this.dateTime = value; }
        }
        #endregion

        #region NumberOfFiles
        /// <summary>
        /// The number of files in the package.
        /// </summary>
        [XmlAttribute("numberOfFiles")]
        public int NumberOfFiles
        {
            get { return this.numberOfFiles; }
            set { this.numberOfFiles = value; }
        }
        #endregion

        #region Size
        /// <summary>
        /// The size of the package.
        /// </summary>
        [XmlAttribute("size")]
        public long Size
        {
            get { return this.size; }
            set { this.size = value; }
        }
        #endregion

        #region FileName
        /// <summary>
        /// The actual name of the file on the server.
        /// </summary>
        [XmlElement("fileName")]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }
        #endregion
        #endregion
    }
}
