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
	/// ������Ŀ��
	/// </summary>
	class WorkItem {

		/// <summary>
		/// ��ʾ��ǰ��״̬��
		/// </summary>
		public enum Status {

			/// <summary>
			/// �ޡ�
			/// </summary>
			None = 0,

			/// <summary>
			/// ������д��
			/// </summary>
			Filling = 1,

			/// <summary>
			/// ��д��ɡ�
			/// </summary>
			Filled = 2,

			/// <summary>
			/// ��ѹ����
			/// </summary>
			Compressing = 3,

			/// <summary>
			/// ѹ����ɡ�
			/// </summary>
			Compressed = 4,

			/// <summary>
			/// ����д�롣
			/// </summary>
			Writing = 5,

			/// <summary>
			/// д�롣
			/// </summary>
			Done = 6
		}

		/// <summary>
		/// �����С��
		/// </summary>
		public byte[] Buffer;

		/// <summary>
		/// ѹ������Ŀ��
		/// </summary>
		public byte[] Compressed;

		/// <summary>
		/// ״̬��
		/// </summary>
		public Status CurrentStatus;

		/// <summary>
		/// �����롣
		/// </summary>
		public int Crc;

		/// <summary>
		/// ������
		/// </summary>
		public int Index;

		/// <summary>
		/// ����ʹ�õ������ֽڡ�
		/// </summary>
		public int InputBytesAvailable;

		/// <summary>
		/// ����ѹ���Ĳ��֡�
		/// </summary>
		public int CompressedBytesAvailable;

		/// <summary>
		/// ��������
		/// </summary>
		public ZlibCodec Compressor;

		/// <summary>
		/// ��ʼ�� <see cref="Py.Zip.Zlib.WorkItem"/> ����ʵ����
		/// </summary>
		/// <param name="size">��С��</param>
		/// <param name="compressLevel">ѹ���ȼ���</param>
		/// <param name="strategy">���ԡ�</param>
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
	/// ֧���ڶ��߳�ʹ�õ�ʹ�� Deflate �㷨��ѹ������
	/// </summary>
	///
	/// <remarks>
	/// <para>
	/// �����ֻ������ѹ����д�롣
	/// </para>
	///
	/// <para>
	///  ������ <see cref="Py.Zip.Zlib.DeflateStream"/> ����, �������׷����֧�ֶ��̵߳Ĳ��֡�
	///  �ڶ��ں˵ļ�����ϣ�ʹ���������������ύЧ�ʡ��ر��ǳ��ȴ���������� һ�� �ļ����� 10 M �ʺ�ʹ������ࡣ
	/// </para>
	///
	/// <para>
	///  �����ʹ�ø����ڴ��ռ�ø��������� ���ڴ��ļ���ѹ���ʿ���������� DeflateStream ��С 1% ������ļ�Сʱ��Ч����ʮ�����ԡ����ֵ�ͻ����СҲ�йء�����С�ļ�������ཫʮ������
	/// </para>
	///
	/// </remarks>
	/// <seealso cref="Py.Zip.Zlib.DeflateStream" />
	public class ParallelDeflateOutputStream : Stream {

		#region ����

#if Zip_Trace

		//private const TraceBits _DesiredTrace = TraceBits.Write | TraceBits.WriteBegin |
		//TraceBits.WriteDone | TraceBits.Lifecycle | TraceBits.Fill | TraceBits.Flush |
		//TraceBits.Session;

		//private const TraceBits _DesiredTrace = TraceBits.WriteBegin | TraceBits.WriteDone | TraceBits.Synch | TraceBits.Lifecycle  | TraceBits.Session ;

		private const TraceBits _DesiredTrace = TraceBits.WriterThread | TraceBits.Synch | TraceBits.Lifecycle | TraceBits.Session;

#endif

		/// <summary>
		/// ����/��������С��
		/// </summary>
		private const int IO_BUFFER_SIZE_DEFAULT = 64 * 1024;  // 128k

		#endregion

		#region ˽��

		/// <summary>
		/// ������Ŀ�б�
		/// </summary>
		private System.Collections.Generic.List<WorkItem> _pool;

		/// <summary>
		/// �Ƿ񱣳����򿪡�
		/// </summary>
		private bool _leaveOpen;

		/// <summary>
		/// �������
		/// </summary>
		private Stream _outStream;

		/// <summary>
		/// ��һ����ӵĵص㡣
		/// </summary>
		private int _nextToFill, _nextToWrite;

		/// <summary>
		/// ����Ĵ�С��
		/// </summary>
		private int _bufferSize = IO_BUFFER_SIZE_DEFAULT;

		/// <summary>
		/// д��ɻص�������
		/// </summary>
		private ManualResetEvent _writingDone;

		/// <summary>
		/// ״̬ˢ�µĻص�������
		/// </summary>
		private ManualResetEvent _sessionReset;

		/// <summary>
		/// ��ǰ�����Ƿ�û���������롣
		/// </summary>
		private bool _noMoreInputForThisSegment;

		/// <summary>
		/// �Ƿ�ر�����
		/// </summary>
		private bool _isClosed;

		/// <summary>
		/// �Ƿ��ͷſռ䡣
		/// </summary>
		private bool _isDisposed;

		/// <summary>
		/// �״�д�롣
		/// </summary>
		private bool _firstWriteDone;

		/// <summary>
		/// �Ĵ档
		/// </summary>
		private int _pc;

		/// <summary>
		/// �����롣
		/// </summary>
		private int _Crc32;

		/// <summary>
		/// ������ֽڡ�
		/// </summary>
		private Int64 _totalBytesProcessed;

		/// <summary>
		/// ѹ���ȼ���
		/// </summary>
		private CompressionLevel _compressLevel;

		/// <summary>
		/// ׷��ʱ���쳣��
		/// </summary>
		private volatile Exception _pendingException;

		/// <summary>
		/// ��ʱ������
		/// </summary>
		private object _eLock = new Object();

		#endregion

		#region ����

		/// <summary>
		/// ��ȡѹ�����ԡ�
		/// </summary>
		///
		public CompressionStrategy Strategy {
			get;
			private set;
		}

		/// <summary>
		/// ��ȡ������ÿ�����������Ļ��档
		/// </summary>
		///
		/// <remarks>
		/// 
		/// <para>
		///   Ĭ�� 4�����ݼ������������ͬ�����ֵ�仯��
		/// </para>
		///
		/// <para>
		///  ȫ���Ŀռ���Ҫ (n*M*S*2), ���� n Ϊ CPU ��, M Ϊ����, S Ϊ����Ĵ�С (<see cref="BufferSize"/>),
		///   ���� 2 �������� compressor ʹ��, 1 ��������������һ���ĺ˵ĵ��ԣ� ÿ������Ϊ 3 �� Ĭ�ϻ���Ϊ 128k�� �����ཫʹ�� 3mb �ڴ档
		/// </para>
		///
		/// <para>
		/// ���ֵ�������κ�ʱ����ģ��������� Write() ֮ǰ�޸ġ�
		/// </para>
		/// </remarks>
		public int BuffersPerCore {
			get;
			set;
		}

		/// <summary>
		/// һ���̵߳Ļ���ĳ��ȡ�
		/// </summary>
		/// <remarks>
		/// Ĭ��Ϊ128k��
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
		/// ��ȡĿǰ�ļ����롣
		/// </summary>
		/// <remarks>
		/// ���ֵֻ�� Close() ֮��������塣
		/// </remarks>
		public int Crc32 {
			get {
				return _Crc32;
			}
		}

		/// <summary>
		/// ��ȡָʾ��ǰ���Ƿ�֧�ֲ��ҹ��ܵ�ֵ��
		/// </summary>
		/// <value>�ܵ��� false ��</value>
		/// <returns>
		/// �����֧�ֲ��ң�Ϊ true������Ϊ false��</returns>
		public override bool CanSeek {
			get {
				return false;
			}
		}

		/// <summary>
		/// ��ȡָʾ��ǰ���Ƿ�֧�ֶ�ȡ��ֵ��
		/// </summary>
		/// <value>�ܵ��� false ��</value>
		/// <returns>
		/// �����֧�ֶ�ȡ��Ϊ true������Ϊ false��</returns>
		public override bool CanRead {
			get {
				return false;
			}
		}

		/// <summary>
		/// ��������������дʱ����ȡָʾ��ǰ���Ƿ�֧��д�빦�ܵ�ֵ��
		/// </summary>
		/// <value></value>
		/// <returns>
		/// �����֧��д�룬Ϊ true������Ϊ false��</returns>
		public override bool CanWrite {
			get {
				return _outStream.CanWrite;
			}
		}

		/// <summary>
		/// ��������������дʱ����ȡ���ֽڱ�ʾ�������ȡ�������Բ�֧�֡�
		/// </summary>
		/// <value></value>
		/// <returns>���ֽڱ�ʾ�����ȵĳ�ֵ��</returns>
		/// <exception cref="T:System.NotSupportedException">�� Stream �������಻֧�ֲ��ҡ�</exception>
		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}


		/// <summary>
		/// ��ȡ�����õ�ǰ���е�λ�á�������Բ�֧�֡�
		/// </summary>
		/// <value></value>
		/// <returns>���еĵ�ǰλ�á�</returns>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧�ֲ��ҡ�</exception>
		/// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// һ�������˵��ֽ�����
		/// </summary>
		/// <remarks>
		/// ���ֵֻ�� Close() ֮��������塣
		/// </remarks>
		public long BytesProcessed {
			get {
				return _totalBytesProcessed;
			}
		}

		#endregion

		#region ��ʼ��

		/// <summary>
		/// ��ʼ�� <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> ����ʵ����
		/// </summary>
		/// <param name="stream">Ҫд�������</param>
		/// <example>
		/// ����������ʾ��ʹ�õ�ǰ��ѹ����
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
		/// ��ʼ�� <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> ����ʵ����
		/// </summary>
		/// <param name="stream">Ҫд�������</param>
		/// <param name="level">ѹ���ȼ���</param>
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
		/// ��ʼ�� <see cref="Py.Zip.Zlib.ParallelDeflateOutputStream"/> ����ʵ����
		/// </summary>
		/// <param name="stream">Ҫд�������</param>
		/// <param name="level">ѹ���ȼ���</param>
		/// <param name="strategy">ѹ�����ԡ�</param>
		/// <param name="leaveOpen">�Ƿ񲻹رջ�������</param>
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

		#region ����

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
			// Fill a work buffer; when full, flip state to 'Filled'

			Thrower.ThrowArgumentNullExceptionIf(buffer, "buffer");
			Thrower.ThrowInvalidOperationExceptionIf(_isClosed, "��ǰ���ѹر�");
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

				#region ��ʼ��

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
					throw new ThreadStateException("�޷����浱ǰ�̡߳�");

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
								throw new ThreadStateException("�޷����浱ǰģ�顣");
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
			Thrower.ThrowInvalidOperationExceptionIf(_isClosed, "��ǰ���ѹر�");
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
						throw new ThreadStateException("�޷�������ǰ�߳�");


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
		/// �رյ�ǰ�����ͷ���֮������������Դ�����׽��ֺ��ļ��������
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
//        /// �ͷ� 
//        /// <see cref="ParallelDeflateOutputStream"/> ��ռ�õ���Դ��
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
        /// �ͷ��ɵ�ǰʵ��ʹ�õ�������Դ��
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
		/// �ͷŵ�ǰʵ��ʹ�õ���Դ����ѡ�Ƿ��ͷŷ��й���Դ��
		/// </summary>
		/// <param name="disposing">���Ϊ true����ͬʱ�ͷ�ռ�õ��й�����й���Դ������ֻ�ͷ��й���Դ��</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_writingDone.Close();
				_sessionReset.Close();
			}
		}

		/// <summary>
		/// ����д�������
		/// </summary>
		/// <remarks>
		///   ��Ϊ���� ParallelDeflateOutputStream ��ܶ����Դ,������������ظ�������Դ�� ������ִ�� Close()�� Ȼ��ִ�� Reset() ��
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
		/// ����д��
		/// </summary>
		/// <param name="state">״̬��</param>
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
						throw new ZlibException("ѹ������: " + compressor.Message);

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
		/// ѹ��һ�����ݡ�
		/// </summary>
		/// <param name="wi">���ݡ�</param>
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

					#region ѹ��

					ZlibCodec compressor = workitem.Compressor;
					ZlibState rc;
					compressor.ResetDeflate();
					compressor.NextIn = 0;

					compressor.AvailableBytesIn = workitem.InputBytesAvailable;

					// �� 1 ��: ѹ������
					compressor.NextOut = 0;
					compressor.AvailableBytesOut = workitem.Compressed.Length;
					do {
						compressor.Deflate(FlushType.None);
					} while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);

					// �� 2 ��: �첽�������
					rc = compressor.Deflate(FlushType.Sync);

					workitem.CompressedBytesAvailable = (int)compressor.TotalBytesOut;

					#endregion


					// ����״̬
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
		/// �ӵ�ǰ����ȡ�ֽ����У����������е�λ��������ȡ���ֽ�����������Բ�֧�֡�
		/// </summary>
		/// <param name="buffer">�ֽ����顣�˷�������ʱ���û���������ָ�����ַ����飬������� <paramref name="offset"/> �� (<paramref name="offset"/> + <paramref name="count"/> -1) ֮���ֵ�ɴӵ�ǰԴ�ж�ȡ���ֽ��滻��</param>
		/// <param name="offset"><paramref name="buffer"/> �еĴ��㿪ʼ���ֽ�ƫ�������Ӵ˴���ʼ�洢�ӵ�ǰ���ж�ȡ�����ݡ�</param>
		/// <param name="count">Ҫ�ӵ�ǰ��������ȡ���ֽ�����</param>
		/// <returns>
		/// ���뻺�����е����ֽ����������ǰ���õ��ֽ���û��������ֽ�����ô�࣬�����ֽ�������С��������ֽ�������������ѵ�������ĩβ����Ϊ�� (0)��
		/// </returns>
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
			throw new NotSupportedException();
		}

		/// <summary>
		/// ���õ�ǰ���е�λ�á�������Բ�֧�֡�
		/// </summary>
		/// <param name="offset">����� <paramref name="origin"/> �������ֽ�ƫ������</param>
		/// <param name="origin"><see cref="T:System.IO.SeekOrigin"/> ���͵�ֵ��ָʾ���ڻ�ȡ��λ�õĲο��㡣</param>
		/// <returns>��ǰ���е���λ�á�</returns>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧�ֲ��ң���������ͨ���ܵ������̨������������¼�Ϊ��ˡ�</exception>
		/// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// ���õ�ǰ���ĳ��ȡ�������Բ�֧�֡�
		/// </summary>
		/// <param name="value">����ĵ�ǰ���ĳ��ȣ����ֽڱ�ʾ����</param>
		/// <exception cref="T:System.IO.IOException">���� I/O ����</exception>
		/// <exception cref="T:System.NotSupportedException">����֧��д��Ͳ��ң���������ͨ���ܵ������̨������������¼�Ϊ��ˡ�</exception>
		/// <exception cref="T:System.ObjectDisposedException">�����رպ���÷�����</exception>
		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		#endregion

	}

}


