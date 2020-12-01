using System.IO;

namespace WellEngineered.CruiseControl.Core.Config
{
    /// <summary>
    /// 	
    /// </summary>
	public interface IConfigurationFileSaver
	{
        /// <summary>
        /// Saves the specified configuration.	
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="file">The file.</param>
        /// <remarks></remarks>
		void Save(IConfiguration configuration, FileInfo file);
	}
}
