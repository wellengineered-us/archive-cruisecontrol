using System;
using System.IO;

using WellEngineered.CruiseControl.Core.Util;

namespace WellEngineered.CruiseControl.Core.Config
{
	// ToDo - make disposable?  need to ensure that the filewatcher is disposed.  should be done in container.
    /// <summary>
    /// 	
    /// </summary>
	public class FileWatcherConfigurationService : IConfigurationService
	{
		private readonly IConfigurationService decoratedService;
		private readonly IFileWatcher fileWatcher;
		private ConfigurationUpdateHandler updateHandler;
	    private ConfigurationSubfileLoadedHandler subfileHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWatcherConfigurationService" /> class.	
        /// </summary>
        /// <param name="decoratedService">The decorated service.</param>
        /// <param name="fileWatcher">The file watcher.</param>
        /// <remarks></remarks>
		public FileWatcherConfigurationService(IConfigurationService decoratedService, IFileWatcher fileWatcher)
		{
			this.decoratedService = decoratedService;
			this.fileWatcher = fileWatcher;
			this.fileWatcher.OnFileChanged += new FileSystemEventHandler(this.HandleConfigurationFileChanged);
		    decoratedService.AddConfigurationSubfileLoadedHandler( this.SubfileLoaded );
		}

        /// <summary>
        /// Loads this instance.	
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
	    public IConfiguration Load()
		{
			return this.decoratedService.Load();
		}

	    private void SubfileLoaded (Uri uri)
	    {
	        this.fileWatcher.AddWatcher( uri.LocalPath );
	    }

        /// <summary>
        /// Saves the specified configuration.	
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks></remarks>
	    public void Save(IConfiguration configuration)
		{
			this.decoratedService.Save(configuration);
		}

        /// <summary>
        /// Adds the configuration update handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
		public void AddConfigurationUpdateHandler(ConfigurationUpdateHandler handler)
		{
			this.updateHandler += handler;
		}

        /// <summary>
        /// Adds the configuration subfile loaded handler.	
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <remarks></remarks>
	    public void AddConfigurationSubfileLoadedHandler (
	        ConfigurationSubfileLoadedHandler handler)
	    {
	        this.subfileHandler += handler;	        
	    }

	    private void HandleConfigurationFileChanged(object source, FileSystemEventArgs args)
		{
			try
			{
				lock (this)
				{
					if (this.updateHandler != null)
					{
						this.updateHandler();
					}
				}
			}
			catch (Exception ex) 
			{
				Log.Error(ex);
			}
		}	
	}
}
