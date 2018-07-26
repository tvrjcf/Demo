// Deflate.cs
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
// Time-stamp: <2009-October-28 13:44:59>
//
// ------------------------------------------------------------------
//
// This module defines logic for handling the Deflate or compression.
//
// This code is based on multiple sources: 
// - the original zlib v1.2.3 source, which is Copyright (C) 1995-2005 Jean-loup Gailly.
// - the original jzlib, which is Copyright (c) 2000-2003 ymnk, JCraft,Inc. 
//
// However, this code is significantly different from both.  
// The object model is not the same, and many of the behaviors are different.
//
// In keeping with the license for these other works, the copyrights for 
// jzlib and zlib are here.
//
// -----------------------------------------------------------------------
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
using Py.Core;
using Py.RunTime;

namespace Py.Zip.Zlib {

    /// <summary>
    /// 表示一个压缩缓存输出类型。
    /// </summary>
    public enum FlushType {

        /// <summary>
        /// 不输出。
        /// </summary>
        None = 0,

        /// <summary>
        /// 仅关闭当前的模块。
        /// </summary>
        /// <remarks>
        /// 不输出缓存，这只在一些极端情况使用。
        /// </remarks>
        Partial,

        /// <summary>
        /// 所有的缓存均输出。
        /// </summary>
        Sync,

        /// <summary>
        /// 输出全部缓存，并重置所有状态。
        /// </summary>
        Full,

        /// <summary>
        /// 标识压缩结束。
        /// </summary>
        Finish,
    }

    /// <summary>
    /// 表示块状态的枚举。
    /// </summary>
    enum BlockState {

        /// <summary>
        /// 块未填满。
        /// </summary>
        NeedMore = 0,

        /// <summary>
        /// 块准备完。
        /// </summary>
        BlockDone,

        /// <summary>
        /// 正在结束。
        /// </summary>
        FinishStarted,

        /// <summary>
        /// 完成。
        /// </summary>
        FinishDone

    }

    /// <summary>
    /// 表示 Deflate 执行速度的枚举。
    /// </summary>
    enum DeflateFlavor {

        /// <summary>
        /// 存储。
        /// </summary>
        Store,

        /// <summary>
        /// 快速。
        /// </summary>
        Fast,

        /// <summary>
        /// 低速。
        /// </summary>
        Slow

    }

    /// <summary>
    /// 用于操作 Deflate 的类。不可继承此类。
    /// </summary>
    sealed class DeflateManager {

        /// <summary>
        /// 等级的最大值。
        /// </summary>
        private const int MEM_LEVEL_MAX = 9;

        /// <summary>
        /// 等级默认大小。
        /// </summary>
        private const int MEM_LEVEL_DEFAULT = 8;

        /// <summary>
        /// 压缩最大的窗口大小。
        /// </summary>
        public const int WindowBitsMax = 15; // 32K LZ77 window

        /// <summary>
        /// 重复先前的字节长度 3 至 6 倍。 （每2）
        /// </summary>
        const int REP_3_6 = 16;

        /// <summary>
        /// 重复先前的字节长度 3 至 10 倍。（每3）
        /// </summary>
        const int REPZ_3_10 = 17;

        /// <summary>
        /// 重复先前的字节长度 11 至 138 倍。 （每6）
        /// </summary>
        const int REPZ_11_138 = 18;

        /// <summary>
        /// 表示压缩函数的委托。
        /// </summary>
        /// <param name="flush">输出类型。</param>
        /// <returns>状态。</returns>
        delegate BlockState CompressFunc(FlushType flush);

        /// <summary>
        /// 表示一个配置类。
        /// </summary>
        class Config {

            /// <summary>
            /// 已合格的大小。
            /// </summary>
            /// <remarks>
            /// 如果上次的匹配比这次多，使搜索更快，减小重复的搜索。
            /// </remarks>
            public int GoodLength;

            /// <summary>
            /// 最大的暴力计算长度。
            /// </summary>
            /// <remarks>
            /// 在这个值以上不再进行暴力计算。
            /// 对压缩等级不小于 4 时，这个值有效，对于压缩等级1-3 ， 这个值和  MaxInsertLength 一样。 （见  DeflateFast 。 ）
            /// </remarks>
            public int MaxLazy;

            /// <summary>
            /// 正确的长度。
            /// </summary>
            /// <remarks>
            /// 到这个长度后不继续。
            /// </remarks>
            public int NiceLength;

            /// <summary>
            /// 需要的最大值。
            /// </summary>
            /// <remarks>
            /// 为了加快速度，哈希链在此值后不计算。这个值随压缩率增加而变大，但同时速度变小。
            /// </remarks>
            public int MaxChainLength;

            /// <summary>
            /// 速度。
            /// </summary>
            public DeflateFlavor Flavor;

            /// <summary>
            /// 初始化 <see cref="Py.Zip.Zlib.DeflateManager.Config"/> 的新实例。
            /// </summary>
            /// <param name="goodLength">合格的长度。</param>
            /// <param name="maxLazy">最大的暴力计算长度。</param>
            /// <param name="niceLength">正确的长度。</param>
            /// <param name="maxChainLength">需要的最大值。</param>
            /// <param name="flavor">速度。</param>
            private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor) {
                GoodLength = goodLength;
                MaxLazy = maxLazy;
                NiceLength = niceLength;
                MaxChainLength = maxChainLength;
                Flavor = flavor;
            }

            /// <summary>
            /// 返回指定等级的配置。
            /// </summary>
            /// <param name="level">压缩等级。</param>
            /// <returns>配置。</returns>
            public static Config Lookup(CompressionLevel level) {
                return Table[(int)level];
            }


            /// <summary>
            /// 初始化 <see cref="Py.Zip.Zlib.DeflateManager.Config"/> 的静态成员。
            /// </summary>
            static Config() {
                Table = new Config[] {
                    new Config(0, 0, 0, 0, DeflateFlavor.Store),
                    new Config(4, 4, 8, 4, DeflateFlavor.Fast),
                    new Config(4, 5, 16, 8, DeflateFlavor.Fast),
                    new Config(4, 6, 32, 32, DeflateFlavor.Fast),

                    new Config(4, 4, 16, 16, DeflateFlavor.Slow),
                    new Config(8, 16, 32, 32, DeflateFlavor.Slow),
                    new Config(8, 16, 128, 128, DeflateFlavor.Slow),
                    new Config(8, 32, 128, 256, DeflateFlavor.Slow),
                    new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
                    new Config(32, 258, 258, 4096, DeflateFlavor.Slow),
                };
            }

            /// <summary>
            /// 配置表。
            /// </summary>
            private static readonly Config[] Table;
        }

        /// <summary>
        /// 压缩的方法。
        /// </summary>
        private CompressFunc DeflateFunction;

        // preset dictionary flag in zlib header
        private const int PRESET_DICT = 0x20;

        /// <summary>
        /// 当前的状态。
        /// </summary>
        enum ManagerState {

            Init = 42,

            Busy = 113,

            Finish = 666

        }

        // The deflate compression method
        private const int Z_DEFLATED = 8;

        private const int STORED_BLOCK = 0;
        private const int STATIC_TREES = 1;
        private const int DYN_TREES = 2;

        private const int Buf_size = 8 * 2;

        private const int MIN_MATCH = 3;
        private const int MAX_MATCH = 258;

        private const int MIN_LOOKAHEAD = (MAX_MATCH + MIN_MATCH + 1);

        private const int HEAP_SIZE = (2 * StaticTree.L_CODES + 1);

        private const int END_BLOCK = 256;

        ZlibCodec _codec; // the zlib encoder/decoder

        /// <summary>
        /// 当前状态。
        /// </summary>
        ManagerState _status;

        /// <summary>
        /// 追加的字节。
        /// </summary>
        public byte[] Pending;

