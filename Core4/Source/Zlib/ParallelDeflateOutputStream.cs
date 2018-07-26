 //       #define Zip_Trace

// ParallelDeflateOutputStream.cs
// ------------------------------------------------------------------
//
// A DeflateStream that does compression only, it uses a
// divide-and-conquer approach with multiple threads to exploit multiple
// CPUs for the DEFLATE computation.
//
// last saved:
// Time-stamp: <2010-January-20 19:24:58>
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2010 by Dino Chiesa
// All rights reserved!
//
// ------------------------------------------------------------------

using System;
using System.Threading;
using System.IO;
using Py.RunTime;


namespace Py.Zip.Zlib {

	/// <summary>
	/// 工作项目。
	/// </summary>
	class WorkItem {

		/// <summary>
		/// 表示当前的状态。
		/// </summary>
		public enum Status {

			/// <summary>
			/// 无。
			/// </summary>
			None = 0,

			/// <summary>
			/// 正则填写。
			/// </summary>
			Filling = 1,

			/// <summary>
			/// 填写完成。
			/// </summary>
			Filled = 2,

			/// <summary>
			/// 正压缩。
			/// </summary>
			Compressing = 3,

			/// <summary>
			/// 压缩完成。
			/// </summary>
			Compressed = 4,

			/// <summary>
			/// 正在写入。
			/// </summary>
			Writing = 5,

			/// <summary>
			/// 写入。
			/// </summary>
			Done = 6
		}

		/// <summary>
		/// 缓存大小。
		/// </summary>
		public byte[] Buffer;

		/// <summary>
		/// 压缩的数目。
		/// </summary>
		public byte[] Compressed;

		/// <summary>
		/// 状态。
		/// </summary>
		public Status CurrentStatus;

		/// <summary>
		/// 检验码。
		/// </summary>
		public int Crc;

		/// <summary>
		/// 索引。
		/// </summary>
		public int Index;

		/// <summary>
		/// 可以使用的输入字节。
		/// </summary>
		public int InputBytesAvailable;

		/// <summary>
		/// 可以压缩的部分。
		/// </summary>
		public int CompressedBytesAvailable;

		/// <summary>
		/// 解码器。
		/// </summary>
		public ZlibCodec Compressor;

		/// <summary>
		/// 初始化 <see cref="Py.Zip.Zlib.WorkItem"/> 的新实例。
		/// </summary>
		/// <param name="size">大小。</param>
		/// <param name="compressLevel">压缩等级。</param>
		/// <param name="strategy">策略。</param>
		public WorkItem(int size, CompressionLevel compressLevel, CompressionStrategy strategy) {
			Buffer = new byte[size];
			// alloc 5 bytes overhead for every block (margin of safety= 2)
			int n = size + ((size / 32768) + 1) * 5 * 2;
			Compressed = new byte[n];

			CurrentStatus = (int)Status.None;
			Compressor = new ZlibCodec();
			Compressor.InitializeDeflate(compressLevel, false);
			Compressor.OutputBuffer = Compressed;
			Compressor.InputBuffer = Buffer;
		}
	}

	/// <summary>
	/// 支持在多线程使用的使用 Deflate 算法的压缩流。
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// 这个类只能用在压缩和写入。
	/// </para>
	///
	/// <para>
	///  这个类和 <see cref="Py.Zip.Zlib.DeflateStream"/> 类似, 但这个类追加了支持多线程的部分。
	///  在多内核的计算机上，使用这个类可以明显提交效率。特别是长度大的数据流。 一般 文件大于 10 M 适合使用这个类。
	/// </para>
	///
	/// <para>
	///  这个类使用更多内存和占用更大处理器， 对于大文件，压缩率可以最多可相对 DeflateStream 的小 1% 。如果文件小时，效果将十分明显。这个值和缓存大小也有关。但对小文件，这个类将十分慢。
	/// </para>
	///
	/// </remarks>
	/// <seealso cref="Py.Zip.Zlib.DeflateStream" />
	public class ParallelDeflateOutputStream : Stream {

		#region 配置

#if Zip_Trace

		//private const TraceBits _DesiredTrace = TraceBits.Write | TraceBits.WriteBegin |
		//TraceBits.WriteDone | TraceBits.Lifecycle | TraceBits.Fill | TraceBits.Flush |
		//TraceBits.Session;

		//private const TraceBits _DesiredTrace = TraceBits.WriteBegin | TraceBits.WriteDone | TraceBits.Synch | TraceBits.Lifecycle  | TraceBits.Session ;

