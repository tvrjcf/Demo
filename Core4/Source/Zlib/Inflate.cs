// Inflate.cs
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
// Time-stamp: <2010-January-08 18:32:12>
//
// ------------------------------------------------------------------
//
// This module defines classes for decompression. This code is derived
// from the jzlib implementation of zlib, but significantly modified.
// The object model is not the same, and many of the behaviors are
// different.  Nonetheless, in keeping with the license for jzlib, I am
// reproducing the copyright to that code here.
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


using System;
using Py.Zip.Zlib;
namespace Py.Zip {

	/// <summary>
	/// 表示 Inflate 块。不可继承此类。
	/// </summary>
	sealed class InflateBlocks {

		/// <summary>
		/// 默认树的大小。
		/// </summary>
		private const int BUFFER = 1440;

		/// <summary>
		/// 用来表示解压的数据表。
		/// </summary>
		/// <remarks>
		/// 来自 PKZIP 的 appnote.txt 。
		/// </remarks>
		static readonly int[] _border = new int[] { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

		/// <summary>
		/// 表示块类型。
		/// </summary>
		private enum InflateBlockMode {

			/// <summary>
			/// 类型。
			/// </summary>
			Type = 0,

			/// <summary>
			/// 已存储长度。
			/// </summary>
			Lens = 1,

			/// <summary>
			/// 处理存储的块。
			/// </summary>
			Stored = 2, 

			/// <summary>
			/// 表长。
			/// </summary>
			Table = 3, 

			/// <summary>
			/// 树长。
			/// </summary>
			Btree = 4,

			/// <summary>
			/// 树节点距离。
			/// </summary>
			Dtree = 5,

			/// <summary>
			/// 动态或固定块。
			/// </summary>
			Codes = 6,

			/// <summary>
			/// 保留输出。
			/// </summary>
			Dry = 7,

			/// <summary>
			/// 完成。
			/// </summary>
			Done = 8,

			/// <summary>
			/// 出现错误。
			/// </summary>
			Bad = 9,
		}

		/// <summary>
		/// 当前的模式。
		/// </summary>
		private InflateBlockMode _mode;

		/// <summary>
		/// 需要读取的大小。
		/// </summary>
		int _left;

		/// <summary>
		/// 表的长度。 （14）
		/// </summary>
		int _table;
		
		/// <summary>
		/// 索引。
		/// </summary>
		int _index;

		/// <summary>
		/// 代码长度。
		/// </summary>
		int[] _blens;

		/// <summary>
		/// 树高度。
		/// </summary>
		int[] _bb = new int[1];

		/// <summary>
		/// 解码树的大小。
		/// </summary>
		int[] _tb = new int[1];

		/// <summary>
		/// 当前解压的代码。
		/// </summary>
		InflateCodes _codes = new InflateCodes();

		/// <summary>
		/// 指示这个块是否为最后一个。
		/// </summary>
		bool _last;

		/// <summary>
		/// 解码器。
		/// </summary>
		public ZlibCodec Codec;

		/// <summary>
		/// 字节缓存中的字缓存长度。
		/// </summary>
		public int Bitk;

		/// <summary>
		/// 字节缓存大小。
		/// </summary>
		public int Bitb;

		/// <summary>
		/// 树节点。
		/// </summary>
		int[] _hufts;

		/// <summary>
		/// 移动的窗口。
		/// </summary>
		public byte[] Window;

		/// <summary>
		/// 移动窗口长度。
		/// </summary>
		public int End;

		/// <summary>
		/// 窗口的读取位置。
		/// </summary>
		public int ReadAt;

		/// <summary>
		/// 窗口的写入位置。
		/// </summary>
		public int WriteAt;

		/// <summary>
		/// 是否检查内容。
		/// </summary>
		bool _checkfn;

		/// <summary>
		/// 检查输出。
		/// </summary>
		uint _check;

		/// <summary>
		/// 解压树。
		/// </summary>
		InfTree _inftree = new InfTree();

		/// <summary>
		/// 初始化 <see cref="Py.Zip.InflateBlocks"/> 的新实例。
		/// </summary>
		/// <param name="codec">解码器。</param>
		/// <param name="checkfn">是否检查。</param>
		/// <param name="w">窗口的大小。</param>
		public InflateBlocks(ZlibCodec codec, bool checkfn, int w) {
			Codec = codec;
			_hufts = new int[BUFFER * 3];
			Window = new byte[w];
			End = w;
			_checkfn = checkfn;
			_mode = InflateBlockMode.Type;
			Reset();
		}

		/// <summary>
		/// 重新设置变量。
		/// </summary>
		/// <returns>检验码。</returns>
		public uint Reset() {
			uint oldCheck = _check;
			_mode = InflateBlockMode.Type;
			Bitk = 0;
			Bitb = 0;
			ReadAt = WriteAt = 0;

			if (_checkfn)
				Codec.Adler32 = _check = Adler.Adler32(0, null, 0, 0);
			return oldCheck;
		}

		/// <summary>
		/// 处理块。
		/// </summary>
		/// <param name="r">返回状态。</param>
		/// <returns>返回处理状态。</returns>
		public void Process(ref ZlibState r) {
			int t; // 临时变量
			int b; // 字节缓存
			int k; // 缓存字节大小
			int p; // 输入位置
			int n; // 可使用的指针
			int q; // 输出位置
			int m; // 尾部的大小

			// 复制 输入/输出 变量值。

			p = Codec.NextIn;
			n = Codec.AvailableBytesIn;
			b = Bitb;
			k = Bitk;

			q = WriteAt;
			m = q < ReadAt ? ReadAt - q - 1 : End - q;


			// 处理当前状态的头。
			while (true) {
				switch (_mode) {
					case InflateBlockMode.Type:

						while (k < (3)) {
							if (n != 0) {
								r = ZlibState.Success;
							} else {
								Bitb = b; Bitk = k;
								Codec.AvailableBytesIn = n;
								Codec.TotalBytesIn += p - Codec.NextIn;
								Codec.NextIn = p;
								WriteAt = q;
								Flush(ref r);
								return;
							}

							n--;
							b |= (Codec.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}
						t = (int)(b & 7);
						_last = ( t & 1 ) == 1;

						switch ((uint)t >> 1) {
							case 0:  // stored
								b >>= 3; k -= (3);
								t = k & 7; // go to byte boundary
								b >>= t; k -= t;
								_mode = InflateBlockMode.Lens; // get length of stored block
								break;

							case 1:  // fixed
								int[] bl = new int[1];
								int[] bd = new int[1];
								int[][] tl = new int[1][];
								int[][] td = new int[1][];
								InfTree.InflateTreesFixed(bl, bd, tl, td, Codec);
								_codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
								b >>= 3; k -= 3;
								_mode = InflateBlockMode.Codes;
								break;

							case 2:  // dynamic
								b >>= 3; k -= 3;
								_mode = InflateBlockMode.Table;
								break;

							case 3:  // illegal
								b >>= 3; k -= 3;
								_mode = InflateBlockMode.Bad;
								Codec.Message = "非法的块状态。";
								r = ZlibState.DataError;
								Bitb = b; Bitk = k;
								Codec.AvailableBytesIn = n;
								Codec.TotalBytesIn += p - Codec.NextIn;
								Codec.NextIn = p;
								WriteAt = q;
								Flush(ref r);
								return;
						}
						break;

					case InflateBlockMode.Lens:

						while (k < (32)) {
							if (n != 0) {
								r = ZlibState.Success;
							} else {
								Bitb = b; Bitk = k;
								Codec.AvailableBytesIn = n;
								Codec.TotalBytesIn += p - Codec.NextIn;
								Codec.NextIn = p;
								WriteAt = q;
								Flush(ref r);
								return;

							}
							;
							n--;
							b |= (Codec.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						if ((((~b) >> 16) & 0xffff) != (b & 0xffff)) {
							_mode = InflateBlockMode.Bad;
							Codec.Message = "非法的存储块长";
							r = ZlibState.DataError;

							Bitb = b; Bitk = k;
							Codec.AvailableBytesIn = n;
							Codec.TotalBytesIn += p - Codec.NextIn;
							Codec.NextIn = p;
							WriteAt = q;
							Flush(ref r);
							return;

						}
						_left = (b & 0xffff);
						b = k = 0; // dump bits
						_mode = _left != 0 ? InflateBlockMode.Stored : (_last ? InflateBlockMode.Dry : InflateBlockMode.Type);
						break;

					case InflateBlockMode.Stored:
						if (n == 0) {
							Bitb = b; Bitk = k;
							Codec.AvailableBytesIn = n;
							Codec.TotalBytesIn += p - Codec.NextIn;
							Codec.NextIn = p;
							WriteAt = q;
							Flush(ref r);
							return;

						}

						if (m == 0) {
							if (q == End && ReadAt != 0) {
								q = 0; m = (int)(q < ReadAt ? ReadAt - q - 1 : End - q);
							}
							if (m == 0) {
								WriteAt = q;
								Flush(ref r);
								q = WriteAt; m = (int)(q < ReadAt ? ReadAt - q - 1 : End - q);
								if (q == End && ReadAt != 0) {
									q = 0; m = (int)(q < ReadAt ? ReadAt - q - 1 : End - q);
								}
								if (m == 0) {
									Bitb = b; Bitk = k;
									Codec.AvailableBytesIn = n;
									Codec.TotalBytesIn += p - Codec.NextIn;
									Codec.NextIn = p;
									WriteAt = q;
									Flush(ref r);
									return;


								}
							}
						}
						r = ZlibState.Success;

						t = _left;
						if (t > n)
							t = n;
						if (t > m)
							t = m;
						Array.Copy(Codec.InputBuffer, p, Window, q, t);
						p += t; n -= t;
						q += t; m -= t;
						if ((_left -= t) != 0)
							break;
						_mode = _last ? InflateBlockMode.Dry : InflateBlockMode.Type;
						break;

					case InflateBlockMode.Table:

						while (k < (14)) {
							if (n != 0) {
								r = ZlibState.Success;
							} else {
								Bitb = b; Bitk = k;
								Codec.AvailableBytesIn = n;
								Codec.TotalBytesIn += p - Codec.NextIn;
								Codec.NextIn = p;
								WriteAt = q;
								Flush(ref r);
								return;
							}

							n--;
							b |= (Codec.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						_table = t = (b & 0x3fff);
						if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29) {
							_mode = InflateBlockMode.Bad;
							Codec.Message = "符号距离或长度不合法";
							r = ZlibState.DataError;

							Bitb = b; Bitk = k;
							Codec.AvailableBytesIn = n;
							Codec.TotalBytesIn += p - Codec.NextIn;
							Codec.NextIn = p;
							WriteAt = q;
							Flush(ref r);
							return;

						}
						t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
						if (_blens == null || _blens.Length < t) {
							_blens = new int[t];
						} else {
							Array.Clear(_blens, 0, t);
							// for (int i = 0; i < t; i++)
							// {
							//     blens[i] = 0;
							// }
						}

						b >>= 14;
						k -= 14;


						_index = 0;
						_mode = InflateBlockMode.Btree;
						goto case InflateBlockMode.Btree;

					case InflateBlockMode.Btree:
						while (_index < 4 + (_table >> 10)) {
							while (k < (3)) {
								if (n != 0) {
									r = ZlibState.Success;
								} else {
									Bitb = b; Bitk = k;
									Codec.AvailableBytesIn = n;
									Codec.TotalBytesIn += p - Codec.NextIn;
									Codec.NextIn = p;
									WriteAt = q;
									Flush(ref r);
									return;
								}

								n--;
								b |= (Codec.InputBuffer[p++] & 0xff) << k;
								k += 8;
							}

							_blens[_border[_index++]] = b & 7;

							b >>= 3; k -= 3;
						}

						while (_index < 19) {
							_blens[_border[_index++]] = 0;
						}

						_bb[0] = 7;
						ZlibState ts  = (ZlibState)_inftree.InflateTreesBits(_blens, _bb, _tb, _hufts, Codec);
						if (ts != ZlibState.Success) {
							r = ts;
							if (r == ZlibState.DataError) {
								_blens = null;
								_mode = InflateBlockMode.Bad;
							}

							Bitb = b; Bitk = k;
							Codec.AvailableBytesIn = n;
							Codec.TotalBytesIn += p - Codec.NextIn;
							Codec.NextIn = p;
							WriteAt = q;
							Flush(ref r);
							return;
						}

						_index = 0;
						_mode = InflateBlockMode.Dtree;
						goto case InflateBlockMode.Dtree;

					case InflateBlockMode.Dtree:
						while (true) {
							t = _table;
							if (!(_index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f))) {
								break;
							}

							int i, j, c;

							t = _bb[0];

							while (k < t) {
								if (n != 0) {
									r = ZlibState.Success;
								} else {
									Bitb = b; Bitk = k;
									Codec.AvailableBytesIn = n;
									Codec.TotalBytesIn += p - Codec.NextIn;
									Codec.NextIn = p;
									WriteAt = q;
									Flush(ref r);
									return;
								}

								n--;
								b |= (Codec.InputBuffer[p++] & 0xff) << k;
								k += 8;
							}

							t = _hufts[(_tb[0] + (b & InflateCodes.InflateMask[t])) * 3 + 1];
							c = _hufts[(_tb[0] + (b & InflateCodes.InflateMask[t])) * 3 + 2];

							if (c < 16) {
								b >>= t; k -= t;
								_blens[_index++] = c;
							} else {
								// c == 16..18
								i = c == 18 ? 7 : c - 14;
								j = c == 18 ? 11 : 3;

								while (k < (t + i)) {
									if (n != 0) {
										r = ZlibState.Success;
									} else {
										Bitb = b; Bitk = k;
										Codec.AvailableBytesIn = n;
										Codec.TotalBytesIn += p - Codec.NextIn;
										Codec.NextIn = p;
										WriteAt = q;
										Flush(ref r);
										return;
									}

									n--;
									b |= (Codec.InputBuffer[p++] & 0xff) << k;
									k += 8;
								}

								b >>= t; k -= t;

								j += (b & InflateCodes.InflateMask[i]);

								b >>= i; k -= i;

								i = _index;
								t = _table;
								if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || (c == 16 && i < 1)) {
									_blens = null;
									_mode = InflateBlockMode.Bad;
									Codec.Message = "重复字节长度不正确";
									r = ZlibState.DataError;

									Bitb = b; Bitk = k;
									Codec.AvailableBytesIn = n;
									Codec.TotalBytesIn += p - Codec.NextIn;
									Codec.NextIn = p;
									WriteAt = q;
									Flush(ref r);
									return;
								}

								c = (c == 16) ? _blens[i - 1] : 0;
								do {
									_blens[i++] = c;
								}
								while (--j != 0);
								_index = i;
							}
						}

						_tb[0] = -1; {
							int[] bl = new int[] { 9 };  // must be <= 9 for lookahead assumptions
							int[] bd = new int[] { 6 }; // must be <= 9 for lookahead assumptions
							int[] tl = new int[1];
							int[] td = new int[1];

							t = _table;
                            ZlibState temp = (ZlibState)_inftree.InflateTreesDynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), _blens, bl, bd, tl, td, _hufts, Codec);

							if (temp != ZlibState.Success) {
								if (temp == ZlibState.DataError) {
									_blens = null;
									_mode = InflateBlockMode.Bad;
								}
								r = temp;

								Bitb = b; Bitk = k;
								Codec.AvailableBytesIn = n;
								Codec.TotalBytesIn += p - Codec.NextIn;
								Codec.NextIn = p;
								WriteAt = q;
								Flush(ref r);
								return;
							}
							_codes.Init(bl[0], bd[0], _hufts, tl[0], _hufts, td[0]);
						}
						_mode = InflateBlockMode.Codes;
						goto case InflateBlockMode.Codes;

					case InflateBlockMode.Codes:
						Bitb = b; Bitk = k;
						Codec.AvailableBytesIn = n;
						Codec.TotalBytesIn += p - Codec.NextIn;
						Codec.NextIn = p;
						WriteAt = q;

						_codes.Process(this,ref r);
						if (r != ZlibState.StreamEnd) {
							Flush(ref r);
							return;
						}

						r = ZlibState.Success;
						p = Codec.NextIn;
						n = Codec.AvailableBytesIn;
						b = Bitb;
						k = Bitk;
						q = WriteAt;
						m = (int)(q < ReadAt ? ReadAt - q - 1 : End - q);

						if (!_last) {
							_mode = InflateBlockMode.Type;
							break;
						}
						_mode = InflateBlockMode.Dry;
						goto case InflateBlockMode.Dry;

					case InflateBlockMode.Dry:
						WriteAt = q;
						Flush(ref r);
						q = WriteAt; m = (int)(q < ReadAt ? ReadAt - q - 1 : End - q);
						if (ReadAt != WriteAt) {
							Bitb = b; Bitk = k;
							Codec.AvailableBytesIn = n;
							Codec.TotalBytesIn += p - Codec.NextIn;
							Codec.NextIn = p;
							WriteAt = q;
							Flush(ref r);
							return;
						}
						_mode = InflateBlockMode.Done;
						goto case InflateBlockMode.Done;

					case InflateBlockMode.Done:
						r = ZlibState.StreamEnd;
						Bitb = b;
						Bitk = k;
						Codec.AvailableBytesIn = n;
						Codec.TotalBytesIn += p - Codec.NextIn;
						Codec.NextIn = p;
						WriteAt = q;
						Flush(ref r);
						return;

					case InflateBlockMode.Bad:
						r = ZlibState.DataError;

						Bitb = b; Bitk = k;
						Codec.AvailableBytesIn = n;
						Codec.TotalBytesIn += p - Codec.NextIn;
						Codec.NextIn = p;
						WriteAt = q;
						Flush(ref r);
						return;

					default:
						r = ZlibState.StreamError;

						Bitb = b; Bitk = k;
						Codec.AvailableBytesIn = n;
						Codec.TotalBytesIn += p - Codec.NextIn;
						Codec.NextIn = p;
						WriteAt = q;
						Flush(ref r);
						return;

				}
			}
		}

		/// <summary>
		/// 清除空间。
		/// </summary>
		public void Dispose() {
			Reset();
			Window = null;
			_hufts = null;
		}

		/// <summary>
		/// 设置字典。
		/// </summary>
		/// <param name="d">字节数组。</param>
		/// <param name="start">开始的位置。</param>
		/// <param name="count">长度。</param>
		public void SetDictionary(byte[] d, int start, int count) {
			Array.Copy(d, start, Window, 0, count);
			ReadAt = WriteAt = count;
		}

		/// <summary>
		/// 指示位置是否在尾部。
		/// </summary>
		/// <returns>如果尾部，则返回 true 。</returns>
		public bool SyncPoint() {
			return _mode == InflateBlockMode.Lens;
		}

		/// <summary>
		/// 输出当前内容。
		/// </summary>
		/// <param name="r">返回结果。</param>
		/// <returns>结果。</returns>
		public void Flush(ref ZlibState r) {
			int nBytes;
			
			for (int pass = 0; pass < 2; pass++) {

				// 计算拷贝的字节数
				if (pass == 0) {
					nBytes = (int)((ReadAt <= WriteAt ? WriteAt : End) - ReadAt);
				} else {
					nBytes = WriteAt - ReadAt;
				}

				if (nBytes == 0) {
					if (r == ZlibState.BufferError)
						r = ZlibState.Success;
					return;
				}

				if (nBytes > Codec.AvailableBytesOut)
					nBytes = Codec.AvailableBytesOut;

				if (nBytes != 0 && r == ZlibState.BufferError)
					r = ZlibState.Success;

				// 更新当前的计数器
				Codec.AvailableBytesOut -= nBytes;
				Codec.TotalBytesOut += nBytes;

				// 更新检验数
				if (_checkfn)
					Codec.Adler32 = _check = Adler.Adler32(_check, Window, ReadAt, nBytes);


				Array.Copy(Window, ReadAt, Codec.OutputBuffer, Codec.NextOut, nBytes);
				Codec.NextOut += nBytes;
				ReadAt += nBytes;

				// 查看窗口前是否应复制
				if (ReadAt == End && pass == 0) {
					
					ReadAt = 0;
					if (WriteAt == End)
						WriteAt = 0;
				} else pass++;
			}
		}

	}

