using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;

namespace warnings.components
{
    class SearchRenameComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchRenameComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        private SearchRenameComponent()
        {
            
        }

        public void Enqueue(IWorkItem item)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "SearchRenameComponent";
        }

        public int GetWorkQueueLength()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
      
        }
    }
}