        /// <summary>
        /// 需要追加到流的字节。
        /// </summary>
        public int NextPending;

        /// <summary>
        /// 追加的大小。
        /// </summary>
        public int PendingCount;

        /// <summary>
        /// 数据格式的类型。
        /// </summary>
        FileDataType _dataType = FileDataType.Binary;

        /// <summary>
        /// 最后一次输出的数。
        /// </summary>
        int _lastFlush;

        /// <summary>
        /// LZ77 的默认窗口。 （默认 32k）
        /// </summary>
        int _size;

        /// <summary>
        /// 窗口大小的质。 （log2(_size) ）  （ 8..16 ）
        /// </summary>
        int _bits;

        /// <summary>
        /// 遮罩的大小。  （_size - 1）
        /// </summary>
        int _mask;

        /// <summary>
        /// 内部字典。
        /// </summary>
        byte[] _window;

        /// <summary>
        /// 窗口大小。
        /// </summary>
        int _windowSize;
        // Actual size of window: 2*wSize, except when the user input buffer
        // is directly used as sliding window.

        short[] _prev;
        // Link to older string with same hash index. To limit the size of this
        // array to 64K, this link is maintained only for the last 32K strings.
        // An index in this array is thus a window index modulo 32K.

        short[] _head;  // Heads of the hash chains or NIL.

        int _insH;     // hash index of string to be inserted
        int _hashSize; // number of elements in hash table
        int _hashBits; // log2(hash_size)
        int _hashMask; // hash_size-1

        // Number of bits by which ins_h must be shifted at each input
        // step. It must be such that after MIN_MATCH steps, the oldest
        // byte no longer takes part in the hash key, that is:
        // hash_shift * MIN_MATCH >= hash_bits
        int _hashShift;

        // Window position at the beginning of the current output block. Gets
        // negative when the window is moved backwards.

        int _blockStart;

        Config _config;
        int _matchLength;    // length of best match
        int _prevMatch;      // previous match
        int _matchAvailable; // set if previous match exists
        int _strstart;        // start of string to insert into.....????
        int _matchStart;     // start of matching string
        int _lookahead;       // number of valid bytes ahead in window

        // Length of the best match at previous step. Matches not greater than this
        // are discarded. This is used in the lazy match evaluation.
        int _prevLength;

        // Insert new strings in the hash table only if the match length is not
        // greater than this length. This saves time but degrades compression.
        // max_insert_length is used only for compression levels <= 3.

        /// <summary>
        /// 压缩等级。
        /// </summary>
        CompressionLevel _compressionLevel;

        /// <summary>
        /// 压缩策略。
        /// </summary>
        CompressionStrategy _compressionStrategy;


        short[] _dynLtree;         // literal and length tree
        short[] _dynDtree;         // distance tree
        short[] _blTree;           // Huffman tree for bit lengths

        Tree _treeLiterals = new Tree();  // desc for literal tree
        Tree _treeDistances = new Tree();  // desc for distance tree
        Tree _treeBitLengths = new Tree(); // desc for bit length tree

        // number of codes at each bit length for an optimal tree
        public short[] BlCount = new short[StaticTree.MAX_BITS + 1];

        // heap used to build the Huffman trees
        public int[] Heap = new int[2 * StaticTree.L_CODES + 1];

        public int HeapLen;              // number of elements in the heap
        public int HeapMax;              // element of largest frequency

        // The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used.
        // The same heap array is used to build all trees.

        // Depth of each subtree used as tie breaker for trees of equal frequency
        public sbyte[] Depth = new sbyte[2 * StaticTree.L_CODES + 1];

        int _lengthOffset;                 // index for literals or lengths 


        // Size of match buffer for literals/lengths.  There are 4 reasons for
        // limiting lit_bufsize to 64K:
        //   - frequencies can be kept in 16 bit counters
        //   - if compression is not successful for the first block, all input
        //     data is still in the window so we can still emit a stored block even
        //     when input comes from standard input.  (This can also be done for
        //     all blocks if lit_bufsize is not greater than 32K.)
        //   - if compression is not successful for a file smaller than 64K, we can
        //     even emit a stored file instead of a stored block (saving 5 bytes).
        //     This is applicable only for zip (not gzip or zlib).
        //   - creating new Huffman trees less frequently may not provide fast
        //     adaptation to changes in the input data statistics. (Take for
        //     example a binary file with poorly compressible code followed by
        //     a highly compressible string table.) Smaller buffer sizes give
        //     fast adaptation but have of course the overhead of transmitting
        //     trees more frequently.

        int _litBufsize;

        int _lastLit;     // running index in l_buf

        // Buffer for distances. To simplify the code, d_buf and l_buf have
        // the same number of elements. To use different lengths, an extra flag
        // array would be necessary.

        int _distanceOffset;        // index into pending; points to distance data??

        /// <summary>
        /// 当前块的附加树的长度。
        /// </summary>
        public int OptLen;

        /// <summary>
        /// 当前块的静态树的长度。
        /// </summary>
        public int StaticLen;

        int _matches;      // number of string matches in current block
        int _lastEobLen; // bit length of EOB code for last block

        // Output buffer. bits are inserted starting at the bottom (least
        // significant bits).
        short _biBuf;

        // Number of valid bits in bi_buf.  All bits above the last valid bit
        // are always zero.
        int _biValid;


        /// <summary>
        /// 初始化 <see cref="Py.Zip.Zlib.DeflateManager"/> 的新实例。
        /// </summary>
        public DeflateManager() {
            _dynLtree = new short[HEAP_SIZE * 2];
            _dynDtree = new short[(2 * StaticTree.D_CODES + 1) * 2]; // distance tree
            _blTree = new short[(2 * StaticTree.BL_CODES + 1) * 2]; // Huffman tree for bit lengths
            WantRfc1950HeaderBytes = true;
        }

        void _InitializeBlocks() {
            // Initialize the trees.
            for (int i = 0; i < StaticTree.L_CODES; i++)
                _dynLtree[i * 2] = 0;
            for (int i = 0; i < StaticTree.D_CODES; i++)
                _dynDtree[i * 2] = 0;
            for (int i = 0; i < StaticTree.BL_CODES; i++)
                _blTree[i * 2] = 0;

            _dynLtree[END_BLOCK * 2] = 1;
            OptLen = StaticLen = 0;
            _lastLit = _matches = 0;
        }

        // Restore the heap property by moving down the tree starting at node k,
        // exchanging a node with the smallest of its two sons if necessary, stopping
        // when the heap property is re-established (each father smaller than its
        // two sons).
        public void pqdownheap(short[] tree, int k) {
            int v = Heap[k];
            int j = k << 1; // left son of k
            while (j <= HeapLen) {
                // Set j to the smallest of the two sons:
                if (j < HeapLen && _IsSmaller(tree, Heap[j + 1], Heap[j], Depth)) {
                    j++;
                }
                // Exit if v is smaller than both sons
                if (_IsSmaller(tree, v, Heap[j], Depth))
                    break;

                // Exchange v with the smallest son
                Heap[k] = Heap[j]; k = j;
                // And continue down the tree, setting j to the left son of k
                j <<= 1;
            }
            Heap[k] = v;
        }

