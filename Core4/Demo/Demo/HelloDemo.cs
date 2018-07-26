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
using System.Text;

using Py.Collections.ObjectModel;
using Py.Logging;
using Py.Core;
using Py.Text;
using Py.Windows;
using Py.IO;
using Py.Html;
using Py.Sql;
using Py.Net;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using Py.RunTime;

namespace Py.Demo {
    public class HelloDemo :DemoBase {


		public override void Start() {


            // 这是最普通的
          //  Logger.Write("aa");



            string[] a = new string[2];

            a[0] = "f";
            a[1] = "c";

            //  这也是很常用的。
          //  Console.Write(a);  

          //  Logger.Write(a);    // 明白  ?  ok
            // 另外 ， Console 只能在软件中用  Logger 可以在任何地方用
            //  比如网站，  一样的用法 


            Logger.Write(Number.ToCapitalCurrency((decimal)232221))  ;

            Console.WriteLine();















            return;

            Logger.Start("欢迎下载。 ");
            Logger.Write("打开并修改 Program.cs 中标识的位置，运行其他演示。");
            Logger.Indent();
            Logger.Write("修改 DemoBase demo = new HelloDemo(); ");
            Logger.Write("到 DemoBase demo = new ZipDemo(); ");
            Logger.Write("即运行 Zip 演示。");
            Logger.UnIndent();

		}

        void p_PageChanged(object sender, ValueChangedEventArgs<int> e) {
            Logger.Write("{0}  ->   {1}", e.From, e.To);
        }
	}
}
