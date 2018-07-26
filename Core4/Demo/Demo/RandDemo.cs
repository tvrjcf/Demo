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

namespace Py.Demo {

	/// <summary>
	/// 随机数演示。
	/// </summary>
	public class RandDemo : DemoBase {

		public override void Start() {

			Logger.Info("基本");

            Logger.Write("数字", Rand.Int());
            Logger.Write("字符", Rand.String(2));

            Logger.Info("类型");
            Logger.Write(Rand.Bool());
            Logger.Write(Rand.Char());
            Logger.Write(Rand.DateTime(300));
            Logger.Write(Rand.Double());

            Logger.Info("工具");
            Logger.Write(Rand.Dight());
            Logger.Write(Rand.Literal(2));
            Logger.Write(Rand.Id(2));
            Logger.Write(Rand.Letter());
            Logger.Write(Rand.Number(2));
            Logger.Write(Rand.Password());
            Logger.Write(Rand.UserName());

            Logger.Info("分配");
            Logger.Write(Rand.Permutation(103, 4, true));
            Logger.Write(Rand.Permutation(103, 4, false));;

		}
	}
}
