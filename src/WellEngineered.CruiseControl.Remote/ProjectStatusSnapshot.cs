using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// Defines the status of a project.
    /// </summary>
    [Serializable]
    [XmlRoot("projectStatusSnapshot")]
    public class ProjectStatusSnapshot
        : ItemStatus
    {
        #region Private fields
        private DateTime timeOfSnapshot = DateTime.Now;
        #endregion

        #region Public properties
        #region TimeOfSnapshot
        /// <summary>
        /// The date and time this snapshot was taken.
        /// </summary>
        [XmlElement("timeOfSnapshot")]
        public DateTime TimeOfSnapshot
        {
            get { return this.timeOfSnapshot; }
            set { this.timeOfSnapshot = value; }
        }
        #endregion
        #endregion

        #region Public methods
        #region Clone()
        /// <summary>
        /// Generates a clone of this snapshot.
        /// </summary>
        /// <returns></returns>
        public new ProjectStatusSnapshot Clone()
        {
            ProjectStatusSnapshot clone = new ProjectStatusSnapshot();
            this.CopyTo(clone);
            return clone;
        }
        #endregion
        #endregion
    }
}
