// ZlibBaseStream.cs
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
// Time-stamp: <2009-October-28 15:45:15>
//
// ------------------------------------------------------------------
//
// This module defines the ZlibBaseStream class, which is an intnernal
// base class for DeflateStream, ZlibStream and GZipStream.
//
// ------------------------------------------------------------------

using System;
using System.IO;
using Py.RunTime;

namespace Py.Zip.Zlib {

    /// <summary>
    /// 表示Zip流的种类。
    /// </summary>
    public enum ZipStreamType {

        /// <summary>
        /// Zip 。
        /// </summary>
        Zlib = 1950,

        /// <summary>
        /// 默认。
        /// </summary>
        Default = 1951,

        /// <summary>
        /// Gzip 。
        /// </summary>
        Gzip = 1952

    }

    /// <summary>
    /// 所有文件流的内部流。
    /// </summary>
	public class ZipBaseStream : Stream {

		#region 配置

		/// <summary>
		/// 默认的工作缓存空间。 默认 8192 字节。
		/// </summary>
		const int WorkingBufferSizeDefault = 16384;

		/// <summary>
		/// 默认的最小缓存大小。默认 128 字节。
		/// </summary>
		const int WorkingBufferSizeMin = 1024;

		/// <summary>
		/// 类名的字符串。用于抛出异常。
		/// </summary>
		protected const string TypeName = "ZipBaseStream";

		#endregion

        #region 私有

		/// <summary>
		/// 当前的解码器。
		/// </summary>
        internal ZlibCodec _z = null;

		/// <summary>
		/// 流模式。
		/// </summary>
        StreamMode _streamMode = StreamMode.Undefined;

		/// <summary>
		/// 获取或设置使用的流模式。
		/// </summary>
        protected StreamMode UsingStreamMode { get { return _streamMode; } set { _streamMode = value; } }
		
		/// <summary>
		/// 输出的类型。
		/// </summary>
        FlushType _flushMode;

		/// <summary>
		/// 解压模式。
		/// </summary>
        CompressionMode _compressionMode;

		/// <summary>
		/// 解压等级。
		/// </summary>
        CompressionLevel _level;

		/// <summary>
		/// 是否保留打开。
		/// </summary>
        bool _leaveOpen;

		/// <summary>
		/// 工作的缓存。
		/// </summary>
		byte[] _workingBuffer;

		/// <summary>
		/// 内部操作的流。
		/// </summary>
		Stream _stream;

		/// <summary>
		/// 解压的策略。
		/// </summary>
		CompressionStrategy _strategy = CompressionStrategy.Default;

		/// <summary>
		/// 没有更多输入。
		/// </summary>
		bool _noMore = false;

		/// <summary>
		/// 是否为 GZipStream 实例。
		/// </summary>
		bool _isGZip;

		#endregion

		#region 属性

		/// <summary>
		/// 获取或设置一个值，该值表示流资源是否已释放。
		/// </summary>
		protected bool IsDisposed {
			get;
			set;
		}

		/// <summary>
		/// 获取对基础流的引用。
		/// </summary>
		public Stream BaseStream {
			get {
				return _stream;
			}
		}

		/// <summary>
		/// 获取或设置缓存大小。
		/// </summary>
		///
		/// <remarks>
		/// <para>
		///   这个内容对所有流都有效。  默认大小为 
		///   1024 B。  最小 128 B。 效率随缓存变大而增加，但有极限大小。
		/// </para>
		///
		/// <para>
		/// 在 <c>Read()</c> 或 <c>Write()</c> 前设置这个值。
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">流已经关闭。</exception>
		/// <exception cref="ZlibException">缓存已设置。</exception>
		/// <exception cref="ArgumentOutOfRangeException">参数小于最小值。</exception>
		public int BufferSize {
			get {
				return _workingBuffer.Length;
			}
			set {
				Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
				Thrower.ThrowArgumentOutOfRangeExceptionIf(value < WorkingBufferSizeMin, "value", String.Format(" {0} 字节 少于最小值 {1}，参数无效。", value, WorkingBufferSizeMin));
                byte[] b = new byte[value];
                if (_workingBuffer != null)
                    Array.Copy(_workingBuffer, b, value);
                _workingBuffer = b;
			}
		}

