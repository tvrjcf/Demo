
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
using Py.Json;

namespace Py.Demo {
	public class JsonDemo:DemoBase {


		public override void Start() {

			#region 准备

			Logger.Start("使用 Json中的工具 处理 Json 表达式。");

			const string S = "{a:1, b:[2, 3], c: {2:[7] }}";
			
			#endregion

			#region 初

			Logger.Info("获值");

			JsonExpression json = new JsonExpression(S);

			Logger.Write(json["c"]["2"][0]); //  7

			Logger.Write(json["c"].Json);   //  {2:[7] }

			Logger.Write(json["a"].Value);


			Logger.Info("改值");

			json["hh"] = new JsonArray(new int[] {3,4,2,5,6 });

			json["ss"] = new JsonBoolean(false);

			Logger.Write(json.ToString());

			#endregion
		}
	}
}
