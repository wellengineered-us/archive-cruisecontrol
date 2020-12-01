using System.IO;

namespace WellEngineered.CruiseControl.Core.Config
{
    /// <summary>
    /// 	
    /// </summary>
	public class FileConfigurationService : IConfigurationService
	{
		private readonly FileInfo configFile;
		private readonly IConfigurationFileSaver saver;
		private readonly IConfigurationFileLoader loader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileConfigurationService" /> class.	
        /// </summary>
        /// <param name="loader">The loader.</param>
        /// <param name="saver">The saver.</param>
        /// <param name="configFile">The config file.</param>
        /// <remarks></remarks>
		public FileConfigurationService(IConfigurationFileLoader loader, IConfigurationFileSaver saver, FileInfo configFile)
		{
			this.loader = loader;
			this.saver = saver;
			this.configFile = configFile;            
		}

        /// <summary>
        /// Loads this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public IConfiguration Load()
		{
			lock (this.configFile)
			{
				return this.loader.Load(this.configFile);
			}
		}

        /// <summary>
        /// Saves the specified configuration.	
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks></remarks>
		public void Save(IConfiguration configuration)
		{
			lock (this.configFile)
			{
				this.saver.Save(configuration, this.configFile);
			}
		}

        /// <summary>
        /// Adds the configuration update handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
		public void AddConfigurationUpdateHandler(ConfigurationUpdateHandler handler)
		{}

        /// <summary>
        /// Adds the configuration subfile loaded handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
	    public void AddConfigurationSubfileLoadedHandler (
	        ConfigurationSubfileLoadedHandler handler)
	    {
	        this.loader.AddSubfileLoadedHandler( handler );
	    }
	}
}