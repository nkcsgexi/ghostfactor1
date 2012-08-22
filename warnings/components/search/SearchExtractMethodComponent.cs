using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.components
{
    class SearchExtractMethodComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }


        private SearchExtractMethodComponent()
        {
            
        }

        public void Enqueue(object item)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "SearchExtractMethodComponent";
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
