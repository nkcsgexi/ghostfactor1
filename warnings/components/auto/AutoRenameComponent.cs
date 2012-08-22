using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.util;

namespace warnings.components
{
    class AutoRenameComponent : IFactorComponent
    {

        /* Singleton this component. */
        private static IFactorComponent instance = new AutoRenameComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        /* Workqueue for running the automatic renaming. */
        private readonly WorkQueue queue;


        private AutoRenameComponent()
        {
            // Initialize a workqueue running a single thread.
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;

        }

        public void Enqueue(IWorkItem item)
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            return "AutoRenameComponent";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }
    }

    public class AutoRenameWorkItem : WorkItem
    {
        private IRenameService service;
        private IDocument document;
        private ISymbol symbol;

        public AutoRenameWorkItem(IDocument document, ISymbol symbol)
        {
            service = ServiceArchive.getInstance().RenameService;
            this.document = document;
            this.symbol = symbol;
        }

        public override void Perform()
        {
           // TODO: finsih the perform auto refactoring.
        }
    }
}