		private const TraceBits _DesiredTrace = TraceBits.WriterThread | TraceBits.Synch | TraceBits.Lifecycle | TraceBits.Session;

#endif

		/// <summary>
		/// 输入/输出缓存大小。
		/// </summary>
		private const int IO_BUFFER_SIZE_DEFAULT = 64 * 1024;  // 128k

		#endregion

		#region 私有

		/// <summary>
		/// 工作项目列表。
		/// </summary>
		private System.Collections.Generic.List<WorkItem> _pool;

		/// <summary>
		/// 是否保持流打开。
		/// </summary>
		private bool _leaveOpen;

		/// <summary>
		/// 输出流。
		/// </summary>
		private Stream _outStream;

		/// <summary>
		/// 下一个添加的地点。
		/// </summary>
		private int _nextToFill, _nextToWrite;

		/// <summary>
		/// 缓存的大小。
		/// </summary>
		private int _bufferSize = IO_BUFFER_SIZE_DEFAULT;

		/// <summary>
		/// 写完成回调函数。
		/// </summary>
		private ManualResetEvent _writingDone;

		/// <summary>
		/// 状态刷新的回调函数。
		/// </summary>
		private ManualResetEvent _sessionReset;

		/// <summary>
		/// 当前代码是否没有其它输入。
		/// </summary>
		private bool _noMoreInputForThisSegment;

		/// <summary>
		/// 是否关闭流。
		/// </summary>
		private bool _isClosed;

		/// <summary>
		/// 是否释放空间。
		/// </summary>
		private bool _isDisposed;

		/// <summary>
		/// 首次写入。
		/// </summary>
		private bool _firstWriteDone;

		/// <summary>
		/// 寄存。
		/// </summary>
		private int _pc;

		/// <summary>
		/// 检验码。
		/// </summary>
		private int _Crc32;

		/// <summary>
		/// 处理的字节。
		/// </summary>
		private Int64 _totalBytesProcessed;

		/// <summary>
		/// 压缩等级。
		/// </summary>
		private CompressionLevel _compressLevel;

		/// <summary>
		/// 追加时的异常。
		/// </summary>
		private volatile Exception _pendingException;

		/// <summary>
		/// 加时的锁。
		/// </summary>
		private object _eLock = new Object();

		#endregion

		#region 属性

		/// <summary>
		/// 获取压缩策略。
		/// </summary>
		///
		public CompressionStrategy Strategy {
			get;
			private set;
		}

		/// <summary>
		/// 获取或设置每个处理器含的缓存。
		/// </summary>
		///
		/// <remarks>
		/// 
		/// <para>
		///   默认 4。根据计算机的能力不同，这个值变化。
		/// </para>
		///
		/// <para>
		///  全部的空间需要 (n*M*S*2), 其中 n 为 CPU 数, M 为个数, S 为缓存的大小 (<see cref="BufferSize"/>),
		///   其中 2 个缓存由 compressor 使用, 1 个输入和输出。如一个四核的电脑， 每个缓存为 3 ， 默认缓存为 128k， 整个类将使用 3mb 内存。
		/// </para>
		///
		/// <para>
		/// 这个值可以在任何时间更改，但必须在 Write() 之前修改。
		/// </para>
		/// </remarks>
		public int BuffersPerCore {
			get;
			set;
		}

		/// <summary>
		/// 一个线程的缓存的长度。
		/// </summary>
		/// <remarks>
		/// 默认为128k。
		/// </remarks>
		public int BufferSize {
			get {
				return _bufferSize;
			}
			set {
				Py.RunTime.Thrower.ThrowArgumentOutOfRangeExceptionIf(value < 1024, "value");
				_bufferSize = value;
			}
		}

		/// <summary>
		/// 获取目前的检验码。
		/// </summary>
		/// <remarks>
		/// 这个值只在 Close() 之后才有意义。
		/// </remarks>
		public int Crc32 {
			get {
				return _Crc32;
			}
		}

		/// <summary>
		/// 获取指示当前流是否支持查找功能的值。
		/// </summary>
		/// <value>总等于 false 。</value>
		/// <returns>
		/// 如果流支持查找，为 true；否则为 false。</returns>
		public override bool CanSeek {
			get {
				return false;
			}
		}

		/// <summary>
		/// 获取指示当前流是否支持读取的值。
		/// </summary>
		/// <value>总等于 false 。</value>
		/// <returns>
		/// 如果流支持读取，为 true；否则为 false。</returns>
		public override bool CanRead {
			get {
				return false;
			}
		}

