using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace WellEngineered.CruiseControl.Core.Util
{
    /// <summary>
    /// 	
    /// </summary>
	public class FileChangedWatcher : IFileWatcher
	{
		private List<FileSystemWatcher> watchers= new List< FileSystemWatcher >( );
		private Timer timer;
        /// <summary>
        ///  Event args of first event to fire (filesystem watcher reports
        /// multiple events on a single save)
        /// </summary>
	    private FileSystemEventArgs firstArgs = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangedWatcher" /> class.	
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <remarks></remarks>
		public FileChangedWatcher(params string[] filenames)
		{
            for( int idx = 0; idx < filenames.Length; ++idx )
            {
                this.AddWatcher( filenames[ idx ] );
            }
            this.timer = new Timer(500);
			this.timer.AutoReset = false;
			this.timer.Elapsed += this.HandleTimerElapsed;
		}

        /// <summary>
        /// Occurs when [on file changed].	
        /// </summary>
        /// <remarks></remarks>
	    public event FileSystemEventHandler OnFileChanged;

        /// <summary>
        /// Adds the watcher.	
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <remarks></remarks>
	    public void AddWatcher (string filename)
	    {
	        FileSystemWatcher watcher = new FileSystemWatcher();
            this.watchers.Add( watcher );
	        watcher.Filter = Path.GetFileName( filename );
	        watcher.Path = new FileInfo(filename).DirectoryName;
	    	watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes |
	    	                       NotifyFilters.Size | NotifyFilters.LastWrite;
	        watcher.Changed += this.HandleFileChanged;
	        watcher.Renamed += this.HandleFileChanged;
	        watcher.EnableRaisingEvents = true;
			Log.Debug(string.Concat("[FileChangedWatcher] Add config file '", filename, "' to file change watcher collection."));
	    }

	    private void HandleFileChanged(object sender, FileSystemEventArgs args)
		{
	        this.firstArgs = this.firstArgs ?? args;            
			this.timer.Start();
		}

		private void HandleFileChanged(object sender, RenamedEventArgs args)
		{
            this.firstArgs = this.firstArgs ?? args;            
			this.timer.Start();
		}

		private void HandleTimerElapsed(object sender, ElapsedEventArgs args)
		{
			this.timer.Stop();
            Log.Info( "Config file modification detected for  " + this.firstArgs.FullPath );            
			this.OnFileChanged(sender, this.firstArgs);
		    this.firstArgs = null;
		}

		void IDisposable.Dispose()
		{
		    foreach ( FileSystemWatcher watcher in this.watchers )
		    {
                watcher.EnableRaisingEvents = false;
			    watcher.Dispose();		        
		    }			
		}
	}
}