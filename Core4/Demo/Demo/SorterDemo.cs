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

namespace Py.Demo {
	public class SorterDemo: DemoBase {

		Comparison<int> o = new Comparison<int>(Compare);

		void Test(Sorter.Algorithm<int> s){
			int[] list = new int[] { 1, 2, 34, 5, 2, 3, 2, 454, 123, 23, 12, 12, 3, 12 };
			;
			s(list, 0, list.Length, Compare);
            Logger.Write("{0}      {1}", s.Method.Name, list);
		}

		public override void Start() {
			Test(Sorter.BubbleSort<int>);
            Test(Sorter.BucketSort);
			//Test(Sorter.Count);
            Test(Sorter.HeapSort<int>);
            Test(Sorter.InsertSort<int>);
            Test(Sorter.SelectSort<int>);
            Test(Sorter.QuickSort<int>);
            Test(Sorter.ShellSort<int>);
            Test(Sorter.MergeSort<int>);

			Sorter.Sort(new int[] { });
		}


		public static int Compare(int a, int b) {
			return a - b;
		}

	}
}
