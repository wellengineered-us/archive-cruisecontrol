using System;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Core;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Zip
{
	/// <summary>
	/// FastZipEvents supports all events applicable to <see cref="FastZip">FastZip</see> operations.
	/// </summary>
	public class FastZipEvents
	{
		/// <summary>
		/// Delegate to invoke when processing directories.
		/// </summary>
		public event EventHandler<DirectoryEventArgs> ProcessDirectory;

		/// <summary>
		/// Delegate to invoke when processing files.
		/// </summary>
		public ProcessFileHandler ProcessFile;

		/// <summary>
		/// Delegate to invoke during processing of files.
		/// </summary>
		public ProgressHandler Progress;

		/// <summary>
		/// Delegate to invoke when processing for a file has been completed.
		/// </summary>
		public CompletedFileHandler CompletedFile;

		/// <summary>
		/// Delegate to invoke when processing directory failures.
		/// </summary>
		public DirectoryFailureHandler DirectoryFailure;

		/// <summary>
		/// Delegate to invoke when processing file failures.
		/// </summary>
		public FileFailureHandler FileFailure;

		/// <summary>
		/// Raise the <see cref="DirectoryFailure">directory failure</see> event.
		/// </summary>
		/// <param name="directory">The directory causing the failure.</param>
		/// <param name="e">The exception for this event.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnDirectoryFailure(string directory, Exception e)
		{
			bool result = false;
			DirectoryFailureHandler handler = this.DirectoryFailure;

			if (handler != null) {
				var args = new ScanFailureEventArgs(directory, e);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="FileFailure"> file failure handler delegate</see>.
		/// </summary>
		/// <param name="file">The file causing the failure.</param>
		/// <param name="e">The exception for this failure.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnFileFailure(string file, Exception e)
		{
			FileFailureHandler handler = this.FileFailure;
			bool result = (handler != null);

			if (result) {
				var args = new ScanFailureEventArgs(file, e);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="ProcessFile">ProcessFile delegate</see>.
		/// </summary>
		/// <param name="file">The file being processed.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnProcessFile(string file)
		{
			bool result = true;
			ProcessFileHandler handler = this.ProcessFile;

			if (handler != null) {
				var args = new ScanEventArgs(file);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="CompletedFile"/> delegate
		/// </summary>
		/// <param name="file">The file whose processing has been completed.</param>
		/// <returns>A boolean indicating if execution should continue or not.</returns>
		public bool OnCompletedFile(string file)
		{
			bool result = true;
			CompletedFileHandler handler = this.CompletedFile;
			if (handler != null) {
				var args = new ScanEventArgs(file);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// Fires the <see cref="ProcessDirectory">process directory</see> delegate.
		/// </summary>
		/// <param name="directory">The directory being processed.</param>
		/// <param name="hasMatchingFiles">Flag indicating if the directory has matching files as determined by the current filter.</param>
		/// <returns>A <see cref="bool"/> of true if the operation should continue; false otherwise.</returns>
		public bool OnProcessDirectory(string directory, bool hasMatchingFiles)
		{
			bool result = true;
			EventHandler<DirectoryEventArgs> handler = this.ProcessDirectory;
			if (handler != null) {
				var args = new DirectoryEventArgs(directory, hasMatchingFiles);
				handler(this, args);
				result = args.ContinueRunning;
			}
			return result;
		}

		/// <summary>
		/// The minimum timespan between <see cref="Progress"/> events.
		/// </summary>
		/// <value>The minimum period of time between <see cref="Progress"/> events.</value>
		/// <seealso cref="Progress"/>
		/// <remarks>The default interval is three seconds.</remarks>
		public TimeSpan ProgressInterval {
			get { return this.progressInterval_; }
			set { this.progressInterval_ = value; }
		}

		#region Instance Fields
		TimeSpan progressInterval_ = TimeSpan.FromSeconds(3);
		#endregion
	}

	/// <summary>
	/// FastZip provides facilities for creating and extracting zip files.
	/// </summary>
	public class FastZip
	{
		#region Enumerations
		/// <summary>
		/// Defines the desired handling when overwriting files during extraction.
		/// </summary>
		public enum Overwrite
		{
			/// <summary>
			/// Prompt the user to confirm overwriting
			/// </summary>
			Prompt,
			/// <summary>
			/// Never overwrite files.
			/// </summary>
			Never,
			/// <summary>
			/// Always overwrite files.
			/// </summary>
			Always
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initialise a default instance of <see cref="FastZip"/>.
		/// </summary>
		public FastZip()
		{
		}

		/// <summary>
		/// Initialise a new instance of <see cref="FastZip"/>
		/// </summary>
		/// <param name="events">The <see cref="FastZipEvents">events</see> to use during operations.</param>
		public FastZip(FastZipEvents events)
		{
			this.events_ = events;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Get/set a value indicating wether empty directories should be created.
		/// </summary>
		public bool CreateEmptyDirectories {
			get { return this.createEmptyDirectories_; }
			set { this.createEmptyDirectories_ = value; }
		}

		/// <summary>
		/// Get / set the password value.
		/// </summary>
		public string Password {
			get { return this.password_; }
			set { this.password_ = value; }
		}

		/// <summary>
		/// Get or set the <see cref="INameTransform"></see> active when creating Zip files.
		/// </summary>
		/// <seealso cref="EntryFactory"></seealso>
		public INameTransform NameTransform {
			get { return this.entryFactory_.NameTransform; }
			set {
				this.entryFactory_.NameTransform = value;
			}
		}

		/// <summary>
		/// Get or set the <see cref="IEntryFactory"></see> active when creating Zip files.
		/// </summary>
		public IEntryFactory EntryFactory {
			get { return this.entryFactory_; }
			set {
				if (value == null) {
					this.entryFactory_ = new ZipEntryFactory();
				} else {
					this.entryFactory_ = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the setting for <see cref="UseZip64">Zip64 handling when writing.</see>
		/// </summary>
		/// <remarks>
		/// The default value is dynamic which is not backwards compatible with old
		/// programs and can cause problems with XP's built in compression which cant
		/// read Zip64 archives. However it does avoid the situation were a large file
		/// is added and cannot be completed correctly.
		/// NOTE: Setting the size for entries before they are added is the best solution!
		/// By default the EntryFactory used by FastZip will set fhe file size.
		/// </remarks>
		public UseZip64 UseZip64 {
			get { return this.useZip64_; }
			set { this.useZip64_ = value; }
		}

		/// <summary>
		/// Get/set a value indicating wether file dates and times should 
		/// be restored when extracting files from an archive.
		/// </summary>
		/// <remarks>The default value is false.</remarks>
		public bool RestoreDateTimeOnExtract {
			get {
				return this.restoreDateTimeOnExtract_;
			}
			set {
				this.restoreDateTimeOnExtract_ = value;
			}
		}

		/// <summary>
		/// Get/set a value indicating wether file attributes should
		/// be restored during extract operations
		/// </summary>
		public bool RestoreAttributesOnExtract {
			get { return this.restoreAttributesOnExtract_; }
			set { this.restoreAttributesOnExtract_ = value; }
		}
		#endregion

		#region Delegates
		/// <summary>
		/// Delegate called when confirming overwriting of files.
		/// </summary>
		public delegate bool ConfirmOverwriteDelegate(string fileName);
		#endregion

		#region CreateZip
		/// <summary>
		/// Create a zip file.
		/// </summary>
		/// <param name="zipFileName">The name of the zip file to create.</param>
		/// <param name="sourceDirectory">The directory to source files from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter">directory filter</see> to apply.</param>
		public void CreateZip(string zipFileName, string sourceDirectory,
			bool recurse, string fileFilter, string directoryFilter)
		{
			this.CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, directoryFilter);
		}

		/// <summary>
		/// Create a zip file/archive.
		/// </summary>
		/// <param name="zipFileName">The name of the zip file to create.</param>
		/// <param name="sourceDirectory">The directory to obtain files and directories from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The file filter to apply.</param>
		public void CreateZip(string zipFileName, string sourceDirectory, bool recurse, string fileFilter)
		{
			this.CreateZip(File.Create(zipFileName), sourceDirectory, recurse, fileFilter, null);
		}

		/// <summary>
		/// Create a zip archive sending output to the <paramref name="outputStream"/> passed.
		/// </summary>
		/// <param name="outputStream">The stream to write archive data to.</param>
		/// <param name="sourceDirectory">The directory to source files from.</param>
		/// <param name="recurse">True to recurse directories, false for no recursion.</param>
		/// <param name="fileFilter">The <see cref="PathFilter">file filter</see> to apply.</param>
		/// <param name="directoryFilter">The <see cref="PathFilter">directory filter</see> to apply.</param>
		/// <remarks>The <paramref name="outputStream"/> is closed after creation.</remarks>
		public void CreateZip(Stream outputStream, string sourceDirectory, bool recurse, string fileFilter, string directoryFilter)
		{
			this.NameTransform = new ZipNameTransform(sourceDirectory);
			this.sourceDirectory_ = sourceDirectory;

			using (this.outputStream_ = new ZipOutputStream(outputStream)) {

				if (this.password_ != null) {
					this.outputStream_.Password = this.password_;
				}

				this.outputStream_.UseZip64 = this.UseZip64;
				var scanner = new FileSystemScanner(fileFilter, directoryFilter);
				scanner.ProcessFile += this.ProcessFile;
				if (this.CreateEmptyDirectories) {
					scanner.ProcessDirectory += this.ProcessDirectory;
				}

				if (this.events_ != null) {
					if (this.events_.FileFailure != null) {
						scanner.FileFailure += this.events_.FileFailure;
					}

					if (this.events_.DirectoryFailure != null) {
						scanner.DirectoryFailure += this.events_.DirectoryFailure;
					}
				}

				scanner.Scan(sourceDirectory, recurse);
			}
		}

		#endregion

		#region ExtractZip
		/// <summary>
		/// Extract the contents of a zip file.
		/// </summary>
		/// <param name="zipFileName">The zip file to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		public void ExtractZip(string zipFileName, string targetDirectory, string fileFilter)
		{
			this.ExtractZip(zipFileName, targetDirectory, Overwrite.Always, null, fileFilter, null, this.restoreDateTimeOnExtract_);
		}

		/// <summary>
		/// Extract the contents of a zip file.
		/// </summary>
		/// <param name="zipFileName">The zip file to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="overwrite">The style of <see cref="Overwrite">overwriting</see> to apply.</param>
		/// <param name="confirmDelegate">A delegate to invoke when confirming overwriting.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		/// <param name="directoryFilter">A filter to apply to directories.</param>
		/// <param name="restoreDateTime">Flag indicating whether to restore the date and time for extracted files.</param>
		public void ExtractZip(string zipFileName, string targetDirectory,
							   Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate,
							   string fileFilter, string directoryFilter, bool restoreDateTime)
		{
			Stream inputStream = File.Open(zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			this.ExtractZip(inputStream, targetDirectory, overwrite, confirmDelegate, fileFilter, directoryFilter, restoreDateTime, true);
		}

		/// <summary>
		/// Extract the contents of a zip file held in a stream.
		/// </summary>
		/// <param name="inputStream">The seekable input stream containing the zip to extract from.</param>
		/// <param name="targetDirectory">The directory to save extracted information in.</param>
		/// <param name="overwrite">The style of <see cref="Overwrite">overwriting</see> to apply.</param>
		/// <param name="confirmDelegate">A delegate to invoke when confirming overwriting.</param>
		/// <param name="fileFilter">A filter to apply to files.</param>
		/// <param name="directoryFilter">A filter to apply to directories.</param>
		/// <param name="restoreDateTime">Flag indicating whether to restore the date and time for extracted files.</param>
		/// <param name="isStreamOwner">Flag indicating whether the inputStream will be closed by this method.</param>
		public void ExtractZip(Stream inputStream, string targetDirectory,
					   Overwrite overwrite, ConfirmOverwriteDelegate confirmDelegate,
					   string fileFilter, string directoryFilter, bool restoreDateTime,
					   bool isStreamOwner)
		{
			if ((overwrite == Overwrite.Prompt) && (confirmDelegate == null)) {
				throw new ArgumentNullException(nameof(confirmDelegate));
			}

			this.continueRunning_ = true;
			this.overwrite_ = overwrite;
			this.confirmDelegate_ = confirmDelegate;
			this.extractNameTransform_ = new WindowsNameTransform(targetDirectory);

			this.fileFilter_ = new NameFilter(fileFilter);
			this.directoryFilter_ = new NameFilter(directoryFilter);
			this.restoreDateTimeOnExtract_ = restoreDateTime;

			using (this.zipFile_ = new ZipFile(inputStream)) {

				if (this.password_ != null) {
					this.zipFile_.Password = this.password_;
				}
				this.zipFile_.IsStreamOwner = isStreamOwner;
				System.Collections.IEnumerator enumerator = this.zipFile_.GetEnumerator();
				while (this.continueRunning_ && enumerator.MoveNext()) {
					var entry = (ZipEntry)enumerator.Current;
					if (entry.IsFile) {
						// TODO Path.GetDirectory can fail here on invalid characters.
						if (this.directoryFilter_.IsMatch(Path.GetDirectoryName(entry.Name)) && this.fileFilter_.IsMatch(entry.Name)) {
							this.ExtractEntry(entry);
						}
					} else if (entry.IsDirectory) {
						if (this.directoryFilter_.IsMatch(entry.Name) && this.CreateEmptyDirectories) {
							this.ExtractEntry(entry);
						}
					} else {
						// Do nothing for volume labels etc...
					}
				}
			}
		}
		#endregion

		#region Internal Processing
		void ProcessDirectory(object sender, DirectoryEventArgs e)
		{
			if (!e.HasMatchingFiles && this.CreateEmptyDirectories) {
				if (this.events_ != null) {
					this.events_.OnProcessDirectory(e.Name, e.HasMatchingFiles);
				}

				if (e.ContinueRunning) {
					if (e.Name != this.sourceDirectory_) {
						ZipEntry entry = this.entryFactory_.MakeDirectoryEntry(e.Name);
						this.outputStream_.PutNextEntry(entry);
					}
				}
			}
		}

		void ProcessFile(object sender, ScanEventArgs e)
		{
			if ((this.events_ != null) && (this.events_.ProcessFile != null)) {
				this.events_.ProcessFile(sender, e);
			}

			if (e.ContinueRunning) {
				try {
					// The open below is equivalent to OpenRead which gaurantees that if opened the 
					// file will not be changed by subsequent openers, but precludes opening in some cases
					// were it could succeed. ie the open may fail as its already open for writing and the share mode should reflect that.
					using (FileStream stream = File.Open(e.Name, FileMode.Open, FileAccess.Read, FileShare.Read)) {
						ZipEntry entry = this.entryFactory_.MakeFileEntry(e.Name);
						this.outputStream_.PutNextEntry(entry);
						this.AddFileContents(e.Name, stream);
					}
				} catch (Exception ex) {
					if (this.events_ != null) {
						this.continueRunning_ = this.events_.OnFileFailure(e.Name, ex);
					} else {
						this.continueRunning_ = false;
						throw;
					}
				}
			}
		}

		void AddFileContents(string name, Stream stream)
		{
			if (stream == null) {
				throw new ArgumentNullException(nameof(stream));
			}

			if (this.buffer_ == null) {
				this.buffer_ = new byte[4096];
			}

			if ((this.events_ != null) && (this.events_.Progress != null)) {
				StreamUtils.Copy(stream, this.outputStream_, this.buffer_,
					this.events_.Progress, this.events_.ProgressInterval, this, name);
			} else {
				StreamUtils.Copy(stream, this.outputStream_, this.buffer_);
			}

			if (this.events_ != null) {
				this.continueRunning_ = this.events_.OnCompletedFile(name);
			}
		}

		void ExtractFileEntry(ZipEntry entry, string targetName)
		{
			bool proceed = true;
			if (this.overwrite_ != Overwrite.Always) {
				if (File.Exists(targetName)) {
					if ((this.overwrite_ == Overwrite.Prompt) && (this.confirmDelegate_ != null)) {
						proceed = this.confirmDelegate_(targetName);
					} else {
						proceed = false;
					}
				}
			}

			if (proceed) {
				if (this.events_ != null) {
					this.continueRunning_ = this.events_.OnProcessFile(entry.Name);
				}

				if (this.continueRunning_) {
					try {
						using (FileStream outputStream = File.Create(targetName)) {
							if (this.buffer_ == null) {
								this.buffer_ = new byte[4096];
							}
							if ((this.events_ != null) && (this.events_.Progress != null)) {
								StreamUtils.Copy(this.zipFile_.GetInputStream(entry), outputStream, this.buffer_,
									this.events_.Progress, this.events_.ProgressInterval, this, entry.Name, entry.Size);
							} else {
								StreamUtils.Copy(this.zipFile_.GetInputStream(entry), outputStream, this.buffer_);
							}

							if (this.events_ != null) {
								this.continueRunning_ = this.events_.OnCompletedFile(entry.Name);
							}
						}

						if (this.restoreDateTimeOnExtract_) {
							File.SetLastWriteTime(targetName, entry.DateTime);
						}

						if (this.RestoreAttributesOnExtract && entry.IsDOSEntry && (entry.ExternalFileAttributes != -1)) {
							var fileAttributes = (FileAttributes)entry.ExternalFileAttributes;
							// TODO: FastZip - Setting of other file attributes on extraction is a little trickier.
							fileAttributes &= (FileAttributes.Archive | FileAttributes.Normal | FileAttributes.ReadOnly | FileAttributes.Hidden);
							File.SetAttributes(targetName, fileAttributes);
						}
					} catch (Exception ex) {
						if (this.events_ != null) {
							this.continueRunning_ = this.events_.OnFileFailure(targetName, ex);
						} else {
							this.continueRunning_ = false;
							throw;
						}
					}
				}
			}
		}

		void ExtractEntry(ZipEntry entry)
		{
			bool doExtraction = entry.IsCompressionMethodSupported();
			string targetName = entry.Name;

			if (doExtraction) {
				if (entry.IsFile) {
					targetName = this.extractNameTransform_.TransformFile(targetName);
				} else if (entry.IsDirectory) {
					targetName = this.extractNameTransform_.TransformDirectory(targetName);
				}

				doExtraction = !(string.IsNullOrEmpty(targetName));
			}

			// TODO: Fire delegate/throw exception were compression method not supported, or name is invalid?

			string dirName = null;

			if (doExtraction) {
				if (entry.IsDirectory) {
					dirName = targetName;
				} else {
					dirName = Path.GetDirectoryName(Path.GetFullPath(targetName));
				}
			}

			if (doExtraction && !Directory.Exists(dirName)) {
				if (!entry.IsDirectory || this.CreateEmptyDirectories) {
					try {
						Directory.CreateDirectory(dirName);
					} catch (Exception ex) {
						doExtraction = false;
						if (this.events_ != null) {
							if (entry.IsDirectory) {
								this.continueRunning_ = this.events_.OnDirectoryFailure(targetName, ex);
							} else {
								this.continueRunning_ = this.events_.OnFileFailure(targetName, ex);
							}
						} else {
							this.continueRunning_ = false;
							throw;
						}
					}
				}
			}

			if (doExtraction && entry.IsFile) {
				this.ExtractFileEntry(entry, targetName);
			}
		}

		static int MakeExternalAttributes(FileInfo info)
		{
			return (int)info.Attributes;
		}

		static bool NameIsValid(string name)
		{
			return !string.IsNullOrEmpty(name) &&
				(name.IndexOfAny(Path.GetInvalidPathChars()) < 0);
		}
		#endregion

		#region Instance Fields
		bool continueRunning_;
		byte[] buffer_;
		ZipOutputStream outputStream_;
		ZipFile zipFile_;
		string sourceDirectory_;
		NameFilter fileFilter_;
		NameFilter directoryFilter_;
		Overwrite overwrite_;
		ConfirmOverwriteDelegate confirmDelegate_;

		bool restoreDateTimeOnExtract_;
		bool restoreAttributesOnExtract_;
		bool createEmptyDirectories_;
		FastZipEvents events_;
		IEntryFactory entryFactory_ = new ZipEntryFactory();
		INameTransform extractNameTransform_;
		UseZip64 useZip64_ = UseZip64.Dynamic;

		string password_;

		#endregion
	}
}
