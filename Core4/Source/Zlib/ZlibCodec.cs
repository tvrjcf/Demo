// ZlibCodec.cs
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
// Time-stamp: <2009-November-03 15:40:51>
//
// ------------------------------------------------------------------
//
// This module defines a Codec for ZLIB compression and
// decompression. This code extends code that was based the jzlib
// implementation of zlib, but this code is completely novel.  The codec
// class is new, and encapsulates some behaviors that are new, and some
// that were present in other classes in the jzlib code base.  In
// keeping with the license for jzlib, the copyright to the jzlib code
// is included below.
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
using Py.RunTime;
using Py.Core;

namespace Py.Zip.Zlib {

    /// <summary>
    /// 为 ZLIB， DEFLATE (IETF RFC1950 and RFC1951) 准备的编码解码器。此类不可以继承。
    /// </summary>
    ///
    /// <remarks>
    /// 这个类根据 Deflate 算法压缩和解压数据， ZLIB 格式参 <see
    /// href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950 - ZLIB</see> 和 <see
    /// href="http://www.ietf.org/rfc/rfc1951.txt">RFC 1951 - DEFLATE</see>。
    /// </remarks>
    sealed class ZlibCodec {

        /// <summary>
        /// 输入的缓存。
        /// </summary>
        public byte[] InputBuffer;

        /// <summary>
        /// 开始输入的位置。
        /// </summary>
        public int NextIn;

        /// <summary>
        /// 可用的输入字节。
        /// </summary>
        /// <remarks>
        /// 一般情况，在第一次Inflate() 或 Deflate() 时，AvailableBytesIn 设为 InputBuffer.Length 。
        /// 在 Inflate/Deflate 执行时，AvailableBytesIn 被修改。
        /// </remarks>
        public int AvailableBytesIn;

        /// <summary>
        /// 在 Inflate()/Deflate() 的全部输入字节数。
        /// </summary>
        public long TotalBytesIn;

        /// <summary>
        /// 输出的缓存。
        /// </summary>
        public byte[] OutputBuffer;

        /// <summary>
        /// 开始输出的位置。
        /// </summary>
        public int NextOut;

        /// <summary>
        /// 可用的输出字节。
        /// </summary>
        /// <remarks>
        /// 一般情况，在第一次Inflate() 或 Deflate() 时，AvailableBytesIn 设为 0  。
        /// 在 Inflate/Deflate 执行时，AvailableBytesIn 被修改。
        /// </remarks>
        public int AvailableBytesOut;

        /// <summary>
        /// 在 Inflate()/Deflate() 的全部输出字节数。
        /// </summary>
        public long TotalBytesOut;

        /// <summary>
        /// 错误的信息。
        /// </summary>
		public string Message;

		/// <summary>
		/// 操作 Deflate 的类。
		/// </summary>
		public DeflateManager DState;

		/// <summary>
		/// 操作 Inflate 的类。
		/// </summary>
		public InflateManager IState;

        /// <summary>
		/// Adler32 校验码。
        /// </summary>
        public uint Adler32;

        /// <summary>
        /// 压缩等级。
        /// </summary>
        public CompressionLevel CompressLevel = CompressionLevel.Default;

        /// <summary>
        /// 窗口位数。
        /// </summary>
		int WindowBits = DeflateManager.WindowBitsMax;

        /// <summary>
        /// 压缩策略。
        /// </summary>
        public CompressionStrategy Strategy = CompressionStrategy.Default;

        /// <summary>
        /// 初始化 <see cref="Py.Zip.Zlib.ZlibCodec"/> 的新实例。
        /// </summary>
        public ZlibCodec() { }

        /// <summary>
        /// 初始化 <see cref="Py.Zip.Zlib.ZlibCodec"/> 的新实例。
        /// </summary>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        public ZlibCodec(CompressionMode mode) {
            if (mode == CompressionMode.Compress) {
				ZlibState rc = InitializeDeflate();
                if (rc != ZlibState.Success) throw new ZlibException("无法初始化解压。");
            } else if (mode == CompressionMode.Decompress) {
				ZlibState rc = InitializeInflate();
				if (rc != ZlibState.Success) throw new ZlibException("无法初始化压缩。");
            } else throw new ZlibException("非法的模式。");
        }