	/// <summary>
	/// 代表 Inflate 代码。
	/// </summary>
	sealed class InflateCodes {

		/// <summary>
		/// 压缩遮罩表。
		/// </summary>
		public static readonly int[] InflateMask = new int[] {
            0x00000000, 0x00000001, 0x00000003, 0x00000007,
            0x0000000f, 0x0000001f, 0x0000003f, 0x0000007f,
            0x000000ff, 0x000001ff, 0x000003ff, 0x000007ff,
            0x00000fff, 0x00001fff, 0x00003fff, 0x00007fff, 0x0000ffff };

		// waiting for "i:"=input,
		//             "o:"=output,
		//             "x:"=nothing
		private const int START = 0; // x: set up for LEN
		private const int LEN = 1; // i: get length/literal/eob next
		private const int LENEXT = 2; // i: getting length extra (have base)
		private const int DIST = 3; // i: get distance next
		private const int DISTEXT = 4; // i: getting distance extra
		private const int COPY = 5; // o: copying bytes in window, waiting for space
		private const int LIT = 6; // o: got literal, waiting for output space
		private const int WASH = 7; // o: got eob, possibly still output waiting
		private const int END = 8; // x: got eob and all data flushed
		private const int BADCODE = 9; // x: got error

		int mode;        // current inflate_codes mode

