using System;
using System.IO;

using WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Checksum;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.BZip2
{
	/// <summary>
	/// An output stream that compresses into the BZip2 format 
	/// including file header chars into another stream.
	/// </summary>
	public class BZip2OutputStream : Stream
	{
		#region Constants
		const int SETMASK = (1 << 21);
		const int CLEARMASK = (~SETMASK);
		const int GREATER_ICOST = 15;
		const int LESSER_ICOST = 0;
		const int SMALL_THRESH = 20;
		const int DEPTH_THRESH = 10;

		/*--
		If you are ever unlucky/improbable enough
		to get a stack overflow whilst sorting,
		increase the following constant and try
		again.  In practice I have never seen the
		stack go above 27 elems, so the following
		limit seems very generous.
		--*/
		const int QSORT_STACK_SIZE = 1000;

		/*--
		Knuth's increments seem to work better
		than Incerpi-Sedgewick here.  Possibly
		because the number of elems to sort is
		usually small, typically <= 20.
		--*/
		readonly int[] increments = {
												  1, 4, 13, 40, 121, 364, 1093, 3280,
												  9841, 29524, 88573, 265720,
												  797161, 2391484
											  };
		#endregion

		#region Constructors
		/// <summary>
		/// Construct a default output stream with maximum block size
		/// </summary>
		/// <param name="stream">The stream to write BZip data onto.</param>
		public BZip2OutputStream(Stream stream) : this(stream, 9)
		{
		}

		/// <summary>
		/// Initialise a new instance of the <see cref="BZip2OutputStream"></see> 
		/// for the specified stream, using the given blocksize.
		/// </summary>
		/// <param name="stream">The stream to write compressed data to.</param>
		/// <param name="blockSize">The block size to use.</param>
		/// <remarks>
		/// Valid block sizes are in the range 1..9, with 1 giving 
		/// the lowest compression and 9 the highest.
		/// </remarks>
		public BZip2OutputStream(Stream stream, int blockSize)
		{
			this.BsSetStream(stream);

			this.workFactor = 50;
			if (blockSize > 9) {
				blockSize = 9;
			}

			if (blockSize < 1) {
				blockSize = 1;
			}
			this.blockSize100k = blockSize;
			this.AllocateCompressStructures();
			this.Initialize();
			this.InitBlock();
		}
		#endregion

		#region Destructor
		/// <summary>
		/// Ensures that resources are freed and other cleanup operations 
		/// are performed when the garbage collector reclaims the BZip2OutputStream.
		/// </summary>
		~BZip2OutputStream()
		{
			this.Dispose(false);
		}
		#endregion

		/// <summary>
		/// Get/set flag indicating ownership of underlying stream.
		/// When the flag is true <see cref="Close"></see> will close the underlying stream also.
		/// </summary>
		public bool IsStreamOwner
		{
			get { return this.isStreamOwner; }
			set { this.isStreamOwner = value; }
		}


		#region Stream overrides
		/// <summary>
		/// Gets a value indicating whether the current stream supports reading
		/// </summary>
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking
		/// </summary>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing
		/// </summary>
		public override bool CanWrite
		{
			get
			{
				return this.baseStream.CanWrite;
			}
		}

		/// <summary>
		/// Gets the length in bytes of the stream
		/// </summary>
		public override long Length
		{
			get
			{
				return this.baseStream.Length;
			}
		}

		/// <summary>
		/// Gets or sets the current position of this stream.
		/// </summary>
		public override long Position
		{
			get
			{
				return this.baseStream.Position;
			}
			set
			{
				throw new NotSupportedException("BZip2OutputStream position cannot be set");
			}
		}

		/// <summary>
		/// Sets the current position of this stream to the given value.
		/// </summary>
		/// <param name="offset">The point relative to the offset from which to being seeking.</param>
		/// <param name="origin">The reference point from which to begin seeking.</param>
		/// <returns>The new position in the stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("BZip2OutputStream Seek not supported");
		}

		/// <summary>
		/// Sets the length of this stream to the given value.
		/// </summary>
		/// <param name="value">The new stream length.</param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("BZip2OutputStream SetLength not supported");
		}

		/// <summary>
		/// Read a byte from the stream advancing the position.
		/// </summary>
		/// <returns>The byte read cast to an int; -1 if end of stream.</returns>
		public override int ReadByte()
		{
			throw new NotSupportedException("BZip2OutputStream ReadByte not supported");
		}

		/// <summary>
		/// Read a block of bytes
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to start storing data at.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>The total number of bytes read. This might be less than the number of bytes
		/// requested if that number of bytes are not currently available, or zero 
		/// if the end of the stream is reached.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("BZip2OutputStream Read not supported");
		}

		/// <summary>
		/// Write a block of bytes to the stream
		/// </summary>
		/// <param name="buffer">The buffer containing data to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (buffer == null) {
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0) {
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 0) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (buffer.Length - offset < count) {
				throw new ArgumentException("Offset/count out of range");
			}

			for (int i = 0; i < count; ++i) {
				this.WriteByte(buffer[offset + i]);
			}
		}

		/// <summary>
		/// Write a byte to the stream.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		public override void WriteByte(byte value)
		{
			int b = (256 + value) % 256;
			if (this.currentChar != -1) {
				if (this.currentChar == b) {
					this.runLength++;
					if (this.runLength > 254) {
						this.WriteRun();
						this.currentChar = -1;
						this.runLength = 0;
					}
				} else {
					this.WriteRun();
					this.runLength = 1;
					this.currentChar = b;
				}
			} else {
				this.currentChar = b;
				this.runLength++;
			}
		}

		#endregion
		void MakeMaps()
		{
			this.nInUse = 0;
			for (int i = 0; i < 256; i++) {
				if (this.inUse[i]) {
					this.seqToUnseq[this.nInUse] = (char)i;
					this.unseqToSeq[i] = (char)this.nInUse;
					this.nInUse++;
				}
			}
		}

		/// <summary>
		/// Get the number of bytes written to output.
		/// </summary>
		void WriteRun()
		{
			if (this.last < this.allowableBlockSize) {
				this.inUse[this.currentChar] = true;
				for (int i = 0; i < this.runLength; i++) {
					this.mCrc.Update(this.currentChar);
				}

				switch (this.runLength) {
				case 1:
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					break;
				case 2:
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					break;
				case 3:
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					break;
				default:
					this.inUse[this.runLength - 4] = true;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)this.currentChar;
					this.last++;
					this.block[this.last + 1] = (byte)(this.runLength - 4);
					break;
				}
			} else {
				this.EndBlock();
				this.InitBlock();
				this.WriteRun();
			}
		}

		/// <summary>
		/// Get the number of bytes written to the output.
		/// </summary>
		public int BytesWritten
		{
			get { return this.bytesOut; }
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="BZip2OutputStream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		override protected void Dispose(bool disposing)
		{
			try {
				try {
					base.Dispose(disposing);
					if (!this.disposed_) {
						this.disposed_ = true;

						if (this.runLength > 0) {
							this.WriteRun();
						}

						this.currentChar = -1;
						this.EndBlock();
						this.EndCompression();
						this.Flush();
					}
				} finally {
					if (disposing) {
						if (this.IsStreamOwner) {
							this.baseStream.Dispose();
						}
					}
				}
			} catch {
			}
		}

		/// <summary>
		/// Flush output buffers
		/// </summary>		
		public override void Flush()
		{
			this.baseStream.Flush();
		}

		void Initialize()
		{
			this.bytesOut = 0;
			this.nBlocksRandomised = 0;

			/*--- Write header `magic' bytes indicating file-format == huffmanised,
			followed by a digit indicating blockSize100k.
			---*/

			this.BsPutUChar('B');
			this.BsPutUChar('Z');

			this.BsPutUChar('h');
			this.BsPutUChar('0' + this.blockSize100k);

			this.combinedCRC = 0;
		}

		void InitBlock()
		{
			this.mCrc.Reset();
			this.last = -1;

			for (int i = 0; i < 256; i++) {
				this.inUse[i] = false;
			}

			/*--- 20 is just a paranoia constant ---*/
			this.allowableBlockSize = BZip2Constants.BaseBlockSize * this.blockSize100k - 20;
		}

		void EndBlock()
		{
			if (this.last < 0) {       // dont do anything for empty files, (makes empty files compatible with original Bzip)
				return;
			}

			this.blockCRC = unchecked((uint)this.mCrc.Value);
			this.combinedCRC = (this.combinedCRC << 1) | (this.combinedCRC >> 31);
			this.combinedCRC ^= this.blockCRC;

			/*-- sort the block and establish position of original string --*/
			this.DoReversibleTransformation();

			/*--
			A 6-byte block header, the value chosen arbitrarily
			as 0x314159265359 :-).  A 32 bit value does not really
			give a strong enough guarantee that the value will not
			appear by chance in the compressed datastream.  Worst-case
			probability of this event, for a 900k block, is about
			2.0e-3 for 32 bits, 1.0e-5 for 40 bits and 4.0e-8 for 48 bits.
			For a compressed file of size 100Gb -- about 100000 blocks --
			only a 48-bit marker will do.  NB: normal compression/
			decompression do *not* rely on these statistical properties.
			They are only important when trying to recover blocks from
			damaged files.
			--*/
			this.BsPutUChar(0x31);
			this.BsPutUChar(0x41);
			this.BsPutUChar(0x59);
			this.BsPutUChar(0x26);
			this.BsPutUChar(0x53);
			this.BsPutUChar(0x59);

			/*-- Now the block's CRC, so it is in a known place. --*/
			unchecked {
				this.BsPutint((int)this.blockCRC);
			}

			/*-- Now a single bit indicating randomisation. --*/
			if (this.blockRandomised) {
				this.BsW(1, 1);
				this.nBlocksRandomised++;
			} else {
				this.BsW(1, 0);
			}

			/*-- Finally, block's contents proper. --*/
			this.MoveToFrontCodeAndSend();
		}

		void EndCompression()
		{
			/*--
			Now another magic 48-bit number, 0x177245385090, to
			indicate the end of the last block.  (sqrt(pi), if
			you want to know.  I did want to use e, but it contains
			too much repetition -- 27 18 28 18 28 46 -- for me
			to feel statistically comfortable.  Call me paranoid.)
			--*/
			this.BsPutUChar(0x17);
			this.BsPutUChar(0x72);
			this.BsPutUChar(0x45);
			this.BsPutUChar(0x38);
			this.BsPutUChar(0x50);
			this.BsPutUChar(0x90);

			unchecked {
				this.BsPutint((int)this.combinedCRC);
			}

			this.BsFinishedWithStream();
		}

		void BsSetStream(Stream stream)
		{
			this.baseStream = stream;
			this.bsLive = 0;
			this.bsBuff = 0;
			this.bytesOut = 0;
		}

		void BsFinishedWithStream()
		{
			while (this.bsLive > 0) {
				int ch = (this.bsBuff >> 24);
				this.baseStream.WriteByte((byte)ch); // write 8-bit
				this.bsBuff <<= 8;
				this.bsLive -= 8;
				this.bytesOut++;
			}
		}

		void BsW(int n, int v)
		{
			while (this.bsLive >= 8) {
				int ch = (this.bsBuff >> 24);
				unchecked { this.baseStream.WriteByte((byte)ch); } // write 8-bit
				this.bsBuff <<= 8;
				this.bsLive -= 8;
				++this.bytesOut;
			}
			this.bsBuff |= (v << (32 - this.bsLive - n));
			this.bsLive += n;
		}

		void BsPutUChar(int c)
		{
			this.BsW(8, c);
		}

		void BsPutint(int u)
		{
			this.BsW(8, (u >> 24) & 0xFF);
			this.BsW(8, (u >> 16) & 0xFF);
			this.BsW(8, (u >> 8) & 0xFF);
			this.BsW(8, u & 0xFF);
		}

		void BsPutIntVS(int numBits, int c)
		{
			this.BsW(numBits, c);
		}

		void SendMTFValues()
		{
			char[][] len = new char[BZip2Constants.GroupCount][];
			for (int i = 0; i < BZip2Constants.GroupCount; ++i) {
				len[i] = new char[BZip2Constants.MaximumAlphaSize];
			}

			int gs, ge, totc, bt, bc, iter;
			int nSelectors = 0, alphaSize, minLen, maxLen, selCtr;
			int nGroups;

			alphaSize = this.nInUse + 2;
			for (int t = 0; t < BZip2Constants.GroupCount; t++) {
				for (int v = 0; v < alphaSize; v++) {
					len[t][v] = (char)GREATER_ICOST;
				}
			}

			/*--- Decide how many coding tables to use ---*/
			if (this.nMTF <= 0) {
				Panic();
			}

			if (this.nMTF < 200) {
				nGroups = 2;
			} else if (this.nMTF < 600) {
				nGroups = 3;
			} else if (this.nMTF < 1200) {
				nGroups = 4;
			} else if (this.nMTF < 2400) {
				nGroups = 5;
			} else {
				nGroups = 6;
			}

			/*--- Generate an initial set of coding tables ---*/
			int nPart = nGroups;
			int remF = this.nMTF;
			gs = 0;
			while (nPart > 0) {
				int tFreq = remF / nPart;
				int aFreq = 0;
				ge = gs - 1;
				while (aFreq < tFreq && ge < alphaSize - 1) {
					ge++;
					aFreq += this.mtfFreq[ge];
				}

				if (ge > gs && nPart != nGroups && nPart != 1 && ((nGroups - nPart) % 2 == 1)) {
					aFreq -= this.mtfFreq[ge];
					ge--;
				}

				for (int v = 0; v < alphaSize; v++) {
					if (v >= gs && v <= ge) {
						len[nPart - 1][v] = (char)LESSER_ICOST;
					} else {
						len[nPart - 1][v] = (char)GREATER_ICOST;
					}
				}

				nPart--;
				gs = ge + 1;
				remF -= aFreq;
			}

			int[][] rfreq = new int[BZip2Constants.GroupCount][];
			for (int i = 0; i < BZip2Constants.GroupCount; ++i) {
				rfreq[i] = new int[BZip2Constants.MaximumAlphaSize];
			}

			int[] fave = new int[BZip2Constants.GroupCount];
			short[] cost = new short[BZip2Constants.GroupCount];
			/*---
			Iterate up to N_ITERS times to improve the tables.
			---*/
			for (iter = 0; iter < BZip2Constants.NumberOfIterations; ++iter) {
				for (int t = 0; t < nGroups; ++t) {
					fave[t] = 0;
				}

				for (int t = 0; t < nGroups; ++t) {
					for (int v = 0; v < alphaSize; ++v) {
						rfreq[t][v] = 0;
					}
				}

				nSelectors = 0;
				totc = 0;
				gs = 0;
				while (true) {
					/*--- Set group start & end marks. --*/
					if (gs >= this.nMTF) {
						break;
					}
					ge = gs + BZip2Constants.GroupSize - 1;
					if (ge >= this.nMTF) {
						ge = this.nMTF - 1;
					}

					/*--
					Calculate the cost of this group as coded
					by each of the coding tables.
					--*/
					for (int t = 0; t < nGroups; t++) {
						cost[t] = 0;
					}

					if (nGroups == 6) {
						short cost0, cost1, cost2, cost3, cost4, cost5;
						cost0 = cost1 = cost2 = cost3 = cost4 = cost5 = 0;
						for (int i = gs; i <= ge; ++i) {
							short icv = this.szptr[i];
							cost0 += (short)len[0][icv];
							cost1 += (short)len[1][icv];
							cost2 += (short)len[2][icv];
							cost3 += (short)len[3][icv];
							cost4 += (short)len[4][icv];
							cost5 += (short)len[5][icv];
						}
						cost[0] = cost0;
						cost[1] = cost1;
						cost[2] = cost2;
						cost[3] = cost3;
						cost[4] = cost4;
						cost[5] = cost5;
					} else {
						for (int i = gs; i <= ge; ++i) {
							short icv = this.szptr[i];
							for (int t = 0; t < nGroups; t++) {
								cost[t] += (short)len[t][icv];
							}
						}
					}

					/*--
					Find the coding table which is best for this group,
					and record its identity in the selector table.
					--*/
					bc = 999999999;
					bt = -1;
					for (int t = 0; t < nGroups; ++t) {
						if (cost[t] < bc) {
							bc = cost[t];
							bt = t;
						}
					}
					totc += bc;
					fave[bt]++;
					this.selector[nSelectors] = (char)bt;
					nSelectors++;

					/*--
					Increment the symbol frequencies for the selected table.
					--*/
					for (int i = gs; i <= ge; ++i) {
						++rfreq[bt][this.szptr[i]];
					}

					gs = ge + 1;
				}

				/*--
				Recompute the tables based on the accumulated frequencies.
				--*/
				for (int t = 0; t < nGroups; ++t) {
					HbMakeCodeLengths(len[t], rfreq[t], alphaSize, 20);
				}
			}

			rfreq = null;
			fave = null;
			cost = null;

			if (!(nGroups < 8)) {
				Panic();
			}

			if (!(nSelectors < 32768 && nSelectors <= (2 + (900000 / BZip2Constants.GroupSize)))) {
				Panic();
			}

			/*--- Compute MTF values for the selectors. ---*/
			char[] pos = new char[BZip2Constants.GroupCount];
			char ll_i, tmp2, tmp;

			for (int i = 0; i < nGroups; i++) {
				pos[i] = (char)i;
			}

			for (int i = 0; i < nSelectors; i++) {
				ll_i = this.selector[i];
				int j = 0;
				tmp = pos[j];
				while (ll_i != tmp) {
					j++;
					tmp2 = tmp;
					tmp = pos[j];
					pos[j] = tmp2;
				}
				pos[0] = tmp;
				this.selectorMtf[i] = (char)j;
			}

			int[][] code = new int[BZip2Constants.GroupCount][];

			for (int i = 0; i < BZip2Constants.GroupCount; ++i) {
				code[i] = new int[BZip2Constants.MaximumAlphaSize];
			}

			/*--- Assign actual codes for the tables. --*/
			for (int t = 0; t < nGroups; t++) {
				minLen = 32;
				maxLen = 0;
				for (int i = 0; i < alphaSize; i++) {
					if (len[t][i] > maxLen) {
						maxLen = len[t][i];
					}
					if (len[t][i] < minLen) {
						minLen = len[t][i];
					}
				}
				if (maxLen > 20) {
					Panic();
				}
				if (minLen < 1) {
					Panic();
				}
				HbAssignCodes(code[t], len[t], minLen, maxLen, alphaSize);
			}

			/*--- Transmit the mapping table. ---*/
			bool[] inUse16 = new bool[16];
			for (int i = 0; i < 16; ++i) {
				inUse16[i] = false;
				for (int j = 0; j < 16; ++j) {
					if (this.inUse[i * 16 + j]) {
						inUse16[i] = true;
					}
				}
			}

			for (int i = 0; i < 16; ++i) {
				if (inUse16[i]) {
					this.BsW(1, 1);
				} else {
					this.BsW(1, 0);
				}
			}

			for (int i = 0; i < 16; ++i) {
				if (inUse16[i]) {
					for (int j = 0; j < 16; ++j) {
						if (this.inUse[i * 16 + j]) {
							this.BsW(1, 1);
						} else {
							this.BsW(1, 0);
						}
					}
				}
			}

			/*--- Now the selectors. ---*/
			this.BsW(3, nGroups);
			this.BsW(15, nSelectors);
			for (int i = 0; i < nSelectors; ++i) {
				for (int j = 0; j < this.selectorMtf[i]; ++j) {
					this.BsW(1, 1);
				}
				this.BsW(1, 0);
			}

			/*--- Now the coding tables. ---*/
			for (int t = 0; t < nGroups; ++t) {
				int curr = len[t][0];
				this.BsW(5, curr);
				for (int i = 0; i < alphaSize; ++i) {
					while (curr < len[t][i]) {
						this.BsW(2, 2);
						curr++; /* 10 */
					}
					while (curr > len[t][i]) {
						this.BsW(2, 3);
						curr--; /* 11 */
					}
					this.BsW(1, 0);
				}
			}

			/*--- And finally, the block data proper ---*/
			selCtr = 0;
			gs = 0;
			while (true) {
				if (gs >= this.nMTF) {
					break;
				}
				ge = gs + BZip2Constants.GroupSize - 1;
				if (ge >= this.nMTF) {
					ge = this.nMTF - 1;
				}

				for (int i = gs; i <= ge; i++) {
					this.BsW(len[this.selector[selCtr]][this.szptr[i]], code[this.selector[selCtr]][this.szptr[i]]);
				}

				gs = ge + 1;
				++selCtr;
			}
			if (!(selCtr == nSelectors)) {
				Panic();
			}
		}

		void MoveToFrontCodeAndSend()
		{
			this.BsPutIntVS(24, this.origPtr);
			this.GenerateMTFValues();
			this.SendMTFValues();
		}

		void SimpleSort(int lo, int hi, int d)
		{
			int i, j, h, bigN, hp;
			int v;

			bigN = hi - lo + 1;
			if (bigN < 2) {
				return;
			}

			hp = 0;
			while (this.increments[hp] < bigN) {
				hp++;
			}
			hp--;

			for (; hp >= 0; hp--) {
				h = this.increments[hp];

				i = lo + h;
				while (true) {
					/*-- copy 1 --*/
					if (i > hi)
						break;
					v = this.zptr[i];
					j = i;
					while (this.FullGtU(this.zptr[j - h] + d, v + d)) {
						this.zptr[j] = this.zptr[j - h];
						j = j - h;
						if (j <= (lo + h - 1))
							break;
					}
					this.zptr[j] = v;
					i++;

					/*-- copy 2 --*/
					if (i > hi) {
						break;
					}
					v = this.zptr[i];
					j = i;
					while (this.FullGtU(this.zptr[j - h] + d, v + d)) {
						this.zptr[j] = this.zptr[j - h];
						j = j - h;
						if (j <= (lo + h - 1)) {
							break;
						}
					}
					this.zptr[j] = v;
					i++;

					/*-- copy 3 --*/
					if (i > hi) {
						break;
					}
					v = this.zptr[i];
					j = i;
					while (this.FullGtU(this.zptr[j - h] + d, v + d)) {
						this.zptr[j] = this.zptr[j - h];
						j = j - h;
						if (j <= (lo + h - 1)) {
							break;
						}
					}
					this.zptr[j] = v;
					i++;

					if (this.workDone > this.workLimit && this.firstAttempt) {
						return;
					}
				}
			}
		}

		void Vswap(int p1, int p2, int n)
		{
			int temp = 0;
			while (n > 0) {
				temp = this.zptr[p1];
				this.zptr[p1] = this.zptr[p2];
				this.zptr[p2] = temp;
				p1++;
				p2++;
				n--;
			}
		}

		void QSort3(int loSt, int hiSt, int dSt)
		{
			int unLo, unHi, ltLo, gtHi, med, n, m;
			int lo, hi, d;

			StackElement[] stack = new StackElement[QSORT_STACK_SIZE];

			int sp = 0;

			stack[sp].ll = loSt;
			stack[sp].hh = hiSt;
			stack[sp].dd = dSt;
			sp++;

			while (sp > 0) {
				if (sp >= QSORT_STACK_SIZE) {
					Panic();
				}

				sp--;
				lo = stack[sp].ll;
				hi = stack[sp].hh;
				d = stack[sp].dd;

				if (hi - lo < SMALL_THRESH || d > DEPTH_THRESH) {
					this.SimpleSort(lo, hi, d);
					if (this.workDone > this.workLimit && this.firstAttempt) {
						return;
					}
					continue;
				}

				med = Med3(this.block[this.zptr[lo] + d + 1],
						   this.block[this.zptr[hi] + d + 1],
						   this.block[this.zptr[(lo + hi) >> 1] + d + 1]);

				unLo = ltLo = lo;
				unHi = gtHi = hi;

				while (true) {
					while (true) {
						if (unLo > unHi) {
							break;
						}
						n = ((int)this.block[this.zptr[unLo] + d + 1]) - med;
						if (n == 0) {
							int temp = this.zptr[unLo];
							this.zptr[unLo] = this.zptr[ltLo];
							this.zptr[ltLo] = temp;
							ltLo++;
							unLo++;
							continue;
						}
						if (n > 0) {
							break;
						}
						unLo++;
					}

					while (true) {
						if (unLo > unHi) {
							break;
						}
						n = ((int)this.block[this.zptr[unHi] + d + 1]) - med;
						if (n == 0) {
							int temp = this.zptr[unHi];
							this.zptr[unHi] = this.zptr[gtHi];
							this.zptr[gtHi] = temp;
							gtHi--;
							unHi--;
							continue;
						}
						if (n < 0) {
							break;
						}
						unHi--;
					}

					if (unLo > unHi) {
						break;
					}

					{
						int temp = this.zptr[unLo];
						this.zptr[unLo] = this.zptr[unHi];
						this.zptr[unHi] = temp;
						unLo++;
						unHi--;
					}
				}

				if (gtHi < ltLo) {
					stack[sp].ll = lo;
					stack[sp].hh = hi;
					stack[sp].dd = d + 1;
					sp++;
					continue;
				}

				n = ((ltLo - lo) < (unLo - ltLo)) ? (ltLo - lo) : (unLo - ltLo);
				this.Vswap(lo, unLo - n, n);
				m = ((hi - gtHi) < (gtHi - unHi)) ? (hi - gtHi) : (gtHi - unHi);
				this.Vswap(unLo, hi - m + 1, m);

				n = lo + unLo - ltLo - 1;
				m = hi - (gtHi - unHi) + 1;

				stack[sp].ll = lo;
				stack[sp].hh = n;
				stack[sp].dd = d;
				sp++;

				stack[sp].ll = n + 1;
				stack[sp].hh = m - 1;
				stack[sp].dd = d + 1;
				sp++;

				stack[sp].ll = m;
				stack[sp].hh = hi;
				stack[sp].dd = d;
				sp++;
			}
		}

		void MainSort()
		{
			int i, j, ss, sb;
			int[] runningOrder = new int[256];
			int[] copy = new int[256];
			bool[] bigDone = new bool[256];
			int c1, c2;
			int numQSorted;

			/*--
			In the various block-sized structures, live data runs
			from 0 to last+NUM_OVERSHOOT_BYTES inclusive.  First,
			set up the overshoot area for block.
			--*/

			//   if (verbosity >= 4) fprintf ( stderr, "        sort initialise ...\n" );
			for (i = 0; i < BZip2Constants.OvershootBytes; i++) {
				this.block[this.last + i + 2] = this.block[(i % (this.last + 1)) + 1];
			}
			for (i = 0; i <= this.last + BZip2Constants.OvershootBytes; i++) {
				this.quadrant[i] = 0;
			}

			this.block[0] = (byte)(this.block[this.last + 1]);

			if (this.last < 4000) {
				/*--
				Use simpleSort(), since the full sorting mechanism
				has quite a large constant overhead.
				--*/
				for (i = 0; i <= this.last; i++) {
					this.zptr[i] = i;
				}
				this.firstAttempt = false;
				this.workDone = this.workLimit = 0;
				this.SimpleSort(0, this.last, 0);
			} else {
				numQSorted = 0;
				for (i = 0; i <= 255; i++) {
					bigDone[i] = false;
				}
				for (i = 0; i <= 65536; i++) {
					this.ftab[i] = 0;
				}

				c1 = this.block[0];
				for (i = 0; i <= this.last; i++) {
					c2 = this.block[i + 1];
					this.ftab[(c1 << 8) + c2]++;
					c1 = c2;
				}

				for (i = 1; i <= 65536; i++) {
					this.ftab[i] += this.ftab[i - 1];
				}

				c1 = this.block[1];
				for (i = 0; i < this.last; i++) {
					c2 = this.block[i + 2];
					j = (c1 << 8) + c2;
					c1 = c2;
					this.ftab[j]--;
					this.zptr[this.ftab[j]] = i;
				}

				j = ((this.block[this.last + 1]) << 8) + (this.block[1]);
				this.ftab[j]--;
				this.zptr[this.ftab[j]] = this.last;

				/*--
				Now ftab contains the first loc of every small bucket.
				Calculate the running order, from smallest to largest
				big bucket.
				--*/

				for (i = 0; i <= 255; i++) {
					runningOrder[i] = i;
				}

				int vv;
				int h = 1;
				do {
					h = 3 * h + 1;
				} while (h <= 256);
				do {
					h = h / 3;
					for (i = h; i <= 255; i++) {
						vv = runningOrder[i];
						j = i;
						while ((this.ftab[((runningOrder[j - h]) + 1) << 8] - this.ftab[(runningOrder[j - h]) << 8]) > (this.ftab[((vv) + 1) << 8] - this.ftab[(vv) << 8])) {
							runningOrder[j] = runningOrder[j - h];
							j = j - h;
							if (j <= (h - 1)) {
								break;
							}
						}
						runningOrder[j] = vv;
					}
				} while (h != 1);

				/*--
				The main sorting loop.
				--*/
				for (i = 0; i <= 255; i++) {

					/*--
					Process big buckets, starting with the least full.
					--*/
					ss = runningOrder[i];

					/*--
					Complete the big bucket [ss] by quicksorting
					any unsorted small buckets [ss, j].  Hopefully
					previous pointer-scanning phases have already
					completed many of the small buckets [ss, j], so
					we don't have to sort them at all.
					--*/
					for (j = 0; j <= 255; j++) {
						sb = (ss << 8) + j;
						if (!((this.ftab[sb] & SETMASK) == SETMASK)) {
							int lo = this.ftab[sb] & CLEARMASK;
							int hi = (this.ftab[sb + 1] & CLEARMASK) - 1;
							if (hi > lo) {
								this.QSort3(lo, hi, 2);
								numQSorted += (hi - lo + 1);
								if (this.workDone > this.workLimit && this.firstAttempt) {
									return;
								}
							}
							this.ftab[sb] |= SETMASK;
						}
					}

					/*--
					The ss big bucket is now done.  Record this fact,
					and update the quadrant descriptors.  Remember to
					update quadrants in the overshoot area too, if
					necessary.  The "if (i < 255)" test merely skips
					this updating for the last bucket processed, since
					updating for the last bucket is pointless.
					--*/
					bigDone[ss] = true;

					if (i < 255) {
						int bbStart = this.ftab[ss << 8] & CLEARMASK;
						int bbSize = (this.ftab[(ss + 1) << 8] & CLEARMASK) - bbStart;
						int shifts = 0;

						while ((bbSize >> shifts) > 65534) {
							shifts++;
						}

						for (j = 0; j < bbSize; j++) {
							int a2update = this.zptr[bbStart + j];
							int qVal = (j >> shifts);
							this.quadrant[a2update] = qVal;
							if (a2update < BZip2Constants.OvershootBytes) {
								this.quadrant[a2update + this.last + 1] = qVal;
							}
						}

						if (!(((bbSize - 1) >> shifts) <= 65535)) {
							Panic();
						}
					}

					/*--
					Now scan this big bucket so as to synthesise the
					sorted order for small buckets [t, ss] for all t != ss.
					--*/
					for (j = 0; j <= 255; j++) {
						copy[j] = this.ftab[(j << 8) + ss] & CLEARMASK;
					}

					for (j = this.ftab[ss << 8] & CLEARMASK; j < (this.ftab[(ss + 1) << 8] & CLEARMASK); j++) {
						c1 = this.block[this.zptr[j]];
						if (!bigDone[c1]) {
							this.zptr[copy[c1]] = this.zptr[j] == 0 ? this.last : this.zptr[j] - 1;
							copy[c1]++;
						}
					}

					for (j = 0; j <= 255; j++) {
						this.ftab[(j << 8) + ss] |= SETMASK;
					}
				}
			}
		}

		void RandomiseBlock()
		{
			int i;
			int rNToGo = 0;
			int rTPos = 0;
			for (i = 0; i < 256; i++) {
				this.inUse[i] = false;
			}

			for (i = 0; i <= this.last; i++) {
				if (rNToGo == 0) {
					rNToGo = (int)BZip2Constants.RandomNumbers[rTPos];
					rTPos++;
					if (rTPos == 512) {
						rTPos = 0;
					}
				}
				rNToGo--;
				this.block[i + 1] ^= (byte)((rNToGo == 1) ? 1 : 0);
				// handle 16 bit signed numbers
				this.block[i + 1] &= 0xFF;

				this.inUse[this.block[i + 1]] = true;
			}
		}

		void DoReversibleTransformation()
		{
			this.workLimit = this.workFactor * this.last;
			this.workDone = 0;
			this.blockRandomised = false;
			this.firstAttempt = true;

			this.MainSort();

			if (this.workDone > this.workLimit && this.firstAttempt) {
				this.RandomiseBlock();
				this.workLimit = this.workDone = 0;
				this.blockRandomised = true;
				this.firstAttempt = false;
				this.MainSort();
			}

			this.origPtr = -1;
			for (int i = 0; i <= this.last; i++) {
				if (this.zptr[i] == 0) {
					this.origPtr = i;
					break;
				}
			}

			if (this.origPtr == -1) {
				Panic();
			}
		}

		bool FullGtU(int i1, int i2)
		{
			int k;
			byte c1, c2;
			int s1, s2;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			c1 = this.block[i1 + 1];
			c2 = this.block[i2 + 1];
			if (c1 != c2) {
				return c1 > c2;
			}
			i1++;
			i2++;

			k = this.last + 1;

			do {
				c1 = this.block[i1 + 1];
				c2 = this.block[i2 + 1];
				if (c1 != c2) {
					return c1 > c2;
				}
				s1 = this.quadrant[i1];
				s2 = this.quadrant[i2];
				if (s1 != s2) {
					return s1 > s2;
				}
				i1++;
				i2++;

				c1 = this.block[i1 + 1];
				c2 = this.block[i2 + 1];
				if (c1 != c2) {
					return c1 > c2;
				}
				s1 = this.quadrant[i1];
				s2 = this.quadrant[i2];
				if (s1 != s2) {
					return s1 > s2;
				}
				i1++;
				i2++;

				c1 = this.block[i1 + 1];
				c2 = this.block[i2 + 1];
				if (c1 != c2) {
					return c1 > c2;
				}
				s1 = this.quadrant[i1];
				s2 = this.quadrant[i2];
				if (s1 != s2) {
					return s1 > s2;
				}
				i1++;
				i2++;

				c1 = this.block[i1 + 1];
				c2 = this.block[i2 + 1];
				if (c1 != c2) {
					return c1 > c2;
				}
				s1 = this.quadrant[i1];
				s2 = this.quadrant[i2];
				if (s1 != s2) {
					return s1 > s2;
				}
				i1++;
				i2++;

				if (i1 > this.last) {
					i1 -= this.last;
					i1--;
				}
				if (i2 > this.last) {
					i2 -= this.last;
					i2--;
				}

				k -= 4;
				++this.workDone;
			} while (k >= 0);

			return false;
		}

		void AllocateCompressStructures()
		{
			int n = BZip2Constants.BaseBlockSize * this.blockSize100k;
			this.block = new byte[(n + 1 + BZip2Constants.OvershootBytes)];
			this.quadrant = new int[(n + BZip2Constants.OvershootBytes)];
			this.zptr = new int[n];
			this.ftab = new int[65537];

			if (this.block == null || this.quadrant == null || this.zptr == null || this.ftab == null) {
				//		int totalDraw = (n + 1 + NUM_OVERSHOOT_BYTES) + (n + NUM_OVERSHOOT_BYTES) + n + 65537;
				//		compressOutOfMemory ( totalDraw, n );
			}

			/*
			The back end needs a place to store the MTF values
			whilst it calculates the coding tables.  We could
			put them in the zptr array.  However, these values
			will fit in a short, so we overlay szptr at the
			start of zptr, in the hope of reducing the number
			of cache misses induced by the multiple traversals
			of the MTF values when calculating coding tables.
			Seems to improve compression speed by about 1%.
			*/
			//	szptr = zptr;


			this.szptr = new short[2 * n];
		}

		void GenerateMTFValues()
		{
			char[] yy = new char[256];
			int i, j;
			char tmp;
			char tmp2;
			int zPend;
			int wr;
			int EOB;

			this.MakeMaps();
			EOB = this.nInUse + 1;

			for (i = 0; i <= EOB; i++) {
				this.mtfFreq[i] = 0;
			}

			wr = 0;
			zPend = 0;
			for (i = 0; i < this.nInUse; i++) {
				yy[i] = (char)i;
			}


			for (i = 0; i <= this.last; i++) {
				char ll_i;

				ll_i = this.unseqToSeq[this.block[this.zptr[i]]];

				j = 0;
				tmp = yy[j];
				while (ll_i != tmp) {
					j++;
					tmp2 = tmp;
					tmp = yy[j];
					yy[j] = tmp2;
				}
				yy[0] = tmp;

				if (j == 0) {
					zPend++;
				} else {
					if (zPend > 0) {
						zPend--;
						while (true) {
							switch (zPend % 2) {
							case 0:
								this.szptr[wr] = (short)BZip2Constants.RunA;
								wr++;
								this.mtfFreq[BZip2Constants.RunA]++;
								break;
							case 1:
								this.szptr[wr] = (short)BZip2Constants.RunB;
								wr++;
								this.mtfFreq[BZip2Constants.RunB]++;
								break;
							}
							if (zPend < 2) {
								break;
							}
							zPend = (zPend - 2) / 2;
						}
						zPend = 0;
					}
					this.szptr[wr] = (short)(j + 1);
					wr++;
					this.mtfFreq[j + 1]++;
				}
			}

			if (zPend > 0) {
				zPend--;
				while (true) {
					switch (zPend % 2) {
					case 0:
						this.szptr[wr] = (short)BZip2Constants.RunA;
						wr++;
						this.mtfFreq[BZip2Constants.RunA]++;
						break;
					case 1:
						this.szptr[wr] = (short)BZip2Constants.RunB;
						wr++;
						this.mtfFreq[BZip2Constants.RunB]++;
						break;
					}
					if (zPend < 2) {
						break;
					}
					zPend = (zPend - 2) / 2;
				}
			}

			this.szptr[wr] = (short)EOB;
			wr++;
			this.mtfFreq[EOB]++;

			this.nMTF = wr;
		}

		static void Panic()
		{
			throw new BZip2Exception("BZip2 output stream panic");
		}

		static void HbMakeCodeLengths(char[] len, int[] freq, int alphaSize, int maxLen)
		{
			/*--
			Nodes and heap entries run from 1.  Entry 0
			for both the heap and nodes is a sentinel.
			--*/
			int nNodes, nHeap, n1, n2, j, k;
			bool tooLong;

			int[] heap = new int[BZip2Constants.MaximumAlphaSize + 2];
			int[] weight = new int[BZip2Constants.MaximumAlphaSize * 2];
			int[] parent = new int[BZip2Constants.MaximumAlphaSize * 2];

			for (int i = 0; i < alphaSize; ++i) {
				weight[i + 1] = (freq[i] == 0 ? 1 : freq[i]) << 8;
			}

			while (true) {
				nNodes = alphaSize;
				nHeap = 0;

				heap[0] = 0;
				weight[0] = 0;
				parent[0] = -2;

				for (int i = 1; i <= alphaSize; ++i) {
					parent[i] = -1;
					nHeap++;
					heap[nHeap] = i;
					int zz = nHeap;
					int tmp = heap[zz];
					while (weight[tmp] < weight[heap[zz >> 1]]) {
						heap[zz] = heap[zz >> 1];
						zz >>= 1;
					}
					heap[zz] = tmp;
				}
				if (!(nHeap < (BZip2Constants.MaximumAlphaSize + 2))) {
					Panic();
				}

				while (nHeap > 1) {
					n1 = heap[1];
					heap[1] = heap[nHeap];
					nHeap--;
					int zz = 1;
					int yy = 0;
					int tmp = heap[zz];
					while (true) {
						yy = zz << 1;
						if (yy > nHeap) {
							break;
						}
						if (yy < nHeap && weight[heap[yy + 1]] < weight[heap[yy]]) {
							yy++;
						}
						if (weight[tmp] < weight[heap[yy]]) {
							break;
						}

						heap[zz] = heap[yy];
						zz = yy;
					}
					heap[zz] = tmp;
					n2 = heap[1];
					heap[1] = heap[nHeap];
					nHeap--;

					zz = 1;
					yy = 0;
					tmp = heap[zz];
					while (true) {
						yy = zz << 1;
						if (yy > nHeap) {
							break;
						}
						if (yy < nHeap && weight[heap[yy + 1]] < weight[heap[yy]]) {
							yy++;
						}
						if (weight[tmp] < weight[heap[yy]]) {
							break;
						}
						heap[zz] = heap[yy];
						zz = yy;
					}
					heap[zz] = tmp;
					nNodes++;
					parent[n1] = parent[n2] = nNodes;

					weight[nNodes] = (int)((weight[n1] & 0xffffff00) + (weight[n2] & 0xffffff00)) |
						(int)(1 + (((weight[n1] & 0x000000ff) > (weight[n2] & 0x000000ff)) ? (weight[n1] & 0x000000ff) : (weight[n2] & 0x000000ff)));

					parent[nNodes] = -1;
					nHeap++;
					heap[nHeap] = nNodes;

					zz = nHeap;
					tmp = heap[zz];
					while (weight[tmp] < weight[heap[zz >> 1]]) {
						heap[zz] = heap[zz >> 1];
						zz >>= 1;
					}
					heap[zz] = tmp;
				}
				if (!(nNodes < (BZip2Constants.MaximumAlphaSize * 2))) {
					Panic();
				}

				tooLong = false;
				for (int i = 1; i <= alphaSize; ++i) {
					j = 0;
					k = i;
					while (parent[k] >= 0) {
						k = parent[k];
						j++;
					}
					len[i - 1] = (char)j;
					tooLong |= j > maxLen;
				}

				if (!tooLong) {
					break;
				}

				for (int i = 1; i < alphaSize; ++i) {
					j = weight[i] >> 8;
					j = 1 + (j / 2);
					weight[i] = j << 8;
				}
			}
		}

		static void HbAssignCodes(int[] code, char[] length, int minLen, int maxLen, int alphaSize)
		{
			int vec = 0;
			for (int n = minLen; n <= maxLen; ++n) {
				for (int i = 0; i < alphaSize; ++i) {
					if (length[i] == n) {
						code[i] = vec;
						++vec;
					}
				}
				vec <<= 1;
			}
		}

		static byte Med3(byte a, byte b, byte c)
		{
			byte t;
			if (a > b) {
				t = a;
				a = b;
				b = t;
			}
			if (b > c) {
				t = b;
				b = c;
				c = t;
			}
			if (a > b) {
				b = a;
			}
			return b;
		}

		struct StackElement
		{
			public int ll;
			public int hh;
			public int dd;
		}

		#region Instance Fields
		bool isStreamOwner = true;

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

		int bytesOut;
		int bsBuff;
		int bsLive;
		IChecksum mCrc = new BZip2Crc();

		bool[] inUse = new bool[256];
		int nInUse;

		char[] seqToUnseq = new char[256];
		char[] unseqToSeq = new char[256];

		char[] selector = new char[BZip2Constants.MaximumSelectors];
		char[] selectorMtf = new char[BZip2Constants.MaximumSelectors];

		byte[] block;
		int[] quadrant;
		int[] zptr;
		short[] szptr;
		int[] ftab;

		int nMTF;

		int[] mtfFreq = new int[BZip2Constants.MaximumAlphaSize];

		/*
		* Used when sorting.  If too many long comparisons
		* happen, we stop sorting, randomise the block
		* slightly, and try again.
		*/
		int workFactor;
		int workDone;
		int workLimit;
		bool firstAttempt;
		int nBlocksRandomised;

		int currentChar = -1;
		int runLength;
		uint blockCRC, combinedCRC;
		int allowableBlockSize;
		Stream baseStream;
		bool disposed_;
		#endregion
	}
}
