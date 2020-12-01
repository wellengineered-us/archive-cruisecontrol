using System;

using WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Zip.Compression.Streams;

namespace WellEngineered.CruiseControl.PrivateBuild.SharpZipLib.Zip.Compression
{
	class InflaterDynHeader
	{
		#region Constants
		const int LNUM = 0;
		const int DNUM = 1;
		const int BLNUM = 2;
		const int BLLENS = 3;
		const int LENS = 4;
		const int REPS = 5;

		static readonly int[] repMin = { 3, 3, 11 };
		static readonly int[] repBits = { 2, 3, 7 };

		static readonly int[] BL_ORDER =
		{ 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
		#endregion

		public bool Decode(StreamManipulator input)
		{
			decode_loop:
			for (;;) {
				switch (this.mode) {
					case LNUM:
						this.lnum = input.PeekBits(5);
						if (this.lnum < 0) {
							return false;
						}
						this.lnum += 257;
						input.DropBits(5);
						//  	    System.err.println("LNUM: "+lnum);
						this.mode = DNUM;
						goto case DNUM; // fall through
					case DNUM:
						this.dnum = input.PeekBits(5);
						if (this.dnum < 0) {
							return false;
						}
						this.dnum++;
						input.DropBits(5);
						//  	    System.err.println("DNUM: "+dnum);
						this.num = this.lnum + this.dnum;
						this.litdistLens = new byte[this.num];
						this.mode = BLNUM;
						goto case BLNUM; // fall through
					case BLNUM:
						this.blnum = input.PeekBits(4);
						if (this.blnum < 0) {
							return false;
						}
						this.blnum += 4;
						input.DropBits(4);
						this.blLens = new byte[19];
						this.ptr = 0;
						//  	    System.err.println("BLNUM: "+blnum);
						this.mode = BLLENS;
						goto case BLLENS; // fall through
					case BLLENS:
						while (this.ptr < this.blnum) {
							int len = input.PeekBits(3);
							if (len < 0) {
								return false;
							}
							input.DropBits(3);
							//  		System.err.println("blLens["+BL_ORDER[ptr]+"]: "+len);
							this.blLens[BL_ORDER[this.ptr]] = (byte)len;
							this.ptr++;
						}
						this.blTree = new InflaterHuffmanTree(this.blLens);
						this.blLens = null;
						this.ptr = 0;
						this.mode = LENS;
						goto case LENS; // fall through
					case LENS: {
							int symbol;
							while (((symbol = this.blTree.GetSymbol(input)) & ~15) == 0) {
								/* Normal case: symbol in [0..15] */

								//  		  System.err.println("litdistLens["+ptr+"]: "+symbol);
								this.litdistLens[this.ptr++] = this.lastLen = (byte)symbol;

								if (this.ptr == this.num) {
									/* Finished */
									return true;
								}
							}

							/* need more input ? */
							if (symbol < 0) {
								return false;
							}

							/* otherwise repeat code */
							if (symbol >= 17) {
								/* repeat zero */
								//  		  System.err.println("repeating zero");
								this.lastLen = 0;
							} else {
								if (this.ptr == 0) {
									throw new SharpZipBaseException();
								}
							}
							this.repSymbol = symbol - 16;
						}
						this.mode = REPS;
						goto case REPS; // fall through
					case REPS: {
							int bits = repBits[this.repSymbol];
							int count = input.PeekBits(bits);
							if (count < 0) {
								return false;
							}
							input.DropBits(bits);
							count += repMin[this.repSymbol];
							//  	      System.err.println("litdistLens repeated: "+count);

							if (this.ptr + count > this.num) {
								throw new SharpZipBaseException();
							}
							while (count-- > 0) {
								this.litdistLens[this.ptr++] = this.lastLen;
							}

							if (this.ptr == this.num) {
								/* Finished */
								return true;
							}
						}
						this.mode = LENS;
						goto decode_loop;
				}
			}
		}

		public InflaterHuffmanTree BuildLitLenTree()
		{
			byte[] litlenLens = new byte[this.lnum];
			Array.Copy(this.litdistLens, 0, litlenLens, 0, this.lnum);
			return new InflaterHuffmanTree(litlenLens);
		}

		public InflaterHuffmanTree BuildDistTree()
		{
			byte[] distLens = new byte[this.dnum];
			Array.Copy(this.litdistLens, this.lnum, distLens, 0, this.dnum);
			return new InflaterHuffmanTree(distLens);
		}

		#region Instance Fields
		byte[] blLens;
		byte[] litdistLens;

		InflaterHuffmanTree blTree;

		/// <summary>
		/// The current decode mode
		/// </summary>
		int mode;
		int lnum, dnum, blnum, num;
		int repSymbol;
		byte lastLen;
		int ptr;
		#endregion

	}
}
