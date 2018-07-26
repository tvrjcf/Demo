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
    /// ����ѹ���㷨������
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
	/// �����ʾ Deflate �㷨����������ѹ���ͽ�ѹ���ļ�����ҵ��׼�㷨��������� LZ77 �㷨�ͻ��������롣ֻ��ʹ����ǰ�󶨵��м�洢����������ʹ�����ݣ���ʹ�������ⳤ�ȵġ���˳����ֵ�����������Ҳ����ˡ����ָ�ʽ����ͨ�����漰ר��ʹ��Ȩ�ķ�ʽ����ʵ�֡��йظ�����Ϣ����μ� RFC 1951��" <see href="http://www.ietf.org/rfc/rfc1951.txt"> DEFLATE Compressed Data Format Specification version 1.3 </see>��DEFLATE ѹ�����ݸ�ʽ�淶�汾 1.3����"
    /// </para>
	/// 
	/// <para>
	/// ����ԭ�������ṩ������ .zip �浵������ļ���� .zip �浵����ȡ�ļ��Ĺ��ܡ�
	/// </para>
    ///
	/// <para>
	/// <see cref="GZipStream"/> ��ʹ�� Gzip ���ݸ�ʽ�����ָ�ʽ����һ�����ڼ�������𻵵�ѭ������У��ֵ�� Gzip ���ݸ�ʽ�� <c>DeflateStream</c>  ��ʹ����ͬ��ѹ���㷨�� 
	/// </para>
	/// 
    /// <para>
	/// <c>DeflateStream</c> �� <see cref="GZipStream"/> �е�ѹ��������Ϊ�������� ���������������ֽڵķ�ʽ��ȡ�ģ�����޷�ͨ�����ж�δ�����ȷ��ѹ�������ļ���������ݿ����ѷ���������δѹ��������Դ�����ʹ�� <c>DeflateStream</c> �� <see cref="GZipStream"/> �ࡣ ���Դ������ѹ������ʹ����Щ��ʱʵ���Ͽ��ܻ��������Ĵ�С��
    /// </para>
    ///
    /// </remarks>
    ///
    /// <seealso cref="GZipStream" />
    /// <example>
    /// ���������ʾ��ʹ�� DeflateStream ѹ��һ���ļ���Ȼ��д������һ���ļ���
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

        #region ����

        #endregion

        #region ��ʼ��

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ��ʼ�� <see cref="Py.Zip.Zlib.DeflateStream"/> ����ʵ����
        /// </summary>
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <remarks>
        /// ���ѹ��ģʽΪ <c>CompressionMode.Compress</c>, DeflateStream �Զ�ʹ��Ĭ�ϵĵȼ��� "�߼�" �������� DeflateStream ���رա�
        /// </remarks>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public DeflateStream(Stream stream, CompressionMode mode)
            : base(stream, mode) {
        }

        /// <summary>
        /// ʹ��ָ������, CompressionLevel ֵ�� CompressionMode ֵ����ʼ�� <see cref="Py.Zip.Zlib.DeflateStream"/> ����ʵ����
        /// </summary>
        /// <remarks>
        /// ���ѹ��ģʽΪ <c>CompressionMode.Compress</c>, DeflateStream �Զ�ʹ��Ĭ�ϵĵȼ��� "�߼�" �������� DeflateStream ���رա�
        /// </remarks>
        /// <param name="stream">����д����ȡ������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="level">ѹ���ȼ���</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : base(stream, mode, level) {
        }

        /// <summary>
        /// ʹ��ָ�������� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� <see cref="Py.Zip.Zlib.DeflateStream"/> ����ʵ����
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
        ///   ��� <c>DeflateStream</c> ʹ��Ĭ��ѹ���ȼ���
        /// </para>
        ///
        /// <para>
        ///   �μ��������캯����
        /// </para>
        /// </remarks>
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="leaveOpen">��� true ���򱣳ֻ�������򿪡�</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {
        }

        /// <summary>
        /// ʹ��ָ������, CompressionLevel ֵ�� CompressionMode ֵ�Լ�һ��ָ���Ƿ�������Ϊ��״̬��ֵ����ʼ�� <see cref="Py.Zip.Zlib.DeflateStream"/> ����ʵ����������ָ���������Ƿ��ڵ�ǰ���رպ�رա�
        /// </summary>
        /// <remarks>
        /// <para>
        ///  ������캯������Ӧ�ó�����Ҫ�������ִ�״̬ �� ���
        ///   <c>Close()</c> ����������, ��ôĬ�ϻ���Ҳ�رա� ����Щʱ���û���ϣ��ͬʱ�رգ������ض���ָ�� <paramref name="leaveOpen"/> ȷ�������ִ򿪡�
        /// </para>
        /// <para>
        ///   �μ��������캯����
        /// </para>
        /// </remarks>
        /// <param name="stream">Ҫѹ�����ѹ��������</param>
        /// <param name="mode">ָʾ��ǰ������ѹ�����ѹ��</param>
        /// <param name="leaveOpen">true ��������Ϊ��״̬������Ϊ false��</param>
        /// <param name="level">ʹ�õ�ѹ���ȼ���</param>
        /// <exception cref="ArgumentNullException">stream Ϊ null��</exception>
        /// <exception cref="InvalidOperationException">stream ����Ȩ��Ϊ ReadOnly��mode ֵΪ Compress��</exception>
        public DeflateStream(System.IO.Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
            :base(stream, mode, level, leaveOpen){
        }

        #endregion

        #region ����

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

		#region ����

        /// <summary>
        /// ʹ�� DEFLATE (RFC 1951) ѹ��һ���ַ��������顣
        /// </summary>
        ///
        /// <remarks>
        ///  �� <see cref="DeflateStream.UncompressString(byte[])"/> ��ѹ��
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressString(string)">GZipStream.CompressString(string)</seealso>
        /// <param name="s"> Ҫѹ�����ַ����� </param>
        /// <returns>ѹ�����ֽڵ����顣</returns>
        public static byte[] CompressString(string s) {
            using (var ms = new System.IO.MemoryStream()) {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZipBaseStream.CompressString(s, compressor);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// ʹ�� DEFLATE ѹ��һ�����鵽���顣
        /// </summary>
        ///
        /// <remarks>
        ///   ʹ�� <see cref="DeflateStream.UncompressBuffer(byte[])"/> ��ѹ��
        /// </remarks>
        ///
        /// <seealso cref="DeflateStream.CompressString(string)">DeflateStream.CompressString(string)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.CompressBuffer(byte[])">GZipStream.CompressBuffer(byte[])</seealso>
        ///
        /// <param name="b">
        /// ѹ���Ļ��档
        /// </param>
        ///
        /// <returns>ѹ���Ľ����</returns>
        public static byte[] CompressBuffer(byte[] b) {
            using (var ms = new System.IO.MemoryStream()) {
                System.IO.Stream compressor =
                    new DeflateStream(ms, CompressionMode.Compress, CompressionLevel.BestCompression);

                ZipBaseStream.CompressBuffer(b, compressor);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// ʹ��  DEFLATE ��ѹ�ֽڵ����顣
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressString(String)">DeflateStream.CompressString(String)</seealso>
        /// <seealso cref="DeflateStream.UncompressBuffer(byte[])">DeflateStream.UncompressBuffer(byte[])</seealso>
        /// <seealso cref="GZipStream.UncompressString(byte[])">GZipStream.UncompressString(byte[])</seealso>
        ///
        /// <param name="compressed">
        /// ѹ�������顣
        /// </param>
        ///
        /// <returns>��ѹ����ַ�����</returns>
        public static string UncompressString(byte[] compressed) {
            using (var input = new System.IO.MemoryStream(compressed)) {
                System.IO.Stream decompressor =
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZipBaseStream.UncompressString(compressed, decompressor);
            }
        }

        /// <summary>
        /// ʹ��  DEFLATE ��ѹ�ֽڵ����顣
        /// </summary>
        ///
        /// <seealso cref="DeflateStream.CompressBuffer(byte[])">DeflateStream.CompressBuffer(byte[])</seealso>
        /// <seealso cref="DeflateStream.UncompressString(byte[])">DeflateStream.UncompressString(byte[])</seealso>
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
                    new DeflateStream(input, CompressionMode.Decompress);

                return ZipBaseStream.UncompressBuffer(compressed, decompressor);
            }
		}

		#endregion

    }

}

