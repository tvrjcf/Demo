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

namespace Py.Demo {
	public class CollectionDemo : DemoBase {


		public override void Start() {


            Logger.Write("集合");


            Logger.Info("集合");

            MyCollection c = new MyCollection();
            c.Add(3);
            c.Remove(1);   //  不存在
            c.Remove(3);

            Logger.Info("字典");


            MyDictionary d = new MyDictionary();
            d[1] = "444";
            d.Add(1, "444");
            d.Add(1, "555");  // 重复
            Logger.Write(d);

            Logger.Write("d[0]  ->   {0}",   d[0]);

		}


        class MyCollection :Collection<int> {



            protected override bool OnAddItem(int index, int item) {
                Logger.Write("加  {0}", item);
                return base.OnAddItem(index, item);
            }


            protected override bool OnRemoveItem(int index) {
                Logger.Write("除  {0}", this[index]);
                return base.OnRemoveItem(index);
            }
        }

        class MyDictionary :Dictionary<int, string> {



            protected override bool OnAddExistKey(int key) {
                // 忽视重复的 key
                return false;
            }

            protected override string OnKeyNotFound(int key) {
                return "找不到";
            }

        }
	}
}
