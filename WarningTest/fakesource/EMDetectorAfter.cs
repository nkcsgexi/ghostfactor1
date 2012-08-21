using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarningTest.fakesource.after
{
    public class FakeClass
    {
        public void fake()
        {
            int k;
            k = 2;
            foo();
        }
        private void foo()
        {
            int i;
            int j;
            i = j = 10;
        }
    }
}
