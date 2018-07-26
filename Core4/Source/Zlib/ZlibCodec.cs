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
    /// Ϊ ZLIB�� DEFLATE (IETF RFC1950 and RFC1951) ׼���ı�������������಻���Լ̳С�
    /// </summary>
    ///
    /// <remarks>
    /// �������� Deflate �㷨ѹ���ͽ�ѹ���ݣ� ZLIB ��ʽ�� <see
    /// href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950 - ZLIB</see> �� <see
    /// href="http://www.ietf.org/rfc/rfc1951.txt">RFC 1951 - DEFLATE</see>��
    /// </remarks>
    sealed class ZlibCodec {

        /// <summary>
        /// ����Ļ��档
        /// </summary>
        public byte[] InputBuffer;

        /// <summary>
        /// ��ʼ�����λ�á�
        /// </summary>
        public int NextIn;

        /// <summary>
        /// ���õ������ֽڡ�
        /// </summary>
        /// <remarks>
        /// һ��������ڵ�һ��Inflate() �� Deflate() ʱ��AvailableBytesIn ��Ϊ InputBuffer.Length ��
        /// �� Inflate/Deflate ִ��ʱ��AvailableBytesIn ���޸ġ�
        /// </remarks>
        public int AvailableBytesIn;

        /// <summary>
        /// �� Inflate()/Deflate() ��ȫ�������ֽ�����
        /// </summary>
        public long TotalBytesIn;

        /// <summary>
        /// ����Ļ��档
        /// </summary>
        public byte[] OutputBuffer;

        /// <summary>
        /// ��ʼ�����λ�á�
        /// </summary>
        public int NextOut;

        /// <summary>
        /// ���õ�����ֽڡ�
        /// </summary>
        /// <remarks>
        /// һ��������ڵ�һ��Inflate() �� Deflate() ʱ��AvailableBytesIn ��Ϊ 0  ��
        /// �� Inflate/Deflate ִ��ʱ��AvailableBytesIn ���޸ġ�
        /// </remarks>
        public int AvailableBytesOut;

        /// <summary>
        /// �� Inflate()/Deflate() ��ȫ������ֽ�����
        /// </summary>
        public long TotalBytesOut;

        /// <summary>
        /// �������Ϣ��
        /// </summary>
		public string Message;

		/// <summary>
		/// ���� Deflate ���ࡣ
		/// </summary>
		public DeflateManager DState;

		/// <summary>
		/// ���� Inflate ���ࡣ
		/// </summary>
		public InflateManager IState;

        /// <summary>
		/// Adler32 У���롣
        /// </summary>
        public uint Adler32;

        /// <summary>
        /// ѹ���ȼ���
        /// </summary>
        public CompressionLevel CompressLevel = CompressionLevel.Default;

        /// <summary>
        /// ����λ����
        /// </summary>
		int WindowBits = DeflateManager.WindowBitsMax;

        /// <summary>
        /// ѹ�����ԡ�
        /// </summary>
        public CompressionStrategy Strategy = CompressionStrategy.Default;

        /// <summary>
        /// ��ʼ�� <see cref="Py.Zip.Zlib.ZlibCodec"/> ����ʵ����
        /// </summary>
        public ZlibCodec() { }

        /// <summary>
        /// ��ʼ�� <see cref="Py.Zip.Zlib.ZlibCodec"/> ����ʵ����
        /// </summary>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        public ZlibCodec(CompressionMode mode) {
            if (mode == CompressionMode.Compress) {
				ZlibState rc = InitializeDeflate();
                if (rc != ZlibState.Success) throw new ZlibException("�޷���ʼ����ѹ��");
            } else if (mode == CompressionMode.Decompress) {
				ZlibState rc = InitializeInflate();
				if (rc != ZlibState.Success) throw new ZlibException("�޷���ʼ��ѹ����");
            } else throw new ZlibException("�Ƿ���ģʽ��");
        }

        /// <summary>
        /// ��ʼ�� inflation ״̬��
        /// </summary>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeInflate() {
            return InitializeInflate(WindowBits);
        }

        /// <summary>
        /// ��ʼ�� inflation ״̬����ѡ���ų� RFC1950 ��ͷ�ֽڡ�
        /// </summary>
        /// <param name="expectRfc1950Header">ָʾ�Ƿ��ų� RFC1950 ��ͷ�ֽڡ�</param>
        ///
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeInflate(bool expectRfc1950Header) {
            return InitializeInflate(WindowBits, expectRfc1950Header);
        }

        /// <summary>
        /// ��ʼ�� inflation ״̬��ָ������λ����
        /// </summary>
        /// <param name="windowBits">����λ����</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeInflate(int windowBits) {
            WindowBits = windowBits;
            return InitializeInflate(windowBits, true);
        }

        /// <summary>
        /// ��ʼ�� inflation ״̬��ָ������λ������ѡ���ų� RFC1950 ��ͷ�ֽڡ�
        /// </summary>
        /// <param name="expectRfc1950Header">ָʾ�Ƿ��ų� RFC1950 ��ͷ�ֽڡ�</param>
        /// <param name="windowBits">����λ����</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeInflate(int windowBits, bool expectRfc1950Header) {
            WindowBits = windowBits;
			Thrower.ThrowZlibExceptionIf(DState != null, "�� InitializeDeflate() ���ܵ��� InitializeInflate() ��");
            IState = new InflateManager(expectRfc1950Header);
            return IState.Initialize(this, windowBits);
        }

        /// <summary>
        /// ѹ�� InputBuffer ������, ��ŵ� OutputBuffer��
        /// </summary>
        /// <remarks>
        /// ���������� InputBuffer �� OutputBuffer�� NextIn �� NextOut �� AvailableBytesIn �� AvailableBytesOut ��
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
        /// <param name="flush">�������͡�</param>
        /// <returns>����������� ZlibState.Success ��</returns>
        public ZlibState Inflate(FlushType flush) {
			Thrower.ThrowZlibExceptionIf(IState == null, "û�г�ʼ�� Inflate ״̬��");
            return IState.Inflate(flush);
        }

        /// <summary>
        /// ֹͣ����
        /// </summary>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState EndInflate() {
			Thrower.ThrowZlibExceptionIf(IState == null, "û�г�ʼ�� Inflate ״̬��");
			ZlibState ret = IState.End();
            IState = null;
            return ret;
        }

        /// <summary>
        /// �첽����
        /// </summary>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState SyncInflate() {
			Thrower.ThrowZlibExceptionIf(IState == null, "û�г�ʼ�� Inflate ״̬��");
            return IState.Sync();
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬��
        /// </summary>
        /// <remarks>
        /// Ĭ��ʹ�� MAX ���ڴ�С��Ĭ�ϵȼ���ѹ����
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
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeDeflate() {
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬��ָ��ѹ���ȼ���
        /// </summary>
        /// <remarks>
        /// Ĭ��ʹ����󴰿ڴ�С��
        /// </remarks>
        /// <param name="level">ѹ���ȼ���</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeDeflate(CompressionLevel level) {
            CompressLevel = level;
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬��ָ��ѹ���ȼ���ʹ�� Rfc1950 �ļ�ͷ��
        /// </summary>
        /// <remarks>
        /// Ĭ��ʹ����󴰿ڴ�С��
        /// </remarks>
        /// <param name="level">ѹ���ȼ���</param>
        /// <param name="wantRfc1950Header">�Ƿ�ʹ�� Rfc1950 �ļ�ͷ��</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, bool wantRfc1950Header) {
            CompressLevel = level;
            return InitializeDeflateInternal(wantRfc1950Header);
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬�� ָ��ѹ���ȼ��ʹ��ڴ�С��
        /// </summary>
        /// <remarks>
        /// ʹ��ָ���Ĵ��ڴ�С��
        /// </remarks>
        /// <param name="level">ѹ���ȼ���</param>
        /// <param name="bits">�����Ĵ��ڴ�С��</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, int bits) {
            CompressLevel = level;
            WindowBits = bits;
            return InitializeDeflateInternal(true);
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬�� ָ��ѹ���ȼ���ʹ�� Rfc1950 �ļ�ͷ�����ڴ�С��
        /// </summary>
        /// <param name="level">ѹ���ȼ���</param>
        /// <param name="wantRfc1950Header">�Ƿ�ʹ�� Rfc1950 �ļ�ͷ��</param>
        /// <param name="bits">�����Ĵ��ڴ�С��</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header) {
            CompressLevel = level;
            WindowBits = bits;
            return InitializeDeflateInternal(wantRfc1950Header);
        }

        /// <summary>
        /// ��ʼ�� deflation ״̬��
        /// </summary>
        /// <param name="wantRfc1950Header">�Ƿ�ʹ�� Rfc1950 �ļ�ͷ��</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		private ZlibState InitializeDeflateInternal(bool wantRfc1950Header) {
			Thrower.ThrowZlibExceptionIf(IState != null, "�� InitializeInflate() ���ܵ��� InitializeDeflate() ��");
            DState = new DeflateManager();
            DState.WantRfc1950HeaderBytes = wantRfc1950Header;

            return DState.Initialize(this, CompressLevel, WindowBits, Strategy);
        }

        /// <summary>
        /// ��ѹ���ݡ�
        /// </summary>
        /// <remarks>
        /// ���������� InputBuffer �� OutputBuffer ��
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
        /// <param name="flush">�����ʽ��</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState Deflate(FlushType flush) {
			Thrower.ThrowZlibExceptionIf(DState == null, "û�г�ʼ�� Deflate ״̬��");
            return DState.Deflate(flush);
        }

        /// <summary>
        /// ���� deflation ״̬��
        /// </summary>
        /// <remarks>
        /// �� Deflate() ����á�
        /// </remarks>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState EndDeflate() {
			Thrower.ThrowZlibExceptionIf(DState == null, "û�г�ʼ�� Deflate ״̬��");
            DState = null;
            return ZlibState.Success;
        }

        /// <summary>
        /// ���ý�ѹ״̬��
        /// </summary>
        /// <returns>����������� ZlibState.Success ��</returns>
        public void ResetDeflate() {
			Thrower.ThrowZlibExceptionIf(DState == null, "û�г�ʼ�� Deflate ״̬��");
            DState.Reset();
        }

        /// <summary>
        /// ���ý�ѹ������
        /// </summary>
        /// <param name="level">ѹ���ȼ���</param>
        /// <param name="strategy">ѹ�����ԡ�</param>
		/// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState SetDeflateParams(CompressionLevel level, CompressionStrategy strategy) {
			Thrower.ThrowZlibExceptionIf(DState == null, "û�г�ʼ�� Deflate ״̬��");
            return DState.SetParams(level, strategy);
        }

        /// <summary>
        /// ����ѹ��ʱ���ֵ䡣
        /// </summary>
        /// <param name="dictionary">�ֵ䡣</param>
        /// <returns>����������� ZlibState.Success ��</returns>
		public ZlibState SetDictionary(byte[] dictionary) {
            if (IState != null)
                return IState.SetDictionary(dictionary);

            if (DState != null)
                return DState.SetDictionary(dictionary);

			throw new ZlibException("û�г�ʼ�� Inflate �� Deflate  ״̬ ��");
        }

		/// <summary>
		/// ������ݡ�
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
                throw new ZlibException(Str.FormatX("����״̬ �� (pending.Length={0}, pendingCount={1})",
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
		/// �ӵ�ǰ��������ȡָ�����ݵ�����Ļ��档
		/// </summary>
		/// <param name="buf">���档</param>
		/// <param name="start">��ʼ��λ�á�</param>
		/// <param name="count">��С��</param>
		/// <returns>��ȡ�ĳ��ȡ�</returns>
		/// <remarks>
		/// �������Զ����¼����롣
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