		/// <summary>
		/// 当在派生类中重写时，获取指示当前流是否支持写入功能的值。
		/// </summary>
		/// <value></value>
		/// <returns>
		/// 如果流支持写入，为 true；否则为 false。</returns>
		public override bool CanWrite {
			get {
				return _outStream.CanWrite;
			}
		}

		/// <summary>
		/// 当在派生类中重写时，获取用字节表示的流长度。这个属性不支持。
		/// </summary>
		/// <value></value>
		/// <returns>用字节表示流长度的长值。</returns>
		/// <exception cref="T:System.NotSupportedException">从 Stream 派生的类不支持查找。</exception>
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}


		/// <summary>
		/// 获取或设置当前流中的位置。这个属性不支持。
		/// </summary>
		/// <value></value>
		/// <returns>流中的当前位置。</returns>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持查找。</exception>
		/// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一共处理了的字节数。
		/// </summary>
		/// <remarks>
		/// 这个值只在 Close() 之后才有意义。
		/// </remarks>
		public long BytesProcessed {
			get {
				return _totalBytesProcessed;
			}
		}

		#endregion

		#region 初始化

		/// <summary>
		/// 初始化 <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> 的新实例。
		/// </summary>
		/// <param name="stream">要写入的流。</param>
		/// <example>
		/// 下面例子演示了使用当前类压缩。
		/// <code>
		/// byte[] buffer = new byte[WORKING_BUFFER_SIZE];
		/// int n= -1;
		/// String outputFile = fileToCompress + ".compressed";
		/// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
		/// {
		/// using (var raw = System.IO.File.Create(outputFile))
		/// {
		/// using (Stream compressor = new ParallelDeflateOutputStream(raw))
		/// {
		/// while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
		/// {
		/// compressor.Write(buffer, 0, n);
		/// }
		/// }
		/// }
		/// }
		/// </code>
		/// 	<code lang="VB">
		/// Dim buffer As Byte() = New Byte(4096) {}
		/// Dim n As Integer = -1
		/// Dim outputFile As String = (fileToCompress &amp; ".compressed")
		/// Using input As Stream = File.OpenRead(fileToCompress)
		/// Using raw As FileStream = File.Create(outputFile)
		/// Using compressor As Stream = New ParallelDeflateOutputStream(raw)
		/// Do While (n &lt;&gt; 0)
		/// If (n &gt; 0) Then
		/// compressor.Write(buffer, 0, n)
		/// End If
		/// n = input.Read(buffer, 0, buffer.Length)
		/// Loop
		/// End Using
		/// End Using
		/// End Using
		/// </code>
		/// </example>
		public ParallelDeflateOutputStream(Stream stream)
			: this(stream, CompressionLevel.Default, CompressionStrategy.Default, false) {
		}

		/// <summary>
		/// 初始化 <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> 的新实例。
		/// </summary>
		/// <param name="stream">要写入的流。</param>
		/// <param name="level">压缩等级。</param>
		public ParallelDeflateOutputStream(Stream stream, CompressionLevel level)
			: this(stream, level, CompressionStrategy.Default, false) {
		}

		/// <summary>
		/// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
		/// when the ParallelDeflateOutputStream is closed.
		/// </summary>
		/// <remarks>
		///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
		///   constructor for example code.
		/// </remarks>
		/// <param name="stream">The stream to which compressed data will be written.</param>
		/// <param name="leaveOpen">
		///    true if the application would like the stream to remain open after inflation/deflation.
		/// </param>
		public ParallelDeflateOutputStream(Stream stream, bool leaveOpen)
			: this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen) {
		}

		/// <summary>
		/// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
		/// when the ParallelDeflateOutputStream is closed.
		/// </summary>
		/// <remarks>
		///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
		///   constructor for example code.
		/// </remarks>
		/// <param name="stream">The stream to which compressed data will be written.</param>
		/// <param name="level">A tuning knob to trade speed for effectiveness.</param>
		/// <param name="leaveOpen">
		///    true if the application would like the stream to remain open after inflation/deflation.
		/// </param>
		public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, bool leaveOpen)
			: this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen) {
		}

		/// <summary>
		/// 初始化 <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> 的新实例。
		/// </summary>
		/// <param name="stream">要写入的流。</param>
		/// <param name="level">压缩等级。</param>
		/// <param name="strategy">压缩策略。</param>
		/// <param name="leaveOpen">是否不关闭基础流。</param>
		public ParallelDeflateOutputStream(Stream stream,
										   CompressionLevel level,
										   CompressionStrategy strategy,
										   bool leaveOpen) {
#if Zip_Trace
			TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "-------------------------------------------------------");
			TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "Create {0:X8}", this.GetHashCode());