        /// <summary>
        /// 初始化 inflation 状态。
        /// </summary>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeInflate() {
            return InitializeInflate(WindowBits);
        }

        /// <summary>
        /// 初始化 inflation 状态。可选需排除 RFC1950 的头字节。
        /// </summary>
        /// <param name="expectRfc1950Header">指示是否排除 RFC1950 的头字节。</param>
        ///
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeInflate(bool expectRfc1950Header) {
            return InitializeInflate(WindowBits, expectRfc1950Header);
        }

        /// <summary>
        /// 初始化 inflation 状态，指明窗口位数。
        /// </summary>
        /// <param name="windowBits">窗口位数。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeInflate(int windowBits) {
            WindowBits = windowBits;
            return InitializeInflate(windowBits, true);
        }

        /// <summary>
        /// 初始化 inflation 状态，指明窗口位数。可选需排除 RFC1950 的头字节。
        /// </summary>
        /// <param name="expectRfc1950Header">指示是否排除 RFC1950 的头字节。</param>
        /// <param name="windowBits">窗口位数。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeInflate(int windowBits, bool expectRfc1950Header) {
            WindowBits = windowBits;
			Thrower.ThrowZlibExceptionIf(DState != null, "在 InitializeDeflate() 后不能调用 InitializeInflate() 。");
            IState = new InflateManager(expectRfc1950Header);
            return IState.Initialize(this, windowBits);
        }

        /// <summary>
        /// 压缩 InputBuffer 的数据, 存放到 OutputBuffer。
        /// </summary>
        /// <remarks>
        /// 必须先设置 InputBuffer ， OutputBuffer， NextIn ， NextOut ， AvailableBytesIn 和 AvailableBytesOut 。
        /// </remarks>
        /// <example>
        /// <code>
        /// private void InflateBuffer()
        /// {
        ///     int bufferSize = 1024;
        ///     byte[] buffer = new byte[bufferSize];
        ///     ZlibCodec decompressor = new ZlibCodec();
        /// 
        ///     Console.WriteLine("\n============================================");
        ///     Console.WriteLine("Size of Buffer to Inflate: {0} bytes.", CompressedBytes.Length);
        ///     MemoryStream ms = new MemoryStream(DecompressedBytes);
        /// 
        ///     int rc = decompressor.InitializeInflate();
        /// 
        ///     decompressor.InputBuffer = CompressedBytes;
        ///     decompressor.NextIn = 0;
        ///     decompressor.AvailableBytesIn = CompressedBytes.Length;
        /// 
        ///     decompressor.OutputBuffer = buffer;
        /// 
        ///     // pass 1: inflate 
        ///     do
        ///     {
        ///         decompressor.NextOut = 0;
        ///         decompressor.AvailableBytesOut = buffer.Length;
        ///         rc = decompressor.Inflate(FlushType.None);
        /// 
        ///         if (rc != ZlibState.Success &amp;&amp; rc != ZlibState.StreamEnd)
        ///             throw new Exception("inflating: " + decompressor.Message);
        /// 
        ///         ms.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        ///     }
        ///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
        /// 
        ///     // pass 2: finish and flush
        ///     do
        ///     {
        ///         decompressor.NextOut = 0;
        ///         decompressor.AvailableBytesOut = buffer.Length;
        ///         rc = decompressor.Inflate(FlushType.Finish);
        /// 
        ///         if (rc != ZlibState.StreamEnd &amp;&amp; rc != ZlibState.Success)
        ///             throw new Exception("inflating: " + decompressor.Message);
        /// 
        ///         if (buffer.Length - decompressor.AvailableBytesOut &gt; 0)
        ///             ms.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
        ///     }
        ///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
        /// 
        ///     decompressor.EndInflate();
        /// }
        ///
        /// </code>
        /// </example>
        /// <param name="flush">缓存类型。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
        public ZlibState Inflate(FlushType flush) {
			Thrower.ThrowZlibExceptionIf(IState == null, "没有初始化 Inflate 状态。");
            return IState.Inflate(flush);
        }

        /// <summary>
        /// 停止处理。
        /// </summary>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState EndInflate() {
			Thrower.ThrowZlibExceptionIf(IState == null, "没有初始化 Inflate 状态。");
			ZlibState ret = IState.End();
            IState = null;
            return ret;
        }

        /// <summary>
        /// 异步处理。
        /// </summary>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState SyncInflate() {
			Thrower.ThrowZlibExceptionIf(IState == null, "没有初始化 Inflate 状态。");
            return IState.Sync();
        }

        /// <summary>
        /// 初始化 deflation 状态。
        /// </summary>
        /// <remarks>
        /// 默认使用 MAX 窗口大小和默认等级的压缩。
        /// </remarks>
        /// <example>
        /// <code>
        ///  int bufferSize = 40000;
        ///  byte[] CompressedBytes = new byte[bufferSize];
        ///  byte[] DecompressedBytes = new byte[bufferSize];
        ///  
        ///  ZlibCodec compressor = new ZlibCodec();
        ///  
        ///  compressor.InitializeDeflate(CompressionLevel.Default);
        ///  
        ///  compressor.InputBuffer = System.Text.ASCIIEncoding.ASCII.GetBytes(TextToCompress);
        ///  compressor.NextIn = 0;
        ///  compressor.AvailableBytesIn = compressor.InputBuffer.Length;
        ///  
        ///  compressor.OutputBuffer = CompressedBytes;
        ///  compressor.NextOut = 0;
        ///  compressor.AvailableBytesOut = CompressedBytes.Length;
        ///  
        ///  while (compressor.TotalBytesIn != TextToCompress.Length &amp;&amp; compressor.TotalBytesOut &lt; bufferSize) {
        ///    compressor.Deflate(FlushType.None);
        ///  }
        ///  
        ///  while (true) {
        ///    int rc= compressor.Deflate(FlushType.Finish);
        ///    if (rc == ZlibState.StreamEnd) break;
        ///  }
        ///  
        ///  compressor.EndDeflate();
        ///   
        /// </code>
        /// </example>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeDeflate() {
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// 初始化 deflation 状态。指定压缩等级。
        /// </summary>
        /// <remarks>
        /// 默认使用最大窗口大小。
        /// </remarks>
        /// <param name="level">压缩等级。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeDeflate(CompressionLevel level) {
            CompressLevel = level;
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// 初始化 deflation 状态。指定压缩等级，使用 Rfc1950 文件头。
        /// </summary>
        /// <remarks>
        /// 默认使用最大窗口大小。
        /// </remarks>
        /// <param name="level">压缩等级。</param>
        /// <param name="wantRfc1950Header">是否使用 Rfc1950 文件头。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, bool wantRfc1950Header) {
            CompressLevel = level;
            return InitializeDeflateInternal(wantRfc1950Header);
        }

        /// <summary>
        /// 初始化 deflation 状态。 指定压缩等级和窗口大小。
        /// </summary>
        /// <remarks>
        /// 使用指定的窗口大小。
        /// </remarks>
        /// <param name="level">压缩等级。</param>
        /// <param name="bits">创建的窗口大小。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, int bits) {
            CompressLevel = level;
            WindowBits = bits;
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// 初始化 deflation 状态。 指定压缩等级，使用 Rfc1950 文件头，窗口大小。
        /// </summary>
        /// <param name="level">压缩等级。</param>
        /// <param name="wantRfc1950Header">是否使用 Rfc1950 文件头。</param>
        /// <param name="bits">创建的窗口大小。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header) {
            CompressLevel = level;
            WindowBits = bits;
            return InitializeDeflateInternal(wantRfc1950Header);
        }

        /// <summary>
        /// 初始化 deflation 状态。
        /// </summary>
        /// <param name="wantRfc1950Header">是否使用 Rfc1950 文件头。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		private ZlibState InitializeDeflateInternal(bool wantRfc1950Header) {
			Thrower.ThrowZlibExceptionIf(IState != null, "在 InitializeInflate() 后不能调用 InitializeDeflate() 。");
            DState = new DeflateManager();
            DState.WantRfc1950HeaderBytes = wantRfc1950Header;

            return DState.Initialize(this, CompressLevel, WindowBits, Strategy);
        }

        /// <summary>
        /// 解压数据。
        /// </summary>
        /// <remarks>
        /// 必须先设置 InputBuffer 和 OutputBuffer 。
        /// </remarks>
        /// <example>
        /// <code>
        /// private void DeflateBuffer(CompressionLevel level)
        /// {
        ///     int bufferSize = 1024;
        ///     byte[] buffer = new byte[bufferSize];
        ///     ZlibCodec compressor = new ZlibCodec();
        /// 
        ///     Console.WriteLine("\n============================================");
        ///     Console.WriteLine("Size of Buffer to Deflate: {0} bytes.", UncompressedBytes.Length);
        ///     MemoryStream ms = new MemoryStream();
        /// 
        ///     int rc = compressor.InitializeDeflate(level);
        /// 
        ///     compressor.InputBuffer = UncompressedBytes;
        ///     compressor.NextIn = 0;
        ///     compressor.AvailableBytesIn = UncompressedBytes.Length;
        /// 
        ///     compressor.OutputBuffer = buffer;
        /// 
        ///     // pass 1: deflate 
        ///     do
        ///     {
        ///         compressor.NextOut = 0;
        ///         compressor.AvailableBytesOut = buffer.Length;
        ///         rc = compressor.Deflate(FlushType.None);
        /// 
        ///         if (rc != ZlibState.Success &amp;&amp; rc != ZlibState.StreamEnd)
        ///             throw new Exception("deflating: " + compressor.Message);
        /// 
        ///         ms.Write(compressor.OutputBuffer, 0, buffer.Length - compressor.AvailableBytesOut);
        ///     }
        ///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
        /// 
        ///     // pass 2: finish and flush
        ///     do
        ///     {
        ///         compressor.NextOut = 0;
        ///         compressor.AvailableBytesOut = buffer.Length;
        ///         rc = compressor.Deflate(FlushType.Finish);
        /// 
        ///         if (rc != ZlibState.StreamEnd &amp;&amp; rc != ZlibState.Success)
        ///             throw new Exception("deflating: " + compressor.Message);
        /// 
        ///         if (buffer.Length - compressor.AvailableBytesOut &gt; 0)
        ///             ms.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);
        ///     }
        ///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
        /// 
        ///     compressor.EndDeflate();
        /// 
        ///     ms.Seek(0, SeekOrigin.Begin);
        ///     CompressedBytes = new byte[compressor.TotalBytesOut];
        ///     ms.Read(CompressedBytes, 0, CompressedBytes.Length);
        /// }
        /// </code>
        /// </example>
        /// <param name="flush">输出方式。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState Deflate(FlushType flush) {
			Thrower.ThrowZlibExceptionIf(DState == null, "没有初始化 Deflate 状态。");
            return DState.Deflate(flush);
        }

        /// <summary>
        /// 结束 deflation 状态。
        /// </summary>
        /// <remarks>
        /// 在 Deflate() 后调用。
        /// </remarks>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState EndDeflate() {
			Thrower.ThrowZlibExceptionIf(DState == null, "没有初始化 Deflate 状态。");
            DState = null;
            return ZlibState.Success;
        }

        /// <summary>
        /// 重置解压状态。
        /// </summary>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
        public void ResetDeflate() {
			Thrower.ThrowZlibExceptionIf(DState == null, "没有初始化 Deflate 状态。");
            DState.Reset();
        }

        /// <summary>
        /// 设置解压参数。
        /// </summary>
        /// <param name="level">压缩等级。</param>
        /// <param name="strategy">压缩策略。</param>
		/// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState SetDeflateParams(CompressionLevel level, CompressionStrategy strategy) {
			Thrower.ThrowZlibExceptionIf(DState == null, "没有初始化 Deflate 状态。");
            return DState.SetParams(level, strategy);
        }

        /// <summary>
        /// 设置压缩时的字典。
        /// </summary>
        /// <param name="dictionary">字典。</param>
        /// <returns>如果正常返回 ZlibState.Success 。</returns>
		public ZlibState SetDictionary(byte[] dictionary) {
            if (IState != null)
                return IState.SetDictionary(dictionary);

            if (DState != null)
                return DState.SetDictionary(dictionary);

			throw new ZlibException("没有初始化 Inflate 或 Deflate  状态 。");
        }

		/// <summary>
		/// 输出内容。
		/// </summary>
		/// <seealso cref="ReadBuf"/>
        public void FlushPending() {
            int len = DState.PendingCount;

            if (len > AvailableBytesOut)
                len = AvailableBytesOut;
            if (len == 0)
                return;

            if (DState.Pending.Length <= DState.NextPending ||
                OutputBuffer.Length <= NextOut ||
                DState.Pending.Length < (DState.NextPending + len) ||
                OutputBuffer.Length < (NextOut + len)) {
                throw new ZlibException(Str.FormatX("错误状态 。 (pending.Length={0}, pendingCount={1})",
                    DState.Pending.Length, DState.PendingCount));
            }

            Array.Copy(DState.Pending, DState.NextPending, OutputBuffer, NextOut, len);

            NextOut += len;
            DState.NextPending += len;
            TotalBytesOut += len;
            AvailableBytesOut -= len;
            DState.PendingCount -= len;
            if (DState.PendingCount == 0) {
                DState.NextPending = 0;
            }
        }

		/// <summary>
		/// 从当前输入流读取指定内容到数组的缓存。
		/// </summary>
		/// <param name="buf">缓存。</param>
		/// <param name="start">开始的位置。</param>
		/// <param name="count">大小。</param>
		/// <returns>读取的长度。</returns>
		/// <remarks>
		/// 操作将自动更新检验码。
		/// </remarks>
		/// <seealso cref="FlushPending"/>
        public int ReadBuf(byte[] buf, int start, int count) {
            int len = AvailableBytesIn;

            if (len > count)
                len = count;
            if (len == 0)
                return 0;

            AvailableBytesIn -= len;

            if (DState.WantRfc1950HeaderBytes) {
                Adler32 = Adler.Adler32(Adler32, InputBuffer, NextIn, len);
            }
            Array.Copy(InputBuffer, NextIn, buf, start, len);
            NextIn += len;
            TotalBytesIn += len;
            return len;
        }

    }
}