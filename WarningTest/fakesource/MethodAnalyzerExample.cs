using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarningTest.fakesource
{
    class MethodAnalyzerExample
    {
        public void method1()
        {
            
        }

        public int method2()
        {
            return 1;
        }

        public IEnumerable method3()
        {
            return Enumerable.Empty<int>();
        }

        public void method4(int a)
        {
        }

        public IEnumerable<Object> method5(int a, int b, bool c, Object d, IEnumerable<int> e)
        {
            return Enumerable.Empty<Object>();
        }

    }
}