#endif
			_compressLevel = level;
			_leaveOpen = leaveOpen;
			Strategy = strategy;

			BuffersPerCore = 4; // default

			_writingDone = new ManualResetEvent(false);
			_sessionReset = new ManualResetEvent(false);

			_outStream = stream;
		}


		#endregion

		#region 方法

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
			// Fill a work buffer; when full, flip state to 'Filled'

			Thrower.ThrowArgumentNullExceptionIf(buffer, "buffer");
			Thrower.ThrowInvalidOperationExceptionIf(_isClosed, "当前流已关闭");
			Thrower.ThrowArgumentOutOfRangeExceptionIf(count < 0, "count");
			Thrower.ThrowArgumentOutOfRangeExceptionIf(offset < 0, "offset");
			// dispense any exceptions that occurred on the BG threads
			if (_pendingException != null)
				throw _pendingException;

			if (count == 0)
				return;

			if (!_firstWriteDone) {
				// Want to do this on first Write, first session, and not in the
				// constructor.  We want to allow the BufferSize and BuffersPerCore to
				// change after construction, but before first Write.

				#region 初始化

				_pool = new System.Collections.Generic.List<WorkItem>();
				for (int i = 0; i < BuffersPerCore * Environment.ProcessorCount; i++)
					_pool.Add(new WorkItem(_bufferSize, _compressLevel, Strategy));
				_pc = _pool.Count;

				for (int i = 0; i < _pc; i++)
					_pool[i].Index = i;

				// set the pointers
				_nextToFill = _nextToWrite = 0;

				#endregion

				// Only do this once (ever), the first time Write() is called:
				if (!ThreadPool.QueueUserWorkItem(new WaitCallback(PerpetualWriterMethod)))
					throw new ThreadStateException("无法保存当前线程。");

				// Release the writer thread.
#if Zip_Trace
				TraceOutput(TraceBits.Synch, "Synch    _sessionReset.Set()          Write (first)");
#endif
				_sessionReset.Set();

				_firstWriteDone = true;
			}


			do {
				int ix = _nextToFill % _pc;
				WorkItem workitem = _pool[ix];
				lock (workitem) {
#if Zip_Trace
					TraceOutput(TraceBits.Fill,
								   "Fill     lock     wi({0}) stat({1}) iba({2}) nf({3})",
								   workitem.Index,
								   workitem.CurrentStatus,
								   workitem.InputBytesAvailable,
								   _nextToFill
								   );
#endif
					// If the status is what we want, then use the workitem.
					if (workitem.CurrentStatus == WorkItem.Status.None ||
						workitem.CurrentStatus == WorkItem.Status.Done ||
						workitem.CurrentStatus == WorkItem.Status.Filling) {
						workitem.CurrentStatus = WorkItem.Status.Filling;
						int limit = ((workitem.Buffer.Length - workitem.InputBytesAvailable) > count)
							? count
							: (workitem.Buffer.Length - workitem.InputBytesAvailable);

						// copy from the provided buffer to our workitem, starting at
						// the tail end of whatever data we might have in there currently.
						Array.Copy(buffer, offset, workitem.Buffer, workitem.InputBytesAvailable, limit);

						count -= limit;
						offset += limit;
						workitem.InputBytesAvailable += limit;
						if (workitem.InputBytesAvailable == workitem.Buffer.Length) {
							workitem.CurrentStatus = WorkItem.Status.Filled;
							// No need for interlocked.increment: the Write() method
							// is documented as not multi-thread safe, so we can assume Write()
							// calls come in from only one thread.
							_nextToFill++;
#if Zip_Trace
							TraceOutput(TraceBits.Fill,
										   "Fill     QUWI     wi({0}) stat({1}) iba({2}) nf({3})",
										   workitem.Index,
										   workitem.CurrentStatus,
										   workitem.InputBytesAvailable,
										   _nextToFill
										   );
#endif
							if (!ThreadPool.QueueUserWorkItem(DeflateOne, workitem))
								throw new ThreadStateException("无法保存当前模块。");
						}

					} else {
						int wcycles = 0;

						while (workitem.CurrentStatus != WorkItem.Status.None &&
							   workitem.CurrentStatus != WorkItem.Status.Done &&
							   workitem.CurrentStatus != WorkItem.Status.Filling) {
#if Zip_Trace
							TraceOutput(TraceBits.Fill,
										   "Fill     waiting  wi({0}) stat({1}) nf({2})",
										   workitem.Index,
										   workitem.CurrentStatus,
										   _nextToFill);
#endif
							wcycles++;

							Monitor.Pulse(workitem);
							Monitor.Wait(workitem);
#if Zip_Trace
							if (workitem.CurrentStatus == WorkItem.Status.None ||
								workitem.CurrentStatus == WorkItem.Status.Done ||
								workitem.CurrentStatus == WorkItem.Status.Filling)
								TraceOutput(TraceBits.Fill,
											   "Fill     A-OK     wi({0}) stat({1}) iba({2}) cyc({3})",
											   workitem.Index,
											   workitem.CurrentStatus,
											   workitem.InputBytesAvailable,
											   wcycles);
#endif
						}
					}
				}
			}   while (count > 0);  // until no more to write

			return;
		}

		/// <summary>
		/// Flush the stream.
		/// </summary>
		public override void Flush() {
			FlushInternal(false);
		}

		private void FlushInternal(bool lastInput) {
			Thrower.ThrowInvalidOperationExceptionIf(_isClosed, "当前流已关闭");
			// pass any partial buffer out to the compressor workers:
			WorkItem workitem = _pool[_nextToFill % _pc];
			lock (workitem) {
				if (workitem.CurrentStatus == WorkItem.Status.Filling) {
					workitem.CurrentStatus = WorkItem.Status.Filled;
					_nextToFill++;

					// When flush is called from Close(), we set _noMore.
					// can't do it before updating nextToFill, though.
					if (lastInput)
						_noMoreInputForThisSegment = true;
#if Zip_Trace
					TraceOutput(TraceBits.Flush,
								   "Flush    filled   wi({0})  iba({1}) nf({2}) nomore({3})",
								   workitem.Index, workitem.InputBytesAvailable, _nextToFill, _noMoreInputForThisSegment);
#endif
					if (!ThreadPool.QueueUserWorkItem(DeflateOne, workitem))
						throw new ThreadStateException("无法启动当前线程");


					//Monitor.Pulse(workitem);
				} else {
					// When flush is called from Close(), we set _noMore.
					// Gotta do this whether or not there is another packet to send along.
					if (lastInput)
						_noMoreInputForThisSegment = true;
#if Zip_Trace
					TraceOutput(TraceBits.Flush,
								   "Flush    noaction wi({0}) stat({1}) nf({2})  nomore({3})",
								   workitem.Index, workitem.CurrentStatus, _nextToFill, _noMoreInputForThisSegment);
#endif
				}
			}
		}

		/// <summary>
		/// 关闭当前流并释放与之关联的所有资源（如套接字和文件句柄）。
		/// </summary>
		public override void Close() {
#if Zip_Trace
			TraceOutput(TraceBits.Session, "Close {0:X8}", this.GetHashCode());
#endif
			if (_isClosed)
				return;

			FlushInternal(true);

			//System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1);
			//System.Console.WriteLine(st.ToString());

			// need to get Writer off the workitem, in case he's waiting forever
			WorkItem workitem = _pool[_nextToFill % _pc];
			lock (workitem) {
				Monitor.PulseAll(workitem);
			}

            // wait for the writer to complete his work

#if Zip_Trace
			TraceOutput(TraceBits.Synch, "Synch    _writingDone.WaitOne(begin)  Close");
#endif
            _writingDone.WaitOne();
#if Zip_Trace
			TraceOutput(TraceBits.Synch, "Synch    _writingDone.WaitOne(done)   Close");

			TraceOutput(TraceBits.Session, "-------------------------------------------------------");
#endif
			if (!_leaveOpen)
				_outStream.Close();

			_isClosed = true;
		}

