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
    /// ��ʾZip�������ࡣ
    /// </summary>
    public enum ZipStreamType {

        /// <summary>
        /// Zip ��
        /// </summary>
        Zlib = 1950,

        /// <summary>
        /// Ĭ�ϡ�
        /// </summary>
        Default = 1951,

        /// <summary>
        /// Gzip ��
        /// </summary>
        Gzip = 1952

    }

    /// <summary>
    /// �����ļ������ڲ�����
    /// </summary>
	public class ZipBaseStream : Stream {

		#region ����

		/// <summary>
		/// Ĭ�ϵĹ�������ռ䡣 Ĭ�� 8192 �ֽڡ�
		/// </summary>
		const int WorkingBufferSizeDefault = 16384;

		/// <summary>
		/// Ĭ�ϵ���С�����С��Ĭ�� 128 �ֽڡ�
		/// </summary>
		const int WorkingBufferSizeMin = 1024;

		/// <summary>
		/// �������ַ����������׳��쳣��
		/// </summary>
		protected const string TypeName = "ZipBaseStream";

		#endregion

        #region ˽��

		/// <summary>
		/// ��ǰ�Ľ�������
		/// </summary>
        internal ZlibCodec _z = null;

		/// <summary>
		/// ��ģʽ��
		/// </summary>
        StreamMode _streamMode = StreamMode.Undefined;

		/// <summary>
		/// ��ȡ������ʹ�õ���ģʽ��
		/// </summary>
        protected StreamMode UsingStreamMode { get { return _streamMode; } set { _streamMode = value; } }
		
		/// <summary>
		/// ��������͡�
		/// </summary>
        FlushType _flushMode;

		/// <summary>
		/// ��ѹģʽ��
		/// </summary>
        CompressionMode _compressionMode;

		/// <summary>
		/// ��ѹ�ȼ���
		/// </summary>
        CompressionLevel _level;

		/// <summary>
		/// �Ƿ����򿪡�
		/// </summary>
        bool _leaveOpen;

		/// <summary>
		/// �����Ļ��档
		/// </summary>
		byte[] _workingBuffer;

		/// <summary>
		/// �ڲ�����������
		/// </summary>
		Stream _stream;

		/// <summary>
		/// ��ѹ�Ĳ��ԡ�
		/// </summary>
		CompressionStrategy _strategy = CompressionStrategy.Default;

		/// <summary>
		/// û�и������롣
		/// </summary>
		bool _noMore = false;

		/// <summary>
		/// �Ƿ�Ϊ GZipStream ʵ����
		/// </summary>
		bool _isGZip;

		#endregion

		#region ����

		/// <summary>
		/// ��ȡ������һ��ֵ����ֵ��ʾ����Դ�Ƿ����ͷš�
		/// </summary>
		protected bool IsDisposed {
			get;
			set;
		}

		/// <summary>
		/// ��ȡ�Ի����������á�
		/// </summary>
		public Stream BaseStream {
			get {
				return _stream;
			}
		}

		/// <summary>
		/// ��ȡ�����û����С��
		/// </summary>
		///
		/// <remarks>
		/// <para>
		///   ������ݶ�����������Ч��  Ĭ�ϴ�СΪ 
		///   1024 B��  ��С 128 B�� Ч���滺��������ӣ����м��޴�С��
		/// </para>
		///
		/// <para>
		/// �� <c>Read()</c> �� <c>Write()</c> ǰ�������ֵ��
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">���Ѿ��رա�</exception>
		/// <exception cref="ZlibException">���������á�</exception>
		/// <exception cref="ArgumentOutOfRangeException">����С����Сֵ��</exception>
		public int BufferSize {
			get {
				return _workingBuffer.Length;
			}
			set {
				Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
				Thrower.ThrowArgumentOutOfRangeExceptionIf(value < WorkingBufferSizeMin, "value", String.Format(" {0} �ֽ� ������Сֵ {1}��������Ч��", value, WorkingBufferSizeMin));
                byte[] b = new byte[value];
                if (_workingBuffer != null)
                    Array.Copy(_workingBuffer, b, value);
                _workingBuffer = b;
			}
		}

		/// <summary>
		/// ��ȡ��ǰ������ֽڳ��ȡ�
		/// </summary>
		public virtual long TotalIn {
			get {
				return _z.TotalBytesIn;
			}
		}

		/// <summary>
		/// ��ȡ��ǰд����ֽڳ��ȡ�
		/// </summary>
		public virtual long TotalOut {
			get {
				return _z.TotalBytesOut;
			}
		}

		/// <summary>
		/// ��������������дʱ����ȡָʾ��ǰ���Ƿ�֧�ֶ�ȡ��ֵ��
		/// </summary>
		/// <value>�����֧�ֶ�ȡ��Ϊ true������Ϊ false��</value>
		public override bool CanRead {
			get {
				return _stream.CanRead;
			}
		}

		/// <summary>
		/// ��ȡָʾ��ǰ���Ƿ�֧�ֲ��ҹ��ܵ�ֵ��
		/// </summary>
		/// <value>�����֧�ֲ��ң�Ϊ true������Ϊ false��</value>
		public override bool CanSeek {
			get {
				return false;
			}
		}

		/// <summary>
		/// ��ȡ�����ý�ѹ���ԡ�
		/// </summary>
		/// <exception cref="ObjectDisposedException">���Ѿ��رա�</exception>
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
		/// ��ȡָʾ��ǰ���Ƿ�֧��д�빦�ܵ�ֵ��
		/// </summary>
		/// <value>�����֧��д�룬Ϊ true������Ϊ false��</value>
		public override bool CanWrite {
			get {
				return _stream.CanWrite;
			}
		}

		/// <summary>
		/// ��ȡ��ǰ�Ƿ���Ҫ��ѹ��
		/// </summary>
		protected bool WantCompress {
			get {
				return (this._compressionMode == CompressionMode.Compress);
			}
		}

		/// <summary>
		/// ��ȡ���ֽڱ�ʾ�������ȡ������Բ���֧�֣������������� System.NotSupportedException��
		/// </summary>
		/// <value>���ֽڱ�ʾ�����ȵĳ�ֵ��</value>
		/// <exception cref="T:System.NotSupportedException">�� Stream �������಻֧�ֲ��ҡ�</exception>
		public override long Length {
			get {
				throw new NotSupportedException();
				//return _stream.Length;
			}
		}

		/// <summary>
		/// ��ȡ�����õ�ǰ���е�λ�á������Ե����ò���֧�֣������������� System.NotSupportedException��
		/// </summary>
		/// <value>���еĵ�ǰλ�á�</value>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧�ֲ��ҡ�</exception>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// ��ȡ�������Ƿ���Ҫ Rfc1950 ͷ��
		/// </summary>
		/// <value>��� true������; ����, false��</value>
		public bool WantRfc1950Header {
			get;
			set;
		}

		#endregion

        #region ��ʼ��

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ��ʼ�� <see cref="Py.Zip.Zlib.ZipBaseStream"/> ����ʵ����
        /// </summary>
        /// <param name="stream">Ҫ��ѹ��ѹ����������</param>
        /// <param name="compressionMode">ָʾ��ǰ�����ǽ�ѹ��ѹ����</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> Ϊ null��</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode)
            : this(stream, compressionMode, CompressionLevel.Default, false) {

        }

        /// <summary>
        /// ʹ��ָ������, CompressionLevel ֵ�� CompressionMode ֵ����ʼ�� <see cref="Py.Zip.Zlib.ZipBaseStream"/> ����ʵ����
        /// </summary>
        /// <param name="stream">Ҫ��ѹ��ѹ����������</param>
        /// <param name="compressionMode">ָʾ��ǰ�����ǽ�ѹ��ѹ����</param>
        /// <param name="level">ʹ�õĽ�ѹ�ȼ���</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> Ϊ null��</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level)
            : this(stream, compressionMode, level, false) {

        }

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� <see cref="Py.Zip.Zlib.ZipBaseStream"/> ����ʵ����
        /// </summary>
        /// <param name="stream">Ҫ��ѹ��ѹ����������</param>
        /// <param name="compressionMode">ָʾ��ǰ�����ǽ�ѹ��ѹ����</param>
        /// <param name="leaveOpen">true ��������Ϊ��״̬������Ϊ false��</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> Ϊ null��</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, bool leaveOpen)
            : this(stream, compressionMode, CompressionLevel.Default, leaveOpen) {

        }

        /// <summary>
        /// ʹ��ָ������, CompressionLevel ֵ�� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� <see cref="Py.Zip.Zlib.ZipBaseStream"/> ����ʵ����
        /// </summary>
        /// <param name="stream">Ҫ��ѹ��ѹ����������</param>
        /// <param name="compressionMode">ָʾ��ǰ�����ǽ�ѹ��ѹ����</param>
        /// <param name="leaveOpen">true ��������Ϊ��״̬������Ϊ false��</param>
        /// <param name="level">ʹ�õĽ�ѹ�ȼ���</param>
		/// <exception cref="ArgumentNullException"><paramref name="stream"/> Ϊ null��</exception>
        /// <exception cref="InvalidOperationException"><paramref name="stream"/> ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public ZipBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, bool leaveOpen)
            : base() {  
            Thrower.ThrowArgumentNullExceptionIf(stream, "stream");
            Thrower.ThrowInvalidOperationExceptionIf(!stream.CanWrite && compressionMode == CompressionMode.Compress, "��ǰ������д");
            _flushMode = FlushType.None;
            //_workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
            _stream = stream;
            _leaveOpen = leaveOpen;
            _compressionMode = compressionMode;
            _level = level;
			_isGZip = this is GZipStream;
        }


        #endregion

        #region ����

        /// <summary>
        /// ������ѹ�������ֽڶ���ָ�����ֽ����顣
        /// </summary>
        /// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="ObjectDisposedException">���Ѿ��رա�</exception>
        public override void Flush() {
            Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
            _stream.Flush();
        }

        /// <summary>
        /// ���õ�ǰ���е�λ�á����汾��֧�����������
        /// </summary>
        /// <param name="offset">����� <paramref name="origin"/> �������ֽ�ƫ������</param>
        /// <param name="origin"><see cref="T:System.IO.SeekOrigin"/> ���͵�ֵ��ָʾ���ڻ�ȡ��λ�õĲο��㡣</param>
        /// <returns>��ǰ���е���λ�á�</returns>
        /// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
        /// <exception cref="T:System.NotSupportedException">����֧�ֲ��ң���������ͨ���ܵ������̨������������¼�Ϊ��ˡ�</exception>
        /// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// ���õ�ǰ���ĳ��ȡ����汾��֧�����������
        /// </summary>
        /// <param name="value">����ĵ�ǰ���ĳ��ȣ����ֽڱ�ʾ����</param>
        /// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
        /// <exception cref="T:System.NotSupportedException">����֧��д��Ͳ��ң���������ͨ���ܵ������̨������������¼�Ϊ��ˡ�</exception>
        /// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
        public override void SetLength(long value) {
            //_stream.SetLength(value);
            throw new NotSupportedException();
        }

		/// <summary>
		/// ��ȡ��ǰ�����Ľ�������
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
		/// ��ȡ��ǰ�����Ļ��档
		/// </summary>
        protected byte[] WorkingBuffer {
            get {
                if (_workingBuffer == null)
                    _workingBuffer = new byte[WorkingBufferSizeDefault];
                return _workingBuffer;
            }
        }

		/// <summary>
		/// ��鹤��ģʽ��
		/// </summary>
		/// <param name="streamMode">��Ҫ��ģʽ��</param>
		[System.Diagnostics.Conditional(Thrower.DEBUG), System.Diagnostics.Conditional(Thrower.LIBSTD)]
		void ValidStreamMode(StreamMode streamMode) {
			if (_streamMode == StreamMode.Undefined)
				_streamMode = streamMode;
			else if (_streamMode != streamMode) {
				throw new ZlibException(streamMode == StreamMode.Reader ? "�޷��ڶ�ȡ��ʱ��д�롣" : "�޷���д��ʱ���ȡ��");
			}
		}

		/// <summary>
		/// ��ǰ����д���ֽ����У����������еĵ�ǰλ������д����ֽ�����
		/// </summary>
		/// <param name="buffer">�ֽ����顣�˷����� <paramref name="count"/> ���ֽڴ� <paramref name="buffer"/> ���Ƶ���ǰ����</param>
		/// <param name="offset"><paramref name="buffer"/> �еĴ��㿪ʼ���ֽ�ƫ�������Ӵ˴���ʼ���ֽڸ��Ƶ���ǰ����</param>
		/// <param name="count">Ҫд�뵱ǰ�����ֽ�����</param>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="offset"/> �� <paramref name="count"/> �ĺʹ��ڻ��������ȡ�</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer"/> Ϊ null��</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset"/> �� <paramref name="count"/> Ϊ����</exception>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧��д�롣</exception>
		/// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
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
                    throw new ZlibException((WantCompress ? "ѹ��" : "��ѹ") + "����: " + _z.Message);

                //if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
                _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);

                done = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;

                // If GZIP and de-compress, we're done when 8 bytes remain.
				if (_isGZip && !WantCompress)
                    done = (_z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0);

            }   while (!done);
        }

		/// <summary>
		/// ʵ�ֹر�����
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
					Thrower.ThrowZlibExceptionIf(rc != ZlibState.StreamEnd && rc != ZlibState.Success, (WantCompress ? "ѹ��" : "��ѹ") + "����" + (_z.Message == null ? ("״̬ = " + Enum.GetName(typeof(ZlibState), rc)) : _z.Message));

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
		/// �رյ�ǰ�����ͷ���֮������������Դ�����׽��ֺ��ļ��������
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
        /// ��ȡ�����û��������ʽ��
        /// </summary>
        /// <remarks>
        /// ���������ʽ���� �� ZLIB �й��ĵ���
        /// </remarks>
        /// <exception cref="ObjectDisposedException">��Դ���ͷš�</exception>
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
		/// ������ѹ�������ֽڶ���ָ�����ֽ����顣
		/// </summary>
		/// <param name="buffer">���ڴ洢ѹ�������ֽڵ����顣</param>
		/// <param name="offset">�����п�ʼ��ȡ��λ�á�</param>
		/// <param name="count">��ȡ��ѹ�����ֽ�����</param>
		/// <returns>ѹ�������ֽ������е��ֽ�����</returns>
		/// <remarks>
		/// 	<para>
		/// �����Ҫʹ�� <c>DeflateStream</c> �ڶ�ȡʱͬ����ѹ�� �������ý�ѹģʽ�� <c>CompressionMode.Compress</c>��Ȼ��ʹ�� Read() ������ȡ����ѹ��
		/// �����Ҫʹ�� <c>DeflateStream</c> �ڶ�ȡʱͬ��ѹ���� �������ý�ѹģʽ�� <c>CompressionMode.DeCompress</c>��Ȼ��ʹ�� Read() ������ȡ����ѹ��
		/// </para>
		/// 	<para>
		/// һ�� <c>DeflateStream</c> ֻ������ <c>Read()</c> �� <c>Write()</c>, ������ͬʱ��д��
		/// </para>
		/// </remarks>
		/// <exception cref="ZlibException">�Ѿ�ִ�й� Write() ��</exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="offset"/> �� <paramref name="count"/> �ĺʹ��ڻ��������ȡ�</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="buffer"/> Ϊ null��</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="offset"/> �� <paramref name="count"/> Ϊ����</exception>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧�ֶ�ȡ��</exception>
		/// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
		public override int Read(byte[] buffer, int offset, int count) {
            // According to MS documentation, any implementation of the IO.Stream.Read function must:
            // (a) throw an exception if offset & count reference an invalid part of the buffer,
            //     or if count < 0, or if buffer is null
            // (b) return 0 only upon EOF, or if count = 0
            // (c) if not EOF, then return at least 1 byte, up to <count> bytes

			Thrower.CheckArgumentException(buffer, offset, count);
            Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
            if (_streamMode == StreamMode.Undefined) {
				Thrower.ThrowNotSupportedExceptionIf(!_stream.CanRead, "����֧�ֶ�ȡ��");

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
                    throw new ZlibException(Py.Core.Str.FormatX("{0}:  ���={1} ��Ϣ={2}", (WantCompress ? "ѹ��" : "��ѹ"), rc, _z.Message));

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
                            throw new ZlibException(String.Format("ѹ��:   ״̬={0}  ��Ϣ={1}", rc, _z.Message));
                    }
                }
            }


            return count - _z.AvailableBytesOut;
        }

		/// <summary>
		/// �ͷ��� <see cref="T:Py.Zip.Zlib.ZlibStream"/> ռ�õķ��й���Դ���������������ͷ��й���Դ��
		/// </summary>
		/// <param name="disposing">Ϊ true ���ͷ��й���Դ�ͷ��й���Դ��Ϊ false ����ͷŷ��й���Դ��</param>
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
        /// ָʾ��ǰ���õ���ģʽ��
        /// </summary>
        protected enum StreamMode {

            /// <summary>
            /// δ���塣
            /// </summary>
            Undefined,

            /// <summary>
            /// д�롣
            /// </summary>
            Writer,

            /// <summary>
            /// ��ȡ��
            /// </summary>
            Reader,
		}

		#endregion

		#region ��̬

		/// <summary>
		/// ��ѹ�ַ�������ѹ�Ľ�����������С�
		/// </summary>
		/// <param name="value">�ַ�����</param>
		/// <param name="compressor">������ѹ������</param>
        public static void CompressString(string value, Stream compressor) {
			Thrower.ThrowArgumentNullExceptionIf(compressor, "compressor");
            byte[] uncompressed = System.Text.Encoding.UTF8.GetBytes(value);
            using (compressor) {
                compressor.Write(uncompressed, 0, uncompressed.Length);
            }
        }

		/// <summary>
		/// ��ѹ���顣��ѹ�Ľ�����������С�
		/// </summary>
		/// <param name="b">�ֽڡ�</param>
		/// <param name="compressor">������ѹ������</param>
        public static void CompressBuffer(byte[] b, Stream compressor) {
			Thrower.ThrowArgumentNullExceptionIf(compressor, "compressor");
            using (compressor) {
                compressor.Write(b, 0, b.Length);
            }
        }

		/// <summary>
		/// ѹ���ַ�����
		/// </summary>
		/// <param name="compressed">�ַ�����</param>
		/// <param name="decompressor">����ѹ��������</param>
		/// <returns>�ַ�����</returns>
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
		/// ѹ�����顣
		/// </summary>
		/// <param name="compressed">��ѹ�����顣</param>
		/// <param name="decompressor">����ѹ��������</param>
		/// <returns>�ֽڡ�</returns>
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