		/// <summary>
		/// 获取当前读入的字节长度。
		/// </summary>
		public virtual long TotalIn {
			get {
				return _z.TotalBytesIn;
			}
		}

		/// <summary>
		/// 获取当前写完的字节长度。
		/// </summary>
		public virtual long TotalOut {
			get {
				return _z.TotalBytesOut;
			}
		}

		/// <summary>
		/// 当在派生类中重写时，获取指示当前流是否支持读取的值。
		/// </summary>
		/// <value>如果流支持读取，为 true；否则为 false。</value>
		public override bool CanRead {
			get {
				return _stream.CanRead;
			}
		}

		/// <summary>
		/// 获取指示当前流是否支持查找功能的值。
		/// </summary>
		/// <value>如果流支持查找，为 true；否则为 false。</value>
		public override bool CanSeek {
			get {
				return false;
			}
		}

		/// <summary>
		/// 获取或设置解压策略。
		/// </summary>
		/// <exception cref="ObjectDisposedException">流已经关闭。</exception>
		public CompressionStrategy Strategy {
			get {
				return _strategy;
			}
			set {
				Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
				_strategy = value;
			}
		}

		/// <summary>
		/// 获取指示当前流是否支持写入功能的值。
		/// </summary>
		/// <value>如果流支持写入，为 true；否则为 false。</value>
		public override bool CanWrite {
			get {
				return _stream.CanWrite;
			}
		}

		/// <summary>
		/// 获取当前是否需要解压。
		/// </summary>
		protected bool WantCompress {
			get {
				return (this._compressionMode == CompressionMode.Compress);
			}
		}

		/// <summary>
		/// 获取用字节表示的流长度。此属性不受支持，并且总是引发 System.NotSupportedException。
		/// </summary>
		/// <value>用字节表示流长度的长值。</value>
		/// <exception cref="T:System.NotSupportedException">从 Stream 派生的类不支持查找。</exception>
		public override long Length {
			get {
				throw new NotSupportedException();
				//return _stream.Length;
			}
		}

		/// <summary>
		/// 获取或设置当前流中的位置。此属性的设置不受支持，并且总是引发 System.NotSupportedException。
		/// </summary>
		/// <value>流中的当前位置。</value>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持查找。</exception>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 获取或设置是否需要 Rfc1950 头。
		/// </summary>
		/// <value>如果 true，则需; 否则, false。</value>
		public bool WantRfc1950Header {
			get;
			set;
		}

		#endregion

        #region 初始化

        /// <summary>
        /// 使用指定的流和 CompressionMode 值初始化 <see cref="Py.Zip.Zlib.ZipBaseStream"/> 的新实例。
        /// </summary>
        /// <param name="stream">要解压或压缩缩的流。</param>
        /// <param name="compressionMode">指示当前操作是解压或压缩。</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode)
            : this(stream, compressionMode, CompressionLevel.Default, false) {

        }

        /// <summary>
        /// 使用指定的流, CompressionLevel 值和 CompressionMode 值，初始化 <see cref="Py.Zip.Zlib.ZipBaseStream"/> 的新实例。
        /// </summary>
        /// <param name="stream">要解压或压缩缩的流。</param>
        /// <param name="compressionMode">指示当前操作是解压或压缩。</param>
        /// <param name="level">使用的解压等级。</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level)
            : this(stream, compressionMode, level, false) {

        }