//        /// <summary>
//        /// 释放 
//        /// <see cref="ParallelDeflateOutputStream"/> 所占用的资源。
//        /// </summary>
//        ~ParallelDeflateOutputStream() {
//#if Zip_Trace
//            TraceOutput(TraceBits.Lifecycle, "Destructor  {0:X8}", this.GetHashCode());
//#endif
//            // call Dispose with false.  Since we're in the
//            // destructor call, the managed resources will be
//            // disposed of anyways.
//            Dispose(false);
//        }

        /// <summary>
        /// 释放由当前实例使用的所有资源。
        /// </summary>
        public new void Dispose() {

        #if Zip_Trace
                    TraceOutput(TraceBits.Lifecycle, "Dispose  {0:X8}", this.GetHashCode());
        #endif
                    _isDisposed = true;
                    _pool = null;
        #if Zip_Trace
                    TraceOutput(TraceBits.Synch, "Synch    _sessionReset.Set()  Dispose");
        #endif
                    _sessionReset.Set();
            Dispose(true);
        }

		/// <summary>
		/// 释放当前实例使用的资源，可选是否释放非托管资源。
		/// </summary>
		/// <param name="disposing">如果为 true，则同时释放占用的托管与非托管资源，否则只释放托管资源。</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_writingDone.Close();
				_sessionReset.Close();
			}
		}

		/// <summary>
		/// 更换写入的流。
		/// </summary>
		/// <remarks>
		///   因为创建 ParallelDeflateOutputStream 需很多的资源,这个方法用于重复利用资源。 可以先执行 Close()， 然后执行 Reset() 。
		/// </remarks>
		///
		/// <example>
		/// <code>
		/// ParallelDeflateOutputStream deflater = null;
		/// foreach (var inputFile in listOfFiles)
		/// {
		///     string outputFile = inputFile + ".compressed";
		///     using (System.IO.Stream input = System.IO.File.OpenRead(inputFile))
		///     {
		///         using (var outStream = System.IO.File.Create(outputFile))
		///         {
		///             if (deflater == null)
		///                 deflater = new ParallelDeflateOutputStream(outStream,
		///                                                            CompressionLevel.Best,
		///                                                            CompressionStrategy.Default,
		///                                                            true);
		///             deflater.Reset(outStream);
		///
		///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
		///             {
		///                 deflater.Write(buffer, 0, n);
		///             }
		///         }
		///     }
		/// }
		/// </code>
		/// </example>
		public void Reset(Stream stream) {
#if Zip_Trace
			TraceOutput(TraceBits.Session, "-------------------------------------------------------");
			TraceOutput(TraceBits.Session, "Reset {0:X8} firstDone({1})", this.GetHashCode(), _firstWriteDone);
#endif
			if (!_firstWriteDone)
				return;

			if (_noMoreInputForThisSegment) {
				// wait til done writing:
#if Zip_Trace
				TraceOutput(TraceBits.Synch, "Synch    _writingDone.WaitOne(begin)  Reset");
				_writingDone.WaitOne();
				TraceOutput(TraceBits.Synch, "Synch    _writingDone.WaitOne(done)   Reset");
#endif
				// reset all status
				foreach (var workitem in _pool)
					workitem.CurrentStatus = (int)WorkItem.Status.None;

				_noMoreInputForThisSegment = false;
				_nextToFill = _nextToWrite = 0;
				_totalBytesProcessed = 0L;
				_Crc32 = 0;
				_isClosed = false;
#if Zip_Trace
				TraceOutput(TraceBits.Synch, "Synch    _writingDone.Reset()         Reset");
#endif
				_writingDone.Reset();
			}
#if Zip_Trace
			else {
				TraceOutput(TraceBits.Synch, "Synch                           Reset noMore=false");
			}
#endif
			_outStream = stream;


			// release the writer thread for the next "session"
#if Zip_Trace
			TraceOutput(TraceBits.Synch, "Synch    _sessionReset.Set()          Reset");
#endif
			_sessionReset.Set();
		}

		/// <summary>
		/// 处理写。
		/// </summary>
		/// <param name="state">状态。</param>
		private void PerpetualWriterMethod(object state) {
#if Zip_Trace
			TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod START");
#endif
			try {
				do {
					// wait for the next session
#if Zip_Trace
					TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.WaitOne(begin) PWM");
#endif
					_sessionReset.WaitOne();
#if Zip_Trace
					TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.WaitOne(done)  PWM");
#endif
					if (_isDisposed)
						break;
#if Zip_Trace
					TraceOutput(TraceBits.Synch | TraceBits.WriterThread, "Synch    _sessionReset.Reset()        PWM");
#endif
					_sessionReset.Reset();

					// repeatedly write buffers as they become ready
					WorkItem workitem = null;
					Py.Algorithm.Crc32 c = new Py.Algorithm.Crc32();
					do {
						workitem = _pool[_nextToWrite % _pc];
						lock (workitem) {
#if Zip_Trace
							if (_noMoreInputForThisSegment)
								TraceOutput(TraceBits.Write,
											   "Write    drain    wi({0}) stat({1}) canuse({2})  cba({3})",
											   workitem.Index,
											   workitem.CurrentStatus,
											   (workitem.CurrentStatus == WorkItem.Status.Compressed),
											   workitem.CompressedBytesAvailable);

#endif

							do {
								if (workitem.CurrentStatus == WorkItem.Status.Compressed) {
#if Zip_Trace
									TraceOutput(TraceBits.WriteBegin,
												   "Write    begin    wi({0}) stat({1})              cba({2})",
												   workitem.Index,
												   workitem.CurrentStatus,
												   workitem.CompressedBytesAvailable);
#endif

									workitem.CurrentStatus = WorkItem.Status.Writing;
									_outStream.Write(workitem.Compressed, 0, workitem.CompressedBytesAvailable);
									c.Combine(workitem.Crc, workitem.InputBytesAvailable);
									_totalBytesProcessed += workitem.InputBytesAvailable;
									_nextToWrite++;
									workitem.InputBytesAvailable = 0;
									workitem.CurrentStatus = WorkItem.Status.Done;
#if Zip_Trace
									TraceOutput(TraceBits.WriteDone,
												   "Write    done     wi({0}) stat({1})              cba({2})",
												   workitem.Index,
												   workitem.CurrentStatus,
												   workitem.CompressedBytesAvailable);

#endif
									Monitor.Pulse(workitem);
									break;
								} else {
									int wcycles = 0;
									// I've locked a workitem I cannot use.
									// Therefore, wake someone else up, and then release the lock.
									while (workitem.CurrentStatus != WorkItem.Status.Compressed) {
#if Zip_Trace
										TraceOutput(TraceBits.WriteWait,
													   "Write    waiting  wi({0}) stat({1}) nw({2}) nf({3}) nomore({4})",
													   workitem.Index,
													   workitem.CurrentStatus,
													   _nextToWrite, _nextToFill,
													   _noMoreInputForThisSegment);
#endif
										if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
											break;

										wcycles++;

										// wake up someone else
										Monitor.Pulse(workitem);
										// release and wait
										Monitor.Wait(workitem);
#if Zip_Trace
										if (workitem.CurrentStatus == WorkItem.Status.Compressed)
											TraceOutput(TraceBits.WriteWait,
														   "Write    A-OK     wi({0}) stat({1}) iba({2}) cba({3}) cyc({4})",
														   workitem.Index,
														   workitem.CurrentStatus,
														   workitem.InputBytesAvailable,
														   workitem.CompressedBytesAvailable,
														   wcycles);

#endif


									}

									if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
										break;

								}
							}
							while (true);
						}
#if Zip_Trace
						if (_noMoreInputForThisSegment)
							TraceOutput(TraceBits.Write,
										   "Write    nomore  nw({0}) nf({1}) break({2})",
										   _nextToWrite, _nextToFill, (_nextToWrite == _nextToFill));
#endif
						if (_noMoreInputForThisSegment && _nextToWrite == _nextToFill)
							break;

					} while (true);


					// Finish:
					// After writing a series of buffers, closing each one with
					// Flush.Sync, we now write the final one as Flush.Finish, and
					// then stop.
					byte[] buffer = new byte[128];
					ZlibCodec compressor = new ZlibCodec();
					ZlibState rc = compressor.InitializeDeflate(_compressLevel, false);
					compressor.InputBuffer = null;
					compressor.NextIn = 0;
					compressor.AvailableBytesIn = 0;
					compressor.OutputBuffer = buffer;
					compressor.NextOut = 0;
					compressor.AvailableBytesOut = buffer.Length;
					rc = compressor.Deflate(FlushType.Finish);
					if (rc != ZlibState.StreamEnd && rc != ZlibState.Success)
						throw new ZlibException("压缩错误: " + compressor.Message);

					if (buffer.Length - compressor.AvailableBytesOut > 0) {
#if Zip_Trace
						TraceOutput(TraceBits.WriteBegin,
									   "Write    begin    flush bytes({0})",
									   buffer.Length - compressor.AvailableBytesOut);
#endif

						_outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

#if Zip_Trace
						TraceOutput(TraceBits.WriteBegin,
									   "Write    done     flush");
#endif

					}

					compressor.EndDeflate();

					_Crc32 = c.Crc32Result;

					// signal that writing is complete:
#if Zip_Trace
					TraceOutput(TraceBits.Synch, "Synch    _writingDone.Set()           PWM");
#endif
					_writingDone.Set();
				}
				while (true);
			} catch (System.Exception exc1) {
				lock (_eLock) {
					// expose the exception to the main thread
					if (_pendingException != null)
						_pendingException = exc1;
				}
			}
#if Zip_Trace
			TraceOutput(TraceBits.WriterThread, "_PerpetualWriterMethod FINIS");
#endif
		}

		/// <summary>
		/// 压缩一个内容。
		/// </summary>
		/// <param name="wi">内容。</param>
		private void DeflateOne(Object wi) {
			WorkItem workitem = (WorkItem)wi;
			try {
				// compress one buffer
				int myItem = workitem.Index;

				lock (workitem) {
					if (workitem.CurrentStatus != WorkItem.Status.Filled)
						throw new InvalidOperationException();

					Py.Algorithm.Crc32 crc = new Py.Algorithm.Crc32();

					// use the workitem:
					// calc CRC on the buffer
					crc.SlurpBlock(workitem.Buffer, 0, workitem.InputBytesAvailable);

					#region 压缩

					ZlibCodec compressor = workitem.Compressor;
					ZlibState rc;
					compressor.ResetDeflate();
					compressor.NextIn = 0;

					compressor.AvailableBytesIn = workitem.InputBytesAvailable;

					// 第 1 步: 压缩缓存
					compressor.NextOut = 0;
					compressor.AvailableBytesOut = workitem.Compressed.Length;
					do {
						compressor.Deflate(FlushType.None);
					} while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);

					// 第 2 步: 异步输出缓存
					rc = compressor.Deflate(FlushType.Sync);

					workitem.CompressedBytesAvailable = (int)compressor.TotalBytesOut;

					#endregion


					// 更新状态
					workitem.CurrentStatus = WorkItem.Status.Compressed;
					workitem.Crc = crc.Crc32Result;
#if Zip_Trace
					TraceOutput(TraceBits.Compress,
								   "Compress          wi({0}) stat({1}) len({2})",
								   workitem.Index,
								   workitem.CurrentStatus,
								   workitem.CompressedBytesAvailable
								   );
#endif

					// release the item
					Monitor.Pulse(workitem);
				}
			} catch (System.Exception exc1) {
				lock (_eLock) {
					// expose the exception to the main thread
					if (_pendingException != null)
						_pendingException = exc1;
				}
			}
		}

