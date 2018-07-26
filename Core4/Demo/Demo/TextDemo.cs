/* *****************************************************************************
 *
 * Copyright (c) 2009-2011 Xuld. All rights reserved.
 * 
 * Project Url: http://play.xuld.net
 * 
 * This source code is a demo page for the 
 * Project Py.Core in .Net .
 * 
 * This code is licensed under MIT License.
 * 
 * 
 * You must not remove this notice, or any other, from this software.
 *
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE. 
 * 
 * ******************************************************************************/

using Py.Core;
using Py.Logging;
using Py.Text;
using Py.Windows;
using Py.IO;
using System.Text;

namespace Py.Demo {

	public class TextDemo : DemoBase {

		/// <summary>
		/// 开始测试。
		/// </summary>
		public override void Start() {

			#region 设置

			Logger.Start("下面演示了如何使用 IniFile 创建/处理一个 ini 文件。");

			const string DIRECTORY = "H:\\测试\\设置\\";

			const string IN = DIRECTORY + "输入.ini";

			const string OUT = DIRECTORY + "输出.ini";

		//	const string PARENT = DIRECTORY + "测试";

			#endregion

			#region 准备

			FileHelper.CreateFile(IN).Dispose();

			FileHelper.Write(IN, @"[aa]
a = 23
g = 3", OverwriteType.Skip);
			
			#endregion

			#region 初


			Logger.Write(IniFile.ReadInt(IN,   "aa", "a"));
			Logger.Write(IniFile.ReadSections(IN));
			Logger.Write(IniFile.WriteValue(IN, "aa", "e", "asdsd"));
			IniFile.WriteValueIf(IN, "aa", "e", "aswdsd")   ;

			#endregion

			#region 高

			IniFile ini = new IniFile(IN);
			

			Logger.Info("读取");
			ini.Sections["a"]["f"] = "2";
			
			ini.Save();
			ini.SaveAs(OUT);
			

			#endregion

		}
	}
}
