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
    /// 提供用于压缩和解压缩流的方法和属性。
    /// </summary>
    /// <remarks>
    /// <para>此类表示 GZip 数据格式，它使用无损压缩和解压缩文件的行业标准算法。这种格式包括一个检测数据损坏的循环冗余校验值。GZip 数据格式使用的算法与 DeflateStream 类的算法相同，但它可以扩展以使用其他压缩格式。 这种格式可以通过不涉及专利使用权的方式轻松实现。此类不能用于压缩大于 4 GB 的文件。更多资料见 <see href="http://www.ietf.org/rfc/rfc1952.txt">IETF RFC 1952</see>, "GZIP 压缩文档格式  4.3 版"。</para>
    /// <para>
    /// 可以使用许多常见的压缩工具对写入到扩展名为 .gz 的文件的压缩 GZipStream 对象进行解压缩；但是，此类原本并不提供用于向 .zip 存档中添加文件或从 .zip 存档中提取文件的功能。 
	/// </para>
    /// <para>
    ///   一个 <c>GZipStream</c> 可以读或写，但不能同时。
    /// </para>
    /// <para>
    ///   虽然 GZIP 格式允许数据来自不同的文件, 这个流只操作一个 GZIP 格式, 表现为一个文件。
    /// </para>
	/// 
    /// <para>
	/// <see cref="DeflateStream"/> 和 <c>GZipStream</c> 中的压缩功能作为流公开。 由于数据是以逐字节的方式读取的，因此无法通过进行多次传递来确定压缩整个文件或大型数据块的最佳方法。对于未压缩的数据源，最好使用 <see cref="DeflateStream"/> 和 <c>GZipStream</c> 类。 如果源数据已压缩，则使用这些类时实际上可能会增加流的大小。
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="GZipStream" />
    ///
    /// <example>
    /// 这个例子演示了如何使用 GZipStream 创建一个压缩流。
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
    /// 这个例子演示了如何使用 GZipStream 解压一个压缩流。
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
        // GZip 格式
        // 来源: http://tools.ietf.org/html/rfc1952
        //
        //  文件头:              2 bytes    1F 8B
        //  压缩方式               1 byte     8= DEFLATE (其它不支持 )
        //  标记                   1 byte     位标记 (见表)
        //  mtime                4 bytes    time_t (从 1970/1/1  UTC 开始)
        //  xflg                 1 byte     2 = 最大努力压缩 , 4 = 最大速度压缩 (肯能忽视)
        //  OS(操作系统)         1 byte     原单元的操作系统. 压缩时设置 0xFF 。
        //  扩展字段长度           2 bytes    可选 - 仅当 FEXTRA 设置后有效。
        //  扩展字段               varies
        //  文件名                 varies     可选 - 如果 FNAME 设置有效。 空值可用。 ISO-8859-1.
        //  文件注释               varies     可选 - 如果 FCOMMENT 设置有效。 空值可用 ISO-8859-1.
        //  crc16                1 byte     可选 - 目前只有 FHCRC 设置有效。 
        //  压缩数据               varies
        //  CRC32                4 bytes
        //  isize                4 bytes    文件大小模 2^32 的余数
        //
        //     标记
        //                bit 0   FTEXT - 指示文件时 ASCII 文件 (可以安全忽略)
        //                bit 1   FHCRC - 文件头有 CRC16 检验码。
        //                bit 2   FEXTRA - 扩展字段存在。
        //                bit 3   FNAME - 文件名。编码: ISO-8859-1.
        //                bit 4   FCOMMENT  - 文件注释。 编码: ISO-8859-1
        //                bit 5   保留，供将来使用。
        //                bit 6   保留，供将来使用。
        //                bit 7   保留，供将来使用。
        //
        // consumption时
        // 额外的字段可以忽视。
        //
        // 生产时
        // 除了 OS 返回 255， 其它所有可选字段返回 0。
        //

        #region 私有字段

        /// <summary>
        /// 文件头大小。
        /// </summary>
        int _headerByteCount;

        /// <summary>
        /// 第一次读取完成。
        /// </summary>
        bool _firstReadDone;

        /// <summary>
        /// 文件名。
        /// </summary>
        string _fileName;

        /// <summary>
        /// 注释。
        /// </summary>
        string _comment;

		/// <summary>
		/// 用于 GZIP 的文件名。
		/// </summary>
        string _gzipFileName;
		
		/// <summary>
		/// 用于 GZIP 的注释。
		/// </summary>
		string _gzipComment;
		
		/// <summary>
		/// 当前的检验码。
		/// </summary>
		Py.Algorithm.Crc32 _crc = new Algorithm.Crc32();

		/// <summary>
		/// 用于 GZIP 的文件访问实际。
		/// </summary>
        DateTime _gzipMtime;

		/// <summary>
		/// 文件头大小。
		/// </summary>
		int _gzipHeaderByteCount;

		/// <summary>
		/// 缓存。
		/// </summary>
		byte[] _buf = new byte[1];

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置 Gzip 的注释。
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   格式 GZIP 允许每个文件都包括一个注释。 注释的编码是 ISO-8859-1
        ///   代码页。 要写入 GZIP 流一个注释, 在 <c>Write()</c> 前设置此属性。
        /// </para>
        ///
        /// <para>
        ///   如使用 <c>GZipStream</c> 解压, 可以在第一次 <c>Read()</c> 后得到注释。  如果在
        ///   GZIP 字节流没有注释, 注释属性返回 <c>null</c>
        ///   (在 VB 中为<c>Nothing</c>)。
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">流已关闭。</exception>
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
        /// 当前文件的文件名。
        /// </summary>
        /// <exception cref="ObjectDisposedException">流已关闭。</exception>
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
                    throw new IOException("非法文件名。");
                if (_fileName.IndexOf('\\') != -1) {
                    // trim any leading path
                    _fileName = Path.GetFileName(_fileName);
                }
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
		/// 获取当前的 CRC 检验码。
		/// </summary>
		public int Crc32 {
			get {
				return _crc.Crc32Result;
			}
		}

        /// <summary>
        /// 获取或设置最后修改时间。
        /// </summary>
        public DateTime? LastModified {
            get;
            set;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 使用指定的流和 CompressionMode 值初始化 GZipStream 类的新实例。
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///  如果模式为 <c>CompressionMode.Compress</c>, <c>GZipStream</c> 会使用默认压缩等级。  
        /// </para>
        ///
        /// </remarks>
        ///
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public GZipStream(Stream stream, CompressionMode mode)
            : base(stream, mode) {
				
        }

        /// <summary>
        /// 使用指定的流和 CompressionMode 值以及一个指定是否将流保留为打开状态的值来初始化 GZipStream 类的新实例。
        /// </summary>
        /// <remarks>
        ///
        /// 如果压缩模式为 <c>CompressionMode.Compress</c>, GZipStream 自动使用默认的等级。 "高级" 的流会随 GZipStream 而关闭。
        ///
        /// </remarks>
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : base(stream, mode, level) {
        }

        /// <summary>
        /// 使用指定的流和 CompressionMode 值以及一个指定是否将流保留为打开状态的值来初始化 GZipStream 类的新实例。
        /// </summary>
        /// 
        /// <remarks><para>
        ///  这个构造函数用在应用程序需要基流保持打开状态 。 如果
        ///   <c>Close()</c> 方法被调用, 那么默认基流也关闭。 在有些时候，用户不希望同时关闭，比如重读。指定 <paramref name="leaveOpen"/> 确保流保持打开。
        /// </para>
        /// </remarks>
        /// 
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="leaveOpen">true 将流保留为打开状态，否则为 false。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {
        }

        /// <summary>
        /// 使用指定的流， CompressionLevel 值和 CompressionMode 值以及一个指定是否将流保留为打开状态的值来初始化 GZipStream 类的新实例。
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///  这个构造函数用在应用程序需要基流保持打开状态 。 如果
        ///   <c>Close()</c> 方法被调用, 那么默认基流也关闭。 在有些时候，用户不希望同时关闭，比如重读。指定 <paramref name="leaveOpen"/> 确保流保持打开。
        /// </para>
        ///
        /// <para>
        ///   参见其他构造函数。
        /// </para>
        ///
        /// </remarks>
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="leaveOpen">true 将流保留为打开状态，否则为 false。</param>
        /// <param name="level">表示压缩等级。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
            : base(stream, mode, level, leaveOpen) {
        }

        #endregion

        #region 方法

		/// <summary>
		/// 实现关闭流。
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
					throw new ZlibException("不支持写入的同时解压。");
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
								throw new ZlibException(String.Format("协议错误 AvailableBytesIn={0}, 需要 8 。",
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
							throw new ZlibException(String.Format("在 GZIP 流发现错误的 CRC32 校验码。 (应该{1:X8}； 实际{0:X8}。)", crc32_actual, crc32_expected));

						if (isize_actual != isize_expected)
							throw new ZlibException(String.Format("提供的 GZIP 长度错误。 (应该{1}； 实际{0}。)", isize_actual, isize_expected));

					} else {
						throw new ZlibException("不支持读取的同时压缩。");
					}
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
            if (UsingStreamMode == StreamMode.Undefined) {

				Thrower.ThrowInvalidOperationExceptionIf(!BaseStream.CanRead, "此流不可读。");
                
                UsingStreamMode = StreamMode.Reader;

                ZlibCodec.AvailableBytesIn = 0;

				#region 读取验证

					_gzipHeaderByteCount = 0;
					
					byte[] header = new byte[10];
					int nf = BaseStream.Read(header, 0, header.Length);


					if (nf == 0)
						return 0;

					if (nf != 10)
						throw new ZlibException("不是合法的 GZIP 流。");

					if (header[0] != 0x1F || header[1] != 0x8B || header[2] != 8)
						throw new ZlibException("错误的 GZIP 头。");

					Int32 timet = BitConverter.ToInt32(header, 4);
					_gzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
					_gzipHeaderByteCount += nf;
					if ((header[3] & 0x04) == 0x04) {
						// 读取扩展字段
						nf = BaseStream.Read(header, 0, 2); // 2字节长度
						_gzipHeaderByteCount += nf;

						short extraLength = (short)(header[0] + header[1] * 256);
						byte[] extra = new byte[extraLength];
						nf = BaseStream.Read(extra, 0, extra.Length);
						if (nf != extraLength)
							throw new ZlibException("读取 GZIP 头得到 EOF 。");
						_gzipHeaderByteCount += nf;
					}
					if ((header[3] & 0x08) == 0x08)
						_gzipFileName = ReadZeroTerminatedString();
					if ((header[3] & 0x10) == 0x010)
						_gzipComment = ReadZeroTerminatedString();
					if ((header[3] & 0x02) == 0x02)
						Read(_buf, 0, 1); // CRC16, 忽视

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

					_headerByteCount = header.Length; // 写的大小

					#endregion

                } else {
                    throw new InvalidOperationException();
                }
            }

			_crc.SlurpBlock(buffer, offset, count);
            base.Write(buffer, offset, count);
        }

        #endregion

        #region GZip 方法

        private string ReadZeroTerminatedString() {
            var list = new System.Collections.Generic.List<byte>();
            bool done = false;
            do {
                // workitem 7740
                int n = BaseStream.Read(_buf, 0, 1);
                if (n != 1)
					throw new ZlibException("读取 GZIP 头得到 EOF 。");
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
        /// 偏移时间。
        /// </summary>
        static readonly DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// iso 8859 编码。
        /// </summary>
        static readonly System.Text.Encoding iso8859dash1 = System.Text.Encoding.GetEncoding("iso-8859-1");

		#endregion

		#region 静态

		/// <summary>
		/// 使用 GZip 压缩一个字符串到数组。
		/// </summary>
		/// 
		/// <remarks>
		///  用 <see cref="GZipStream.UncompressString(byte[])"/> 解压。
		/// </remarks>
		///
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
		/// <param name="s"> 要压缩的字符串。 </param>
		/// <returns>压缩的字节的数组。</returns>
		public static byte[] CompressString(string s) {
			using (var ms = new MemoryStream()) {
				System.IO.Stream compressor =
					new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
				ZipBaseStream.CompressString(s, compressor);
				return ms.ToArray();
			}
		}


		/// <summary>
		/// 使用 DEFLATE 压缩一个数组到数组。
		/// </summary>
		///
		/// <remarks>
		///   使用 <see cref="GZipStream.UncompressBuffer(byte[])"/> 解压。
		/// </remarks>
		///
		/// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		///
		/// <param name="b">
		/// 压缩的缓存。
		/// </param>
		///
		/// <returns>压缩的结果。</returns>
		public static byte[] CompressBuffer(byte[] b) {
			using (var ms = new MemoryStream()) {
				System.IO.Stream compressor =
					new GZipStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

				ZipBaseStream.CompressBuffer(b, compressor);
				return ms.ToArray();
			}
		}


		/// <summary>
		/// 使用  DEFLATE 解压字节的数组。
		/// </summary>
		///
		/// <seealso cref="GZipStream.CompressString(String)">GZipStream.CompressString(String)</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		///
		/// <param name="compressed">
		/// 压缩的数组。
		/// </param>
		///
		/// <returns>解压后的字符串。</returns>
		public static String UncompressString(byte[] compressed) {
			using (var input = new MemoryStream(compressed)) {
				Stream decompressor = new GZipStream(input, CompressionMode.Decompress);
				return ZipBaseStream.UncompressString(compressed, decompressor);
			}
		}


		/// <summary>
		/// 使用  DEFLATE 解压字节的数组。
		/// </summary>
		///
		/// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
		/// <seealso cref="GZipStream.UncompressBuffer(byte[])">GZipStream.UncompressBuffer(byte[])</seealso>
		///
		/// <param name="compressed">
		/// 压缩的数组。
		/// </param>
		///
		/// <returns>解压后的数组。</returns>
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
