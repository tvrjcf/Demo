/* *****************************************************************************
 *
 * Copyright (c) 2009-2010 Xuld. All rights reserved.
 * 
 * Project Url: http://play.xuld.net
 * 
 * This source code is part of the Project Py.Core for .Net .
 * 
 * This code is licensed under Py.Core Licence.
 * See the file License.html for the license details.
 * 
 * 
 * You must not remove this notice, or any other, from this software.
 *
 * 
 * FileExistsException.cs by xuld
 * 
 * ******************************************************************************/


using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Py.Core;

namespace System.IO {

	/// <summary>
	/// 试图写入磁盘上已存在的文件时引发的异常。
	/// </summary>
	[Serializable, ComVisible(true)]
	public class FileExistsException : IOException {
		
		/// <summary>
		/// 错误指针的 API long 值。
		/// </summary>
		static readonly int COR_E_POINTER;

		static FileExistsException() {
			unchecked{
				COR_E_POINTER = (int)2147500035;
			}
		}

		/// <summary>
		/// 文件名。
		/// </summary>
		string _fileName;

		/// <summary>
		/// 日志。
		/// </summary>
		string _fusionLog;

		/// <summary>
		/// 初始化 System.IO.FileExistsException 类的新实例，使其消息字符串设置为系统所提供的消息，其 HRESULT 设置为 COR_E_POINTER。
		/// </summary>
		public FileExistsException() : base(Py.Properties.Messages.FileExists, COR_E_POINTER) {}
  
		/// <summary>
		/// 初始化 System.IO.FileNotFoundException 类的新实例，使其消息字符串设置为 message，其 HRESULT 设置为 COR_E_POINTER。
		/// </summary>
		/// <param name="message">描述该错误的 System.String。message 的内容被设计为人可理解的形式。此构造函数的调用方需要确保此字符串已针对当前系统区域性进行了本地化。</param>
		public FileExistsException(string message) : base(message, COR_E_POINTER) {  }

		/// <summary>
		/// 用指定的序列化和上下文信息初始化 System.IO.FileExistsException 类的新实例。
		/// </summary>
		/// <param name="info">用于序列化或反序列化文件的数据。</param>
		/// <param name="context">文件的源和目标。</param>
		protected FileExistsException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			_fileName = info.GetString("FileExists_FileName");
			try {
				_fusionLog = info.GetString("FileExists_FusionLog");
			} catch {
				_fusionLog = null;
			}
		}
		   
		/// <summary>
		/// 使用指定错误信息和对作为此异常原因的内部异常的引用来初始化 System.IO.FileExistsException 类的新实例。
		/// </summary>
		/// <param name="message">描述该错误的 System.String。message 的内容被设计为人可理解的形式。此构造函数的调用方需要确保此字符串已针对当前系统区域性进行了本地化。</param>
		/// <param name="innerException">导致当前异常的异常。如果 innerException 参数不为 null，则当前异常在处理内部异常的 catch 块中引发。</param>
		public FileExistsException(string message, Exception innerException):base(message, innerException) {
			HResult = COR_E_POINTER;
		}

		/// <summary>
		/// 初始化 System.IO.FileNotFoundException 类的新实例，使其消息字符串设置为 message（用于指定无法找到的文件名），其 HRESULT 设置为 COR_E_POINTER。
		/// </summary>
		/// <param name="message">描述该错误的 System.String。message 的内容被设计为人可理解的形式。此构造函数的调用方需要确保此字符串已针对当前系统区域性进行了本地化。</param>
		/// <param name="fileName">一个 System.String，它包含具有无效图像的文件的完整名称。		</param>
		public FileExistsException(string message, string fileName):this(message) {
			_fileName = fileName;
		}

		/// <summary>
		/// 使用指定错误信息和对作为此异常原因的内部异常的引用来初始化 System.IO.FileExistsException 类的新实例。
		/// </summary>
		/// <param name="message">释异常原因的错误信息。</param>
		/// <param name="fileName">一个 System.String，它包含具有无效图像的文件的完整名称。</param>
		/// <param name="innerException">导致当前异常的异常。如果 innerException 参数不为 null，则当前异常在处理内部异常的 catch 块中引发。</param>
		public FileExistsException(string message, string fileName, Exception innerException)
			: this(message, innerException) {
			_fileName = fileName;
		}
 
		/// <summary>
		/// 获取无法找到的文件的名称。
		/// </summary>
		/// <value>包含文件名的 System.String；或者，如果没有将文件名传递给此实例的构造函数，则为 null。</value>
		public string FileName { get {return _fileName; } }

		/// <summary>
		/// 获取日志文件，该文件描述加载程序集失败的原因。
		/// </summary>
		/// <value>一个 String，包含由程序集缓存报告的错误。</value>
		/// <exception cref="System.Security.SecurityException">调用方没有所要求的权限。</exception>
		public string FusionLog {
			[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
			get {
				return this._fusionLog;
			}
		}

		/// <summary>
		/// 获取描述当前异常的消息。
		/// </summary>
		/// <returns>解释异常原因的错误消息或空字符串 ("")。</returns>
		public override string Message {
			get {
				return base.Message ?? (_fileName == null ? Str.FormatX(Py.Properties.Messages.FileExistsWithName, _fileName) : Py.Properties.Messages.FileExists);
			}
		}

		/// <summary>
		/// 设置带有文件名和附加异常信息的 System.Runtime.Serialization.SerializationInfo 对象。
		/// </summary>
		/// <param name="info">System.Runtime.Serialization.SerializationInfo，它存有有关所引发异常的序列化的对象数据。</param>
		/// <param name="context">System.Runtime.Serialization.StreamingContext，它包含有关源或目标的上下文信息。</param>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData(info, context);
			info.AddValue("FileExists_FileName", _fileName, typeof(string));
			try {
				info.AddValue("FileExists_FusionLog", FusionLog, typeof(string));
			} catch(System.Security.SecurityException) {
			}
		}
  
		/// <summary>
		/// 返回该异常的完全限定名，还可能返回错误信息、内部异常的名称和堆栈跟踪。
		/// </summary>
		/// <returns> 一个字符串，包含该异常的完全限定名，还可能包含错误信息、内部异常的名称和堆栈跟踪。</returns>
		public override string ToString() {
			string str = base.GetType().FullName + ": " + this.Message;
			if((this._fileName != null) && (this._fileName.Length != 0)) {
				str = str + Environment.NewLine + Str.FormatX(Py.Properties.Messages.FileExistsWithName, _fileName);
			}
			if(base.InnerException != null) {
				str = str + " ---> " + base.InnerException.ToString();
			}
			if(this.StackTrace != null) {
				str = str + Environment.NewLine + this.StackTrace;
			}
			try {
				if(this.FusionLog == null) {
					return str;
				}
				if(str == null) {
					str = " ";
				}
				str = str + Environment.NewLine;
				str = str + Environment.NewLine;
				str = str + this.FusionLog;
			} catch(System.Security.SecurityException) {
			}
			return str;
		}
	}
}