        /// <summary>
        /// 使用指定的流和 CompressionMode 值以及一个指定是否将流保留为打开状态的值，初始化 <see cref="Py.Zip.Zlib.ZipBaseStream"/> 的新实例。
        /// </summary>
        /// <param name="stream">要解压或压缩缩的流。</param>
        /// <param name="compressionMode">指示当前操作是解压或压缩。</param>
        /// <param name="leaveOpen">true 将流保留为打开状态，否则为 false。</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, bool leaveOpen)
            : this(stream, compressionMode, CompressionLevel.Default, leaveOpen) {

        }

        /// <summary>
        /// 使用指定的流, CompressionLevel 值和 CompressionMode 值以及一个指定是否将流保留为打开状态的值，初始化 <see cref="Py.Zip.Zlib.ZipBaseStream"/> 的新实例。
        /// </summary>
        /// <param name="stream">要解压或压缩缩的流。</param>
        /// <param name="compressionMode">指示当前操作是解压或压缩。</param>
        /// <param name="leaveOpen">true 将流保留为打开状态，否则为 false。</param>
        /// <param name="level">使用的解压等级。</param>
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, bool leaveOpen)
            : base() {  
            Thrower.ThrowArgumentNullExceptionIf(stream, "stream");
            Thrower.ThrowInvalidOperationExceptionIf(!stream.CanWrite && compressionMode == CompressionMode.Compress, "当前流不可写");
            _flushMode = FlushType.None;
            //_workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
            _stream = stream;
            _leaveOpen = leaveOpen;
            _compressionMode = compressionMode;
            _level = level;
			_isGZip = this is GZipStream;
        }


        #endregion

        #region 方法

        /// <summary>
        /// 将若干压缩缩的字节读入指定的字节数组。
        /// </summary>
        /// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="ObjectDisposedException">流已经关闭。</exception>
        public override void Flush() {
            Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
            _stream.Flush();
        }

        /// <summary>
        /// 设置当前流中的位置。本版本不支持这个操作。
        /// </summary>
        /// <param name="offset">相对于 <paramref name="origin"/> 参数的字节偏移量。</param>
        /// <param name="origin"><see cref="T:System.IO.SeekOrigin"/> 类型的值，指示用于获取新位置的参考点。</param>
        /// <returns>当前流中的新位置。</returns>
        /// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
        /// <exception cref="T:System.NotSupportedException">流不支持查找，例如在流通过管道或控制台输出构造的情况下即为如此。</exception>
        /// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 设置当前流的长度。本版本不支持这个操作。
        /// </summary>
        /// <param name="value">所需的当前流的长度（以字节表示）。</param>
        /// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
        /// <exception cref="T:System.NotSupportedException">流不支持写入和查找，例如在流通过管道或控制台输出构造的情况下即为如此。</exception>
        /// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
        public override void SetLength(long value) {
            //_stream.SetLength(value);
            throw new NotSupportedException();
        }

		/// <summary>
		/// 获取当前操作的解码器。
		/// </summary>
        internal ZlibCodec ZlibCodec {
            get {
                if (_z == null) {
                    _z = new ZlibCodec();
                    if (this._compressionMode == CompressionMode.Decompress) {
						_z.InitializeInflate(WantRfc1950Header);
                    } else {
                        _z.Strategy = _strategy;
						_z.InitializeDeflate(_level, WantRfc1950Header);
                    }
                }
                return _z;
            }
        }

		/// <summary>
		/// 获取当前操作的缓存。
		/// </summary>
        protected byte[] WorkingBuffer {
            get {
                if (_workingBuffer == null)
                    _workingBuffer = new byte[WorkingBufferSizeDefault];
                return _workingBuffer;
            }
        }

		/// <summary>
		/// 检查工作模式。
		/// </summary>
		/// <param name="streamMode">需要的模式。</param>
		[System.Diagnostics.Conditional(Thrower.DEBUG), System.Diagnostics.Conditional(Thrower.LIBSTD)]
		void ValidStreamMode(StreamMode streamMode) {
			if (_streamMode == StreamMode.Undefined)
				_streamMode = streamMode;
			else if (_streamMode != streamMode) {
				throw new ZlibException(streamMode == StreamMode.Reader ? "无法在读取的时候写入。" : "无法在写入时候读取。");
			}
		}

