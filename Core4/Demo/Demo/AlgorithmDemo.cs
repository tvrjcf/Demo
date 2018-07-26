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

using Py.Algorithm;
using Py.Logging;
using Py.Core;

namespace Py.Demo {
    class AlgorithmDemo :DemoBase {

        const string s = "qqqqqqqqqqq";

        void Do(Encryption.Algorithm fn, Encryption.Algorithm fn2) {
            string c;
            Logger.Write("{0}(\"{1}\") -> {2}", fn.Method.Name, s, c =fn(s));
			Logger.Write("{0}(\"{1}\") -> {2}", fn2.Method.Name, c, fn2(c));
        }

        public override void Start() {

            #region 准备

			Logger.Start("加密");

            string c;

            #endregion

            #region 测试

            Do(Encryption.EncryptString, Encryption.DecryptString);
            Logger.Write("Encryption.MD5(\"{0}\")  ->  {1}", s, Encryption.MD5(s));
            Logger.Write("Encryption.MD5t(\"{0}\")  ->  {1}", s, Encryption.MD5t(s));
            Logger.Write("Encryption.MD5d(\"{0}\")  ->  {1}", s, Encryption.MD5d(s));
            Logger.Write("Encryption.MD5x(\"{0}\")  ->  {1}", s, Encryption.MD5x(s));
            Logger.Write("Encryption.SHA1(\"{0}\")  ->  {1}", s, Encryption.SHA1(s));
            Logger.Write("Encryption.EncryptAES(\"{0}\")  ->  {1}", s, c = Encryption.EncryptAES(s, "11", "1"));
            Logger.Write("Encryption.DecryptAES(\"{0}\")  ->  {1}", c, Encryption.DecryptAES(c, "11", "1"));
            Logger.Write("Encryption.EncryptDES(\"{0}\")  ->  {1}", s, c = Encryption.EncryptDES(s, "11", "1"));
            Logger.Write("Encryption.DecryptDES(\"{0}\")  ->  {1}", c, Encryption.DecryptDES(c, "11", "1"));

            #endregion

        }
    }
}