#if Zip_Trace
		
		private void TraceOutput(TraceBits bits, string format, params object[] varParams) {
            lock (this) {
                int tid = Thread.CurrentThread.GetHashCode();
                Console.ForegroundColor = (ConsoleColor)(tid % 8 + 8);
                Console.Write("{0:000} PDOS ", tid);
                Console.WriteLine(format, varParams);
                Console.ResetColor();
            }
		}


		// used only when Trace is defined
		[Flags]
		enum TraceBits {
			None = 0,
			Write = 1,    // write out
			WriteBegin = 2,    // begin to write out
			WriteDone = 4,    // done writing out
			WriteWait = 8,    // write thread waiting for buffer
			Flush = 16,
			Compress = 32,   // async compress
			Fill = 64,   // filling buffers, when caller invokes Write()
			Lifecycle = 128,  // constructor/disposer
			Session = 256,  // Close/Reset
			Synch = 512,  // thread synchronization
			WriterThread = 1024, // writer thread
		}

#endif

		/// <summary>
		/// 从当前流读取字节序列，并将此流中的位置提升读取的字节数。这个属性不支持。
		/// </summary>
		/// <param name="buffer">字节数组。此方法返回时，该缓冲区包含指定的字符数组，该数组的 <paramref name="offset"/> 和 (<paramref name="offset"/> + <paramref name="count"/> -1) 之间的值由从当前源中读取的字节替换。</param>
		/// <param name="offset"><paramref name="buffer"/> 中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
		/// <param name="count">要从当前流中最多读取的字节数。</param>
		/// <returns>
		/// 读入缓冲区中的总字节数。如果当前可用的字节数没有请求的字节数那么多，则总字节数可能小于请求的字节数，或者如果已到达流的末尾，则为零 (0)。
		/// </returns>
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
			throw new NotSupportedException();
		}

		/// <summary>
		/// 设置当前流中的位置。这个属性不支持。
		/// </summary>
		/// <param name="offset">相对于 <paramref name="origin"/> 参数的字节偏移量。</param>
		/// <param name="origin"><see cref="T:System.IO.SeekOrigin"/> 类型的值，指示用于获取新位置的参考点。</param>
		/// <returns>当前流中的新位置。</returns>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持查找，例如在流通过管道或控制台输出构造的情况下即为如此。</exception>
		/// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// 设置当前流的长度。这个属性不支持。
		/// </summary>
		/// <param name="value">所需的当前流的长度（以字节表示）。</param>
		/// <exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
		/// <exception cref="T:System.NotSupportedException">流不支持写入和查找，例如在流通过管道或控制台输出构造的情况下即为如此。</exception>
		/// <exception cref="T:System.ObjectDisposedException">在流关闭后调用方法。</exception>
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		#endregion

	}

}


