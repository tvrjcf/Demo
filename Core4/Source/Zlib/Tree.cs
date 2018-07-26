// Tree.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.  
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs): 
// Time-stamp: <2009-October-28 13:29:50>
//
// ------------------------------------------------------------------
//
// This module defines classes for zlib compression and
// decompression. This code is derived from the jzlib implementation of
// zlib. In keeping with the license for jzlib, the copyright to that
// code is below.
//
// ------------------------------------------------------------------
// 
// Copyright (c) 2000,2001,2002,2003 ymnk, JCraft,Inc. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright 
// notice, this list of conditions and the following disclaimer in 
// the documentation and/or other materials provided with the distribution.
// 
// 3. The names of the authors may not be used to endorse or promote products
// derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
// INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// -----------------------------------------------------------------------
//
// This program is based on zlib-1.1.3; credit to authors
// Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
// and contributors of zlib.
//
// -----------------------------------------------------------------------
// edited by xuld

using System;

namespace Py.Zip.Zlib {

	/// <summary>
	/// ��ʾһ������������
	/// </summary>
	sealed class Tree {

		/// <summary>
		/// ջ��С��
		/// </summary>
		private const int HEAP_SIZE = (2 * StaticTree.L_CODES + 1);

		/// <summary>
		/// ��չ���ȱ�
		/// </summary>
		public static readonly int[] ExtraLengthBits = new int[] {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
        };

		/// <summary>
		/// ��չ�����
		/// </summary>
		public static readonly int[] ExtraDistanceBits = new int[] {
            0, 0, 0, 0, 1, 1,  2,  2,  3,  3,  4,  4,  5,  5,  6,  6,
            7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13
        };

		/// <summary>
		/// ��չ���ȴ��롣
		/// </summary>
		public static readonly int[] ExtraBlbits = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 7 };

