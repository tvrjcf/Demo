/* *****************************************************************************
 *
 * Copyright (c) 2009-2010 Xuld. All rights reserved.
 * 
 * Project Url: http://play.xuld.net
 * 
 * This source code is a demo page for the 
 * Project Py.Core for .Net .
 * 
 * This code is licensed under MIT Licence.
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
using Py.Json;


using System;

namespace Xuld {

    /// <summary>
    /// 提供百度搜索提示。
    /// </summary>
    public class BSearch {

        const string URI = "http://suggestion.baidu.com/su?wd={0}&action=opensearch&ie=utf-8&from=ie8";

        Py.Net.WebBrowser _wb = new Py.Net.WebBrowser();

        public string[] GetList(string word) {
            if (String.IsNullOrEmpty(word))
                return new string[] { };
            _wb.Open(Str.FormatX(URI, Str.UrlEncode(word)));
            var a = new JsonArray(_wb.SourceCode).GetJsonArray(1);
            return a.ToArray();
        }

        public void GetSynax(string word, Action<string[]> act) {
            _wb.OpenAsync(new Uri(Str.FormatX(URI, word)),t => {
                if (!t.IsCompleted)
                    return;
                var a = new JsonArray(_wb.SourceCode).GetJsonArray(1);
                act(a.ToArray());
            }, null
           );
        }

    }
}
