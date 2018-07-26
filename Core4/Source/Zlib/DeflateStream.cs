// DeflateStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 Dino Chiesa.
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
// Time-stamp: <2010-February-05 08:49:04>
//
// ------------------------------------------------------------------
//
// This module defines the DeflateStream class, which can be used as a replacement for
// the System.IO.Compression.DeflateStream class in the .NET BCL.
//
// ------------------------------------------------------------------
//   edited by xuld  

using System;
using System.IO;

namespace Py.Zip.Zlib {

    /// <summary>
    /// 包含压缩算法的流。
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
	/// 此类表示 Deflate 算法，这是无损压缩和解压缩文件的行业标准算法。它结合了 LZ77 算法和霍夫曼编码。只能使用以前绑定的中间存储量来产生或使用数据，即使对于任意长度的、按顺序出现的输入数据流也是如此。这种格式可以通过不涉及专利使用权的方式轻松实现。有关更多信息，请参见 RFC 1951。" <see href="http://www.ietf.org/rfc/rfc1951.txt"> DEFLATE Compressed Data Format Specification version 1.3 </see>（DEFLATE 压缩数据格式规范版本 1.3）。"
    /// </para>
	/// 
	/// <para>
	/// 此类原本并不提供用来向 .zip 存档中添加文件或从 .zip 存档中提取文件的功能。
	/// </para>
    ///
	/// <para>
	/// <see cref="GZipStream"/> 类使用 Gzip 数据格式，这种格式包括一个用于监测数据损坏的循环冗余校验值。 Gzip 数据格式与 <c>DeflateStream</c>  类使用相同的压缩算法。 
	/// </para>
	/// 
    /// <para>
	/// <c>DeflateStream</c> 和 <see cref="GZipStream"/> 中的压缩功能作为流公开。 由于数据是以逐字节的方式读取的，因此无法通过进行多次传递来确定压缩整个文件或大型数据块的最佳方法。对于未压缩的数据源，最好使用 <c>DeflateStream</c> 和 <see cref="GZipStream"/> 类。 如果源数据已压缩，则使用这些类时实际上可能会增加流的大小。
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="GZipStream" />
    /// <example>
    /// 这个例子演示了使用 DeflateStream 压缩一个文件，然后写到至另一个文件。
    /// <code>
    /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress)){
    ///     using (var raw = System.IO.File.Create(fileToCompress + ".deflated")){
    ///         using (Stream compressor = new DeflateStream(raw, CompressionMode.Compress)){
    ///             byte[] buffer = new byte[WORKING_BUFFER_SIZE];
    ///             int n;
    ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0){
    ///                 compressor.Write(buffer, 0, n);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// <code lang="VB">
    /// Using input As Stream = File.OpenRead(fileToCompress)
    ///		Using raw As FileStream = File.Create(fileToCompress &amp; ".deflated")
    ///			Using compressor As Stream = New DeflateStream(raw, CompressionMode.Compress)
    ///				Dim buffer As Byte() = New Byte(4096) {}
    ///				Dim n As Integer = -1
    ///				Do While (n &lt;&gt; 0)
    ///					If (n &gt; 0) Then
    ///						compressor.Write(buffer, 0, n)
    ///					End If
    ///					n = input.Read(buffer, 0, buffer.Length)
    ///				Loop
    ///			End Using
    ///		End Using
    /// End Using
    /// </code>
    /// </example>
    public class DeflateStream : ZipBaseStream {

        #region 变量

        #endregion

        #region 初始化

        /// <summary>
        /// 使用指定的流和 CompressionMode 值初始化 <see cref="Py.Zip.Zlib.DeflateStream"/> 的新实例。
        /// </summary>
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <remarks>
        /// 如果压缩模式为 <c>CompressionMode.Compress</c>, DeflateStream 自动使用默认的等级。 "高级" 的流会随 DeflateStream 而关闭。
        /// </remarks>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public DeflateStream(Stream stream, CompressionMode mode)
            : base(stream, mode) {
        }

        /// <summary>
        /// 使用指定的流, CompressionLevel 值和 CompressionMode 值，初始化 <see cref="Py.Zip.Zlib.DeflateStream"/> 的新实例。
        /// </summary>
        /// <remarks>
        /// 如果压缩模式为 <c>CompressionMode.Compress</c>, DeflateStream 自动使用默认的等级。 "高级" 的流会随 DeflateStream 而关闭。
        /// </remarks>
        /// <param name="stream">用来写入或读取的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="level">压缩等级。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : base(stream, mode, level) {
        }

        /// <summary>
        /// 使用指定的流和 CompressionMode 值以及一个指定是否将流保留为打开状态的值，初始化 <see cref="Py.Zip.Zlib.DeflateStream"/> 的新实例。
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
        ///   这个 <c>DeflateStream</c> 使用默认压缩等级。
        /// </para>
        ///
        /// <para>
        ///   参见其他构造函数。
        /// </para>
        /// </remarks>
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="leaveOpen">如果 true ，则保持基类的流打开。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {
        }

        /// <summary>
        /// 使用指定的流, CompressionLevel 值和 CompressionMode 值以及一个指定是否将流保留为打开状态的值，初始化 <see cref="Py.Zip.Zlib.DeflateStream"/> 的新实例。并额外指明基类流是否在当前流关闭后关闭。
        /// </summary>
        /// <remarks>
        /// <para>
        ///  这个构造函数用在应用程序需要基流保持打开状态 。 如果
        ///   <c>Close()</c> 方法被调用, 那么默认基流也关闭。 在有些时候，用户不希望同时关闭，比如重读。指定 <paramref name="leaveOpen"/> 确保流保持打开。
        /// </para>
        /// <para>
        ///   参见其他构造函数。
        /// </para>
        /// </remarks>
        /// <param name="stream">要压缩或解压缩的流。</param>
        /// <param name="mode">指示当前操作是压缩或解压。</param>
        /// <param name="leaveOpen">true 将流保留为打开状态，否则为 false。</param>
        /// <param name="level">使用的压缩等级。</param>
        /// <exception cref="ArgumentNullException">stream 为 null。</exception>
        /// <exception cref="InvalidOperationException">stream 访问权限为 ReadOnly，mode 值为 Compress。</exception>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
            :base(stream, mode, level, leaveOpen){
        }

        #endregion

        #region 属性

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
						return _z.TotalBytesOut;
                    case StreamMode.Reader:
                        return _z.TotalBytesIn;
                    default:
                        return 0;
                }
            }
            set { throw new NotSupportedException(); }
        }

        #endregion

		#region 工具

        /// <summary>
        /// 使用 DEFLATE (RFC 1951) 压缩一个字符串到数组。
        /// </summary>
        ///
        /// <remarks>
        ///  用 <see cref="DeflateStream.UncompressString(byte[])"/> 解压。
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
        /// <param name="s"> 要压缩的字符串。 </param>
        /// <returns>压缩的字节的数组。</returns>
        public static byte[] CompressString(string s) {
            using (var ms = new System.IO.MemoryStream()) {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZipBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 使用 DEFLATE 压缩一个数组到数组。
        /// </summary>
        ///
        /// <remarks>
        ///   使用 <see cref="DeflateStream.UncompressBuffer(byte[])"/> 解压。
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.CompressString(string)">DeflateStream.CompressString(string)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
        ///
        /// <param name="b">
        /// 压缩的缓存。
        /// </param>
        ///
        /// <returns>压缩的结果。</returns>
        public static byte[] CompressBuffer(byte[] b) {
            using (var ms = new System.IO.MemoryStream()) {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

                ZipBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 使用  DEFLATE 解压字节的数组。
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressString(String)">DeflateStream.CompressString(String)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
        ///
        /// <param name="compressed">
        /// 压缩的数组。
        /// </param>
        ///
        /// <returns>解压后的字符串。</returns>
        public static string UncompressString(byte[] compressed) {
            using (var input = new System.IO.MemoryStream(compressed)) {
                System.IO.Stream decompressor =
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZipBaseStream.UncompressString(compressed, decompressor);
            }
        }

        /// <summary>
        /// 使用  DEFLATE 解压字节的数组。
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
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
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZipBaseStream.UncompressBuffer(compressed, decompressor);
            }
		}

		#endregion

    }

}