		// mode dependent information
		int len;

		int[] tree;      // pointer into tree
		int tree_index = 0;
		int need;        // bits needed

		int lit;

		// if EXT or COPY, where and how much
		int bitsToGet;   // bits to get for extra
		int dist;        // distance back to copy from

		byte lbits;      // ltree bits decoded per branch
		byte dbits;      // dtree bits decoder per branch
		int[] ltree;     // literal/length/eob tree
		int ltree_index; // literal/length/eob tree
		int[] dtree;     // distance tree
		int dtree_index; // distance tree

		public InflateCodes() {
		}

		/// <summary>
		/// 初始化。
		/// </summary>
		/// <param name="bl"></param>
		/// <param name="bd"></param>
		/// <param name="tl"></param>
		/// <param name="tl_index"></param>
		/// <param name="td"></param>
		/// <param name="td_index"></param>
		public void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index) {
			mode = START;
			lbits = (byte)bl;
			dbits = (byte)bd;
			ltree = tl;
			ltree_index = tl_index;
			dtree = td;
			dtree_index = td_index;
			tree = null;
		}

		/// <summary>
		/// 处理。
		/// </summary>
		/// <param name="blocks">一个块。</param>
		/// <param name="r">状态。</param>
		/// <returns></returns>
		public void Process(InflateBlocks blocks,ref ZlibState r) {
			int j;      // temporary storage
			int tindex; // temporary pointer
			int e;      // extra bits or operation
			int b = 0;  // bit buffer
			int k = 0;  // bits in bit buffer
			int p = 0;  // input data pointer
			int n;      // bytes available there
			int q;      // output window write pointer
			int m;      // bytes to end of window or read pointer
			int f;      // pointer to copy strings from

			ZlibCodec z = blocks.Codec;

			// copy input/output information to locals (UPDATE macro restores)
			p = z.NextIn;
			n = z.AvailableBytesIn;
			b = blocks.Bitb;
			k = blocks.Bitk;
			q = blocks.WriteAt; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;

			// process input and output based on current state
			while (true) {
				switch (mode) {
					// waiting for "i:"=input, "o:"=output, "x:"=nothing
					case START:  // x: set up for LEN
						if (m >= 258 && n >= 10) {
							blocks.Bitb = b; blocks.Bitk = k;
							z.AvailableBytesIn = n;
							z.TotalBytesIn += p - z.NextIn;
							z.NextIn = p;
							blocks.WriteAt = q;
							r = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, blocks, z);

							p = z.NextIn;
							n = z.AvailableBytesIn;
							b = blocks.Bitb;
							k = blocks.Bitk;
							q = blocks.WriteAt; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;

							if (r != ZlibState.Success) {
								mode = (r == ZlibState.StreamEnd) ? WASH : BADCODE;
								break;
							}
						}
						need = lbits;
						tree = ltree;
						tree_index = ltree_index;

						mode = LEN;
						goto case LEN;

					case LEN:  // i: get length/literal/eob next
						j = need;

						while (k < j) {
							if (n != 0)
								r = ZlibState.Success;
							else {
								blocks.Bitb = b; blocks.Bitk = k;
								z.AvailableBytesIn = n;
								z.TotalBytesIn += p - z.NextIn;
								z.NextIn = p;
								blocks.WriteAt = q;
								blocks.Flush(ref r);
								return;
							}
							n--;
							b |= (z.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						tindex = (tree_index + (b & InflateMask[j])) * 3;

						b >>= (tree[tindex + 1]);
						k -= (tree[tindex + 1]);

						e = tree[tindex];

						if (e == 0) {
							// literal
							lit = tree[tindex + 2];
							mode = LIT;
							break;
						}
						if ((e & 16) != 0) {
							// length
							bitsToGet = e & 15;
							len = tree[tindex + 2];
							mode = LENEXT;
							break;
						}
						if ((e & 64) == 0) {
							// next table
							need = e;
							tree_index = tindex / 3 + tree[tindex + 2];
							break;
						}
						if ((e & 32) != 0) {
							// end of block
							mode = WASH;
							break;
						}
						mode = BADCODE; // invalid code
						z.Message = "不合法空白代码";
						r = ZlibState.DataError;

						blocks.Bitb = b; blocks.Bitk = k;
						z.AvailableBytesIn = n;
						z.TotalBytesIn += p - z.NextIn;
						z.NextIn = p;
						blocks.WriteAt = q;
						blocks.Flush(ref r);
						return;


					case LENEXT:  // i: getting length extra (have base)
						j = bitsToGet;

						while (k < j) {
							if (n != 0)
								r = ZlibState.Success;
							else {
								blocks.Bitb = b; blocks.Bitk = k;
								z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
								blocks.WriteAt = q;
								blocks.Flush(ref r);
								return;
							}
							n--; b |= (z.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						len += (b & InflateMask[j]);

						b >>= j;
						k -= j;

						need = dbits;
						tree = dtree;
						tree_index = dtree_index;
						mode = DIST;
						goto case DIST;

					case DIST:  // i: get distance next
						j = need;

						while (k < j) {
							if (n != 0)
								r = ZlibState.Success;
							else {
								blocks.Bitb = b; blocks.Bitk = k;
								z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
								blocks.WriteAt = q;
								blocks.Flush(ref r);
								return;
							}
							n--; b |= (z.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						tindex = (tree_index + (b & InflateMask[j])) * 3;

						b >>= tree[tindex + 1];
						k -= tree[tindex + 1];

						e = (tree[tindex]);
						if ((e & 0x10) != 0) {
							// distance
							bitsToGet = e & 15;
							dist = tree[tindex + 2];
							mode = DISTEXT;
							break;
						}
						if ((e & 64) == 0) {
							// next table
							need = e;
							tree_index = tindex / 3 + tree[tindex + 2];
							break;
						}
						mode = BADCODE; // invalid code
						z.Message = "非法的代码距离";
						r = ZlibState.DataError;

						blocks.Bitb = b; blocks.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						blocks.WriteAt = q;
						blocks.Flush(ref r);
						return;


					case DISTEXT:  // i: getting distance extra
						j = bitsToGet;

						while (k < j) {
							if (n != 0)
								r = ZlibState.Success;
							else {
								blocks.Bitb = b; blocks.Bitk = k;
								z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
								blocks.WriteAt = q;
								blocks.Flush(ref r);
								return;
							}
							n--; b |= (z.InputBuffer[p++] & 0xff) << k;
							k += 8;
						}

						dist += (b & InflateMask[j]);

						b >>= j;
						k -= j;

						mode = COPY;
						goto case COPY;

					case COPY:  // o: copying bytes in window, waiting for space
						f = q - dist;
						while (f < 0) {
							// modulo window size-"while" instead
							f += blocks.End; // of "if" handles invalid distances
						}
						while (len != 0) {
							if (m == 0) {
								if (q == blocks.End && blocks.ReadAt != 0) {
									q = 0; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;
								}
								if (m == 0) {
									blocks.WriteAt = q; blocks.Flush(ref r);
									q = blocks.WriteAt; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;

									if (q == blocks.End && blocks.ReadAt != 0) {
										q = 0; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;
									}

									if (m == 0) {
										blocks.Bitb = b; blocks.Bitk = k;
										z.AvailableBytesIn = n;
										z.TotalBytesIn += p - z.NextIn;
										z.NextIn = p;
										blocks.WriteAt = q;
										blocks.Flush(ref r);
										return;
									}
								}
							}

							blocks.Window[q++] = blocks.Window[f++]; m--;

							if (f == blocks.End)
								f = 0;
							len--;
						}
						mode = START;
						break;

					case LIT:  // o: got literal, waiting for output space
						if (m == 0) {
							if (q == blocks.End && blocks.ReadAt != 0) {
								q = 0; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;
							}
							if (m == 0) {
								blocks.WriteAt = q; blocks.Flush(ref r);
								q = blocks.WriteAt; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;

								if (q == blocks.End && blocks.ReadAt != 0) {
									q = 0; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;
								}
								if (m == 0) {
									blocks.Bitb = b; blocks.Bitk = k;
									z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
									blocks.WriteAt = q;
									blocks.Flush(ref r);
									return;
								}
							}
						}
						r = ZlibState.Success;

						blocks.Window[q++] = (byte)lit; m--;

						mode = START;
						break;

					case WASH:  // o: got eob, possibly more output
						if (k > 7) {
							// return unused byte, if any
							k -= 8;
							n++;
							p--; // can always return one
						}

						blocks.WriteAt = q; blocks.Flush(ref r);
						q = blocks.WriteAt; m = q < blocks.ReadAt ? blocks.ReadAt - q - 1 : blocks.End - q;

						if (blocks.ReadAt != blocks.WriteAt) {
							blocks.Bitb = b; blocks.Bitk = k;
							z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
							blocks.WriteAt = q;
							blocks.Flush(ref r);
							return;
						}
						mode = END;
						goto case END;

					case END:
						r = ZlibState.StreamEnd;
						blocks.Bitb = b; blocks.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						blocks.WriteAt = q;
						blocks.Flush(ref r);
						return;

					case BADCODE:  // x: got error

						r = ZlibState.DataError;

						blocks.Bitb = b; blocks.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						blocks.WriteAt = q;
						blocks.Flush(ref r);
						return;

					default:
						r = ZlibState.StreamError;

						blocks.Bitb = b; blocks.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						blocks.WriteAt = q;
						blocks.Flush(ref r);
						return;
				}
			}
		}


		// Called with number of bytes left to write in window at least 258
		// (the maximum string length) and number of input bytes available
		// at least ten.  The ten bytes are six bytes for the longest length/
		// distance pair plus four bytes for overloading the bit buffer.

		ZlibState InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z) {
			int t;        // temporary pointer
			int[] tp;     // temporary pointer
			int tp_index; // temporary pointer
			int e;        // extra bits or operation
			int b;        // bit buffer
			int k;        // bits in bit buffer
			int p;        // input data pointer
			int n;        // bytes available there
			int q;        // output window write pointer
			int m;        // bytes to end of window or read pointer
			int ml;       // mask for literal/length tree
			int md;       // mask for distance tree
			int c;        // bytes to copy
			int d;        // distance back to copy from
			int r;        // copy source pointer

			int tp_index_t_3; // (tp_index+t)*3

			// load input, output, bit values
			p = z.NextIn; n = z.AvailableBytesIn; b = s.Bitb; k = s.Bitk;
			q = s.WriteAt; m = q < s.ReadAt ? s.ReadAt - q - 1 : s.End - q;

			// initialize masks
			ml = InflateMask[bl];
			md = InflateMask[bd];

			// do until not enough input or output space for fast loop
			do {
				// assume called with m >= 258 && n >= 10
				// get literal/length code
				while (k < (20)) {
					// max bits for literal/length code
					n--;
					b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
				}

				t = b & ml;
				tp = tl;
				tp_index = tl_index;
				tp_index_t_3 = (tp_index + t) * 3;
				if ((e = tp[tp_index_t_3]) == 0) {
					b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

					s.Window[q++] = (byte)tp[tp_index_t_3 + 2];
					m--;
					continue;
				}
				do {

					b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

					if ((e & 16) != 0) {
						e &= 15;
						c = tp[tp_index_t_3 + 2] + ((int)b & InflateMask[e]);

						b >>= e; k -= e;

						// decode distance base of block to copy
						while (k < 15) {
							// max bits for distance code
							n--;
							b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
						}

						t = b & md;
						tp = td;
						tp_index = td_index;
						tp_index_t_3 = (tp_index + t) * 3;
						e = tp[tp_index_t_3];

						do {

							b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);

							if ((e & 16) != 0) {
								// get extra bits to add to distance base
								e &= 15;
								while (k < e) {
									// get extra bits (up to 13)
									n--;
									b |= (z.InputBuffer[p++] & 0xff) << k; k += 8;
								}

								d = tp[tp_index_t_3 + 2] + (b & InflateMask[e]);

								b >>= e; k -= e;

								// do the copy
								m -= c;
								if (q >= d) {
									// offset before dest
									//  just copy
									r = q - d;
									if (q - r > 0 && 2 > (q - r)) {
										s.Window[q++] = s.Window[r++]; // minimum count is three,
										s.Window[q++] = s.Window[r++]; // so unroll loop a little
										c -= 2;
									} else {
										Array.Copy(s.Window, r, s.Window, q, 2);
										q += 2; r += 2; c -= 2;
									}
								} else {
									// else offset after destination
									r = q - d;
									do {
										r += s.End; // force pointer in window
									}
									while (r < 0); // covers invalid distances
									e = s.End - r;
									if (c > e) {
										// if source crosses,
										c -= e; // 包装
										if (q - r > 0 && e > (q - r)) {
											do {
												s.Window[q++] = s.Window[r++];
											}
											while (--e != 0);
										} else {
											Array.Copy(s.Window, r, s.Window, q, e);
											q += e; r += e; e = 0;
										}
										r = 0; // 从窗口开始地方复制
									}
								}

								// 复制未操作的部分
								if (q - r > 0 && c > (q - r)) {
									do {
										s.Window[q++] = s.Window[r++];
									}
									while (--c != 0);
								} else {
									Array.Copy(s.Window, r, s.Window, q, c);
									q += c; r += c; c = 0;
								}
								break;
							} else if ((e & 64) == 0) {
								t += tp[tp_index_t_3 + 2];
								t += (b & InflateMask[e]);
								tp_index_t_3 = (tp_index + t) * 3;
								e = tp[tp_index_t_3];
							} else {
								z.Message = "非法的代码距离 ";

								c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

								s.Bitb = b; s.Bitk = k;
								z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
								s.WriteAt = q;

								return ZlibState.DataError;
							}
						}
						while (true);
						break;
					}

					if ((e & 64) == 0) {
						t += tp[tp_index_t_3 + 2];
						t += (b & InflateMask[e]);
						tp_index_t_3 = (tp_index + t) * 3;
						if ((e = tp[tp_index_t_3]) == 0) {
							b >>= (tp[tp_index_t_3 + 1]); k -= (tp[tp_index_t_3 + 1]);
							s.Window[q++] = (byte)tp[tp_index_t_3 + 2];
							m--;
							break;
						}
					} else if ((e & 32) != 0) {
						c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

						s.Bitb = b; s.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						s.WriteAt = q;

						return ZlibState.StreamEnd;
					} else {
						z.Message = "非法空白代码";

						c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

						s.Bitb = b; s.Bitk = k;
						z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
						s.WriteAt = q;

						return ZlibState.DataError;
					}
				}
				while (true);
			}
			while (m >= 258 && n >= 10);

			// 输入或输出不满足要求，恢复数据
			c = z.AvailableBytesIn - n; c = (k >> 3) < c ? k >> 3 : c; n += c; p -= c; k -= (c << 3);

			s.Bitb = b; s.Bitk = k;
			z.AvailableBytesIn = n; z.TotalBytesIn += p - z.NextIn; z.NextIn = p;
			s.WriteAt = q;

			return ZlibState.Success;
		}
	}

	/// <summary>
	/// 用于操作 Inflate 的类。不可继承此类。
	/// </summary>
	sealed class InflateManager {
		
		/// <summary>
		/// 预设字典标记头长度。
		/// </summary>
		private const int PRESET_DICT = 0x20;

		/// <summary>
		/// 压缩比特。
		/// </summary>
		private const int Z_DEFLATED = 8;

		/// <summary>
		/// 表示一个模式枚举。
		/// </summary>
		enum InflateManagerMode {

			/// <summary>
			/// 等待函数。
			/// </summary>
			Member = 0,

			/// <summary>
			/// 等待标记。
			/// </summary>
			Flag = 1,

			/// <summary>
			/// 字典4个检验。
			/// </summary>
			Dict4 = 2,

			/// <summary>
			/// 字典3个检验。
			/// </summary>
			Dict3 = 3,

			/// <summary>
			/// 字典2个检验。
			/// </summary>
			Dict2 = 4,

			/// <summary>
			/// 字典1个检验。
			/// </summary>
			Dict1 = 5,

			/// <summary>
			/// 等待 inflatesetDictionary
			/// </summary>
			Dict0 = 6,

			/// <summary>
			/// 块。
			/// </summary>
			Blocks = 7,

			/// <summary>
			/// 检查4。
			/// </summary>
			Check4 = 8,

			/// <summary>
			/// 检查3。
			/// </summary>
			Check3 = 9,

			/// <summary>
			/// 检查2。
			/// </summary>
			Check2 = 10,

			/// <summary>
			/// 检查1。
			/// </summary>
			Check1 = 11,

			/// <summary>
			/// 完成。
			/// </summary>
			Done = 12,

			/// <summary>
			/// 错误。
			/// </summary>
			Bad = 13,
		}

		/// <summary>
		/// 模式枚举。
		/// </summary>
		private InflateManagerMode _mode;

		/// <summary>
		/// 使用当前管理者的解码器。
		/// </summary>
		ZlibCodec _codec;

		/// <summary>
		/// 如果模式为 InflateManagerMode.Member 有效。
		/// </summary>
		int _method;

		/// <summary>
		/// 计算返回的检验码。
		/// </summary>
		uint _computedCheck;

		/// <summary>
		/// 期待的检验码。
		/// </summary>
		uint _expectedCheck;

		/// <summary>
		/// 标记。如果模式为 InflateManagerMode.Bad 有效。
		/// </summary>
		int _marker;

		/// <summary>
		/// 指示是否含 Rfc1950 头字节。
		/// </summary>
		bool _handleRfc1950HeaderBytes ;

		/// <summary>
		/// 宽字节。8 - 15  。默认 15 。
		/// </summary>
		int _wbits; // log2(window size)  (8..15, defaults to 15)

		/// <summary>
		/// 当前块。
		/// </summary>
		InflateBlocks _blocks;

		/// <summary>
		/// 初始化 <see cref="Py.Zip.InflateManager"/> 的新实例。
		/// </summary>
		public InflateManager()
			:this(true){  }

		/// <summary>
		/// 初始化 <see cref="Py.Zip.InflateManager"/> 的新实例。
		/// </summary>
		/// <param name="expectRfc1950HeaderBytes">如果 true ，则保留Rfc1950头 。</param>
		public InflateManager(bool expectRfc1950HeaderBytes) {
			_handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
		}

		/// <summary>
		/// 重置当前管理者的状态。
		/// </summary>
		public void Reset() {
			_codec.TotalBytesIn = _codec.TotalBytesOut = 0;
			_codec.Message = null;
			_mode = _handleRfc1950HeaderBytes ? InflateManagerMode.Member : InflateManagerMode.Blocks;
			_blocks.Reset();
		}

		/// <summary>
		/// 结束压缩操作。
		/// </summary>
		/// <returns>结果。</returns>
		public ZlibState End() {
			if (_blocks != null)
				_blocks.Dispose();
			_blocks = null;
			return ZlibState.Success;
		}

		/// <summary>
		/// 初始化。
		/// </summary>
		/// <param name="codec">解码器。</param>
		/// <param name="w">长度。</param>
		/// <returns>结果。</returns>
		public ZlibState Initialize(ZlibCodec codec, int w) {
			_codec = codec;
			_codec.Message = null;
			_blocks = null;

			// handle undocumented nowrap option (no zlib header or check)
			//nowrap = 0;
			//if (w < 0)
			//{
			//    w = - w;
			//    nowrap = 1;
			//}

			// set window size
			if (w < 8 || w > 15) {
				End();
				throw new ZlibException("错误的窗口大小。");

				//return ZlibState.StreamError;
			}
			_wbits = w;

			_blocks = new InflateBlocks(codec,
				_handleRfc1950HeaderBytes,
				1 << w);

			Reset();
			return ZlibState.Success;
		}

		/// <summary>
		/// 压缩。
		/// </summary>
		/// <param name="flush">类型。</param>
		/// <returns>结果。</returns>
		public ZlibState Inflate(FlushType flush) {
			int b;

			if (_codec.InputBuffer == null)
				throw new ZlibException("缓存为空。 ");

			//             int f = (flush == FlushType.Finish)
			//                 ? ZlibState.BufferError
			//                 : ZlibState.Success;

			// workitem 8870
			ZlibState f = ZlibState.Success;
			ZlibState r = ZlibState.BufferError;

			while (true) {
				switch (_mode) {
					case InflateManagerMode.Member:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						if (((_method = _codec.InputBuffer[_codec.NextIn++]) & 0xf) != Z_DEFLATED) {
							_mode = InflateManagerMode.Bad;
							_codec.Message = String.Format("未知压缩模式 (0x{0:X2})", _method);
							_marker = 5; // 不能执行 inflateSync
							break;
						}
						if ((_method >> 4) + 8 > _wbits) {
							_mode = InflateManagerMode.Bad;
							_codec.Message = String.Format("非法的窗口大小 ({0})", (_method >> 4) + 8);
							_marker = 5; // 不能执行 inflateSync
							break;
						}
						_mode = InflateManagerMode.Flag;
						break;


					case InflateManagerMode.Flag:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						b = (_codec.InputBuffer[_codec.NextIn++]) & 0xff;

						if ((((_method << 8) + b) % 31) != 0) {
							_mode = InflateManagerMode.Bad;
							_codec.Message = "错误文件头检验";
							_marker = 5; // 不能执行 inflateSync
							break;
						}

						_mode = ((b & PRESET_DICT) == 0)
							? InflateManagerMode.Blocks
							: InflateManagerMode.Dict4;
						break;

					case InflateManagerMode.Dict4:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						_expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 0xff000000);
						_mode = InflateManagerMode.Dict3;
						break;

					case InflateManagerMode.Dict3:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						_expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0x00ff0000);
						_mode = InflateManagerMode.Dict2;
						break;

					case InflateManagerMode.Dict2:

						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						_expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0x0000ff00);
						_mode = InflateManagerMode.Dict1;
						break;


					case InflateManagerMode.Dict1:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--; _codec.TotalBytesIn++;
						_expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0x000000ff);
						_codec.Adler32 = _expectedCheck;
						_mode = InflateManagerMode.Dict0;
						return  ZlibState.NeedDict;


					case InflateManagerMode.Dict0:
						_mode = InflateManagerMode.Bad;
						_codec.Message = "需要字典";
						_marker = 0; // 能执行 inflateSync
						return ZlibState.StreamEnd;


					case InflateManagerMode.Blocks:
						_blocks.Process(ref r);
						if (r == ZlibState.DataError) {
							_mode = InflateManagerMode.Bad;
							_marker = 0; // 能执行 inflateSync
							break;
						}

						if (r == ZlibState.Success) r = f;

						if (r != ZlibState.StreamEnd)
							return r;

						r = f;
						_computedCheck = _blocks.Reset();
						if (!_handleRfc1950HeaderBytes) {
							_mode = InflateManagerMode.Done;
							return ZlibState.StreamEnd;
						}
						_mode = InflateManagerMode.Check4;
						break;

					case InflateManagerMode.Check4:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						_expectedCheck = (uint)((_codec.InputBuffer[_codec.NextIn++] << 24) & 0xff000000);
						_mode = InflateManagerMode.Check3;
						break;

					case InflateManagerMode.Check3:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--; _codec.TotalBytesIn++;
						_expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 16) & 0x00ff0000);
						_mode = InflateManagerMode.Check2;
						break;

