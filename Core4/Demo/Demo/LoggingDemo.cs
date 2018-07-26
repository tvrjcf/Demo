
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

using System;
using System.Collections.Generic;
using System.Text;
using Py.Logging;
using System.Data;
using Py.Core;

namespace Py.Demo {
	public class LoggingDemo: DemoBase {
		public override void Start() {

			#region 准备

			Logger.Start("使用 Logger 实现日志");

			#endregion

			#region 初


			Logger.Info("基本的功能");


			DataTable dt = new DataTable();
			dt.Rows.Add(dt.NewRow());
			dt.Rows.Add(dt.NewRow());
			dt.Columns.Add("asd", typeof(int));
			dt.Columns.Add("asd1", typeof(int));
			dt.Columns.Add("2343243");
			dt.Rows[0][0] = 25161456;
			dt.Rows[0][1] = 61516;
			dt.Rows[1][0] = 2;

			Logger.Write(dt);

			Logger.Write("asdd:{0}", new Dictionary<string[], Dictionary<int, int>> {
				{new string[]{"a"},new Dictionary<int, int>(){
				{2,34},
				{12,13}
				}},
				{new string[]{"ad"},new Dictionary<int, int>(){
				{253,34456},
				{12546,13456}
				}}   
			});

			Logger.Dir(dt);

			#endregion
		}
	}
}
