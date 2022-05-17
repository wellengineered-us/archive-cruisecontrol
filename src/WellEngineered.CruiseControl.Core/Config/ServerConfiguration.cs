using System.Collections.Generic;

using WellEngineered.CruiseControl.Remote;

namespace WellEngineered.CruiseControl.Core.Config
{
    /// <summary>
    /// The configuration options for the server.
    /// </summary>
    public class ServerConfiguration
    {
        private List<ExtensionConfiguration> extensions = new List<ExtensionConfiguration>();

        /// <summary>
        /// The extensions to load.
        /// </summary>
        public List<ExtensionConfiguration> Extensions
        {
            get { return this.extensions; }
        }
    }
}