		/// <summary>
		/// 向当前流中写入字节序列，并将此流中的当前位置提升写入的字节数。
		/// </summary>
		/// <param name="buffer">字节数组。此方法将 <paramref name="count"/> 个字节从 <paramref name="buffer"/> 复制到当前流。</param>
		/// <param name="offset"><paramref name="buffer"/> 中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
		/// <param name="count">要写入当前流的字节数。</param>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer"/> 为 null。</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持写入。</exception>
		/// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
        public override void Write(byte[] buffer, int offset, int count) {
            
            Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);

			ValidStreamMode(StreamMode.Writer);

            if (count == 0)
                return;

            // first reference of z property will initialize the private var _z
            ZlibCodec.InputBuffer = buffer;
            _z.NextIn = offset;
            _z.AvailableBytesIn = count;
            bool done = false;
            do {
                _z.OutputBuffer = WorkingBuffer;
                _z.NextOut = 0;
                _z.AvailableBytesOut = _workingBuffer.Length;
				ZlibState rc = (WantCompress)
                    ? _z.Deflate(_flushMode)
                    : _z.Inflate(_flushMode);
                if (rc != ZlibState.Success && rc != ZlibState.StreamEnd)
                    throw new ZlibException((WantCompress ? "压缩" : "解压") + "错误: " + _z.Message);

                //if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
                _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);

                done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;

