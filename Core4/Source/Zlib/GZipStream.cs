// GZipStream.cs
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
// Time-stamp: <2010-January-09 12:04:28>
//
// ------------------------------------------------------------------
//
// This module defines the GZipStream class, which can be used as a replacement for
// the System.IO.Compression.GZipStream class in the .NET BCL.  NB: The design is not
// completely OO clean: there is some intelligence in the ZlibBaseStream that reads the
// GZip header.
//
// ------------------------------------------------------------------
// edited by xuld

using System;
using System.IO;
using Py.RunTime;

namespace Py.Zip.Zlib {

    /// <summary>
    /// �ṩ����ѹ���ͽ�ѹ�����ķ��������ԡ�
    /// </summary>
    /// <remarks>
    /// <para>�����ʾ GZip ���ݸ�ʽ����ʹ������ѹ���ͽ�ѹ���ļ�����ҵ��׼�㷨�����ָ�ʽ����һ����������𻵵�ѭ������У��ֵ��GZip ���ݸ�ʽʹ�õ��㷨�� DeflateStream ����㷨��ͬ������������չ��ʹ������ѹ����ʽ�� ���ָ�ʽ����ͨ�����漰ר��ʹ��Ȩ�ķ�ʽ����ʵ�֡����಻������ѹ������ 4 GB ���ļ����������ϼ� <see href="http://www.ietf.org/rfc/rfc1952.txt">IETF RFC 1952</see>, "GZIP ѹ���ĵ���ʽ  4.3 ��"��</para>
    /// <para>
    /// ����ʹ����ೣ����ѹ�����߶�д�뵽��չ��Ϊ .gz ���ļ���ѹ�� GZipStream ������н�ѹ�������ǣ�����ԭ�������ṩ������ .zip �浵������ļ���� .zip �浵����ȡ�ļ��Ĺ��ܡ� 
	/// </para>
    /// <para>
    ///   һ�� <c>GZipStream</c> ���Զ���д��������ͬʱ��
    /// </para>
    /// <para>
    ///   ��Ȼ GZIP ��ʽ�����������Բ�ͬ���ļ�, �����ֻ����һ�� GZIP ��ʽ, ����Ϊһ���ļ���
    /// </para>
	/// 
    /// <para>
	/// <see cref="DeflateStream"/> �� <c>GZipStream</c> �е�ѹ��������Ϊ�������� ���������������ֽڵķ�ʽ��ȡ�ģ�����޷�ͨ�����ж�δ�����ȷ��ѹ�������ļ���������ݿ����ѷ���������δѹ��������Դ�����ʹ�� <see cref="DeflateStream"/> �� <c>GZipStream</c> �ࡣ ���Դ������ѹ������ʹ����Щ��ʱʵ���Ͽ��ܻ��������Ĵ�С��
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="GZipStream" />
    ///
    /// <example>
    /// ���������ʾ�����ʹ�� GZipStream ����һ��ѹ������
    /// <code>
    /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
    /// {
    ///     using (var raw = System.IO.File.Create(outputFile))
    ///     {
    ///         using (Stream compressor = new GZipStream(raw, CompressionMode.Compress))
    ///         {
    ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
    ///             int n;
    ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
    ///             {
    ///                 compressor.Write(buffer, 0, n);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code lang="VB">
    /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
    /// Using input As Stream = File.OpenRead(fileToCompress)
    ///     Using raw As FileStream = File.Create(outputFile)
    ///     Using compressor As Stream = New GZipStream(raw, CompressionMode.Compress)
    ///         Dim buffer As Byte() = New Byte(4096) {}
    ///         Dim n As Integer = -1
    ///         Do While (n &lt;&gt; 0)
    ///             If (n &gt; 0) Then
    ///                 compressor.Write(buffer, 0, n)
    ///             End If
    ///             n = input.Read(buffer, 0, buffer.Length)
    ///         Loop
    ///     End Using
    ///     End Using
    /// End Using
    /// </code>
    /// </example>
    ///
    /// <example>
    /// ���������ʾ�����ʹ�� GZipStream ��ѹһ��ѹ������
    /// <code>
    /// private void GunZipFile(string filename)
    /// {
    ///     if (!filename.EndsWith(".gz))
    ///         throw new ArgumentException("filename");
    ///     var DecompressedFile = filename.Substring(0,filename.Length-3);
    ///     byte[] working = new byte[WORKING_BUFFER_SIZE];
    ///     int n= 1;
    ///     using (System.IO.Stream input = System.IO.File.OpenRead(filename))
    ///     {
    ///         using (Stream decompressor= new Py.Zip.Zlib.GZipStream(input, CompressionMode.Decompress, true))
    ///         {
    ///             using (var output = System.IO.File.Create(DecompressedFile))
    ///             {
    ///                 while (n !=0)
    ///                 {
    ///                     n= decompressor.Read(working, 0, working.Length);
    ///                     if (n > 0)
    ///                     {
    ///                         output.Write(working, 0, n);
    ///                     }
    ///                 }
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    ///
    /// <code lang="VB">
    /// Private Sub GunZipFile(ByVal filename as String)
    ///     If Not (filename.EndsWith(".gz)) Then
    ///         Throw New ArgumentException("filename")
    ///     End If
    ///     Dim DecompressedFile as String = filename.Substring(0,filename.Length-3)
    ///     Dim working(WORKING_BUFFER_SIZE) as Byte
    ///     Dim n As Integer = 1
    ///     Using input As Stream = File.OpenRead(filename)
    ///         Using decompressor As Stream = new Py.Zip.Zlib.GZipStream(input, CompressionMode.Decompress, True)
    ///             Using output As Stream = File.Create(UncompressedFile)
    ///                 Do
    ///                     n= decompressor.Read(working, 0, working.Length)
    ///                     If n > 0 Then
    ///                         output.Write(working, 0, n)
    ///                     End IF
    ///                 Loop While (n  > 0)
    ///             End Using
    ///         End Using
    ///     End Using
    /// End Sub
    /// </code>
    /// </example>
    public class GZipStream : ZipBaseStream {
        // GZip ��ʽ
        // ��Դ: http://tools.ietf.org/html/rfc1952
        //
        //  �ļ�ͷ:              2 bytes    1F 8B
        //  ѹ����ʽ               1 byte     8= DEFLATE (������֧�� )
        //  ���                   1 byte     λ��� (����)
        //  mtime                4 bytes    time_t (�� 1970/1/1  UTC ��ʼ)
        //  xflg                 1 byte     2 = ���Ŭ��ѹ�� , 4 = ����ٶ�ѹ�� (���ܺ���)
        //  OS(����ϵͳ)         1 byte     ԭ��Ԫ�Ĳ���ϵͳ. ѹ��ʱ���� 0xFF ��
        //  ��չ�ֶγ���           2 bytes    ��ѡ - ���� FEXTRA ���ú���Ч��
        //  ��չ�ֶ�               varies
        //  �ļ���                 varies     ��ѡ - ��� FNAME ������Ч�� ��ֵ���á� ISO-8859-1.
        //  �ļ�ע��               varies     ��ѡ - ��� FCOMMENT ������Ч�� ��ֵ���� ISO-8859-1.
        //  crc16                1 byte     ��ѡ - Ŀǰֻ�� FHCRC ������Ч�� 
        //  ѹ������               varies
        //  CRC32                4 bytes
        //  isize                4 bytes    �ļ���Сģ 2^32 ������
        //
        //     ���
        //                bit 0   FTEXT - ָʾ�ļ�ʱ ASCII �ļ� (���԰�ȫ����)
        //                bit 1   FHCRC - �ļ�ͷ�� CRC16 �����롣
        //                bit 2   FEXTRA - ��չ�ֶδ��ڡ�
        //                bit 3   FNAME - �ļ���������: ISO-8859-1.
        //                bit 4   FCOMMENT  - �ļ�ע�͡� ����: ISO-8859-1
        //                bit 5   ������������ʹ�á�
        //                bit 6   ������������ʹ�á�
        //                bit 7   ������������ʹ�á�
        //
        // consumptionʱ
        // ������ֶο��Ժ��ӡ�
        //
        // ����ʱ
        // ���� OS ���� 255�� �������п�ѡ�ֶη��� 0��
        //

