using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.components
{
    class AutoExtractMethodComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new AutoExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        public void Enqueue(object item)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public int GetWorkQueueLength()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
