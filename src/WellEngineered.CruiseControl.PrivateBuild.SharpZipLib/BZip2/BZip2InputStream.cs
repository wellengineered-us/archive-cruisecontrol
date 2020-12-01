using System;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Checksum;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.BZip2
{
	/// <summary>
	/// An input stream that decompresses files in the BZip2 format 
	/// </summary>
	public class BZip2InputStream : Stream
	{
		#region Constants
		const int START_BLOCK_STATE = 1;
		const int RAND_PART_A_STATE = 2;
		const int RAND_PART_B_STATE = 3;
		const int RAND_PART_C_STATE = 4;
		const int NO_RAND_PART_A_STATE = 5;
		const int NO_RAND_PART_B_STATE = 6;
		const int NO_RAND_PART_C_STATE = 7;
		#endregion
		#region Constructors
		/// <summary>
		/// Construct instance for reading from stream
		/// </summary>
		/// <param name="stream">Data source</param>
		public BZip2InputStream(Stream stream)
		{
			// init arrays
			for (int i = 0; i < BZip2Constants.GroupCount; ++i) {
				this.limit[i] = new int[BZip2Constants.MaximumAlphaSize];
				this.baseArray[i] = new int[BZip2Constants.MaximumAlphaSize];
				this.perm[i] = new int[BZip2Constants.MaximumAlphaSize];
			}

			this.BsSetStream(stream);
			this.Initialize();
			this.InitBlock();
			this.SetupBlock();
		}

		#endregion

		/// <summary>
		/// Get/set flag indicating ownership of underlying stream.
		/// When the flag is true <see cref="Close"></see> will close the underlying stream also.
		/// </summary>
		public bool IsStreamOwner {
			get { return this.isStreamOwner; }
			set { this.isStreamOwner = value; }
		}


		#region Stream Overrides
		/// <summary>
		/// Gets a value indicating if the stream supports reading
		/// </summary>
		public override bool CanRead {
			get {
				return this.baseStream.CanRead;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		public override bool CanSeek {
			get {
				return this.baseStream.CanSeek;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// This property always returns false
		/// </summary>
		public override bool CanWrite {
			get {
				return false;
			}
		}

		/// <summary>
		/// Gets the length in bytes of the stream.
		/// </summary>
		public override long Length {
			get {
				return this.baseStream.Length;
			}
		}

		/// <summary>
		/// Gets or sets the streams position.
		/// Setting the position is not supported and will throw a NotSupportException
		/// </summary>
		/// <exception cref="NotSupportedException">Any attempt to set the position</exception>
		public override long Position {
			get {
				return this.baseStream.Position;
			}
			set {
				throw new NotSupportedException("BZip2InputStream position cannot be set");
			}
		}

		/// <summary>
		/// Flushes the stream.
		/// </summary>
		public override void Flush()
		{
			if (this.baseStream != null) {
				this.baseStream.Flush();
			}
		}

		/// <summary>
		/// Set the streams position.  This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position of the stream.</returns>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("BZip2InputStream Seek not supported");
		}

		/// <summary>
		/// Sets the length of this stream to the given value.
		/// This operation is not supported and will throw a NotSupportedExceptionortedException
		/// </summary>
		/// <param name="value">The new length for the stream.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("BZip2InputStream SetLength not supported");
		}

		/// <summary>
		/// Writes a block of bytes to this stream using data from a buffer.
		/// This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="buffer">The buffer to source data from.</param>
		/// <param name="offset">The offset to start obtaining data from.</param>
		/// <param name="count">The number of bytes of data to write.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("BZip2InputStream Write not supported");
		}

		/// <summary>
		/// Writes a byte to the current position in the file stream.
		/// This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="value">The value to write.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void WriteByte(byte value)
		{
			throw new NotSupportedException("BZip2InputStream WriteByte not supported");
		}

		/// <summary>
		/// Read a sequence of bytes and advances the read position by one byte.
		/// </summary>
		/// <param name="buffer">Array of bytes to store values in</param>
		/// <param name="offset">Offset in array to begin storing data</param>
		/// <param name="count">The maximum number of bytes to read</param>
		/// <returns>The total number of bytes read into the buffer. This might be less
		/// than the number of bytes requested if that number of bytes are not 
		/// currently available or zero if the end of the stream is reached.
		/// </returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null) {
				throw new ArgumentNullException(nameof(buffer));
			}

			for (int i = 0; i < count; ++i) {
				int rb = this.ReadByte();
				if (rb == -1) {
					return i;
				}
				buffer[offset + i] = (byte)rb;
			}
			return count;
		}

		/// <summary>
		/// Closes the stream, releasing any associated resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.IsStreamOwner && (this.baseStream != null)) {
				this.baseStream.Dispose();
			}
		}
		/// <summary>
		/// Read a byte from stream advancing position
		/// </summary>
		/// <returns>byte read or -1 on end of stream</returns>
		public override int ReadByte()
		{
			if (this.streamEnd) {
				return -1; // ok
			}

			int retChar = this.currentChar;
			switch (this.currentState) {
				case RAND_PART_B_STATE:
					this.SetupRandPartB();
					break;
				case RAND_PART_C_STATE:
					this.SetupRandPartC();
					break;
				case NO_RAND_PART_B_STATE:
					this.SetupNoRandPartB();
					break;
				case NO_RAND_PART_C_STATE:
					this.SetupNoRandPartC();
					break;
				case START_BLOCK_STATE:
				case NO_RAND_PART_A_STATE:
				case RAND_PART_A_STATE:
					break;
			}
			return retChar;
		}

		#endregion

		void MakeMaps()
		{
			this.nInUse = 0;
			for (int i = 0; i < 256; ++i) {
				if (this.inUse[i]) {
					this.seqToUnseq[this.nInUse] = (byte)i;
					this.unseqToSeq[i] = (byte)this.nInUse;
					this.nInUse++;
				}
			}
		}

		void Initialize()
		{
			char magic1 = this.BsGetUChar();
			char magic2 = this.BsGetUChar();

			char magic3 = this.BsGetUChar();
			char magic4 = this.BsGetUChar();

			if (magic1 != 'B' || magic2 != 'Z' || magic3 != 'h' || magic4 < '1' || magic4 > '9') {
				this.streamEnd = true;
				return;
			}

			this.SetDecompressStructureSizes(magic4 - '0');
			this.computedCombinedCRC = 0;
		}

		void InitBlock()
		{
			char magic1 = this.BsGetUChar();
			char magic2 = this.BsGetUChar();
			char magic3 = this.BsGetUChar();
			char magic4 = this.BsGetUChar();
			char magic5 = this.BsGetUChar();
			char magic6 = this.BsGetUChar();

			if (magic1 == 0x17 && magic2 == 0x72 && magic3 == 0x45 && magic4 == 0x38 && magic5 == 0x50 && magic6 == 0x90) {
				this.Complete();
				return;
			}

			if (magic1 != 0x31 || magic2 != 0x41 || magic3 != 0x59 || magic4 != 0x26 || magic5 != 0x53 || magic6 != 0x59) {
				BadBlockHeader();
				this.streamEnd = true;
				return;
			}

			this.storedBlockCRC = this.BsGetInt32();

			this.blockRandomised = (this.BsR(1) == 1);

			this.GetAndMoveToFrontDecode();

			this.mCrc.Reset();
			this.currentState = START_BLOCK_STATE;
		}

		void EndBlock()
		{
			this.computedBlockCRC = (int)this.mCrc.Value;

			// -- A bad CRC is considered a fatal error. --
			if (this.storedBlockCRC != this.computedBlockCRC) {
				CrcError();
			}

			// 1528150659
			this.computedCombinedCRC = ((this.computedCombinedCRC << 1) & 0xFFFFFFFF) | (this.computedCombinedCRC >> 31);
			this.computedCombinedCRC = this.computedCombinedCRC ^ (uint)this.computedBlockCRC;
		}

		void Complete()
		{
			this.storedCombinedCRC = this.BsGetInt32();
			if (this.storedCombinedCRC != (int)this.computedCombinedCRC) {
				CrcError();
			}

			this.streamEnd = true;
		}

		void BsSetStream(Stream stream)
		{
			this.baseStream = stream;
			this.bsLive = 0;
			this.bsBuff = 0;
		}

		void FillBuffer()
		{
			int thech = 0;

			try {
				thech = this.baseStream.ReadByte();
			} catch (Exception) {
				CompressedStreamEOF();
			}

			if (thech == -1) {
				CompressedStreamEOF();
			}

			this.bsBuff = (this.bsBuff << 8) | (thech & 0xFF);
			this.bsLive += 8;
		}

		int BsR(int n)
		{
			while (this.bsLive < n) {
				this.FillBuffer();
			}

			int v = (this.bsBuff >> (this.bsLive - n)) & ((1 << n) - 1);
			this.bsLive -= n;
			return v;
		}

		char BsGetUChar()
		{
			return (char)this.BsR(8);
		}

		int BsGetIntVS(int numBits)
		{
			return this.BsR(numBits);
		}

		int BsGetInt32()
		{
			int result = this.BsR(8);
			result = (result << 8) | this.BsR(8);
			result = (result << 8) | this.BsR(8);
			result = (result << 8) | this.BsR(8);
			return result;
		}

		void RecvDecodingTables()
		{
			char[][] len = new char[BZip2Constants.GroupCount][];
			for (int i = 0; i < BZip2Constants.GroupCount; ++i) {
				len[i] = new char[BZip2Constants.MaximumAlphaSize];
			}

			bool[] inUse16 = new bool[16];

			//--- Receive the mapping table ---
			for (int i = 0; i < 16; i++) {
				inUse16[i] = (this.BsR(1) == 1);
			}

			for (int i = 0; i < 16; i++) {
				if (inUse16[i]) {
					for (int j = 0; j < 16; j++) {
						this.inUse[i * 16 + j] = (this.BsR(1) == 1);
					}
				} else {
					for (int j = 0; j < 16; j++) {
						this.inUse[i * 16 + j] = false;
					}
				}
			}

			this.MakeMaps();
			int alphaSize = this.nInUse + 2;

			//--- Now the selectors ---
			int nGroups = this.BsR(3);
			int nSelectors = this.BsR(15);

			for (int i = 0; i < nSelectors; i++) {
				int j = 0;
				while (this.BsR(1) == 1) {
					j++;
				}
				this.selectorMtf[i] = (byte)j;
			}

			//--- Undo the MTF values for the selectors. ---
			byte[] pos = new byte[BZip2Constants.GroupCount];
			for (int v = 0; v < nGroups; v++) {
				pos[v] = (byte)v;
			}

			for (int i = 0; i < nSelectors; i++) {
				int v = this.selectorMtf[i];
				byte tmp = pos[v];
				while (v > 0) {
					pos[v] = pos[v - 1];
					v--;
				}
				pos[0] = tmp;
				this.selector[i] = tmp;
			}

			//--- Now the coding tables ---
			for (int t = 0; t < nGroups; t++) {
				int curr = this.BsR(5);
				for (int i = 0; i < alphaSize; i++) {
					while (this.BsR(1) == 1) {
						if (this.BsR(1) == 0) {
							curr++;
						} else {
							curr--;
						}
					}
					len[t][i] = (char)curr;
				}
			}

			//--- Create the Huffman decoding tables ---
			for (int t = 0; t < nGroups; t++) {
				int minLen = 32;
				int maxLen = 0;
				for (int i = 0; i < alphaSize; i++) {
					maxLen = Math.Max(maxLen, len[t][i]);
					minLen = Math.Min(minLen, len[t][i]);
				}
				HbCreateDecodeTables(this.limit[t], this.baseArray[t], this.perm[t], len[t], minLen, maxLen, alphaSize);
				this.minLens[t] = minLen;
			}
		}

		void GetAndMoveToFrontDecode()
		{
			byte[] yy = new byte[256];
			int nextSym;

			int limitLast = BZip2Constants.BaseBlockSize * this.blockSize100k;
			this.origPtr = this.BsGetIntVS(24);

			this.RecvDecodingTables();
			int EOB = this.nInUse + 1;
			int groupNo = -1;
			int groupPos = 0;

			/*--
			Setting up the unzftab entries here is not strictly
			necessary, but it does save having to do it later
			in a separate pass, and so saves a block's worth of
			cache misses.
			--*/
			for (int i = 0; i <= 255; i++) {
				this.unzftab[i] = 0;
			}

			for (int i = 0; i <= 255; i++) {
				yy[i] = (byte)i;
			}

			this.last = -1;

			if (groupPos == 0) {
				groupNo++;
				groupPos = BZip2Constants.GroupSize;
			}

			groupPos--;
			int zt = this.selector[groupNo];
			int zn = this.minLens[zt];
			int zvec = this.BsR(zn);
			int zj;

			while (zvec > this.limit[zt][zn]) {
				if (zn > 20) { // the longest code
					throw new BZip2Exception("Bzip data error");
				}
				zn++;
				while (this.bsLive < 1) {
					this.FillBuffer();
				}
				zj = (this.bsBuff >> (this.bsLive - 1)) & 1;
				this.bsLive--;
				zvec = (zvec << 1) | zj;
			}
			if (zvec - this.baseArray[zt][zn] < 0 || zvec - this.baseArray[zt][zn] >= BZip2Constants.MaximumAlphaSize) {
				throw new BZip2Exception("Bzip data error");
			}
			nextSym = this.perm[zt][zvec - this.baseArray[zt][zn]];

			while (true) {
				if (nextSym == EOB) {
					break;
				}

				if (nextSym == BZip2Constants.RunA || nextSym == BZip2Constants.RunB) {
					int s = -1;
					int n = 1;
					do {
						if (nextSym == BZip2Constants.RunA) {
							s += (0 + 1) * n;
						} else if (nextSym == BZip2Constants.RunB) {
							s += (1 + 1) * n;
						}

						n <<= 1;

						if (groupPos == 0) {
							groupNo++;
							groupPos = BZip2Constants.GroupSize;
						}

						groupPos--;

						zt = this.selector[groupNo];
						zn = this.minLens[zt];
						zvec = this.BsR(zn);

						while (zvec > this.limit[zt][zn]) {
							zn++;
							while (this.bsLive < 1) {
								this.FillBuffer();
							}
							zj = (this.bsBuff >> (this.bsLive - 1)) & 1;
							this.bsLive--;
							zvec = (zvec << 1) | zj;
						}
						nextSym = this.perm[zt][zvec - this.baseArray[zt][zn]];
					} while (nextSym == BZip2Constants.RunA || nextSym == BZip2Constants.RunB);

					s++;
					byte ch = this.seqToUnseq[yy[0]];
					this.unzftab[ch] += s;

					while (s > 0) {
						this.last++;
						this.ll8[this.last] = ch;
						s--;
					}

					if (this.last >= limitLast) {
						BlockOverrun();
					}
					continue;
				} else {
					this.last++;
					if (this.last >= limitLast) {
						BlockOverrun();
					}

					byte tmp = yy[nextSym - 1];
					this.unzftab[this.seqToUnseq[tmp]]++;
					this.ll8[this.last] = this.seqToUnseq[tmp];

					for (int j = nextSym - 1; j > 0; --j) {
						yy[j] = yy[j - 1];
					}
					yy[0] = tmp;

					if (groupPos == 0) {
						groupNo++;
						groupPos = BZip2Constants.GroupSize;
					}

					groupPos--;
					zt = this.selector[groupNo];
					zn = this.minLens[zt];
					zvec = this.BsR(zn);
					while (zvec > this.limit[zt][zn]) {
						zn++;
						while (this.bsLive < 1) {
							this.FillBuffer();
						}
						zj = (this.bsBuff >> (this.bsLive - 1)) & 1;
						this.bsLive--;
						zvec = (zvec << 1) | zj;
					}
					nextSym = this.perm[zt][zvec - this.baseArray[zt][zn]];
					continue;
				}
			}
		}

		void SetupBlock()
		{
			int[] cftab = new int[257];

			cftab[0] = 0;
			Array.Copy(this.unzftab, 0, cftab, 1, 256);

			for (int i = 1; i <= 256; i++) {
				cftab[i] += cftab[i - 1];
			}

			for (int i = 0; i <= this.last; i++) {
				byte ch = this.ll8[i];
				this.tt[cftab[ch]] = i;
				cftab[ch]++;
			}

			cftab = null;

			this.tPos = this.tt[this.origPtr];

			this.count = 0;
			this.i2 = 0;
			this.ch2 = 256;   /*-- not a char and not EOF --*/

			if (this.blockRandomised) {
				this.rNToGo = 0;
				this.rTPos = 0;
				this.SetupRandPartA();
			} else {
				this.SetupNoRandPartA();
			}
		}

		void SetupRandPartA()
		{
			if (this.i2 <= this.last) {
				this.chPrev = this.ch2;
				this.ch2 = this.ll8[this.tPos];
				this.tPos = this.tt[this.tPos];
				if (this.rNToGo == 0) {
					this.rNToGo = BZip2Constants.RandomNumbers[this.rTPos];
					this.rTPos++;
					if (this.rTPos == 512) {
						this.rTPos = 0;
					}
				}
				this.rNToGo--;
				this.ch2 ^= (int)((this.rNToGo == 1) ? 1 : 0);
				this.i2++;

				this.currentChar = this.ch2;
				this.currentState = RAND_PART_B_STATE;
				this.mCrc.Update(this.ch2);
			} else {
				this.EndBlock();
				this.InitBlock();
				this.SetupBlock();
			}
		}

		void SetupNoRandPartA()
		{
			if (this.i2 <= this.last) {
				this.chPrev = this.ch2;
				this.ch2 = this.ll8[this.tPos];
				this.tPos = this.tt[this.tPos];
				this.i2++;

				this.currentChar = this.ch2;
				this.currentState = NO_RAND_PART_B_STATE;
				this.mCrc.Update(this.ch2);
			} else {
				this.EndBlock();
				this.InitBlock();
				this.SetupBlock();
			}
		}

		void SetupRandPartB()
		{
			if (this.ch2 != this.chPrev) {
				this.currentState = RAND_PART_A_STATE;
				this.count = 1;
				this.SetupRandPartA();
			} else {
				this.count++;
				if (this.count >= 4) {
					this.z = this.ll8[this.tPos];
					this.tPos = this.tt[this.tPos];
					if (this.rNToGo == 0) {
						this.rNToGo = BZip2Constants.RandomNumbers[this.rTPos];
						this.rTPos++;
						if (this.rTPos == 512) {
							this.rTPos = 0;
						}
					}
					this.rNToGo--;
					this.z ^= (byte)((this.rNToGo == 1) ? 1 : 0);
					this.j2 = 0;
					this.currentState = RAND_PART_C_STATE;
					this.SetupRandPartC();
				} else {
					this.currentState = RAND_PART_A_STATE;
					this.SetupRandPartA();
				}
			}
		}

		void SetupRandPartC()
		{
			if (this.j2 < (int)this.z) {
				this.currentChar = this.ch2;
				this.mCrc.Update(this.ch2);
				this.j2++;
			} else {
				this.currentState = RAND_PART_A_STATE;
				this.i2++;
				this.count = 0;
				this.SetupRandPartA();
			}
		}

		void SetupNoRandPartB()
		{
			if (this.ch2 != this.chPrev) {
				this.currentState = NO_RAND_PART_A_STATE;
				this.count = 1;
				this.SetupNoRandPartA();
			} else {
				this.count++;
				if (this.count >= 4) {
					this.z = this.ll8[this.tPos];
					this.tPos = this.tt[this.tPos];
					this.currentState = NO_RAND_PART_C_STATE;
					this.j2 = 0;
					this.SetupNoRandPartC();
				} else {
					this.currentState = NO_RAND_PART_A_STATE;
					this.SetupNoRandPartA();
				}
			}
		}

		void SetupNoRandPartC()
		{
			if (this.j2 < (int)this.z) {
				this.currentChar = this.ch2;
				this.mCrc.Update(this.ch2);
				this.j2++;
			} else {
				this.currentState = NO_RAND_PART_A_STATE;
				this.i2++;
				this.count = 0;
				this.SetupNoRandPartA();
			}
		}

		void SetDecompressStructureSizes(int newSize100k)
		{
			if (!(0 <= newSize100k && newSize100k <= 9 && 0 <= this.blockSize100k && this.blockSize100k <= 9)) {
				throw new BZip2Exception("Invalid block size");
			}

			this.blockSize100k = newSize100k;

			if (newSize100k == 0) {
				return;
			}

			int n = BZip2Constants.BaseBlockSize * newSize100k;
			this.ll8 = new byte[n];
			this.tt = new int[n];
		}

		static void CompressedStreamEOF()
		{
			throw new EndOfStreamException("BZip2 input stream end of compressed stream");
		}

		static void BlockOverrun()
		{
			throw new BZip2Exception("BZip2 input stream block overrun");
		}

		static void BadBlockHeader()
		{
			throw new BZip2Exception("BZip2 input stream bad block header");
		}

		static void CrcError()
		{
			throw new BZip2Exception("BZip2 input stream crc error");
		}

		static void HbCreateDecodeTables(int[] limit, int[] baseArray, int[] perm, char[] length, int minLen, int maxLen, int alphaSize)
		{
			int pp = 0;

			for (int i = minLen; i <= maxLen; ++i) {
				for (int j = 0; j < alphaSize; ++j) {
					if (length[j] == i) {
						perm[pp] = j;
						++pp;
					}
				}
			}

			for (int i = 0; i < BZip2Constants.MaximumCodeLength; i++) {
				baseArray[i] = 0;
			}

			for (int i = 0; i < alphaSize; i++) {
				++baseArray[length[i] + 1];
			}

			for (int i = 1; i < BZip2Constants.MaximumCodeLength; i++) {
				baseArray[i] += baseArray[i - 1];
			}

			for (int i = 0; i < BZip2Constants.MaximumCodeLength; i++) {
				limit[i] = 0;
			}

			int vec = 0;

			for (int i = minLen; i <= maxLen; i++) {
				vec += (baseArray[i + 1] - baseArray[i]);
				limit[i] = vec - 1;
				vec <<= 1;
			}

			for (int i = minLen + 1; i <= maxLen; i++) {
				baseArray[i] = ((limit[i - 1] + 1) << 1) - baseArray[i];
			}
		}

		#region Instance Fields
		/*--
		index of the last char in the block, so
		the block size == last + 1.
		--*/
		int last;

		/*--
		index in zptr[] of original string after sorting.
		--*/
		int origPtr;

		/*--
		always: in the range 0 .. 9.
		The current block size is 100000 * this number.
		--*/
		int blockSize100k;

		bool blockRandomised;

		int bsBuff;
		int bsLive;
		IChecksum mCrc = new BZip2Crc();

		bool[] inUse = new bool[256];
		int nInUse;

		byte[] seqToUnseq = new byte[256];
		byte[] unseqToSeq = new byte[256];

		byte[] selector = new byte[BZip2Constants.MaximumSelectors];
		byte[] selectorMtf = new byte[BZip2Constants.MaximumSelectors];

		int[] tt;
		byte[] ll8;

		/*--
		freq table collected to save a pass over the data
		during decompression.
		--*/
		int[] unzftab = new int[256];

		int[][] limit = new int[BZip2Constants.GroupCount][];
		int[][] baseArray = new int[BZip2Constants.GroupCount][];
		int[][] perm = new int[BZip2Constants.GroupCount][];
		int[] minLens = new int[BZip2Constants.GroupCount];

		Stream baseStream;
		bool streamEnd;

		int currentChar = -1;

		int currentState = START_BLOCK_STATE;

		int storedBlockCRC, storedCombinedCRC;
		int computedBlockCRC;
		uint computedCombinedCRC;

		int count, chPrev, ch2;
		int tPos;
		int rNToGo;
		int rTPos;
		int i2, j2;
		byte z;
		bool isStreamOwner = true;
		#endregion
	}
}