        #region ˽���ֶ�

        /// <summary>
        /// �ļ�ͷ��С��
        /// </summary>
        int _headerByteCount;

        /// <summary>
        /// ��һ�ζ�ȡ��ɡ�
        /// </summary>
        bool _firstReadDone;

        /// <summary>
        /// �ļ�����
        /// </summary>
        string _fileName;

        /// <summary>
        /// ע�͡�
        /// </summary>
        string _comment;

		/// <summary>
		/// ���� GZIP ���ļ�����
		/// </summary>
        string _gzipFileName;
		
		/// <summary>
		/// ���� GZIP ��ע�͡�
		/// </summary>
		string _gzipComment;
		
		/// <summary>
		/// ��ǰ�ļ����롣
		/// </summary>
		Py.Algorithm.Crc32 _crc = new Algorithm.Crc32();

		/// <summary>
		/// ���� GZIP ���ļ�����ʵ�ʡ�
		/// </summary>
        DateTime _gzipMtime;

		/// <summary>
		/// �ļ�ͷ��С��
		/// </summary>
		int _gzipHeaderByteCount;

		/// <summary>
		/// ���档
		/// </summary>
		byte[] _buf = new byte[1];

        #endregion

        #region ����

        /// <summary>
        /// ��ȡ������ Gzip ��ע�͡�
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   ��ʽ GZIP ����ÿ���ļ�������һ��ע�͡� ע�͵ı����� ISO-8859-1
        ///   ����ҳ�� Ҫд�� GZIP ��һ��ע��, �� <c>Write()</c> ǰ���ô����ԡ�
        /// </para>
        ///
        /// <para>
        ///   ��ʹ�� <c>GZipStream</c> ��ѹ, �����ڵ�һ�� <c>Read()</c> ��õ�ע�͡�  �����
        ///   GZIP �ֽ���û��ע��, ע�����Է��� <c>null</c>
        ///   (�� VB ��Ϊ<c>Nothing</c>)��
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">���ѹرա�</exception>
        public string Comment {
            get {
                return _comment;
            }
            set {
                Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
                _comment = value;
            }
        }

