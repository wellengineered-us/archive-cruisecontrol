using System;
using System.IO;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Tar
{
	/// <summary>
	/// The TarOutputStream writes a UNIX tar archive as an OutputStream.
	/// Methods are provided to put entries, and then write their contents
	/// by writing to this stream using write().
	/// </summary>
	/// public
	public class TarOutputStream : Stream
	{
		#region Constructors
		/// <summary>
		/// Construct TarOutputStream using default block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		public TarOutputStream(Stream outputStream)
			: this(outputStream, TarBuffer.DefaultBlockFactor)
		{
		}

		/// <summary>
		/// Construct TarOutputStream with user specified block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		/// <param name="blockFactor">blocking factor</param>
		public TarOutputStream(Stream outputStream, int blockFactor)
		{
			if (outputStream == null) {
				throw new ArgumentNullException(nameof(outputStream));
			}

			this.outputStream = outputStream;
			this.buffer = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);

			this.assemblyBuffer = new byte[TarBuffer.BlockSize];
			this.blockBuffer = new byte[TarBuffer.BlockSize];
		}
		#endregion

		/// <summary>
		/// Get/set flag indicating ownership of the underlying stream.
		/// When the flag is true <see cref="Close"></see> will close the underlying stream also.
		/// </summary>
		public bool IsStreamOwner {
			get { return this.buffer.IsStreamOwner; }
			set { this.buffer.IsStreamOwner = value; }
		}

		/// <summary>
		/// true if the stream supports reading; otherwise, false.
		/// </summary>
		public override bool CanRead {
			get {
				return this.outputStream.CanRead;
			}
		}

		/// <summary>
		/// true if the stream supports seeking; otherwise, false.
		/// </summary>
		public override bool CanSeek {
			get {
				return this.outputStream.CanSeek;
			}
		}

		/// <summary>
		/// true if stream supports writing; otherwise, false.
		/// </summary>
		public override bool CanWrite {
			get {
				return this.outputStream.CanWrite;
			}
		}

		/// <summary>
		/// length of stream in bytes
		/// </summary>
		public override long Length {
			get {
				return this.outputStream.Length;
			}
		}

		/// <summary>
		/// gets or sets the position within the current stream.
		/// </summary>
		public override long Position {
			get {
				return this.outputStream.Position;
			}
			set {
				this.outputStream.Position = value;
			}
		}

		/// <summary>
		/// set the position within the current stream
		/// </summary>
		/// <param name="offset">The offset relative to the <paramref name="origin"/> to seek to</param>
		/// <param name="origin">The <see cref="SeekOrigin"/> to seek from.</param>
		/// <returns>The new position in the stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.outputStream.Seek(offset, origin);
		}

		/// <summary>
		/// Set the length of the current stream
		/// </summary>
		/// <param name="value">The new stream length.</param>
		public override void SetLength(long value)
		{
			this.outputStream.SetLength(value);
		}

		/// <summary>
		/// Read a byte from the stream and advance the position within the stream 
		/// by one byte or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>The byte value or -1 if at end of stream</returns>
		public override int ReadByte()
		{
			return this.outputStream.ReadByte();
		}

		/// <summary>
		/// read bytes from the current stream and advance the position within the 
		/// stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">The buffer to store read bytes in.</param>
		/// <param name="offset">The index into the buffer to being storing bytes at.</param>
		/// <param name="count">The desired number of bytes to read.</param>
		/// <returns>The total number of bytes read, or zero if at the end of the stream.
		/// The number of bytes may be less than the <paramref name="count">count</paramref>
		/// requested if data is not avialable.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.outputStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// All buffered data is written to destination
		/// </summary>		
		public override void Flush()
		{
			this.outputStream.Flush();
		}

		/// <summary>
		/// Ends the TAR archive without closing the underlying OutputStream.
		/// The result is that the EOF block of nulls is written.
		/// </summary>
		public void Finish()
		{
			if (this.IsEntryOpen) {
				this.CloseEntry();
			}
			this.WriteEofBlock();
		}

		/// <summary>
		/// Ends the TAR archive and closes the underlying OutputStream.
		/// </summary>
		/// <remarks>This means that Finish() is called followed by calling the
		/// TarBuffer's Close().</remarks>
		protected override void Dispose(bool disposing)
		{
			if (!this.isClosed) {
				this.isClosed = true;
				this.Finish();
				this.buffer.Close();
			}
		}

		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		public int RecordSize {
			get { return this.buffer.RecordSize; }
		}

		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// The TarBuffer record size.
		/// </returns>
		[Obsolete("Use RecordSize property instead")]
		public int GetRecordSize()
		{
			return this.buffer.RecordSize;
		}

		/// <summary>
		/// Get a value indicating wether an entry is open, requiring more data to be written.
		/// </summary>
		bool IsEntryOpen {
			get { return (this.currBytes < this.currSize); }

		}

		/// <summary>
		/// Put an entry on the output stream. This writes the entry's
		/// header and positions the output stream for writing
		/// the contents of the entry. Once this method is called, the
		/// stream is ready for calls to write() to write the entry's
		/// contents. Once the contents are written, closeEntry()
		/// <B>MUST</B> be called to ensure that all buffered data
		/// is completely written to the output stream.
		/// </summary>
		/// <param name="entry">
		/// The TarEntry to be written to the archive.
		/// </param>
		public void PutNextEntry(TarEntry entry)
		{
			if (entry == null) {
				throw new ArgumentNullException(nameof(entry));
			}

			if (entry.TarHeader.Name.Length > TarHeader.NAMELEN) {
				var longHeader = new TarHeader();
				longHeader.TypeFlag = TarHeader.LF_GNU_LONGNAME;
				longHeader.Name = longHeader.Name + "././@LongLink";
				longHeader.Mode = 420;//644 by default
				longHeader.UserId = entry.UserId;
				longHeader.GroupId = entry.GroupId;
				longHeader.GroupName = entry.GroupName;
				longHeader.UserName = entry.UserName;
				longHeader.LinkName = "";
				longHeader.Size = entry.TarHeader.Name.Length + 1;  // Plus one to avoid dropping last char

				longHeader.WriteHeader(this.blockBuffer);
				this.buffer.WriteBlock(this.blockBuffer);  // Add special long filename header block

				int nameCharIndex = 0;

				while (nameCharIndex < entry.TarHeader.Name.Length + 1 /* we've allocated one for the null char, now we must make sure it gets written out */) {
					Array.Clear(this.blockBuffer, 0, this.blockBuffer.Length);
					TarHeader.GetAsciiBytes(entry.TarHeader.Name, nameCharIndex, this.blockBuffer, 0, TarBuffer.BlockSize); // This func handles OK the extra char out of string length
					nameCharIndex += TarBuffer.BlockSize;
					this.buffer.WriteBlock(this.blockBuffer);
				}
			}

			entry.WriteEntryHeader(this.blockBuffer);
			this.buffer.WriteBlock(this.blockBuffer);

			this.currBytes = 0;

			this.currSize = entry.IsDirectory ? 0 : entry.Size;
		}

		/// <summary>
		/// Close an entry. This method MUST be called for all file
		/// entries that contain data. The reason is that we must
		/// buffer data written to the stream in order to satisfy
		/// the buffer's block based writes. Thus, there may be
		/// data fragments still being assembled that must be written
		/// to the output stream before this entry is closed and the
		/// next entry written.
		/// </summary>
		public void CloseEntry()
		{
			if (this.assemblyBufferLength > 0) {
				Array.Clear(this.assemblyBuffer, this.assemblyBufferLength, this.assemblyBuffer.Length - this.assemblyBufferLength);

				this.buffer.WriteBlock(this.assemblyBuffer);

				this.currBytes += this.assemblyBufferLength;
				this.assemblyBufferLength = 0;
			}

			if (this.currBytes < this.currSize) {
				string errorText = string.Format(
					"Entry closed at '{0}' before the '{1}' bytes specified in the header were written",
					this.currBytes, this.currSize);
				throw new TarException(errorText);
			}
		}

		/// <summary>
		/// Writes a byte to the current tar archive entry.
		/// This method simply calls Write(byte[], int, int).
		/// </summary>
		/// <param name="value">
		/// The byte to be written.
		/// </param>
		public override void WriteByte(byte value)
		{
			this.Write(new byte[] { value }, 0, 1);
		}

		/// <summary>
		/// Writes bytes to the current tar archive entry. This method
		/// is aware of the current entry and will throw an exception if
		/// you attempt to write bytes past the length specified for the
		/// current entry. The method is also (painfully) aware of the
		/// record buffering required by TarBuffer, and manages buffers
		/// that are not a multiple of recordsize in length, including
		/// assembling records from small buffers.
		/// </summary>
		/// <param name = "buffer">
		/// The buffer to write to the archive.
		/// </param>
		/// <param name = "offset">
		/// The offset in the buffer from which to get bytes.
		/// </param>
		/// <param name = "count">
		/// The number of bytes to write.
		/// </param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null) {
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0) {
				throw new ArgumentOutOfRangeException(nameof(offset), "Cannot be negative");
			}

			if (buffer.Length - offset < count) {
				throw new ArgumentException("offset and count combination is invalid");
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count), "Cannot be negative");
			}

			if ((this.currBytes + count) > this.currSize) {
				string errorText = string.Format("request to write '{0}' bytes exceeds size in header of '{1}' bytes",
					count, this.currSize);
				throw new ArgumentOutOfRangeException(nameof(count), errorText);
			}

			//
			// We have to deal with assembly!!!
			// The programmer can be writing little 32 byte chunks for all
			// we know, and we must assemble complete blocks for writing.
			// TODO  REVIEW Maybe this should be in TarBuffer? Could that help to
			//        eliminate some of the buffer copying.
			//
			if (this.assemblyBufferLength > 0) {
				if ((this.assemblyBufferLength + count) >= this.blockBuffer.Length) {
					int aLen = this.blockBuffer.Length - this.assemblyBufferLength;

					Array.Copy(this.assemblyBuffer, 0, this.blockBuffer, 0, this.assemblyBufferLength);
					Array.Copy(buffer, offset, this.blockBuffer, this.assemblyBufferLength, aLen);

					this.buffer.WriteBlock(this.blockBuffer);

					this.currBytes += this.blockBuffer.Length;

					offset += aLen;
					count -= aLen;

					this.assemblyBufferLength = 0;
				} else {
					Array.Copy(buffer, offset, this.assemblyBuffer, this.assemblyBufferLength, count);
					offset += count;
					this.assemblyBufferLength += count;
					count -= count;
				}
			}

			//
			// When we get here we have EITHER:
			//   o An empty "assembly" buffer.
			//   o No bytes to write (count == 0)
			//
			while (count > 0) {
				if (count < this.blockBuffer.Length) {
					Array.Copy(buffer, offset, this.assemblyBuffer, this.assemblyBufferLength, count);
					this.assemblyBufferLength += count;
					break;
				}

				this.buffer.WriteBlock(buffer, offset);

				int bufferLength = this.blockBuffer.Length;
				this.currBytes += bufferLength;
				count -= bufferLength;
				offset += bufferLength;
			}
		}

		/// <summary>
		/// Write an EOF (end of archive) block to the tar archive.
		/// An EOF block consists of all zeros.
		/// </summary>
		void WriteEofBlock()
		{
			Array.Clear(this.blockBuffer, 0, this.blockBuffer.Length);
			this.buffer.WriteBlock(this.blockBuffer);
		}

		#region Instance Fields
		/// <summary>
		/// bytes written for this entry so far
		/// </summary>
		long currBytes;

		/// <summary>
		/// current 'Assembly' buffer length
		/// </summary>		
		int assemblyBufferLength;

		/// <summary>
		/// Flag indicating wether this instance has been closed or not.
		/// </summary>
		bool isClosed;

		/// <summary>
		/// Size for the current entry
		/// </summary>
		protected long currSize;

		/// <summary>
		/// single block working buffer 
		/// </summary>
		protected byte[] blockBuffer;

		/// <summary>
		/// 'Assembly' buffer used to assemble data before writing
		/// </summary>
		protected byte[] assemblyBuffer;

		/// <summary>
		/// TarBuffer used to provide correct blocking factor
		/// </summary>
		protected TarBuffer buffer;

		/// <summary>
		/// the destination stream for the archive contents
		/// </summary>
		protected Stream outputStream;
		#endregion
	}
}
