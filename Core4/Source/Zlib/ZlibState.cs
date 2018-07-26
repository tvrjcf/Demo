// ZlibState.cs
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
// Time-stamp: <2009-November-03 18:50:19>
//
// ------------------------------------------------------------------
//
// This module defines constants used by the zlib class library.  This
// code is derived from the jzlib implementation of zlib, but
// significantly modified.  In keeping with the license for jzlib, the
// copyright to that code is included here.
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
//  edited by xuld

using System;

namespace Py.Zip.Zlib {

	/// <summary>
	/// ��ʾ����״̬��
	/// </summary>
	enum ZlibState {

		/// <summary>
		/// ������
		/// </summary>
		Success = 0,

		/// <summary>
		/// ��������
		/// </summary>
		StreamEnd = 1,

		/// <summary>
		/// ��Ҫ�ֵ䡣
		/// </summary>
		NeedDict = 2,

		/// <summary>
		/// �������粻�ɶ���
		/// </summary>
		StreamError = -2,

		/// <summary>
		/// ���ݴ���
		/// </summary>
		DataError = -3,

		/// <summary>
		/// �ڴ���ִ���
		/// </summary>
		MemoryError = -4,

		/// <summary>
		/// �������
		/// </summary>
		BufferError = -5,

		/// <summary>
		/// �汾����
		/// </summary>
		VersionError = -6,

		/// <summary>
		/// ���ִ���
		/// </summary>
		ErrorNo = -1,

	}

}