        static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth) {
            short tn2 = tree[n * 2];
            short tm2 = tree[m * 2];
            return (tn2 < tm2 || (tn2 == tm2 && depth[n] <= depth[m]));
        }


        // Scan a literal or distance tree to determine the frequencies of the codes
        // in the bit length tree.
        void scanTree(short[] tree, int max_code) {
            int n; // iterates over all tree elements
            int prevlen = -1; // last emitted length
            int curlen; // length of current code
            int nextlen = (int)tree[0 * 2 + 1]; // length of next code
            int count = 0; // repeat count of the current code
            int max_count = 7; // max repeat count
            int min_count = 4; // min repeat count

            if (nextlen == 0) {
                max_count = 138; min_count = 3;
            }
            tree[(max_code + 1) * 2 + 1] = (short)0x7fff; // guard //??

            for (n = 0; n <= max_code; n++) {
                curlen = nextlen; nextlen = (int)tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen) {
                    continue;
                } else if (count < min_count) {
                    _blTree[curlen * 2] = (short)(_blTree[curlen * 2] + count);
                } else if (curlen != 0) {
                    if (curlen != prevlen)
                        _blTree[curlen * 2]++;
                    _blTree[REP_3_6 * 2]++;
                } else if (count <= 10) {
                    _blTree[REPZ_3_10 * 2]++;
                } else {
                    _blTree[REPZ_11_138 * 2]++;
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0) {
                    max_count = 138; min_count = 3;
                } else if (curlen == nextlen) {
                    max_count = 6; min_count = 3;
                } else {
                    max_count = 7; min_count = 4;
                }
            }
        }

        // Construct the Huffman tree for the bit lengths and return the index in
        // bl_order of the last bit length code to send.
        int BuildBlTree() {
            int max_blindex; // index of last bit length code of non zero freq

            // Determine the bit length frequencies for literal and distance trees
            scanTree(_dynLtree, _treeLiterals.MaxCode);
            scanTree(_dynDtree, _treeDistances.MaxCode);

            // Build the bit length tree:
            _treeBitLengths.BuildTree(this);
            // opt_len now includes the length of the tree representations, except
            // the lengths of the bit lengths codes and the 5+5+4 bits for the counts.

            // Determine the number of bit length codes to send. The pkzip format
            // requires that at least 4 bit length codes be sent. (appnote.txt says
            // 3 but the actual value used is 4.)
            for (max_blindex = StaticTree.BL_CODES - 1; max_blindex >= 3; max_blindex--) {
                if (_blTree[Tree.BlOrder[max_blindex] * 2 + 1] != 0)
                    break;
            }
            // Update opt_len to include the bit length tree and counts
            OptLen += 3 * (max_blindex + 1) + 5 + 5 + 4;

            return max_blindex;
        }


        // Send the header for a block using dynamic Huffman trees: the counts, the
        // lengths of the bit length codes, the literal tree and the distance tree.
        // IN assertion: lcodes >= 257, dcodes >= 1, blcodes >= 4.
        void SendAllTrees(int lcodes, int dcodes, int blcodes) {
            int rank; // index in bl_order

            SendBits(lcodes - 257, 5); // not +255 as stated in appnote.txt
            SendBits(dcodes - 1, 5);
            SendBits(blcodes - 4, 4); // not -3 as stated in appnote.txt
            for (rank = 0; rank < blcodes; rank++) {
                SendBits(_blTree[Tree.BlOrder[rank] * 2 + 1], 3);
            }
            SendTree(_dynLtree, lcodes - 1); // literal tree
            SendTree(_dynDtree, dcodes - 1); // distance tree
        }

        // Send a literal or distance tree in compressed form, using the codes in
        // bl_tree.
        void SendTree(short[] tree, int max_code) {
            int n;                        // iterates over all tree elements
            int prevlen = -1;              // last emitted length
            int curlen;                      // length of current code
            int nextlen = tree[0 * 2 + 1]; // length of next code
            int count = 0;               // repeat count of the current code
            int max_count = 7;               // max repeat count
            int min_count = 4;               // min repeat count

            if (nextlen == 0) {
                max_count = 138; min_count = 3;
            }

            for (n = 0; n <= max_code; n++) {
                curlen = nextlen; nextlen = tree[(n + 1) * 2 + 1];
                if (++count < max_count && curlen == nextlen) {
                    continue;
                } else if (count < min_count) {
                    do {
                        SendCode(curlen, _blTree);
                    }
                    while (--count != 0);
                } else if (curlen != 0) {
                    if (curlen != prevlen) {
                        SendCode(curlen, _blTree); count--;
                    }
                    SendCode(REP_3_6, _blTree);
                    SendBits(count - 3, 2);
                } else if (count <= 10) {
                    SendCode(REPZ_3_10, _blTree);
                    SendBits(count - 3, 3);
                } else {
                    SendCode(REPZ_11_138, _blTree);
                    SendBits(count - 11, 7);
                }
                count = 0; prevlen = curlen;
                if (nextlen == 0) {
                    max_count = 138; min_count = 3;
                } else if (curlen == nextlen) {
                    max_count = 6; min_count = 3;
                } else {
                    max_count = 7; min_count = 4;
                }
            }
        }

        // Output a block of bytes on the stream.
        // IN assertion: there is enough room in pending_buf.
        private void PutBytes(byte[] p, int start, int len) {
            Array.Copy(p, start, Pending, PendingCount, len);
            PendingCount += len;
        }

        void SendCode(int c, short[] tree) {
            int c2 = c * 2;
            SendBits((tree[c2] & 0xffff), (tree[c2 + 1] & 0xffff));
        }

        void SendBits(int value, int length) {
            int len = length;
            unchecked {
                if (_biValid > (int)Buf_size - len) {
                    //int val = value;
                    //      bi_buf |= (val << bi_valid);

                    _biBuf |= (short)((value << _biValid) & 0xffff);
                    //put_short(bi_buf);
                    Pending[PendingCount++] = (byte)_biBuf;
                    Pending[PendingCount++] = (byte)(_biBuf >> 8);


                    _biBuf = (short)((uint)value >> (Buf_size - _biValid));
                    _biValid += len - Buf_size;
                } else {
                    //      bi_buf |= (value) << bi_valid;
                    _biBuf |= (short)((value << _biValid) & 0xffff);
                    _biValid += len;
                }
            }
        }




        // Save the match info and tally the frequency counts. Return true if
        // the current block must be flushed.
        bool TrTally(int dist, int lc) {
            Pending[_distanceOffset + _lastLit * 2] = unchecked((byte)((uint)dist >> 8));
            Pending[_distanceOffset + _lastLit * 2 + 1] = unchecked((byte)dist);
            Pending[_lengthOffset + _lastLit] = unchecked((byte)lc);
            _lastLit++;

            if (dist == 0) {
                // lc is the unmatched char
                _dynLtree[lc * 2]++;
            } else {
                _matches++;
                // Here, lc is the match length - MIN_MATCH
                dist--; // dist = match distance - 1
                _dynLtree[(Tree.LengthCode[lc] + StaticTree.LITERALS + 1) * 2]++;
                _dynDtree[Tree.DistanceCode(dist) * 2]++;
            }

            if ((_lastLit & 0x1fff) == 0 && (int)_compressionLevel > 2) {
                // Compute an upper bound for the compressed length
                int out_length = _lastLit << 3;
                int in_length = _strstart - _blockStart;
                int dcode;
                for (dcode = 0; dcode < StaticTree.D_CODES; dcode++) {
                    out_length = (int)(out_length + (int)_dynDtree[dcode * 2] * (5L + Tree.ExtraDistanceBits[dcode]));
                }
                out_length >>= 3;
                if ((_matches < (_lastLit / 2)) && out_length < in_length / 2)
                    return true;
            }

            return (_lastLit == _litBufsize - 1) || (_lastLit == _litBufsize);
            // dinoch - wraparound?
            // We avoid equality with lit_bufsize because of wraparound at 64K
            // on 16 bit machines and because stored blocks are restricted to
            // 64K-1 bytes.
        }



        // Send the block data compressed using the given Huffman trees
        void SendCompressedBlock(short[] ltree, short[] dtree) {
            int distance; // distance of matched string
            int lc;       // match length or unmatched char (if dist == 0)
            int lx = 0;   // running index in l_buf
            int code;     // the code to send
            int extra;    // number of extra bits to send

            if (_lastLit != 0) {
                do {
                    int ix = _distanceOffset + lx * 2;
                    distance = ((Pending[ix] << 8) & 0xff00) |
                        (Pending[ix + 1] & 0xff);
                    lc = (Pending[_lengthOffset + lx]) & 0xff;
                    lx++;

                    if (distance == 0) {
                        SendCode(lc, ltree); // send a literal byte
                    } else {
                        // literal or match pair 
                        // Here, lc is the match length - MIN_MATCH
                        code = Tree.LengthCode[lc];

                        // send the length code
                        SendCode(code + StaticTree.LITERALS + 1, ltree);
                        extra = Tree.ExtraLengthBits[code];
                        if (extra != 0) {
                            // send the extra length bits
                            lc -= Tree.LengthBase[code];
                            SendBits(lc, extra);
                        }
                        distance--; // dist is now the match distance - 1
                        code = Tree.DistanceCode(distance);

                        // send the distance code
                        SendCode(code, dtree);

                        extra = Tree.ExtraDistanceBits[code];
                        if (extra != 0) {
                            // send the extra distance bits
                            distance -= Tree.DistanceBase[code];
                            SendBits(distance, extra);
                        }
                    }

                    // Check that the overlay between pending and d_buf+l_buf is ok:
                }
                while (lx < _lastLit);
            }

            SendCode(END_BLOCK, ltree);
            _lastEobLen = ltree[END_BLOCK * 2 + 1];
        }



        // Set the data type to ASCII or BINARY, using a crude approximation:
        // binary if more than 20% of the bytes are <= 6 or >= 128, ascii otherwise.
        // IN assertion: the fields freq of dyn_ltree are set and the total of all
        // frequencies does not exceed 64K (to fit in an int on 16 bit machines).
        void SetDataType() {
            int n = 0;
            int ascii_freq = 0;
            int bin_freq = 0;
            while (n < 7) {
                bin_freq += _dynLtree[n * 2]; n++;
            }
            while (n < 128) {
                ascii_freq += _dynLtree[n * 2]; n++;
            }
            while (n < StaticTree.LITERALS) {
                bin_freq += _dynLtree[n * 2]; n++;
            }
            _dataType = bin_freq > (ascii_freq >> 2) ? FileDataType.Binary : FileDataType.Ascii;
        }



        // Flush the bit buffer, keeping at most 7 bits in it.
        void BiFlush() {
            if (_biValid == 16) {
                Pending[PendingCount++] = (byte)_biBuf;
                Pending[PendingCount++] = (byte)(_biBuf >> 8);
                _biBuf = 0;
                _biValid = 0;
            } else if (_biValid >= 8) {
                //put_byte((byte)bi_buf);
                Pending[PendingCount++] = (byte)_biBuf;
                _biBuf >>= 8;
                _biValid -= 8;
            }
        }

        // Flush the bit buffer and align the output on a byte boundary
        void BiWindUp() {
            if (_biValid > 8) {
                Pending[PendingCount++] = (byte)_biBuf;
                Pending[PendingCount++] = (byte)(_biBuf >> 8);
            } else if (_biValid > 0) {
                //put_byte((byte)bi_buf);
                Pending[PendingCount++] = (byte)_biBuf;
            }
            _biBuf = 0;
            _biValid = 0;
        }

        // Copy a stored block, storing first the length and its
        // one's complement if requested.
        void CopyBlock(int buf, int len, bool header) {
            BiWindUp(); // align on byte boundary
            _lastEobLen = 8; // enough lookahead for inflate

            if (header)
                unchecked {
                    //put_short((short)len);
                    Pending[PendingCount++] = (byte)len;
                    Pending[PendingCount++] = (byte)(len >> 8);
                    //put_short((short)~len);
                    Pending[PendingCount++] = (byte)~len;
                    Pending[PendingCount++] = (byte)(~len >> 8);
                }

            PutBytes(_window, buf, len);
        }

        void FlushBlockOnly(bool eof) {
            TrFlushBlock(_blockStart >= 0 ? _blockStart : -1, _strstart - _blockStart, eof);
            _blockStart = _strstart;
            _codec.FlushPending();
        }

        // Copy without compression as much as possible from the input stream, return
        // the current block state.
        // This function does not insert new strings in the dictionary since
        // uncompressible data is probably not useful. This function is used
        // only for the level=0 compression option.
        // NOTE: this function should be optimized to avoid extra copying from
        // window to pending_buf.
        BlockState DeflateNone(FlushType flush) {
            // Stored blocks are limited to 0xffff bytes, pending is limited
            // to pending_buf_size, and each stored block has a 5 byte header:

            int max_block_size = 0xffff;
            int max_start;

            if (max_block_size > Pending.Length - 5) {
                max_block_size = Pending.Length - 5;
            }

            // Copy as much as possible from input to output:
            while (true) {
                // Fill the window as much as possible:
                if (_lookahead <= 1) {
                    FillWindow();
                    if (_lookahead == 0 && flush == FlushType.None)
                        return BlockState.NeedMore;
                    if (_lookahead == 0)
                        break; // flush the current block
                }

                _strstart += _lookahead;
                _lookahead = 0;

                // Emit a stored block if pending will be full:
                max_start = _blockStart + max_block_size;
                if (_strstart == 0 || _strstart >= max_start) {
                    // strstart == 0 is possible when wraparound on 16-bit machine
                    _lookahead = (int)(_strstart - max_start);
                    _strstart = (int)max_start;

                    FlushBlockOnly(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }

                // Flush if we may have to slide, otherwise block_start may become
                // negative and the data will be gone:
                if (_strstart - _blockStart >= _size - MIN_LOOKAHEAD) {
                    FlushBlockOnly(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }
            }

            FlushBlockOnly(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut == 0)
                return (flush == FlushType.Finish) ? BlockState.FinishStarted : BlockState.NeedMore;

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        // Send a stored block
        void TrStoredBlock(int buf, int stored_len, bool eof) {
            SendBits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3); // send block type
            CopyBlock(buf, stored_len, true); // with header
        }

        // Determine the best encoding for the current block: dynamic trees, static
        // trees or store, and output the encoded block to the zip file.
        void TrFlushBlock(int buf, int stored_len, bool eof) {
            int opt_lenb, static_lenb; // opt_len and static_len in bytes
            int max_blindex = 0; // index of last bit length code of non zero freq

            // Build the Huffman trees unless a stored block is forced
            if (_compressionLevel > 0) {
                // Check if the file is ascii or binary
                if (_dataType == FileDataType.Unknown)
                    SetDataType();

                // Construct the literal and distance trees
                _treeLiterals.BuildTree(this);

                _treeDistances.BuildTree(this);

                // At this point, opt_len and static_len are the total bit lengths of
                // the compressed block data, excluding the tree representations.

                // Build the bit length tree for the above two trees, and get the index
                // in bl_order of the last bit length code to send.
                max_blindex = BuildBlTree();

                // Determine the best encoding. Compute first the block length in bytes
                opt_lenb = (OptLen + 3 + 7) >> 3;
                static_lenb = (StaticLen + 3 + 7) >> 3;

                if (static_lenb <= opt_lenb)
                    opt_lenb = static_lenb;
            } else {
                opt_lenb = static_lenb = stored_len + 5; // force a stored block
            }

            if (stored_len + 4 <= opt_lenb && buf != -1) {
                // 4: two words for the lengths
                // The test buf != NULL is only necessary if LIT_BUFSIZE > WSIZE.
                // Otherwise we can't have processed more than WSIZE input bytes since
                // the last block flush, because compression would have been
                // successful. If LIT_BUFSIZE <= WSIZE, it is never too late to
                // transform a block into a stored block.
                TrStoredBlock(buf, stored_len, eof);
            } else if (static_lenb == opt_lenb) {
                SendBits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                SendCompressedBlock(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
            } else {
                SendBits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                SendAllTrees(_treeLiterals.MaxCode + 1, _treeDistances.MaxCode + 1, max_blindex + 1);
                SendCompressedBlock(_dynLtree, _dynDtree);
            }

            // The above check is made mod 2^32, for files larger than 512 MB
            // and uLong implemented on 32 bits.

            _InitializeBlocks();

            if (eof) {
                BiWindUp();
            }
        }

        // Fill the window when the lookahead becomes insufficient.
        // Updates strstart and lookahead.
        //
        // IN assertion: lookahead < MIN_LOOKAHEAD
        // OUT assertions: strstart <= window_size-MIN_LOOKAHEAD
        //    At least one byte has been read, or avail_in == 0; reads are
        //    performed for at least two bytes (required for the zip translate_eol
        //    option -- not supported here).
        private void FillWindow() {
            int n, m;
            int p;
            int more; // Amount of free space at the end of the window.

            do {
                more = (_windowSize - _lookahead - _strstart);

                // Deal with !@#$% 64K limit:
                if (more == 0 && _strstart == 0 && _lookahead == 0) {
                    more = _size;
                } else if (more == -1) {
                    // Very unlikely, but possible on 16 bit machine if strstart == 0
                    // and lookahead == 1 (input done one byte at time)
                    more--;

                    // If the window is almost full and there is insufficient lookahead,
                    // move the upper half to the lower one to make room in the upper half.
                } else if (_strstart >= _size + _size - MIN_LOOKAHEAD) {
                    Array.Copy(_window, _size, _window, 0, _size);
                    _matchStart -= _size;
                    _strstart -= _size; // we now have strstart >= MAX_DIST
                    _blockStart -= _size;

                    // Slide the hash table (could be avoided with 32 bit values
                    // at the expense of memory usage). We slide even when level == 0
                    // to keep the hash table consistent if we switch back to level > 0
                    // later. (Using level 0 permanently is not an optimal usage of
                    // zlib, so we don't care about this pathological case.)

                    n = _hashSize;
                    p = n;
                    do {
                        m = (_head[--p] & 0xffff);
                        _head[p] = (short)((m >= _size) ? (m - _size) : 0);
                    }
                    while (--n != 0);

                    n = _size;
                    p = n;
                    do {
                        m = (_prev[--p] & 0xffff);
                        _prev[p] = (short)((m >= _size) ? (m - _size) : 0);
                        // If n is not on any hash chain, prev[n] is garbage but
                        // its value will never be used.
                    }
                    while (--n != 0);
                    more += _size;
                }

                if (_codec.AvailableBytesIn == 0)
                    return;

                // If there was no sliding:
                //    strstart <= WSIZE+MAX_DIST-1 && lookahead <= MIN_LOOKAHEAD - 1 &&
                //    more == window_size - lookahead - strstart
                // => more >= window_size - (MIN_LOOKAHEAD-1 + WSIZE + MAX_DIST-1)
                // => more >= window_size - 2*WSIZE + 2
                // In the BIG_MEM or MMAP case (not yet supported),
                //   window_size == input_size + MIN_LOOKAHEAD  &&
                //   strstart + s->lookahead <= input_size => more >= MIN_LOOKAHEAD.
                // Otherwise, window_size == 2*WSIZE so more >= 2.
                // If there was sliding, more >= WSIZE. So in all cases, more >= 2.

                n = _codec.ReadBuf(_window, _strstart + _lookahead, more);
                _lookahead += n;

                // Initialize the hash value now that we have some input:
                if (_lookahead >= MIN_MATCH) {
                    _insH = _window[_strstart] & 0xff;
                    _insH = (((_insH) << _hashShift) ^ (_window[_strstart + 1] & 0xff)) & _hashMask;
                }
                // If the whole input has less than MIN_MATCH bytes, ins_h is garbage,
                // but this is not important since only literal bytes will be emitted.
            }
            while (_lookahead < MIN_LOOKAHEAD && _codec.AvailableBytesIn != 0);
        }

        // Compress as much as possible from the input stream, return the current
        // block state.
        // This function does not perform lazy evaluation of matches and inserts
        // new strings in the dictionary only for unmatched strings or for short
        // matches. It is used only for the fast compression options.
        BlockState DeflateFast(FlushType flush) {
            //    short hash_head = 0; // head of the hash chain
            int hash_head = 0; // head of the hash chain
            bool bflush; // set if current block must be flushed

            while (true) {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.
                if (_lookahead < MIN_LOOKAHEAD) {
                    FillWindow();
                    if (_lookahead < MIN_LOOKAHEAD && flush == FlushType.None) {
                        return BlockState.NeedMore;
                    }
                    if (_lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:
                if (_lookahead >= MIN_MATCH) {
                    _insH = (((_insH) << _hashShift) ^ (_window[(_strstart) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;

                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (_head[_insH] & 0xffff);
                    _prev[_strstart & _mask] = _head[_insH];
                    _head[_insH] = unchecked((short)_strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                // At this point we have always match_length < MIN_MATCH

                if (hash_head != 0L && ((_strstart - hash_head) & 0xffff) <= _size - MIN_LOOKAHEAD) {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).
                    if (_compressionStrategy != CompressionStrategy.HuffmanOnly) {
                        _matchLength = LongestMatch(hash_head);
                    }
                    // longest_match() sets match_start
                }
                if (_matchLength >= MIN_MATCH) {
                    //        check_match(strstart, match_start, match_length);

                    bflush = TrTally(_strstart - _matchStart, _matchLength - MIN_MATCH);

                    _lookahead -= _matchLength;

                    // Insert new strings in the hash table only if the match length
                    // is not too large. This saves time but degrades compression.
                    if (_matchLength <= _config.MaxLazy && _lookahead >= MIN_MATCH) {
                        _matchLength--; // string at strstart already in hash table
                        do {
                            _strstart++;

                            _insH = ((_insH << _hashShift) ^ (_window[(_strstart) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                            //      prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (_head[_insH] & 0xffff);
                            _prev[_strstart & _mask] = _head[_insH];
                            _head[_insH] = unchecked((short)_strstart);

                            // strstart never exceeds WSIZE-MAX_MATCH, so there are
                            // always MIN_MATCH bytes ahead.
                        }
                        while (--_matchLength != 0);
                        _strstart++;
                    } else {
                        _strstart += _matchLength;
                        _matchLength = 0;
                        _insH = _window[_strstart] & 0xff;

                        _insH = (((_insH) << _hashShift) ^ (_window[_strstart + 1] & 0xff)) & _hashMask;
                        // If lookahead < MIN_MATCH, ins_h is garbage, but it does not
                        // matter since it will be recomputed at next deflate call.
                    }
                } else {
                    // No match, output a literal byte

                    bflush = TrTally(0, _window[_strstart] & 0xff);
                    _lookahead--;
                    _strstart++;
                }
                if (bflush) {
                    FlushBlockOnly(false);
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                }
            }

            FlushBlockOnly(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut == 0) {
                if (flush == FlushType.Finish)
                    return BlockState.FinishStarted;
                else
                    return BlockState.NeedMore;
            }
            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }

        // Same as above, but achieves better compression. We use a lazy
        // evaluation for matches: a match is finally adopted only if there is
        // no better match at the next window position.
        BlockState DeflateSlow(FlushType flush) {
            //    short hash_head = 0;    // head of hash chain
            int hash_head = 0; // head of hash chain
            bool bflush; // set if current block must be flushed

            // Process the input block.
            while (true) {
                // Make sure that we always have enough lookahead, except
                // at the end of the input file. We need MAX_MATCH bytes
                // for the next match, plus MIN_MATCH bytes to insert the
                // string following the next match.

                if (_lookahead < MIN_LOOKAHEAD) {
                    FillWindow();
                    if (_lookahead < MIN_LOOKAHEAD && flush == FlushType.None)
                        return BlockState.NeedMore;

                    if (_lookahead == 0)
                        break; // flush the current block
                }

                // Insert the string window[strstart .. strstart+2] in the
                // dictionary, and set hash_head to the head of the hash chain:

                if (_lookahead >= MIN_MATCH) {
                    _insH = (((_insH) << _hashShift) ^ (_window[(_strstart) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                    //  prev[strstart&w_mask]=hash_head=head[ins_h];
                    hash_head = (_head[_insH] & 0xffff);
                    _prev[_strstart & _mask] = _head[_insH];
                    _head[_insH] = unchecked((short)_strstart);
                }

                // Find the longest match, discarding those <= prev_length.
                _prevLength = _matchLength;
                _prevMatch = _matchStart;
                _matchLength = MIN_MATCH - 1;

                if (hash_head != 0 && _prevLength < _config.MaxLazy &&
                    ((_strstart - hash_head) & 0xffff) <= _size - MIN_LOOKAHEAD) {
                    // To simplify the code, we prevent matches with the string
                    // of window index 0 (in particular we have to avoid a match
                    // of the string with itself at the start of the input file).

                    if (_compressionStrategy != CompressionStrategy.HuffmanOnly) {
                        _matchLength = LongestMatch(hash_head);
                    }
                    // longest_match() sets match_start

                    if (_matchLength <= 5 && (_compressionStrategy == CompressionStrategy.Filtered ||
                                              (_matchLength == MIN_MATCH && _strstart - _matchStart > 4096))) {

                        // If prev_match is also MIN_MATCH, match_start is garbage
                        // but we will ignore the current match anyway.
                        _matchLength = MIN_MATCH - 1;
                    }
                }

                // If there was a match at the previous step and the current
                // match is not better, output the previous match:
                if (_prevLength >= MIN_MATCH && _matchLength <= _prevLength) {
                    int max_insert = _strstart + _lookahead - MIN_MATCH;
                    // Do not insert strings in hash table beyond this.

                    //          check_match(strstart-1, prev_match, prev_length);

                    bflush = TrTally(_strstart - 1 - _prevMatch, _prevLength - MIN_MATCH);

                    // Insert in hash table all strings up to the end of the match.
                    // strstart-1 and strstart are already inserted. If there is not
                    // enough lookahead, the last two strings are not inserted in
                    // the hash table.
                    _lookahead -= (_prevLength - 1);
                    _prevLength -= 2;
                    do {
                        if (++_strstart <= max_insert) {
                            _insH = (((_insH) << _hashShift) ^ (_window[(_strstart) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                            //prev[strstart&w_mask]=hash_head=head[ins_h];
                            hash_head = (_head[_insH] & 0xffff);
                            _prev[_strstart & _mask] = _head[_insH];
                            _head[_insH] = unchecked((short)_strstart);
                        }
                    }
                    while (--_prevLength != 0);
                    _matchAvailable = 0;
                    _matchLength = MIN_MATCH - 1;
                    _strstart++;

                    if (bflush) {
                        FlushBlockOnly(false);
                        if (_codec.AvailableBytesOut == 0)
                            return BlockState.NeedMore;
                    }
                } else if (_matchAvailable != 0) {

                    // If there was no match at the previous position, output a
                    // single literal. If there was a match but the current match
                    // is longer, truncate the previous match to a single literal.

                    bflush = TrTally(0, _window[_strstart - 1] & 0xff);

                    if (bflush) {
                        FlushBlockOnly(false);
                    }
                    _strstart++;
                    _lookahead--;
                    if (_codec.AvailableBytesOut == 0)
                        return BlockState.NeedMore;
                } else {
                    // There is no previous match to compare with, wait for
                    // the next step to decide.

                    _matchAvailable = 1;
                    _strstart++;
                    _lookahead--;
                }
            }

            if (_matchAvailable != 0) {
                bflush = TrTally(0, _window[_strstart - 1] & 0xff);
                _matchAvailable = 0;
            }
            FlushBlockOnly(flush == FlushType.Finish);

            if (_codec.AvailableBytesOut == 0) {
                if (flush == FlushType.Finish)
                    return BlockState.FinishStarted;
                else
                    return BlockState.NeedMore;
            }

            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }


        int LongestMatch(int cur_match) {
            int chain_length = _config.MaxChainLength; // max hash chain length
            int scan = _strstart;              // current string
            int match;                                // matched string
            int len;                                  // length of current match
            int best_len = _prevLength;           // best match length so far
            int limit = _strstart > (_size - MIN_LOOKAHEAD) ? _strstart - (_size - MIN_LOOKAHEAD) : 0;

            int niceLength = _config.NiceLength;

            // Stop when cur_match becomes <= limit. To simplify the code,
            // we prevent matches with the string of window index 0.

            int wmask = _mask;

            int strend = _strstart + MAX_MATCH;
            byte scan_end1 = _window[scan + best_len - 1];
            byte scan_end = _window[scan + best_len];

            // The code is optimized for HASH_BITS >= 8 and MAX_MATCH-2 multiple of 16.
            // It is easy to get rid of this optimization if necessary.

            // Do not waste too much time if we already have a good match:
            if (_prevLength >= _config.GoodLength) {
                chain_length >>= 2;
            }

            // Do not look for matches beyond the end of the input. This is necessary
            // to make deflate deterministic.
            if (niceLength > _lookahead)
                niceLength = _lookahead;

            do {
                match = cur_match;

                // Skip to next match if the match length cannot increase
                // or if the match length is less than 2:
                if (_window[match + best_len] != scan_end ||
                    _window[match + best_len - 1] != scan_end1 ||
                    _window[match] != _window[scan] ||
                    _window[++match] != _window[scan + 1])
                    continue;

                // The check at best_len-1 can be removed because it will be made
                // again later. (This heuristic is not always a win.)
                // It is not necessary to compare scan[2] and match[2] since they
                // are always equal when the other bytes match, given that
                // the hash keys are equal and that HASH_BITS >= 8.
                scan += 2; match++;

                // We check for insufficient lookahead only every 8th comparison;
                // the 256th check will be made at strstart+258.
                while (_window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] &&
                       _window[++scan] == _window[++match] && scan < strend) ;

                len = MAX_MATCH - (int)(strend - scan);
                scan = strend - MAX_MATCH;

                if (len > best_len) {
                    _matchStart = cur_match;
                    best_len = len;
                    if (len >= niceLength)
                        break;
                    scan_end1 = _window[scan + best_len - 1];
                    scan_end = _window[scan + best_len];
                }
            }
            while ((cur_match = (_prev[cur_match & wmask] & 0xffff)) > limit && --chain_length != 0);

            if (best_len <= _lookahead)
                return best_len;
            return _lookahead;
        }


        private bool _rfc1950BytesEmitted = false;
        public bool WantRfc1950HeaderBytes;

        /// <summary>
        /// 使用指定的解码器，压缩等级初始化。
        /// </summary>
        /// <param name="codec">解码器。</param>
        /// <param name="level">初始化。</param>
        /// <returns>初始化的返回码。成功返回 0 。</returns>
        public ZlibState Initialize(ZlibCodec codec, CompressionLevel level) {
            return Initialize(codec, level, WindowBitsMax);
        }

        /// <summary>
        /// 使用指定的解码器，压缩等级初始化。
        /// </summary>
        /// <param name="codec">解码器。</param>
        /// <param name="level">初始化。</param>
        /// <param name="bits">大小。</param>
        /// <returns>初始化的返回码。成功返回 0 。</returns>
        public ZlibState Initialize(ZlibCodec codec, CompressionLevel level, int bits) {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
        }

        /// <summary>
        /// 使用指定的解码器，压缩等级初始化。
        /// </summary>
        /// <param name="codec">解码器。</param>
        /// <param name="level">初始化。</param>
        /// <param name="bits">大小。</param>
        /// <param name="compressionStrategy">压缩策略。</param>
        /// <returns>初始化的返回码。成功返回 0 。</returns>
        public ZlibState Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy) {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, compressionStrategy);
        }

        /// <summary>
        /// 使用指定的解码器，压缩等级初始化。
        /// </summary>
        /// <param name="codec">解码器。</param>
        /// <param name="level">初始化。</param>
        /// <param name="windowBits">大小。</param>
        /// <param name="strategy">压缩策略。</param>
        /// <param name="memLevel">内存大小。</param>
        /// <returns>初始化的返回码。成功返回 0 。</returns>
        public ZlibState Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy) {
            _codec = codec;
            _codec.Message = null;

            Thrower.ThrowZlibExceptionIf(windowBits < 9 || windowBits > 15, "窗口大小必须在 9 到 15 之间。");

            Thrower.ThrowZlibExceptionIf(memLevel < 1 || memLevel > MEM_LEVEL_MAX, "memLevel 必须为 1.. " + MEM_LEVEL_MAX + " 之间。");

            _codec.DState = this;

            _bits = windowBits;
            _size = 1 << _bits;
            _mask = _size - 1;

            _hashBits = memLevel + 7;
            _hashSize = 1 << _hashBits;
            _hashMask = _hashSize - 1;
            _hashShift = ((_hashBits + MIN_MATCH - 1) / MIN_MATCH);

            _window = new byte[_size * 2];
            _prev = new short[_size];
            _head = new short[_hashSize];

            // for memLevel==8, this will be 16384, 16k
            _litBufsize = 1 << (memLevel + 6);

            // Use a single array as the buffer for data pending compression,
            // the output distance codes, and the output length codes (aka tree).  
            // orig comment: This works just fine since the average
            // output size for (length,distance) codes is <= 24 bits.
            Pending = new byte[_litBufsize * 4];
            _distanceOffset = _litBufsize;
            _lengthOffset = (1 + 2) * _litBufsize;

            // So, for memLevel 8, the length of the pending buffer is 65536. 64k.
            // The first 16k are pending bytes.
            // The middle slice, of 32k, is used for distance codes. 
            // The final 16k are length codes.

            this._compressionLevel = level;
            this._compressionStrategy = strategy;

            Reset();
            return ZlibState.Success;
        }

        /// <summary>
        /// 重新设置管理器的状态。
        /// </summary>
        public void Reset() {
            _codec.TotalBytesIn = _codec.TotalBytesOut = 0;
            _codec.Message = null;
            //strm.data_type = Z_UNKNOWN;

            PendingCount = 0;
            NextPending = 0;

            _rfc1950BytesEmitted = false;

            _status = (WantRfc1950HeaderBytes) ? ManagerState.Init : ManagerState.Busy;
            _codec.Adler32 = Adler.Adler32(0, null, 0, 0);

            _lastFlush = (int)FlushType.None;


            #region InitializeTreeData

            _treeLiterals.DynTree = _dynLtree;
            _treeLiterals.staticTree = StaticTree.Literals;

            _treeDistances.DynTree = _dynDtree;
            _treeDistances.staticTree = StaticTree.Distances;

            _treeBitLengths.DynTree = _blTree;
            _treeBitLengths.staticTree = StaticTree.BitLengths;

            _biBuf = 0;
            _biValid = 0;
            _lastEobLen = 8; // enough lookahead for inflate

            // Initialize the first block of the first file:
            _InitializeBlocks();

            #endregion

            #region InitializeLazyMatch

            _windowSize = 2 * _size;

            // clear the hash - workitem 9063
            Array.Clear(_head, 0, _hashSize);
            //for (int i = 0; i < hash_size; i++) head[i] = 0;

            _config = Config.Lookup(_compressionLevel);
            SetDeflater();

            _strstart = 0;
            _blockStart = 0;
            _lookahead = 0;
            _matchLength = _prevLength = MIN_MATCH - 1;
            _matchAvailable = 0;
            _insH = 0;

            #endregion
        }


        public ZlibState End() {
            if (_status != ManagerState.Init && _status != ManagerState.Busy && _status != ManagerState.Finish) {
                return ZlibState.StreamError;
            }
            // Deallocate in reverse order of allocations:
            Pending = null;
            _head = null;
            _prev = null;
            _window = null;
            // free
            // dstate=null;
            return _status == ManagerState.Busy ? ZlibState.DataError : ZlibState.Success;
        }

        /// <summary>
        /// 设置 Deflate 需要的函数。
        /// </summary>
        private void SetDeflater() {
            switch (_config.Flavor) {
                case DeflateFlavor.Store:
                    DeflateFunction = DeflateNone;
                    break;
                case DeflateFlavor.Fast:
                    DeflateFunction = DeflateFast;
                    break;
                case DeflateFlavor.Slow:
                    DeflateFunction = DeflateSlow;
                    break;
            }
        }

        /// <summary>
        /// 设置解压参数。
        /// </summary>
        /// <param name="level">压缩等级。</param>
        /// <param name="strategy">压缩策略。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
        public ZlibState SetParams(CompressionLevel level, CompressionStrategy strategy) {
            ZlibState result = ZlibState.Success;

            if (_compressionLevel != level) {
                Config newConfig = Config.Lookup(level);

                // change in the deflate flavor (Fast vs slow vs none)?
                if (newConfig.Flavor != _config.Flavor && _codec.TotalBytesIn != 0) {
                    // Flush the last buffer:
                    result = _codec.Deflate(FlushType.Partial);
                }

                _compressionLevel = level;
                _config = newConfig;
                SetDeflater();
            }

            // no need to flush with change in strategy?  Really? 
            _compressionStrategy = strategy;

            return result;
        }

        /// <summary>
        /// 设置解压时的字典。
        /// </summary>
        /// <param name="dictionary">The dictionary bytes to use.</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
        public ZlibState SetDictionary(byte[] dictionary) {
            int length = dictionary.Length;
            int index = 0;

            Thrower.ThrowZlibExceptionIf(dictionary == null || _status != ManagerState.Init, "流错误。");

            _codec.Adler32 = Adler.Adler32(_codec.Adler32, dictionary, 0, dictionary.Length);

            if (length < MIN_MATCH)
                return ZlibState.Success;
            if (length > _size - MIN_LOOKAHEAD) {
                length = _size - MIN_LOOKAHEAD;
                index = dictionary.Length - length; // use the tail of the dictionary
            }
            Array.Copy(dictionary, index, _window, 0, length);
            _strstart = length;
            _blockStart = length;

            // Insert all strings in the hash table (except for the last two bytes).
            // s->lookahead stays null, so s->ins_h will be recomputed at the next
            // call of fill_window.

            _insH = _window[0] & 0xff;
            _insH = (((_insH) << _hashShift) ^ (_window[1] & 0xff)) & _hashMask;

            for (int n = 0; n <= length - MIN_MATCH; n++) {
                _insH = (((_insH) << _hashShift) ^ (_window[(n) + (MIN_MATCH - 1)] & 0xff)) & _hashMask;
                _prev[n & _mask] = _head[_insH];
                _head[_insH] = (short)n;
            }
            return ZlibState.Success;
        }

        /// <summary>
        /// 解压数据。
        /// </summary>
        /// <remarks>
        /// 必须先设置 InputBuffer 和 OutputBuffer 。
        /// </remarks>
        /// <param name="flush">输出方式。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
        public ZlibState Deflate(FlushType flush) {
            int old_flush;

            if (_codec.OutputBuffer == null ||
                (_codec.InputBuffer == null && _codec.AvailableBytesIn != 0) ||
                (_status == ManagerState.Finish && flush != FlushType.Finish)) {
                _codec.Message = "流错误。";
                throw new ZlibException(String.Format("对象未初始化。[{0}]", _codec.Message));
                //return ZlibState.StreamError;
            }
            if (_codec.AvailableBytesOut == 0) {
                _codec.Message = "缓存错误。";
                throw new ZlibException("缓存已满 (AvailableBytesOut == 0) 。");
                //return ZlibState.BufferError;
            }

            old_flush = _lastFlush;
            _lastFlush = (int)flush;

            // Write the zlib (rfc1950) header bytes
            if (_status == ManagerState.Init) {
                int header = (Z_DEFLATED + ((_bits - 8) << 4)) << 8;
                int level_flags = (((int)_compressionLevel - 1) & 0xff) >> 1;

                if (level_flags > 3)
                    level_flags = 3;
                header |= (level_flags << 6);
                if (_strstart != 0)
                    header |= PRESET_DICT;
                header += 31 - (header % 31);

                _status = ManagerState.Busy;
                //putShortMSB(header);
                unchecked {
                    Pending[PendingCount++] = (byte)(header >> 8);
                    Pending[PendingCount++] = (byte)header;
                }
                // Save the adler32 of the preset dictionary:
                if (_strstart != 0) {
                    ////putShortMSB((int)(SharedUtils.URShift(_codec._Adler32, 16)));
                    //putShortMSB((int)((UInt64)_codec._Adler32 >> 16));
                    //putShortMSB((int)(_codec._Adler32 & 0xffff));
                    Pending[PendingCount++] = (byte)((_codec.Adler32 & 0xFF000000) >> 24);
                    Pending[PendingCount++] = (byte)((_codec.Adler32 & 0x00FF0000) >> 16);
                    Pending[PendingCount++] = (byte)((_codec.Adler32 & 0x0000FF00) >> 8);
                    Pending[PendingCount++] = (byte)(_codec.Adler32 & 0x000000FF);
                }
                _codec.Adler32 = Adler.Adler32(0, null, 0, 0);
            }

            // Flush as much pending output as possible
            if (PendingCount != 0) {
                _codec.FlushPending();
                if (_codec.AvailableBytesOut == 0) {
                    //System.out.println("  avail_out==0");
                    // Since avail_out is 0, deflate will be called again with
                    // more output space, but possibly with both pending and
                    // avail_in equal to zero. There won't be anything to do,
                    // but this is not an error situation so make sure we
                    // return OK instead of BUF_ERROR at next call of deflate:
                    _lastFlush = -1;
                    return ZlibState.Success;
                }

                // Make sure there is something to do and avoid duplicate consecutive
                // flushes. For repeated and useless calls with Z_FINISH, we keep
                // returning Z_STREAM_END instead of Z_BUFF_ERROR.
            } else if (_codec.AvailableBytesIn == 0 &&
                     (int)flush <= old_flush &&
                     flush != FlushType.Finish) {
                // workitem 8557
                // Not sure why this needs to be an error.
                // pendingCount == 0, which means there's nothing to deflate.
                // And the caller has not asked for a FlushType.Finish, but...
                // that seems very non-fatal.  We can just say "OK" and do nthing.

                // _codec.Message = z_errmsg[ZlibState.NeedDict - (ZlibState.BufferError)];
                // throw new ZlibException("AvailableBytesIn == 0 && flush<=old_flush && flush != FlushType.Finish");

                return ZlibState.Success;
            }

            // User must not provide more input after the first FINISH:
            if (_status == ManagerState.Finish && _codec.AvailableBytesIn != 0) {
                _codec.Message = "缓存错误。";
                throw new ZlibException("已完成，但可用字节不为 0 。");
            }


            // Start a new block or continue the current one.
            if (_codec.AvailableBytesIn != 0 || _lookahead != 0 || (flush != FlushType.None && _status != ManagerState.Finish)) {

                BlockState bstate = DeflateFunction(flush);

                if (bstate == BlockState.FinishStarted || bstate == BlockState.FinishDone) {
                    _status = ManagerState.Finish;
                }
                if (bstate == BlockState.NeedMore || bstate == BlockState.FinishStarted) {
                    if (_codec.AvailableBytesOut == 0) {
                        _lastFlush = -1; // avoid BUF_ERROR next call, see above
                    }
                    return ZlibState.Success;
                    // If flush != Z_NO_FLUSH && avail_out == 0, the next call
                    // of deflate should use the same flush parameter to make sure
                    // that the flush is complete. So we don't have to output an
                    // empty block here, this will be done at next call. This also
                    // ensures that for a very small output buffer, we emit at most
                    // one empty block.
                }

                if (bstate == BlockState.BlockDone) {
                    if (flush == FlushType.Partial) {

                        #region tr_align

                        // Send one empty static block to give enough lookahead for inflate.
                        // This takes 10 bits, of which 7 may remain in the bit buffer.
                        // The current inflate code requires 9 bits of lookahead. If the
                        // last two codes for the previous block (real code plus EOB) were coded
                        // on 5 bits or less, inflate may have only 5+3 bits of lookahead to decode
                        // the last real code. In this case we send two empty static blocks instead
                        // of one. (There are no problems if the previous block is stored or fixed.)
                        // To simplify the code, we assume the worst case of last real code encoded
                        // on one bit only.

                        SendBits(STATIC_TREES << 1, 3);
                        SendCode(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);

                        BiFlush();

                        // Of the 10 bits for the empty block, we have already sent
                        // (10 - bi_valid) bits. The lookahead for the last real code (before
                        // the EOB of the previous block) was thus at least one plus the length
                        // of the EOB plus what we have just sent of the empty static block.
                        if (1 + _lastEobLen + 10 - _biValid < 9) {
                            SendBits(STATIC_TREES << 1, 3);
                            SendCode(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
                            BiFlush();
                        }
                        _lastEobLen = 7;

                        #endregion

                    } else {
                        // FlushType.Full or FlushType.Sync
                        TrStoredBlock(0, 0, false);
                        // For a full flush, this empty block will be recognized
                        // as a special marker by inflate_sync().
                        if (flush == FlushType.Full) {
                            // clear hash (forget the history)
                            for (int i = 0; i < _hashSize; i++)
                                _head[i] = 0;
                        }
                    }
                    _codec.FlushPending();
                    if (_codec.AvailableBytesOut == 0) {
                        _lastFlush = -1; // avoid BUF_ERROR at next call, see above
                        return ZlibState.Success;
                    }
                }
            }

            if (flush != FlushType.Finish)
                return ZlibState.Success;

            if (!WantRfc1950HeaderBytes || _rfc1950BytesEmitted)
                return ZlibState.StreamEnd;

            // Write the zlib trailer (adler32)
            Pending[PendingCount++] = (byte)((_codec.Adler32 & 0xFF000000) >> 24);
            Pending[PendingCount++] = (byte)((_codec.Adler32 & 0x00FF0000) >> 16);
            Pending[PendingCount++] = (byte)((_codec.Adler32 & 0x0000FF00) >> 8);
            Pending[PendingCount++] = (byte)(_codec.Adler32 & 0x000000FF);
            //putShortMSB((int)(SharedUtils.URShift(_codec._Adler32, 16)));
            //putShortMSB((int)(_codec._Adler32 & 0xffff));

            _codec.FlushPending();

            // If avail_out is zero, the application will call deflate again
            // to flush the rest.

            _rfc1950BytesEmitted = true; // write the trailer only once!

            return PendingCount != 0 ? ZlibState.Success : ZlibState.StreamEnd;
        }

    }
}