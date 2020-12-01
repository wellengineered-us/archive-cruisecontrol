using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;

namespace WellEngineered.CruiseControl.WebDashboard.Configuration
{
    /// <summary>
    /// Defines the configuration for a stylesheet.
    /// </summary>
	[ReflectorType("stylesheet")]
    public class StylesheetConfiguration
    {
        private string location;
        private string name;
        private bool isDefault;

        /// <summary>
        /// The location of the stylesheet.
        /// </summary>
        [ReflectorProperty("location")]
        public string Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// The name of the stylesheet.
        /// </summary>
        [ReflectorProperty("name", Required = false)]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Whether this is the default stylesheet or not.
        /// </summary>
        [ReflectorProperty("default", Required = false)]
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.isDefault = value; }
        }
    }
}
