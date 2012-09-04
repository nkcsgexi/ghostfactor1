using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WarningTest.fakesource.before
{
    public class FakeClass
    {
        public void method1()
        {
            int i;
            int j;
            i = j = 10;
        }

        public void method2()
        {
            int i = 0; 
            int j = 0;
            i = i + j;
            Console.WriteLine(i);       
        }
    }
}
