using System;
using System.Xml.Serialization;

namespace WellEngineered.CruiseControl.WebDashboard.Plugins.Administration
{
    /// <summary>
    /// Defines a configuration attribute.
    /// </summary>
    public class ConfigurationAttribute
    {
        #region Private fields
        private string name;
        private string value;
        #endregion

        #region Public properties
        #region Name
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        [XmlAttribute("name")]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        #endregion

        #region Value
        /// <summary>
        /// The value of the setting.
        /// </summary>
        [XmlAttribute("value")]
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion
        #endregion
    }
}
