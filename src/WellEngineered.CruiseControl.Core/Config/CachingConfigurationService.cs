namespace WellEngineered.CruiseControl.Core.Config
{
    /// <summary>
    /// 	
    /// </summary>
	public class CachingConfigurationService : IConfigurationService
	{
		private readonly IConfigurationService slaveService;
		private IConfiguration cachedConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingConfigurationService" /> class.	
        /// </summary>
        /// <param name="slaveService">The slave service.</param>
        /// <remarks></remarks>
		public CachingConfigurationService(IConfigurationService slaveService)
		{
			this.slaveService = slaveService;
			this.slaveService.AddConfigurationUpdateHandler(new ConfigurationUpdateHandler(this.InvalidateCache));
		}

        /// <summary>
        /// Loads this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
		public IConfiguration Load()
		{
			if (this.cachedConfig == null)
			{
				this.cachedConfig = this.slaveService.Load();
			}
			return this.cachedConfig;
		}

        /// <summary>
        /// Saves the specified configuration.	
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks></remarks>
		public void Save(IConfiguration configuration)
		{
			this.slaveService.Save(configuration);
		}

        /// <summary>
        /// Adds the configuration update handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
		public void AddConfigurationUpdateHandler(ConfigurationUpdateHandler handler)
		{
			this.slaveService.AddConfigurationUpdateHandler(handler);
		}

        /// <summary>
        /// Adds the configuration subfile loaded handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
	    public void AddConfigurationSubfileLoadedHandler (
	        ConfigurationSubfileLoadedHandler handler)
	    {
	        this.slaveService.AddConfigurationSubfileLoadedHandler( handler );
	    }

        /// <summary>
        /// Invalidates the cache.	
        /// </summary>
        /// <remarks></remarks>
	    public void InvalidateCache()
		{
			this.cachedConfig = null;
		}
	}
}