        /// <summary>
        /// ��ǰ�ļ����ļ�����
        /// </summary>
        /// <exception cref="ObjectDisposedException">���ѹرա�</exception>
        public String FileName {
            get { return _fileName; }
            set {
                Thrower.ThrowObjectDisposedExceptionIf(IsDisposed, TypeName);
                _fileName = value;
                if (_fileName == null) return;
                if (_fileName.IndexOf('/') != -1) {
                    _fileName = _fileName.Replace("/", "\\");
                }
                if (_fileName[_fileName.Length - 1] == '\\')
                    throw new IOException("�Ƿ��ļ�����");
                if (_fileName.IndexOf('\\') != -1) {
                    // trim any leading path
                    _fileName = Path.GetFileName(_fileName);
                }
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
				switch (UsingStreamMode) {
					case StreamMode.Writer:
						return _z.TotalBytesOut + _headerByteCount;
					case StreamMode.Reader:
						return _z.TotalBytesIn + _gzipHeaderByteCount;
					default:
						return 0;


				}
			}

			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// ��ȡ��ǰ�� CRC �����롣
		/// </summary>
		public int Crc32 {
			get {
				return _crc.Crc32Result;
			}
		}

        /// <summary>
        /// ��ȡ����������޸�ʱ�䡣
        /// </summary>
        public DateTime? LastModified {
            get;
            set;
        }

        #endregion

        #region ��ʼ��

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ��ʼ�� GZipStream �����ʵ����
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///  ���ģʽΪ <c>CompressionMode.Compress</c>, <c>GZipStream</c> ��ʹ��Ĭ��ѹ���ȼ���  
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public GZipStream(Stream stream, CompressionMode mode)
            : base(stream, mode) {
				
        }

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� GZipStream �����ʵ����
        /// </summary>
        /// <remarks>
        ///
        /// ���ѹ��ģʽΪ <c>CompressionMode.Compress</c>, GZipStream �Զ�ʹ��Ĭ�ϵĵȼ��� "�߼�" �������� GZipStream ���رա�
        ///
        /// </remarks>
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : base(stream, mode, level) {
        }

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� GZipStream �����ʵ����
        /// </summary>
        /// 
        /// <remarks><para>
        ///  ������캯������Ӧ�ó�����Ҫ�������ִ�״̬ �� ���
        ///   <c>Close()</c> ����������, ��ôĬ�ϻ���Ҳ�رա� ����Щʱ���û���ϣ��ͬʱ�رգ������ض���ָ�� <paramref name="leaveOpen"/> ȷ�������ִ򿪡�
        /// </para>
        /// </remarks>
        /// 
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="leaveOpen">true ��������Ϊ��״̬������Ϊ false��</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {
        }

        /// <summary>
        /// ʹ��ָ�������� CompressionLevel ֵ�� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� GZipStream �����ʵ����
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///  ������캯������Ӧ�ó�����Ҫ�������ִ�״̬ �� ���
        ///   <c>Close()</c> ����������, ��ôĬ�ϻ���Ҳ�رա� ����Щʱ���û���ϣ��ͬʱ�رգ������ض���ָ�� <paramref name="leaveOpen"/> ȷ�������ִ򿪡�
        /// </para>
        ///
        /// <para>
        ///   �μ��������캯����
        /// </para>
        ///
        /// </remarks>
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="leaveOpen">true ��������Ϊ��״̬������Ϊ false��</param>
        /// <param name="level">��ʾѹ���ȼ���</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
            : base(stream, mode, level, leaveOpen) {
        }

        #endregion

        #region ����

		/// <summary>
		/// ʵ�ֹر�����
		/// </summary>
		protected override void CloseInternal() {
			base.CloseInternal();
			if (UsingStreamMode == StreamMode.Writer) {
				if (WantCompress) {
					// Emit the GZIP trailer: CRC32 and  size mod 2^32
					int c1 = _crc.Crc32Result;
					BaseStream.Write(BitConverter.GetBytes(c1), 0, 4);
					int c2 = (int)(_crc.TotalBytesRead & 0x00000000FFFFFFFF);
					BaseStream.Write(BitConverter.GetBytes(c2), 0, 4);
				} else {
					throw new ZlibException("��֧��д���ͬʱ��ѹ��");
				}
			} else if (UsingStreamMode == StreamMode.Reader) {
					if (!WantCompress) {
						// workitem 8501: handle edge case (decompress empty stream)
						if (_z.TotalBytesOut == 0L)
							return;

						// Read and potentially verify the GZIP trailer: CRC32 and  size mod 2^32
						byte[] trailer = new byte[8];

						// workitem 8679
						if (_z.AvailableBytesIn != 8) {
							// Make sure we have read to the end of the stream
							Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, _z.AvailableBytesIn);
							int bytesNeeded = 8 - _z.AvailableBytesIn;
							int bytesRead = BaseStream.Read(trailer,
														 _z.AvailableBytesIn,
														 bytesNeeded);
							if (bytesNeeded != bytesRead) {
								throw new ZlibException(String.Format("Э����� AvailableBytesIn={0}, ��Ҫ 8 ��",
																	  _z.AvailableBytesIn + bytesRead));
							}
						} else {
							Array.Copy(_z.InputBuffer, _z.NextIn, trailer, 0, trailer.Length);
						}


						int crc32_expected = BitConverter.ToInt32(trailer, 0);
						int crc32_actual = _crc.Crc32Result;
						int isize_expected = BitConverter.ToInt32(trailer, 4);
						int isize_actual = (int)(_z.TotalBytesOut & 0x00000000FFFFFFFF);

						if (crc32_actual != crc32_expected)
							throw new ZlibException(String.Format("�� GZIP �����ִ���� CRC32 У���롣 (Ӧ��{1:X8}�� ʵ��{0:X8}��)", crc32_actual, crc32_expected));

						if (isize_actual != isize_expected)
							throw new ZlibException(String.Format("�ṩ�� GZIP ���ȴ��� (Ӧ��{1}�� ʵ��{0}��)", isize_actual, isize_expected));

					} else {
						throw new ZlibException("��֧�ֶ�ȡ��ͬʱѹ����");
					}
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
            if (UsingStreamMode == StreamMode.Undefined) {

				Thrower.ThrowInvalidOperationExceptionIf(!BaseStream.CanRead, "�������ɶ���");
                
                UsingStreamMode = StreamMode.Reader;

                ZlibCodec.AvailableBytesIn = 0;

				#region ��ȡ��֤

					_gzipHeaderByteCount = 0;
					
					byte[] header = new byte[10];
					int nf = BaseStream.Read(header, 0, header.Length);


					if (nf == 0)
						return 0;

					if (nf != 10)
						throw new ZlibException("���ǺϷ��� GZIP ����");

					if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
						throw new ZlibException("����� GZIP ͷ��");

					Int32 timet = BitConverter.ToInt32(header, 4);
					_gzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
					_gzipHeaderByteCount += nf;
					if ((header[3] & 0x04) == 0x04) {
						// ��ȡ��չ�ֶ�
						nf = BaseStream.Read(header, 0, 2); // 2�ֽڳ���
						_gzipHeaderByteCount += nf;

						short extraLength = (short)(header[0] + header[1] * 256);
						byte[] extra = new byte[extraLength];
						nf = BaseStream.Read(extra, 0, extra.Length);
						if (nf != extraLength)
							throw new ZlibException("��ȡ GZIP ͷ�õ� EOF ��");
						_gzipHeaderByteCount += nf;
					}
					if ((header[3] & 0x08) == 0x08)
						_gzipFileName = ReadZeroTerminatedString();
					if ((header[3] & 0x10) == 0x010)
						_gzipComment = ReadZeroTerminatedString();
					if ((header[3] & 0x02) == 0x02)
						Read(_buf, 0, 1); // CRC16, ����

					#endregion


            }


            int n = base.Read(buffer, offset, count);
			_crc.SlurpBlock(buffer, offset, n);
            // Console.WriteLine("GZipStream::Read(buffer, off({0}), c({1}) = {2}", offset, count, n);
            // Console.WriteLine( Util.FormatByteArray(buffer, offset, n) );

            if (!_firstReadDone) {
                _firstReadDone = true;
                FileName = _gzipFileName;
                Comment = _gzipComment;
            }
            return n;
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
            if (UsingStreamMode == StreamMode.Undefined) {
                if (base.WantCompress) {

					#region EmitHeader

					byte[] commentBytes = (Comment == null) ? null : iso8859dash1.GetBytes(Comment);
					byte[] filenameBytes = (FileName == null) ? null : iso8859dash1.GetBytes(FileName);

					int cbLength = (Comment == null) ? 0 : commentBytes.Length + 1;
					int fnLength = (FileName == null) ? 0 : filenameBytes.Length + 1;

					int bufferLength = 10 + cbLength + fnLength;
					byte[] header = new byte[bufferLength];
					int i = 0;
					// ID
					header[i++] = 0x1F;
					header[i++] = 0x8B;

					// compression method
					header[i++] = 8;
					byte flag = 0;
					if (Comment != null)
						flag ^= 0x10;
					if (FileName != null)
						flag ^= 0x8;

					// flag
					header[i++] = flag;

					// mtime
					if (!LastModified.HasValue)
						LastModified = DateTime.Now;
					System.TimeSpan delta = LastModified.Value - _unixEpoch;
					Int32 timet = (Int32)delta.TotalSeconds;
					Array.Copy(BitConverter.GetBytes(timet), 0, header, i, 4);
					i += 4;

					// xflg
					header[i++] = 0;    // this field is totally useless
					// OS
					header[i++] = 0xFF; // 0xFF == unspecified

					// extra field length - only if FEXTRA is set, which it is not.
					//header[i++]= 0;
					//header[i++]= 0;

					// filename
					if (fnLength != 0) {
						Array.Copy(filenameBytes, 0, header, i, fnLength - 1);
						i += fnLength - 1;
						header[i++] = 0; // terminate
					}

					// comment
					if (cbLength != 0) {
						Array.Copy(commentBytes, 0, header, i, cbLength - 1);
						i += cbLength - 1;
						header[i++] = 0; // terminate
					}

					BaseStream.Write(header, 0, header.Length);

					_headerByteCount = header.Length; // д�Ĵ�С

					#endregion

                } else {
                    throw new InvalidOperationException();
                }
            }

			_crc.SlurpBlock(buffer, offset, count);
            base.Write(buffer, offset, count);
        }

        #endregion

        #region GZip ����

        private string ReadZeroTerminatedString() {
            var list = new System.Collections.Generic.List<byte>();
            bool done = false;
            do {
                // workitem 7740
                int n = BaseStream.Read(_buf, 0, 1);
                if (n != 1)
					throw new ZlibException("��ȡ GZIP ͷ�õ� EOF ��");
                else {
                    if (_buf[0] == 0)
                        done = true;
                    else
                        list.Add(_buf[0]);
                }
            } while (!done);
            byte[] a = list.ToArray();
            return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
        }

        /// <summary>
        /// ƫ��ʱ�䡣
        /// </summary>
        static readonly DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// iso 8859 ���롣
        /// </summary>
        static readonly System.Text.Encoding iso8859dash1 = System.Text.Encoding.GetEncoding("iso-8859-1");

		#endregion

		#region ��̬

		/// <summary>
		/// ʹ�� GZip ѹ��һ���ַ��������顣
		/// </summary>
		/// 
		/// <remarks>
		///  �� <see cref="GZipStream.UncompressString(byte[])"/> ��ѹ��
		/// </remarks>
		///
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
		/// <param name="s"> Ҫѹ�����ַ����� </param>
		/// <returns>ѹ�����ֽڵ����顣</returns>
		public static byte[] CompressString(string s) {
			using (var ms = new MemoryStream()) {
				System.IO.Stream compressor =
					new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZipBaseStream.CompressString(s, compressor);
				return ms.ToArray();
			}
		}


		/// <summary>
		/// ʹ�� DEFLATE ѹ��һ�����鵽���顣
		/// </summary>
		///
		/// <remarks>
		///   ʹ�� <see cref="GZipStream.UncompressBuffer(byte[])"/> ��ѹ��
		/// </remarks>
		///
		/// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		///
		/// <param name="b">
		/// ѹ���Ļ��档
		/// </param>
		///
		/// <returns>ѹ���Ľ����</returns>
		public static byte[] CompressBuffer(byte[] b) {
			using (var ms = new MemoryStream()) {
				System.IO.Stream compressor =
					new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

				ZipBaseStream.CompressBuffer(b, compressor);
				return ms.ToArray();
			}
		}


		/// <summary>
		/// ʹ��  DEFLATE ��ѹ�ֽڵ����顣
		/// </summary>
		///
		/// <seealso cref="GZipStream.CompressString(String)">GZipStream.CompressString(String)</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		///
		/// <param name="compressed">
		/// ѹ�������顣
		/// </param>
		///
		/// <returns>��ѹ����ַ�����</returns>
		public static String UncompressString(byte[] compressed) {
			using (var input = new MemoryStream(compressed)) {
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				return ZipBaseStream.UncompressString(compressed, decompressor);
			}
		}


		/// <summary>
		/// ʹ��  DEFLATE ��ѹ�ֽڵ����顣
		/// </summary>
		///
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		///
		/// <param name="compressed">
		/// ѹ�������顣
		/// </param>
		///
		/// <returns>��ѹ������顣</returns>
		public static byte[] UncompressBuffer(byte[] compressed) {
			using (var input = new System.IO.MemoryStream(compressed)) {
				System.IO.Stream decompressor =
					new GZipStream(input, CompressionMode.Decompress);

				return ZipBaseStream.UncompressBuffer(compressed, decompressor);
			}
		}

		#endregion


    }
}
