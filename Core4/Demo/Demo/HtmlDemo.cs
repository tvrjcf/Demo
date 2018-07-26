﻿/* *****************************************************************************
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
using Py.Html;
using Py.IO;
using System.IO;
using Py.Net;
using Py.RunTime;
using Py.Windows;

namespace Py.Demo {
	public class HtmlDemo: DemoBase {

		public override void Start() {

			#region 准备

			Logger.Start("Reader");

			#endregion

	        #region 初

            HtmlDocument html = new HtmlDocument();

            html.LoadHtml("<html><head><title></title></head><body><span id=\"m\"></span></body></html>");

            html.GetElementById("m").AppendHtml("<span></span>");

            Logger.Write(html.OuterHtml);

	        #endregion
		}
	}
}
