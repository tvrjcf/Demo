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
 * IEntry.cs  by xuld
 * 
 * ******************************************************************************/


namespace Py.Collections {

	/// <summary>
	/// 表示一个单元。
	/// </summary>
    public interface IEntry {

		/// <summary>
		/// 获取单元的名字。
		/// </summary>
        string Name { get; }
    }
}
