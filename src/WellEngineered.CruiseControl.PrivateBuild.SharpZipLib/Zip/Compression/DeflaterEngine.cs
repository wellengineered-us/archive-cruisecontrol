using System;

using WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Checksum;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Zip.Compression
{
	/// <summary>
	/// Strategies for deflater
	/// </summary>
	public enum DeflateStrategy
	{
		/// <summary>
		/// The default strategy
		/// </summary>
		Default = 0,

		/// <summary>
		/// This strategy will only allow longer string repetitions.  It is
		/// useful for random data with a small character set.
		/// </summary>
		Filtered = 1,


		/// <summary>
		/// This strategy will not look for string repetitions at all.  It
		/// only encodes with Huffman trees (which means, that more common
		/// characters get a smaller encoding.
		/// </summary>
		HuffmanOnly = 2
	}

	// DEFLATE ALGORITHM:
	// 
	// The uncompressed stream is inserted into the window array.  When
	// the window array is full the first half is thrown away and the
	// second half is copied to the beginning.
	//
	// The head array is a hash table.  Three characters build a hash value
	// and they the value points to the corresponding index in window of 
	// the last string with this hash.  The prev array implements a
	// linked list of matches with the same hash: prev[index & WMASK] points
	// to the previous index with the same hash.
	// 


	/// <summary>
	/// Low level compression engine for deflate algorithm which uses a 32K sliding window
	/// with secondary compression from Huffman/Shannon-Fano codes.
	/// </summary>
	public class DeflaterEngine
	{
		#region Constants
		const int TooFar = 4096;
		#endregion

		#region Constructors
		/// <summary>
		/// Construct instance with pending buffer
		/// </summary>
		/// <param name="pending">
		/// Pending buffer to use
		/// </param>>
		public DeflaterEngine(DeflaterPending pending)
		{
			this.pending = pending;
			this.huffman = new DeflaterHuffman(pending);
			this.adler = new Adler32();

			this.window = new byte[2 * DeflaterConstants.WSIZE];
			this.head = new short[DeflaterConstants.HASH_SIZE];
			this.prev = new short[DeflaterConstants.WSIZE];

			// We start at index 1, to avoid an implementation deficiency, that
			// we cannot build a repeat pattern at index 0.
			this.blockStart = this.strstart = 1;
		}

		#endregion

		/// <summary>
		/// Deflate drives actual compression of data
		/// </summary>
		/// <param name="flush">True to flush input buffers</param>
		/// <param name="finish">Finish deflation with the current input.</param>
		/// <returns>Returns true if progress has been made.</returns>
		public bool Deflate(bool flush, bool finish)
		{
			bool progress;
			do {
				this.FillWindow();
				bool canFlush = flush && (this.inputOff == this.inputEnd);

#if DebugDeflation
				if (DeflaterConstants.DEBUGGING) {
					Console.WriteLine("window: [" + blockStart + "," + strstart + ","
								+ lookahead + "], " + compressionFunction + "," + canFlush);
				}
#endif
				switch (this.compressionFunction) {
					case DeflaterConstants.DEFLATE_STORED:
						progress = this.DeflateStored(canFlush, finish);
						break;
					case DeflaterConstants.DEFLATE_FAST:
						progress = this.DeflateFast(canFlush, finish);
						break;
					case DeflaterConstants.DEFLATE_SLOW:
						progress = this.DeflateSlow(canFlush, finish);
						break;
					default:
						throw new InvalidOperationException("unknown compressionFunction");
				}
			} while (this.pending.IsFlushed && progress); // repeat while we have no pending output and progress was made
			return progress;
		}

		/// <summary>
		/// Sets input data to be deflated.  Should only be called when <code>NeedsInput()</code>
		/// returns true
		/// </summary>
		/// <param name="buffer">The buffer containing input data.</param>
		/// <param name="offset">The offset of the first byte of data.</param>
		/// <param name="count">The number of bytes of data to use as input.</param>
		public void SetInput(byte[] buffer, int offset, int count)
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

			if (this.inputOff < this.inputEnd) {
				throw new InvalidOperationException("Old input was not completely processed");
			}

			int end = offset + count;

			/* We want to throw an ArrayIndexOutOfBoundsException early.  The
			* check is very tricky: it also handles integer wrap around.
			*/
			if ((offset > end) || (end > buffer.Length)) {
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			this.inputBuf = buffer;
			this.inputOff = offset;
			this.inputEnd = end;
		}

		/// <summary>
		/// Determines if more <see cref="SetInput">input</see> is needed.
		/// </summary>		
		/// <returns>Return true if input is needed via <see cref="SetInput">SetInput</see></returns>
		public bool NeedsInput()
		{
			return (this.inputEnd == this.inputOff);
		}

		/// <summary>
		/// Set compression dictionary
		/// </summary>
		/// <param name="buffer">The buffer containing the dictionary data</param>
		/// <param name="offset">The offset in the buffer for the first byte of data</param>
		/// <param name="length">The length of the dictionary data.</param>
		public void SetDictionary(byte[] buffer, int offset, int length)
		{
#if DebugDeflation
			if (DeflaterConstants.DEBUGGING && (strstart != 1) ) 
			{
				throw new InvalidOperationException("strstart not 1");
			}
#endif
			this.adler.Update(buffer, offset, length);
			if (length < DeflaterConstants.MIN_MATCH) {
				return;
			}

			if (length > DeflaterConstants.MAX_DIST) {
				offset += length - DeflaterConstants.MAX_DIST;
				length = DeflaterConstants.MAX_DIST;
			}

			System.Array.Copy(buffer, offset, this.window, this.strstart, length);

			this.UpdateHash();
			--length;
			while (--length > 0) {
				this.InsertString();
				this.strstart++;
			}
			this.strstart += 2;
			this.blockStart = this.strstart;
		}

		/// <summary>
		/// Reset internal state
		/// </summary>		
		public void Reset()
		{
			this.huffman.Reset();
			this.adler.Reset();
			this.blockStart = this.strstart = 1;
			this.lookahead = 0;
			this.totalIn = 0;
			this.prevAvailable = false;
			this.matchLen = DeflaterConstants.MIN_MATCH - 1;

			for (int i = 0; i < DeflaterConstants.HASH_SIZE; i++) {
				this.head[i] = 0;
			}

			for (int i = 0; i < DeflaterConstants.WSIZE; i++) {
				this.prev[i] = 0;
			}
		}

		/// <summary>
		/// Reset Adler checksum
		/// </summary>		
		public void ResetAdler()
		{
			this.adler.Reset();
		}

		/// <summary>
		/// Get current value of Adler checksum
		/// </summary>		
		public int Adler {
			get {
				return unchecked((int)this.adler.Value);
			}
		}

		/// <summary>
		/// Total data processed
		/// </summary>		
		public long TotalIn {
			get {
				return this.totalIn;
			}
		}

		/// <summary>
		/// Get/set the <see cref="DeflateStrategy">deflate strategy</see>
		/// </summary>		
		public DeflateStrategy Strategy {
			get {
				return this.strategy;
			}
			set {
				this.strategy = value;
			}
		}

		/// <summary>
		/// Set the deflate level (0-9)
		/// </summary>
		/// <param name="level">The value to set the level to.</param>
		public void SetLevel(int level)
		{
			if ((level < 0) || (level > 9)) {
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			this.goodLength = DeflaterConstants.GOOD_LENGTH[level];
			this.max_lazy = DeflaterConstants.MAX_LAZY[level];
			this.niceLength = DeflaterConstants.NICE_LENGTH[level];
			this.max_chain = DeflaterConstants.MAX_CHAIN[level];

			if (DeflaterConstants.COMPR_FUNC[level] != this.compressionFunction) {

#if DebugDeflation
				if (DeflaterConstants.DEBUGGING) {
				   Console.WriteLine("Change from " + compressionFunction + " to "
										  + DeflaterConstants.COMPR_FUNC[level]);
				}
#endif
				switch (this.compressionFunction) {
					case DeflaterConstants.DEFLATE_STORED:
						if (this.strstart > this.blockStart) {
							this.huffman.FlushStoredBlock(this.window, this.blockStart,
								this.strstart - this.blockStart, false);
							this.blockStart = this.strstart;
						}
						this.UpdateHash();
						break;

					case DeflaterConstants.DEFLATE_FAST:
						if (this.strstart > this.blockStart) {
							this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart,
								false);
							this.blockStart = this.strstart;
						}
						break;

					case DeflaterConstants.DEFLATE_SLOW:
						if (this.prevAvailable) {
							this.huffman.TallyLit(this.window[this.strstart - 1] & 0xff);
						}
						if (this.strstart > this.blockStart) {
							this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, false);
							this.blockStart = this.strstart;
						}
						this.prevAvailable = false;
						this.matchLen = DeflaterConstants.MIN_MATCH - 1;
						break;
				}
				this.compressionFunction = DeflaterConstants.COMPR_FUNC[level];
			}
		}

		/// <summary>
		/// Fill the window
		/// </summary>
		public void FillWindow()
		{
			/* If the window is almost full and there is insufficient lookahead,
			 * move the upper half to the lower one to make room in the upper half.
			 */
			if (this.strstart >= DeflaterConstants.WSIZE + DeflaterConstants.MAX_DIST) {
				this.SlideWindow();
			}

			/* If there is not enough lookahead, but still some input left,
			 * read in the input
			 */
			if (this.lookahead < DeflaterConstants.MIN_LOOKAHEAD && this.inputOff < this.inputEnd) {
				int more = 2 * DeflaterConstants.WSIZE - this.lookahead - this.strstart;

				if (more > this.inputEnd - this.inputOff) {
					more = this.inputEnd - this.inputOff;
				}

				System.Array.Copy(this.inputBuf, this.inputOff, this.window, this.strstart + this.lookahead, more);
				this.adler.Update(this.inputBuf, this.inputOff, more);

				this.inputOff += more;
				this.totalIn += more;
				this.lookahead += more;
			}

			if (this.lookahead >= DeflaterConstants.MIN_MATCH) {
				this.UpdateHash();
			}
		}

		void UpdateHash()
		{
			/*
						if (DEBUGGING) {
							Console.WriteLine("updateHash: "+strstart);
						}
			*/
			this.ins_h = (this.window[this.strstart] << DeflaterConstants.HASH_SHIFT) ^ this.window[this.strstart + 1];
		}

		/// <summary>
		/// Inserts the current string in the head hash and returns the previous
		/// value for this hash.
		/// </summary>
		/// <returns>The previous hash value</returns>
		int InsertString()
		{
			short match;
			int hash = ((this.ins_h << DeflaterConstants.HASH_SHIFT) ^ this.window[this.strstart + (DeflaterConstants.MIN_MATCH - 1)]) & DeflaterConstants.HASH_MASK;

#if DebugDeflation
			if (DeflaterConstants.DEBUGGING) 
			{
				if (hash != (((window[strstart] << (2*HASH_SHIFT)) ^ 
								  (window[strstart + 1] << HASH_SHIFT) ^ 
								  (window[strstart + 2])) & HASH_MASK)) {
						throw new SharpZipBaseException("hash inconsistent: " + hash + "/"
												+window[strstart] + ","
												+window[strstart + 1] + ","
												+window[strstart + 2] + "," + HASH_SHIFT);
					}
			}
#endif
			this.prev[this.strstart & DeflaterConstants.WMASK] = match = this.head[hash];
			this.head[hash] = unchecked((short)this.strstart);
			this.ins_h = hash;
			return match & 0xffff;
		}

		void SlideWindow()
		{
			Array.Copy(this.window, DeflaterConstants.WSIZE, this.window, 0, DeflaterConstants.WSIZE);
			this.matchStart -= DeflaterConstants.WSIZE;
			this.strstart -= DeflaterConstants.WSIZE;
			this.blockStart -= DeflaterConstants.WSIZE;

			// Slide the hash table (could be avoided with 32 bit values
			// at the expense of memory usage).
			for (int i = 0; i < DeflaterConstants.HASH_SIZE; ++i) {
				int m = this.head[i] & 0xffff;
				this.head[i] = (short)(m >= DeflaterConstants.WSIZE ? (m - DeflaterConstants.WSIZE) : 0);
			}

			// Slide the prev table.
			for (int i = 0; i < DeflaterConstants.WSIZE; i++) {
				int m = this.prev[i] & 0xffff;
				this.prev[i] = (short)(m >= DeflaterConstants.WSIZE ? (m - DeflaterConstants.WSIZE) : 0);
			}
		}

		/// <summary>
		/// Find the best (longest) string in the window matching the 
		/// string starting at strstart.
		///
		/// Preconditions:
		/// <code>
		/// strstart + DeflaterConstants.MAX_MATCH &lt;= window.length.</code>
		/// </summary>
		/// <param name="curMatch"></param>
		/// <returns>True if a match greater than the minimum length is found</returns>
		bool FindLongestMatch( int curMatch )
		{
        int match;
        int scan = this.strstart;
        // scanMax is the highest position that we can look at
        int scanMax = scan + Math.Min( DeflaterConstants.MAX_MATCH, this.lookahead ) - 1;
        int limit = Math.Max( scan - DeflaterConstants.MAX_DIST, 0 );

        byte[] window = this.window;
        short[] prev = this.prev;
        int chainLength = this.max_chain;
        int niceLength = Math.Min( this.niceLength, this.lookahead );

          this.matchLen = Math.Max( this.matchLen, DeflaterConstants.MIN_MATCH - 1 );

          if (scan + this.matchLen > scanMax) return false;

        byte scan_end1 = window[scan + this.matchLen - 1];
        byte scan_end = window[scan + this.matchLen];

          // Do not waste too much time if we already have a good match:
          if (this.matchLen >= this.goodLength) chainLength >>= 2;

          do
          {
            match = curMatch;
            scan = this.strstart;

            if (window[match + this.matchLen] != scan_end
             || window[match + this.matchLen - 1] != scan_end1
             || window[match] != window[scan]
             || window[++match] != window[++scan])
            {
              continue;
            }

            // scan is set to strstart+1 and the comparison passed, so
            // scanMax - scan is the maximum number of bytes we can compare.
            // below we compare 8 bytes at a time, so first we compare
            // (scanMax - scan) % 8 bytes, so the remainder is a multiple of 8

            switch( (scanMax - scan) % 8 )
            {
            case 1: if (window[++scan] == window[++match]) break;
              break;
            case 2: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            case 3: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            case 4: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            case 5: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            case 6: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            case 7: if (window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]
              && window[++scan] == window[++match]) break;
              break;
            }

            if (window[scan] == window[match])
            {
            /* We check for insufficient lookahead only every 8th comparison;
             * the 256th check will be made at strstart + 258 unless lookahead is
             * exhausted first.
             */
              do
              {
                if (scan == scanMax)
                {
                  ++scan;     // advance to first position not matched
                  ++match;

                  break;
                }
              }
              while (window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]
                  && window[++scan] == window[++match]);
            }

            if (scan - this.strstart > this.matchLen)
            {
              #if DebugDeflation
              if (DeflaterConstants.DEBUGGING && (ins_h == 0) )
              Console.Error.WriteLine("Found match: " + curMatch + "-" + (scan - strstart));
              #endif

              this.matchStart = curMatch;
              this.matchLen = scan - this.strstart;

              if (this.matchLen >= niceLength)
                break;
          
              scan_end1 = window[scan - 1];
              scan_end = window[scan];
            }
          } while ((curMatch = (prev[curMatch & DeflaterConstants.WMASK] & 0xffff)) > limit && 0 != --chainLength );

          return this.matchLen >= DeflaterConstants.MIN_MATCH;
        }

		bool DeflateStored(bool flush, bool finish)
		{
			if (!flush && (this.lookahead == 0)) {
				return false;
			}

			this.strstart += this.lookahead;
			this.lookahead = 0;

			int storedLength = this.strstart - this.blockStart;

			if ((storedLength >= DeflaterConstants.MAX_BLOCK_SIZE) || // Block is full
				(this.blockStart < DeflaterConstants.WSIZE && storedLength >= DeflaterConstants.MAX_DIST) ||   // Block may move out of window
				flush) {
				bool lastBlock = finish;
				if (storedLength > DeflaterConstants.MAX_BLOCK_SIZE) {
					storedLength = DeflaterConstants.MAX_BLOCK_SIZE;
					lastBlock = false;
				}

#if DebugDeflation
				if (DeflaterConstants.DEBUGGING) 
				{
				   Console.WriteLine("storedBlock[" + storedLength + "," + lastBlock + "]");
				}
#endif

				this.huffman.FlushStoredBlock(this.window, this.blockStart, storedLength, lastBlock);
				this.blockStart += storedLength;
				return !lastBlock;
			}
			return true;
		}

		bool DeflateFast(bool flush, bool finish)
		{
			if (this.lookahead < DeflaterConstants.MIN_LOOKAHEAD && !flush) {
				return false;
			}

			while (this.lookahead >= DeflaterConstants.MIN_LOOKAHEAD || flush) {
				if (this.lookahead == 0) {
					// We are flushing everything
					this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, finish);
					this.blockStart = this.strstart;
					return false;
				}

				if (this.strstart > 2 * DeflaterConstants.WSIZE - DeflaterConstants.MIN_LOOKAHEAD) {
					/* slide window, as FindLongestMatch needs this.
					 * This should only happen when flushing and the window
					 * is almost full.
					 */
					this.SlideWindow();
				}

				int hashHead;
				if (this.lookahead >= DeflaterConstants.MIN_MATCH &&
					(hashHead = this.InsertString()) != 0 &&
					this.strategy != DeflateStrategy.HuffmanOnly &&
					this.strstart - hashHead <= DeflaterConstants.MAX_DIST &&
					this.FindLongestMatch(hashHead)) {
					// longestMatch sets matchStart and matchLen
#if DebugDeflation
					if (DeflaterConstants.DEBUGGING) 
					{
						for (int i = 0 ; i < matchLen; i++) {
							if (window[strstart + i] != window[matchStart + i]) {
								throw new SharpZipBaseException("Match failure");
							}
						}
					}
#endif

					bool full = this.huffman.TallyDist(this.strstart - this.matchStart, this.matchLen);

					this.lookahead -= this.matchLen;
					if (this.matchLen <= this.max_lazy && this.lookahead >= DeflaterConstants.MIN_MATCH) {
						while (--this.matchLen > 0) {
							++this.strstart;
							this.InsertString();
						}
						++this.strstart;
					} else {
						this.strstart += this.matchLen;
						if (this.lookahead >= DeflaterConstants.MIN_MATCH - 1) {
							this.UpdateHash();
						}
					}
					this.matchLen = DeflaterConstants.MIN_MATCH - 1;
					if (!full) {
						continue;
					}
				} else {
					// No match found
					this.huffman.TallyLit(this.window[this.strstart] & 0xff);
					++this.strstart;
					--this.lookahead;
				}

				if (this.huffman.IsFull()) {
					bool lastBlock = finish && (this.lookahead == 0);
					this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, lastBlock);
					this.blockStart = this.strstart;
					return !lastBlock;
				}
			}
			return true;
		}

		bool DeflateSlow(bool flush, bool finish)
		{
			if (this.lookahead < DeflaterConstants.MIN_LOOKAHEAD && !flush) {
				return false;
			}

			while (this.lookahead >= DeflaterConstants.MIN_LOOKAHEAD || flush) {
				if (this.lookahead == 0) {
					if (this.prevAvailable) {
						this.huffman.TallyLit(this.window[this.strstart - 1] & 0xff);
					}
					this.prevAvailable = false;

					// We are flushing everything
#if DebugDeflation
					if (DeflaterConstants.DEBUGGING && !flush) 
					{
						throw new SharpZipBaseException("Not flushing, but no lookahead");
					}
#endif
					this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart,
						finish);
					this.blockStart = this.strstart;
					return false;
				}

				if (this.strstart >= 2 * DeflaterConstants.WSIZE - DeflaterConstants.MIN_LOOKAHEAD) {
					/* slide window, as FindLongestMatch needs this.
					 * This should only happen when flushing and the window
					 * is almost full.
					 */
					this.SlideWindow();
				}

				int prevMatch = this.matchStart;
				int prevLen = this.matchLen;
				if (this.lookahead >= DeflaterConstants.MIN_MATCH) {

					int hashHead = this.InsertString();

					if (this.strategy != DeflateStrategy.HuffmanOnly &&
						hashHead != 0 &&
						this.strstart - hashHead <= DeflaterConstants.MAX_DIST &&
						this.FindLongestMatch(hashHead)) {

						// longestMatch sets matchStart and matchLen

						// Discard match if too small and too far away
						if (this.matchLen <= 5 && (this.strategy == DeflateStrategy.Filtered || (this.matchLen == DeflaterConstants.MIN_MATCH && this.strstart - this.matchStart > TooFar))) {
							this.matchLen = DeflaterConstants.MIN_MATCH - 1;
						}
					}
				}

				// previous match was better
				if ((prevLen >= DeflaterConstants.MIN_MATCH) && (this.matchLen <= prevLen)) {
#if DebugDeflation
					if (DeflaterConstants.DEBUGGING) 
					{
					   for (int i = 0 ; i < matchLen; i++) {
						  if (window[strstart-1+i] != window[prevMatch + i])
							 throw new SharpZipBaseException();
						}
					}
#endif
					this.huffman.TallyDist(this.strstart - 1 - prevMatch, prevLen);
					prevLen -= 2;
					do {
						this.strstart++;
						this.lookahead--;
						if (this.lookahead >= DeflaterConstants.MIN_MATCH) {
							this.InsertString();
						}
					} while (--prevLen > 0);

					this.strstart++;
					this.lookahead--;
					this.prevAvailable = false;
					this.matchLen = DeflaterConstants.MIN_MATCH - 1;
				} else {
					if (this.prevAvailable) {
						this.huffman.TallyLit(this.window[this.strstart - 1] & 0xff);
					}
					this.prevAvailable = true;
					this.strstart++;
					this.lookahead--;
				}

				if (this.huffman.IsFull()) {
					int len = this.strstart - this.blockStart;
					if (this.prevAvailable) {
						len--;
					}
					bool lastBlock = (finish && (this.lookahead == 0) && !this.prevAvailable);
					this.huffman.FlushBlock(this.window, this.blockStart, len, lastBlock);
					this.blockStart += len;
					return !lastBlock;
				}
			}
			return true;
		}

		#region Instance Fields

		// Hash index of string to be inserted
		int ins_h;

		/// <summary>
		/// Hashtable, hashing three characters to an index for window, so
		/// that window[index]..window[index+2] have this hash code.  
		/// Note that the array should really be unsigned short, so you need
		/// to and the values with 0xffff.
		/// </summary>
		short[] head;

		/// <summary>
		/// <code>prev[index &amp; WMASK]</code> points to the previous index that has the
		/// same hash code as the string starting at index.  This way 
		/// entries with the same hash code are in a linked list.
		/// Note that the array should really be unsigned short, so you need
		/// to and the values with 0xffff.
		/// </summary>
		short[] prev;

		int matchStart;
		// Length of best match
		int matchLen;
		// Set if previous match exists
		bool prevAvailable;
		int blockStart;

		/// <summary>
		/// Points to the current character in the window.
		/// </summary>
		int strstart;

		/// <summary>
		/// lookahead is the number of characters starting at strstart in
		/// window that are valid.
		/// So window[strstart] until window[strstart+lookahead-1] are valid
		/// characters.
		/// </summary>
		int lookahead;

		/// <summary>
		/// This array contains the part of the uncompressed stream that 
		/// is of relevance.  The current character is indexed by strstart.
		/// </summary>
		byte[] window;

		DeflateStrategy strategy;
		int max_chain, max_lazy, niceLength, goodLength;

		/// <summary>
		/// The current compression function.
		/// </summary>
		int compressionFunction;

		/// <summary>
		/// The input data for compression.
		/// </summary>
		byte[] inputBuf;

		/// <summary>
		/// The total bytes of input read.
		/// </summary>
		long totalIn;

		/// <summary>
		/// The offset into inputBuf, where input data starts.
		/// </summary>
		int inputOff;

		/// <summary>
		/// The end offset of the input data.
		/// </summary>
		int inputEnd;

		DeflaterPending pending;
		DeflaterHuffman huffman;

		/// <summary>
		/// The adler checksum
		/// </summary>
		Adler32 adler;
		#endregion
	}
}
