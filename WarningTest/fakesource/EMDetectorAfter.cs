﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarningTest.fakesource.after
{
    public class FakeClass
    {
        public void method1()
        {
            int k;
            k = 2;
            extracted1();
        }
        private void extracted1()
        {
            int i;
            int j;
            i = j = 10;
        }

        public void method2()
        {
            int i = 0;
            int j = 0;
            extracted2(i, j);
            Console.WriteLine(i);
        }

        private void extracted2(int i, int j)
        {
            i = i + j;
        }
    }
}
