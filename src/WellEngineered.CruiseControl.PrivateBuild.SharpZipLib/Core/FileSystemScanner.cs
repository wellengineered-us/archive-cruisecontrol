using System;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Core
{
	#region EventArgs
	/// <summary>
	/// Event arguments for scanning.
	/// </summary>
	public class ScanEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanEventArgs"/>
		/// </summary>
		/// <param name="name">The file or directory name.</param>
		public ScanEventArgs(string name)
		{
			this.name_ = name;
		}
		#endregion

		/// <summary>
		/// The file or directory name for this event.
		/// </summary>
		public string Name {
			get { return this.name_; }
		}

		/// <summary>
		/// Get set a value indicating if scanning should continue or not.
		/// </summary>
		public bool ContinueRunning {
			get { return this.continueRunning_; }
			set { this.continueRunning_ = value; }
		}

		#region Instance Fields
		string name_;
		bool continueRunning_ = true;
		#endregion
	}

	/// <summary>
	/// Event arguments during processing of a single file or directory.
	/// </summary>
	public class ProgressEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanEventArgs"/>
		/// </summary>
		/// <param name="name">The file or directory name if known.</param>
		/// <param name="processed">The number of bytes processed so far</param>
		/// <param name="target">The total number of bytes to process, 0 if not known</param>
		public ProgressEventArgs(string name, long processed, long target)
		{
			this.name_ = name;
			this.processed_ = processed;
			this.target_ = target;
		}
		#endregion

		/// <summary>
		/// The name for this event if known.
		/// </summary>
		public string Name {
			get { return this.name_; }
		}

		/// <summary>
		/// Get set a value indicating wether scanning should continue or not.
		/// </summary>
		public bool ContinueRunning {
			get { return this.continueRunning_; }
			set { this.continueRunning_ = value; }
		}

		/// <summary>
		/// Get a percentage representing how much of the <see cref="Target"></see> has been processed
		/// </summary>
		/// <value>0.0 to 100.0 percent; 0 if target is not known.</value>
		public float PercentComplete {
			get {
				float result;
				if (this.target_ <= 0) {
					result = 0;
				} else {
					result = ((float)this.processed_ / (float)this.target_) * 100.0f;
				}
				return result;
			}
		}

		/// <summary>
		/// The number of bytes processed so far
		/// </summary>
		public long Processed {
			get { return this.processed_; }
		}

		/// <summary>
		/// The number of bytes to process.
		/// </summary>
		/// <remarks>Target may be 0 or negative if the value isnt known.</remarks>
		public long Target {
			get { return this.target_; }
		}

		#region Instance Fields
		string name_;
		long processed_;
		long target_;
		bool continueRunning_ = true;
		#endregion
	}

	/// <summary>
	/// Event arguments for directories.
	/// </summary>
	public class DirectoryEventArgs : ScanEventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialize an instance of <see cref="DirectoryEventArgs"></see>.
		/// </summary>
		/// <param name="name">The name for this directory.</param>
		/// <param name="hasMatchingFiles">Flag value indicating if any matching files are contained in this directory.</param>
		public DirectoryEventArgs(string name, bool hasMatchingFiles)
			: base(name)
		{
			this.hasMatchingFiles_ = hasMatchingFiles;
		}
		#endregion

		/// <summary>
		/// Get a value indicating if the directory contains any matching files or not.
		/// </summary>
		public bool HasMatchingFiles {
			get { return this.hasMatchingFiles_; }
		}

		readonly

		#region Instance Fields
		bool hasMatchingFiles_;
		#endregion
	}

	/// <summary>
	/// Arguments passed when scan failures are detected.
	/// </summary>
	public class ScanFailureEventArgs : EventArgs
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="ScanFailureEventArgs"></see>
		/// </summary>
		/// <param name="name">The name to apply.</param>
		/// <param name="e">The exception to use.</param>
		public ScanFailureEventArgs(string name, Exception e)
		{
			this.name_ = name;
			this.exception_ = e;
			this.continueRunning_ = true;
		}
		#endregion

		/// <summary>
		/// The applicable name.
		/// </summary>
		public string Name {
			get { return this.name_; }
		}

		/// <summary>
		/// The applicable exception.
		/// </summary>
		public Exception Exception {
			get { return this.exception_; }
		}

		/// <summary>
		/// Get / set a value indicating wether scanning should continue.
		/// </summary>
		public bool ContinueRunning {
			get { return this.continueRunning_; }
			set { this.continueRunning_ = value; }
		}

		#region Instance Fields
		string name_;
		Exception exception_;
		bool continueRunning_;
		#endregion
	}

	#endregion

	#region Delegates
	/// <summary>
	/// Delegate invoked before starting to process a file.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void ProcessFileHandler(object sender, ScanEventArgs e);

	/// <summary>
	/// Delegate invoked during processing of a file or directory
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void ProgressHandler(object sender, ProgressEventArgs e);

	/// <summary>
	/// Delegate invoked when a file has been completely processed.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void CompletedFileHandler(object sender, ScanEventArgs e);

	/// <summary>
	/// Delegate invoked when a directory failure is detected.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void DirectoryFailureHandler(object sender, ScanFailureEventArgs e);

	/// <summary>
	/// Delegate invoked when a file failure is detected.
	/// </summary>
	/// <param name="sender">The source of the event</param>
	/// <param name="e">The event arguments.</param>
	public delegate void FileFailureHandler(object sender, ScanFailureEventArgs e);
	#endregion

	/// <summary>
	/// FileSystemScanner provides facilities scanning of files and directories.
	/// </summary>
	public class FileSystemScanner
	{
		#region Constructors
		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="filter">The <see cref="PathFilter">file filter</see> to apply when scanning.</param>
		public FileSystemScanner(string filter)
		{
			this.fileFilter_ = new PathFilter(filter);
		}

		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter"> directory filter</see> to apply.</param>
		public FileSystemScanner(string fileFilter, string directoryFilter)
		{
			this.fileFilter_ = new PathFilter(fileFilter);
			this.directoryFilter_ = new PathFilter(directoryFilter);
		}

		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The file <see cref="IScanFilter">filter</see> to apply.</param>
		public FileSystemScanner(IScanFilter fileFilter)
		{
			this.fileFilter_ = fileFilter;
		}

		/// <summary>
		/// Initialise a new instance of <see cref="FileSystemScanner"></see>
		/// </summary>
		/// <param name="fileFilter">The file <see cref="IScanFilter">filter</see>  to apply.</param>
		/// <param name="directoryFilter">The directory <see cref="IScanFilter">filter</see>  to apply.</param>
		public FileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
		{
			this.fileFilter_ = fileFilter;
			this.directoryFilter_ = directoryFilter;
		}
		#endregion

		#region Delegates
		/// <summary>
		/// Delegate to invoke when a directory is processed.
		/// </summary>
		public event EventHandler<DirectoryEventArgs> ProcessDirectory;

		/// <summary>
		/// Delegate to invoke when a file is processed.
		/// </summary>
		public ProcessFileHandler ProcessFile;

		/// <summary>
		/// Delegate to invoke when processing for a file has finished.
		/// </summary>
		public CompletedFileHandler CompletedFile;

		/// <summary>
		/// Delegate to invoke when a directory failure is detected.
		/// </summary>
		public DirectoryFailureHandler DirectoryFailure;

		/// <summary>
		/// Delegate to invoke when a file failure is detected.
		/// </summary>
		public FileFailureHandler FileFailure;
		#endregion

		/// <summary>
		/// Raise the DirectoryFailure event.
		/// </summary>
		/// <param name="directory">The directory name.</param>
		/// <param name="e">The exception detected.</param>
		bool OnDirectoryFailure(string directory, Exception e)
		{
			DirectoryFailureHandler handler = this.DirectoryFailure;
			bool result = (handler != null);
			if (result) {
				var args = new ScanFailureEventArgs(directory, e);
				handler(this, args);
				this.alive_ = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Raise the FileFailure event.
		/// </summary>
		/// <param name="file">The file name.</param>
		/// <param name="e">The exception detected.</param>
		bool OnFileFailure(string file, Exception e)
		{
			FileFailureHandler handler = this.FileFailure;

			bool result = (handler != null);

			if (result) {
				var args = new ScanFailureEventArgs(file, e);
				this.FileFailure(this, args);
				this.alive_ = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Raise the ProcessFile event.
		/// </summary>
		/// <param name="file">The file name.</param>
		void OnProcessFile(string file)
		{
			ProcessFileHandler handler = this.ProcessFile;

			if (handler != null) {
				var args = new ScanEventArgs(file);
				handler(this, args);
				this.alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Raise the complete file event
		/// </summary>
		/// <param name="file">The file name</param>
		void OnCompleteFile(string file)
		{
			CompletedFileHandler handler = this.CompletedFile;

			if (handler != null) {
				var args = new ScanEventArgs(file);
				handler(this, args);
				this.alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Raise the ProcessDirectory event.
		/// </summary>
		/// <param name="directory">The directory name.</param>
		/// <param name="hasMatchingFiles">Flag indicating if the directory has matching files.</param>
		void OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			EventHandler<DirectoryEventArgs> handler = this.ProcessDirectory;

			if (handler != null) {
				var args = new DirectoryEventArgs(directory, hasMatchingFiles);
				handler(this, args);
				this.alive_ = args.ContinueRunning;
			}
		}

		/// <summary>
		/// Scan a directory.
		/// </summary>
		/// <param name="directory">The base directory to scan.</param>
		/// <param name="recurse">True to recurse subdirectories, false to scan a single directory.</param>
		public void Scan(string directory, bool recurse)
		{
			this.alive_ = true;
			this.ScanDir(directory, recurse);
		}

		void ScanDir(string directory, bool recurse)
		{

			try {
				string[] names = System.IO.Directory.GetFiles(directory);
				bool hasMatch = false;
				for (int fileIndex = 0; fileIndex < names.Length; ++fileIndex) {
					if (!this.fileFilter_.IsMatch(names[fileIndex])) {
						names[fileIndex] = null;
					} else {
						hasMatch = true;
					}
				}

				this.OnProcessDirectory(directory, hasMatch);

				if (this.alive_ && hasMatch) {
					foreach (string fileName in names) {
						try {
							if (fileName != null) {
								this.OnProcessFile(fileName);
								if (!this.alive_) {
									break;
								}
							}
						} catch (Exception e) {
							if (!this.OnFileFailure(fileName, e)) {
								throw;
							}
						}
					}
				}
			} catch (Exception e) {
				if (!this.OnDirectoryFailure(directory, e)) {
					throw;
				}
			}

			if (this.alive_ && recurse) {
				try {
					string[] names = System.IO.Directory.GetDirectories(directory);
					foreach (string fulldir in names) {
						if ((this.directoryFilter_ == null) || (this.directoryFilter_.IsMatch(fulldir))) {
							this.ScanDir(fulldir, true);
							if (!this.alive_) {
								break;
							}
						}
					}
				} catch (Exception e) {
					if (!this.OnDirectoryFailure(directory, e)) {
						throw;
					}
				}
			}
		}

		#region Instance Fields
		/// <summary>
		/// The file filter currently in use.
		/// </summary>
		IScanFilter fileFilter_;
		/// <summary>
		/// The directory filter currently in use.
		/// </summary>
		IScanFilter directoryFilter_;
		/// <summary>
		/// Flag indicating if scanning should continue running.
		/// </summary>
		bool alive_;
		#endregion
	}
}
