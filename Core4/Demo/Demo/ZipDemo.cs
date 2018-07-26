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
using Py.Zip;

namespace Py.Demo {

	public class ZipDemo : DemoBase {

		/// <summary>
		/// 开始测试。
		/// </summary>
		public override void Start() {

			#region 设置

			Logger.Start("下面演示了如何使用 ZipFile 创建/解压一个 zip 文件。");

			const string DIRECTORY = "H:\\测试\\压缩\\";

			const string IN = DIRECTORY + "输入.txt";

			const string OUT = DIRECTORY + "输出.zip";

			const string PARENT = DIRECTORY + "测试";

			#endregion

			#region 准备

			FileHelper.CreateFile(IN).Dispose();

			FileHelper.CreateFile(PARENT + "\\测试.txt").Dispose();
			
			#endregion

			#region 初

			Logger.Info("压缩");

			ZipFile zip = new ZipFile();  // 创建一个 ZipFile 示例。

			zip.AddFile(IN);  //  增加一个文件单元。 即压缩一个文件

			zip.SaveAs(OUT);  //  保持当前文件到指定的位置。
			

			Logger.Info("解压");

			zip = new ZipFile();  // 重新创建一个 ZipFile 示例。

			zip.Open(OUT);  // 打开一个文件。

			zip.Extract();     //  解压。

			zip.Close();   //  释放占用的文件资源。


			Logger.Info("编辑已有的文件然后存档");

			zip = new ZipFile();  // 重新创建一个 ZipFile 示例。

			zip.Open(OUT);  // 打开一个文件。

			zip.AddFile(IN, "内部文件夹/内部");  //  增加一个文件单元。 即压缩一个文件

			zip.Save();     // 保存。

			zip.Close();   //  释放占用的文件资源。

			#endregion

			#region 高

			Logger.Info("快速地压缩");

			ZipFile.CreateFile(PARENT);   //快速压缩一个文件夹。

			Logger.Info("快速地解压");

			ZipFile.ExtractFile(OUT);    //  快速地解压文件。

			Logger.Info("遍历全部文件");

			zip.Open(OUT);  // 打开一个文件。

			foreach (ZipEntry e in zip.Entries) {
				Logger.Write(e);
			}

			Logger.Info("设置密码");

			zip.Password = "123";   // 设置密码。

			zip.SaveAs(PARENT  + "/加密.zip");

			#endregion

		}
	}
}