					case InflateManagerMode.Check2:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--;
						_codec.TotalBytesIn++;
						_expectedCheck += (uint)((_codec.InputBuffer[_codec.NextIn++] << 8) & 0x0000ff00);
						_mode = InflateManagerMode.Check1;
						break;

					case InflateManagerMode.Check1:
						if (_codec.AvailableBytesIn == 0) return r;
						r = f;
						_codec.AvailableBytesIn--; _codec.TotalBytesIn++;
						_expectedCheck += (uint)(_codec.InputBuffer[_codec.NextIn++] & 0x000000ff);
						if (_computedCheck != _expectedCheck) {
							_mode = InflateManagerMode.Bad;
							_codec.Message = "错误数据检验";
							_marker = 5; // 不能执行  inflateSync
							break;
						}
						_mode = InflateManagerMode.Done;
						return ZlibState.StreamEnd;

					case InflateManagerMode.Done:
						return ZlibState.StreamEnd;

					case InflateManagerMode.Bad:
						throw new ZlibException(String.Format("错误状态 ({0})", _codec.Message));

					default:
						throw new ZlibException("流错误。");

				}
			}
		}


		/// <summary>
		/// 设置字典内容。
		/// </summary>
		/// <param name="dictionary">字节字典。</param>
		/// <returns>表示成功码。</returns>
		public ZlibState SetDictionary(byte[] dictionary) {
			int index = 0;
			int length = dictionary.Length;
			if (_mode != InflateManagerMode.Dict0)
				throw new ZlibException("流错误。");

			if (Adler.Adler32(1, dictionary, 0, dictionary.Length) != _codec.Adler32) {
				return ZlibState.DataError;
			}

			_codec.Adler32 = Adler.Adler32(0, null, 0, 0);

			if (length >= (1 << _wbits)) {
				length = (1 << _wbits) - 1;
				index = dictionary.Length - length;
			}
			_blocks.SetDictionary(dictionary, index, length);
			_mode = InflateManagerMode.Blocks;
			return ZlibState.Success;
		}

		/// <summary>
		/// 标记数组。
		/// </summary>
		private static readonly byte[] mark = new byte[] { 0, 0, 0xff, 0xff };

		/// <summary>
		/// 执行异步。
		/// </summary>
		/// <returns>长度。</returns>
		public ZlibState Sync() {
			int n; // number of bytes to look at
			int p; // pointer to bytes
			int m; // number of marker bytes found in a row
			long r, w; // temporaries to save total_in and total_out

			// set up
			if (_mode != InflateManagerMode.Bad) {
				_mode = InflateManagerMode.Bad;
				_marker = 0;
			}
			if ((n = _codec.AvailableBytesIn) == 0)
				return ZlibState.BufferError;
			p = _codec.NextIn;
			m = _marker;

			// search
			while (n != 0 && m < 4) {
				if (_codec.InputBuffer[p] == mark[m]) {
					m++;
				} else if (_codec.InputBuffer[p] != 0) {
					m = 0;
				} else {
					m = 4 - m;
				}
				p++; n--;
			}

			// restore
			_codec.TotalBytesIn += p - _codec.NextIn;
			_codec.NextIn = p;
			_codec.AvailableBytesIn = n;
			_marker = m;

			// return no joy or set up to restart on a new block
			if (m != 4) {
				return ZlibState.DataError;
			}
			r = _codec.TotalBytesIn;
			w = _codec.TotalBytesOut;
			Reset();
			_codec.TotalBytesIn = r;
			_codec.TotalBytesOut = w;
			_mode = InflateManagerMode.Blocks;
			return ZlibState.Success;
		}

		/// <summary>
		/// 异步编码。
		/// </summary>
		/// <param name="z">解码器。</param>
		/// <returns>如果 inflate是块尾，返回trye。</returns>
		/// <remarks>
		/// Z_SYNC_FLUSH 或 Z_FULL_FLUSH用于指向块尾。 这个函数由 PPP 实现并提供安全的 检验。 PPP 使用 Z_SYNC_FLUSH 但会在解压时移除空块 , PPP 检查输入数据, inflate 等待帧长度计算然后继续。
		/// </remarks>
		public bool SyncPoint(ZlibCodec z) {
			return _blocks.SyncPoint();
		}
	}
}