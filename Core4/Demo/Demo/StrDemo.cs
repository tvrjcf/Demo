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

using Py.Core;
using Py.Logging;

namespace Py.Demo {
    public class StrDemo:DemoBase {
        public override void Start() {

            const string T = "用于调试字符串，字符串。";

            Logger.Info("子字符串");

            Logger.Write(Str.Remove(T, 2, 1));
            Logger.Write(Str.RemoveEnd(T, 2));
            Logger.Write(Str.RemoveStart(T, 2));
            Logger.Write(Str.RemoveBefore(T, '调'));
            Logger.Write(Str.RemoveAfter(T, '调'));

            Logger.Write(Str.Substring(T, 1));
            Logger.Write(Str.TrimToLength(T, 3, "..."));

            string T3 = "aa-asd-aa";

            Logger.Info("长度不变的处理");

            Logger.Write(T3 = Str.Capitalize(T3));
            Logger.Write(Str.UnCapitalize(T3));
            Logger.Write(T3 = Str.ToCamel(T3));
            Logger.Write(T3 = Str.ToPascal(T3));
            Logger.Write(T3 = Str.Hyphenate(T3));
            Logger.Write(Str.ToLength(T3, 10));
            Logger.Write(Str.ToLength(T3, 8));

            Logger.Info("字符性质");

            Logger.Write(Str.CLength(T));
            Logger.Write(Str.Count(T, '用'));
            Logger.Write(Str.Count("aaabbbaaaa", "aa"));

            Logger.Info("编码");

            Logger.Write(int.Parse("12fe", System.Globalization.NumberStyles.HexNumber));


            Logger.Write(Str.HtmlEncode("<br>sdasd<div>sdasd</div>"));

            Logger.Write(Str.HtmlToString("<br>sdasd<div>sdasd</div>"));

            Logger.Write(Str.HtmlNoScript("<script>ddd</script><div onclick='alert()' style='height:expression(alert());font-size:2' sa='ggg'></div>"));

            Logger.Write(Str.UnicodeDecode(Str.UnicodeEncode("不飞asdaa?=s", "%u"), "%u"));
            Logger.Write(Str.UrlEncode("不飞asdaa?=s"));
            Logger.Write(Str.Unescape(Str.Escape("不飞asdaa?=s ")));


            ArrayString s = new ArrayString("ss,ss,dasd,asd", ',');
            
            s.Add("asfdsf");
            s.RemoveAt(0);
            Logger.Write(s.Count);

            Logger.Write(s.Length);

            Logger.Write(s.ToString());

        }
    }
}