		/// <summary>
		/// ˳��
		/// </summary>
		public static readonly sbyte[] BlOrder = new sbyte[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

		/// <summary>
		/// �ֽڻ����С��
		/// </summary>
		const int BUFFER_SIZE = 8 * 2;

		//const int DIST_CODE_LEN = 512;

		/// <summary>
		/// �����
		/// </summary>
		static readonly sbyte[] _dist_code = new sbyte[] {
            0,  1,  2,  3,  4,  4,  5,  5,  6,  6,  6,  6,  7,  7,  7,  7, 
            8,  8,  8,  8,  8,  8,  8,  8,  9,  9,  9,  9,  9,  9,  9,  9,
            10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 
            11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 
            0,   0, 16, 17, 18, 18, 19, 19, 20, 20, 20, 20, 21, 21, 21, 21, 
            22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 
            29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29
        };

		/// <summary>
		/// ���ȵĴ����
		/// </summary>
		public static readonly sbyte[] LengthCode = new sbyte[] {
            0,   1,  2,  3,  4,  5,  6,  7,  8,  8,  9,  9, 10, 10, 11, 11,
            12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15,
            16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17,
            18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19,
            20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
            21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
            22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
            23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23,
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
            24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
            25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
            26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
            27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28
        };

		/// <summary>
		/// ���Ⱦ���
		/// </summary>
		public static readonly int[] LengthBase = new int[] {
            0,   1,  2,  3,  4,  5,  6,   7,   8,  10,  12,  14, 16, 20, 24, 28,
            32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 0
        };

		/// <summary>
		/// �������
		/// </summary>
		public static readonly int[] DistanceBase = new int[] {
            0, 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192,
            256, 384, 512, 768, 1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576
        };

		/// <summary>
		/// ƥ����롣
		/// </summary>
		/// <remarks> 
		/// û�б߽�. _dist_code[256] �� _dist_code[257] ��ʹ�á�
		/// </remarks>
		public static int DistanceCode(int dist) {
			return (dist < 256)
				? _dist_code[dist]
				: _dist_code[256 + (int)((uint)dist >> 7)];
		}

		/// <summary>
		/// ��̬���ɵ�����
		/// </summary>
		public short[] DynTree;

		/// <summary>
		/// ���Ĵ�С��
		/// </summary>
		public int MaxCode;

		/// <summary>
		/// ��̬������
		/// </summary>
		public StaticTree staticTree; // ʵ��

		/// <summary>
		/// ���ɴ��볤�ȡ�
		/// </summary>
		/// <param name="s">�����ߡ�</param>
		/// <remarks>
		/// �����ѡ�ĳ��ȵ��������µ�ǰ�Ŀ顣ȷ����Щ�ֶβ��ա�  heap[heap_max]Ϊ��������
		/// </remarks>
		void GenBitLen(DeflateManager s) {
			short[] tree = DynTree;
			short[] stree = staticTree.TreeCodes;
			int[] extra = staticTree.ExtraBits;
			int base_Renamed = staticTree.ExtraBase;
			int max_length = staticTree.MaxLength;
			int h; // heap index
			int n, m; // iterate over the tree elements
			int bits; // bit length
			int xbits; // extra bits
			short f; // frequency
			int overflow = 0; // number of elements with bit length too large

			for (bits = 0; bits <= StaticTree.MAX_BITS; bits++)
				s.BlCount[bits] = 0;

			// In a first pass, compute the optimal bit lengths (which may
			// overflow in the case of the bit length tree).
			tree[s.Heap[s.HeapMax] * 2 + 1] = 0; // root of the heap

			for (h = s.HeapMax + 1; h < HEAP_SIZE; h++) {
				n = s.Heap[h];
				bits = tree[tree[n * 2 + 1] * 2 + 1] + 1;
				if (bits > max_length) {
					bits = max_length; overflow++;
				}
				tree[n * 2 + 1] = (short)bits;
				// We overwrite tree[n*2+1] which is no longer needed

				if (n > MaxCode)
					continue; // not a leaf node

				s.BlCount[bits]++;
				xbits = 0;
				if (n >= base_Renamed)
					xbits = extra[n - base_Renamed];
				f = tree[n * 2];
				s.OptLen += f * (bits + xbits);
				if (stree != null)
					s.StaticLen += f * (stree[n * 2 + 1] + xbits);
			}
			if (overflow == 0)
				return;

			// This happens for example on obj2 and pic of the Calgary corpus
			// Find the first bit length which could increase:
			do {
				bits = max_length - 1;
				while (s.BlCount[bits] == 0)
					bits--;
				s.BlCount[bits]--; // move one leaf down the tree
				s.BlCount[bits + 1] = (short)(s.BlCount[bits + 1] + 2); // move one overflow item as its brother
				s.BlCount[max_length]--;
				// The brother of the overflow item also moves one step up,
				// but this does not affect bl_count[max_length]
				overflow -= 2;
			}
			while (overflow > 0);

			for (bits = max_length; bits != 0; bits--) {
				n = s.BlCount[bits];
				while (n != 0) {
					m = s.Heap[--h];
					if (m > MaxCode)
						continue;
					if (tree[m * 2 + 1] != bits) {
						s.OptLen = (int)(s.OptLen + ((long)bits - (long)tree[m * 2 + 1]) * (long)tree[m * 2]);
						tree[m * 2 + 1] = (short)bits;
					}
					n--;
				}
			}
		}

		/// <summary>
		/// ��������
		/// </summary>
		/// <param name="s">�����ߡ�</param>
		public void BuildTree(DeflateManager s) {
			short[] tree = DynTree;
			short[] stree = staticTree.TreeCodes;
			int elems = staticTree.Elems;
			int n, m;            // iterate over heap elements
			int max_code = -1;  // largest code with non zero frequency
			int node;            // new node being created

			// Construct the initial heap, with least frequent element in
			// heap[1]. The sons of heap[n] are heap[2*n] and heap[2*n+1].
			// heap[0] is not used.
			s.HeapLen = 0;
			s.HeapMax = HEAP_SIZE;

			for (n = 0; n < elems; n++) {
				if (tree[n * 2] != 0) {
					s.Heap[++s.HeapLen] = max_code = n;
					s.Depth[n] = 0;
				} else {
					tree[n * 2 + 1] = 0;
				}
			}

			// The pkzip format requires that at least one distance code exists,
			// and that at least one bit should be sent even if there is only one
			// possible code. So to avoid special checks later on we force at least
			// two codes of non zero frequency.
			while (s.HeapLen < 2) {
				node = s.Heap[++s.HeapLen] = (max_code < 2 ? ++max_code : 0);
				tree[node * 2] = 1;
				s.Depth[node] = 0;
				s.OptLen--;
				if (stree != null)
					s.StaticLen -= stree[node * 2 + 1];
				// node is 0 or 1 so it does not have extra bits
			}
			this.MaxCode = max_code;

			// The elements heap[heap_len/2+1 .. heap_len] are leaves of the tree,
			// establish sub-heaps of increasing lengths:

			for (n = s.HeapLen / 2; n >= 1; n--)
				s.pqdownheap(tree, n);

			// Construct the Huffman tree by repeatedly combining the least two
			// frequent nodes.

			node = elems; // next internal node of the tree
			do {
				// n = node of least frequency
				n = s.Heap[1];
				s.Heap[1] = s.Heap[s.HeapLen--];
				s.pqdownheap(tree, 1);
				m = s.Heap[1]; // m = node of next least frequency

				s.Heap[--s.HeapMax] = n; // keep the nodes sorted by frequency
				s.Heap[--s.HeapMax] = m;

				// Create a new node father of n and m
				tree[node * 2] = unchecked((short)(tree[n * 2] + tree[m * 2]));
				s.Depth[node] = (sbyte)(System.Math.Max((byte)s.Depth[n], (byte)s.Depth[m]) + 1);
				tree[n * 2 + 1] = tree[m * 2 + 1] = (short)node;

				// and insert the new node in the heap
				s.Heap[1] = node++;
				s.pqdownheap(tree, 1);
			}
			while (s.HeapLen >= 2);

			s.Heap[--s.HeapMax] = s.Heap[1];

			// At this point, the fields freq and dad are set. We can now
			// generate the bit lengths.

			GenBitLen(s);

			// The field len is now set, we can generate the bit codes
			GenCodes(tree, max_code, s.BlCount);
		}

		// Generate the codes for a given tree and bit counts (which need not be
		// optimal).
		// IN assertion: the array bl_count contains the bit length statistics for
		// the given tree and the field len is set for all tree elements.
		// OUT assertion: the field code is set for all tree elements of non
		//     zero code length.
		static void GenCodes(short[] tree, int max_code, short[] bl_count) {
			short[] next_code = new short[StaticTree.MAX_BITS + 1]; // next code value for each bit length
			short code = 0; // running code value
			int bits; // bit index
			int n; // code index

			// The distribution counts are first used to generate the code values
			// without bit reversal.
			for (bits = 1; bits <= StaticTree.MAX_BITS; bits++)
				unchecked {
					next_code[bits] = code = (short)((code + bl_count[bits - 1]) << 1);
				}

			// Check that the bit counts in bl_count are consistent. The last code
			// must be all ones.
			//Assert (code + bl_count[MAX_BITS]-1 == (1<<MAX_BITS)-1,
			//        "inconsistent bit counts");
			//Tracev((stderr,"\ngen_codes: max_code %d ", max_code));

			for (n = 0; n <= max_code; n++) {
				int len = tree[n * 2 + 1];
				if (len == 0)
					continue;
				// Now reverse the bits
				tree[n * 2] = unchecked((short)(BiReverse(next_code[len]++, len)));
			}
		}

		/// <summary>
		/// ���������ͷβ�ֽڡ�
		/// </summary>
		/// <param name="code">���롣</param>
		/// <param name="count">���ȡ�1 &lt;= <paramref name="count"/> &lt;= 15</param>
		/// <returns>�ߵ������</returns>
		static int BiReverse(int code, int count) {
			int res = 0;
			do {
				res |= code & 1;
				code >>= 1; //SharedUtils.URShift(code, 1);
				res <<= 1;
			} while (--count > 0);
			return res >> 1;
		}
	}
}