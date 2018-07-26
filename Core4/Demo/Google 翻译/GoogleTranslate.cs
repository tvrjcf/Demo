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



using System.Text;
using System.Net;
using System.IO;

using Py.Json;
using Py.Net;
using Py.Logging;
using System;

namespace Xuld {

    /// <summary>
    /// 表示一个用于支持 谷歌翻译 提供接口的工具。
    /// </summary>
    public class GoogleTranslate {

        const string URL = "http://translate.google.com/translate_a/t";

        WebBrowser _wb = new WebBrowser();

        /// <summary>
        /// 初始化 <see cref="Xuld.GoogleTranslate"/> 的新实例。
        /// </summary>
        public GoogleTranslate() {
            SourceLanguage = "tl";
            TargetLanguage = "zh-CN";
        }

        /// <summary>
        /// 翻译一个字符串。
        /// </summary>
        /// <param name="word">要翻译的字符串。</param>
        /// <returns>翻译后的字符串。</returns>
        public string Translate(string word) {
            return Translate(word, false);
        }

        /// <summary>
        /// 翻译一个字符串。
        /// </summary>
        /// <param name="word">要翻译的字符串。</param>
        /// <param name="decectLanguage">如果为 true ，则自动决定翻译的语言是 中英 还是 英中 。</param>
        /// <returns>翻译后的字符串。</returns>
        public string Translate(string word, bool decectLanguage) {
            if(decectLanguage && Py.Core.Check.IsChinese(word)){
                return Translate(word, "zh-CN", SourceLanguage == "zh-CN" ? TargetLanguage : SourceLanguage);
            }
            return Translate(word, SourceLanguage, TargetLanguage);
        }

        /// <summary>
        /// 翻译一个字符串。
        /// </summary>
        /// <param name="word">要翻译的字符串。</param>
        /// <param name="sourceLanguage">来源语言。</param>
        /// <param name="targetLanguage">目标语言。</param>
        /// <returns>翻译后的字符串。</returns>
		public string Translate(string word, string sourceLanguage, string targetLanguage) {
            if (String.IsNullOrEmpty(word))
                return String.Empty;

            _wb.Open(URL, Py.Core.Str.FormatX("client=t&hl=zh-CN&otf=2&pc=1&sl={1}&tl={0}&text={2}", targetLanguage, sourceLanguage, Py.Core.Str.UrlEncode( word )));
            JsonArray json = new JsonArray(_wb.SourceCode);
              return json[0][0][0].Value.ToString();
		}

        /// <summary>
        /// 设置翻译的来源语言名字。 默认 en (英语) 。
        /// </summary>
        public string SourceLanguage {
            get;
            set;
        }

        /// <summary>
        /// 设置翻译的目标的语言名字。 默认 zh-cn (中文) 。
        /// </summary>
        public string TargetLanguage {
            get;
            set;
        }

	}

}