                // If GZIP and de-compress, we're done when 8 bytes remain.
				if (_isGZip && !WantCompress)
                    done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

            }   while (!done);
        }

		/// <summary>
		/// 实现关闭流。
		/// </summary>
		protected virtual void CloseInternal() {
			if (_streamMode == StreamMode.Writer) {
				bool done = false;
				do {
					_z.OutputBuffer = WorkingBuffer;
					_z.NextOut = 0;
					_z.AvailableBytesOut = _workingBuffer.Length;
					ZlibState rc = (WantCompress)
						? _z.Deflate(FlushType.Finish)
						: _z.Inflate(FlushType.Finish);
					Thrower.ThrowZlibExceptionIf(rc != ZlibState.StreamEnd && rc != ZlibState.Success, (WantCompress ? "压缩" : "解压") + "错误" + (_z.Message == null ? ("状态 = " + Enum.GetName(typeof(ZlibState), rc)) : _z.Message));

					if (_workingBuffer.Length - _z.AvailableBytesOut > 0) {
						_stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
					}

					done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
					// If GZIP and de-compress, we're done when 8 bytes remain.
					if (_isGZip && !WantCompress)
						done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

				} while (!done);

				Flush();

			}
		}

		/// <summary>
		/// 关闭当前流并释放与之关联的所有资源（如套接字和文件句柄）。
		/// </summary>
        public override void Close() {
            if (_stream == null) return;
            try {
				if (_z == null) return;
				CloseInternal();
            } finally {
				if (_z != null) {

					if (WantCompress) {
						_z.EndDeflate();
					} else {
						_z.EndInflate();
					}
					_z = null;
					if (!_leaveOpen)
						_stream.Close();
					_stream = null;
				}
            }
        }

        /// <summary>
        /// 获取或设置缓存输出方式。
        /// </summary>
        /// <remarks>
        /// 缓存输出方式意义 见 ZLIB 有关文档。
        /// </remarks>
        /// <exception cref="ObjectDisposedException">资源已释放。</exception>
        public virtual FlushType FlushMode {
            get {
                return _flushMode;
            }
            set {
                Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
                _flushMode = value;
            }
        }

		/// <summary>
		/// 将若干压缩缩的字节读入指定的字节数组。
		/// </summary>
		/// <param name="buffer">用于存储压缩缩的字节的数组。</param>
		/// <param name="offset">数组中开始读取的位置。</param>
		/// <param name="count">读取的压缩缩字节数。</param>
		/// <returns>压缩缩到字节数组中的字节数。</returns>
		/// <remarks>
		/// 	<para>
		/// 如果需要使用 <c>DeflateStream</c> 在读取时同步解压， 可以设置解压模式到 <c>CompressionMode.Compress</c>。然后使用 Read() 方法读取并解压。
		/// 如果需要使用 <c>DeflateStream</c> 在读取时同步压缩， 可以设置解压模式到 <c>CompressionMode.DeCompress</c>。然后使用 Read() 方法读取并解压。
		/// </para>
		/// 	<para>
		/// 一个 <c>DeflateStream</c> 只能用于 <c>Read()</c> 或 <c>Write()</c>, 但不能同时读写。
		/// </para>
		/// </remarks>
		/// <exception cref="ZlibException">已经执行过 Write() 。</exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="offset"/> 与 <paramref name="count"/> 的和大于缓冲区长度。</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer"/> 为 null。</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset"/> 或 <paramref name="count"/> 为负。</exception>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持读取。</exception>
		/// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
		public override int Read(byte[] buffer, int offset, int count) {
            // According to MS documentation, any implementation of the IO.Stream.Read function must:
            // (a) throw an exception if offset & count reference an invalid part of the buffer,
            //     or if count < 0, or if buffer is null
            // (b) return 0 only upon EOF, or if count = 0
            // (c) if not EOF, then return at least 1 byte, up to <count> bytes

			Thrower.CheckArgumentException(buffer, offset, count);
            Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
            if (_streamMode == StreamMode.Undefined) {
				Thrower.ThrowNotSupportedExceptionIf(!_stream.CanRead, "流不支持读取。");

                _streamMode = StreamMode.Reader;

                ZlibCodec.AvailableBytesIn = 0;

            }

			ValidStreamMode(StreamMode.Reader);
            if (count == 0) return 0;
            if (_noMore && WantCompress) return 0;

			Thrower.ThrowArgumentNullExceptionIf(buffer, "buffer");
			Thrower.ThrowArgumentOutOfRangeExceptionIf(count < 0 || (offset + count) > buffer.GetLength(0), "count");
			Thrower.ThrowArgumentOutOfRangeExceptionIf(offset < buffer.GetLowerBound(0), "offset");

			ZlibState rc;

            // set up the output of the deflate/inflate codec:
            _z.OutputBuffer = buffer;
            _z.NextOut = offset;
            _z.AvailableBytesOut = count;

            // This is necessary in case _workingBuffer has been resized. (new byte[])
            // (The first reference to _workingBuffer goes through the private accessor which
            // may initialize it.)
            _z.InputBuffer = WorkingBuffer;

            do {
                // need data in _workingBuffer in order to deflate/inflate.  Here, we check if we have any.
                if ((_z.AvailableBytesIn == 0) && (!_noMore)) {
                    // No data available, so try to Read data from the captive stream.
                    _z.NextIn = 0;
                    _z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
                    if (_z.AvailableBytesIn == 0)
                        _noMore = true;

                }
                // we have data in InputBuffer; now compress or decompress as appropriate
                rc = (WantCompress)
                    ? _z.Deflate(_flushMode)
                    : _z.Inflate(_flushMode);

                if (_noMore && (rc == ZlibState.BufferError))
                    return 0;

                if (rc != ZlibState.Success && rc != ZlibState.StreamEnd)
                    throw new ZlibException(Py.Core.Str.FormatX("{0}:  结果={1} 信息={2}", (WantCompress ? "压缩" : "解压"), rc, _z.Message));

                if ((_noMore || rc == ZlibState.StreamEnd) && (_z.AvailableBytesOut == count))
                    break; // nothing more to read
            }  while (_z.AvailableBytesOut > 0 && !_noMore && rc == ZlibState.Success);


            // workitem 8557
            // is there more room in output? 
            if (_z.AvailableBytesOut > 0) {
                if (rc == ZlibState.Success && _z.AvailableBytesIn == 0) {
                    // deferred
                }

                // are we completely done reading?
                if (_noMore) {
                    // and in compression?
                    if (WantCompress) {
                        // no more input data available; therefore we flush to
                        // try to complete the read
                        rc = _z.Deflate(FlushType.Finish);

                        if (rc != ZlibState.Success && rc != ZlibState.StreamEnd)
                            throw new ZlibException(String.Format("压缩:   状态={0}  消息={1}", rc, _z.Message));
                    }
                }
            }


            return count - _z.AvailableBytesOut;
        }

		/// <summary>
		/// 释放由 <see cref="T:Py.Zip.Zlib.ZlibStream"/> 占用的非托管资源，还可以另外再释放托管资源。
		/// </summary>
		/// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
		protected override void Dispose(bool disposing) {
			try {
				if (!IsDisposed) {
					if (disposing)
						Close();
					IsDisposed = true;
				}
			} finally {
				base.Dispose(disposing);
			}
		}

        /// <summary>
        /// 指示当前采用的流模式。
        /// </summary>
        protected enum StreamMode {

            /// <summary>
            /// 未定义。
            /// </summary>
            Undefined,

            /// <summary>
            /// 写入。
            /// </summary>
            Writer,

            /// <summary>
            /// 读取。
            /// </summary>
            Reader,
		}

		#endregion

		#region 静态

		/// <summary>
		/// 解压字符串。解压的结果保存在流中。
		/// </summary>
		/// <param name="value">字符串。</param>
		/// <param name="compressor">用来解压的流。</param>
        public static void CompressString(string value, Stream compressor) {
			Thrower.ThrowArgumentNullExceptionIf(compressor, "compressor");
            byte[] uncompressed = System.Text.Encoding.UTF8.GetBytes(value);
            using (compressor) {
                compressor.Write(uncompressed, 0, uncompressed.Length);
            }
        }

		/// <summary>
		/// 解压数组。解压的结果保存在流中。
		/// </summary>
		/// <param name="b">字节。</param>
		/// <param name="compressor">用来解压的流。</param>
        public static void CompressBuffer(byte[] b, Stream compressor) {
			Thrower.ThrowArgumentNullExceptionIf(compressor, "compressor");
            using (compressor) {
                compressor.Write(b, 0, b.Length);
            }
        }

		/// <summary>
		/// 压缩字符串。
		/// </summary>
		/// <param name="compressed">字符串。</param>
		/// <param name="decompressor">用来压缩的流。</param>
		/// <returns>字符串。</returns>
        public static string UncompressString(byte[] compressed, Stream decompressor) {
            // workitem 8460
            byte[] working = new byte[1024];
            var encoding = System.Text.Encoding.UTF8;
            using (var output = new MemoryStream()) {
                using (decompressor) {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0) {
                        output.Write(working, 0, n);
                    }
                }

                // reset to allow read from start
                output.Seek(0, SeekOrigin.Begin);
                var sr = new StreamReader(output, encoding);
                return sr.ReadToEnd();
            }
        }

		/// <summary>
		/// 压缩数组。
		/// </summary>
		/// <param name="compressed">解压的数组。</param>
		/// <param name="decompressor">用来压缩的流。</param>
		/// <returns>字节。</returns>
        public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor) {
            // workitem 8460
            byte[] working = new byte[1024];
            using (var output = new MemoryStream()) {
                using (decompressor) {
                    int n;
                    while ((n = decompressor.Read(working, 0, working.Length)) != 0) {
                        output.Write(working, 0, n);
                    }
                }
                return output.ToArray();
            }
        }

		#endregion

    }
}
