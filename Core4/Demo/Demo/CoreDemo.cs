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
using Py.Core;
using Py.Text;

namespace Py.Demo {
	public class CoreDemo : DemoBase {


		/// <summary>
		/// 开始测试。
		/// </summary>
		public override void Start() {

			#region 准备

            Logger.Info("Core 是 库的重要部分， 提供了简化代码量的方法。");

			#endregion

			#region QC

            Logger.Info("QC 提供了常用转换函数");
		
			Logger.Write(QC.IP( QC.IP("127.158.0.0") ));


			#endregion

			#region RegExp

			Logger.Info("正则表达式");

            Logger.Write(RegExp.Test("4", RegExp.Number));

			#endregion

			#region Check

            Logger.Info("Check  容纳了检查字符串的方法");

            Logger.Write(" Check.CheckRequestString(\"asdag\'g1=1\")  ->   {0}", Check.CheckRequestString("asdag'g1=1"));
            Logger.Write(" Check.IsIP(\"192.168.1.1\")  ->   {0}",  Check.IsIP("192.168.1.1"));

			#endregion

			#region Str

			Logger.Info("字符串");

			DateTime dt = DateTime.Now.Add(new TimeSpan(4, 0, 0, -1));

			Logger.Write(Str.ToTimeString(dt));

			#endregion

			#region ArrayString

			Logger.Info("ArrayString 数组字符串");
				
			ArrayString s = new ArrayString("aaa,bbb,ccc", ',');

			Logger.Write(s[0]);

			s[1] = "bbbb";

			Logger.Write(s);

			s.Add("sss");

			Logger.Write(s);

			s.RemoveAt(1);

			Logger.Write(s);

			s.Clear();

			Logger.Write(s == "");

			s += "aaa,ddd,ccc";

			Logger.Write(s);

			s.Sort();

			Logger.Write(s);

			s -= "aaa";

			Logger.Write(s);

			s -= s;

			Logger.Write(s);

			Logger.Write(ArrayString.ReplaceValue("aaa,aaaaa,wqe,aaa,sfd,aaa", "aaa", "gg", ","));

			#endregion

			#region StringHelper

			Logger.Info("使用 StringHelper辅助字符串");

			StringHelper sh = new StringHelper();

			string input = @"用法 223， 三大殿,固定首发";
            
			sh.Pattern = @"用法{$N}， 三大殿,{$F}";


			sh.Match(input);


            foreach (string k in sh)
                Logger.Write("{0}  ->    {1}", k, sh[k]);

			#endregion


		}
	}
}
