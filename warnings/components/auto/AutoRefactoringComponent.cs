using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using warnings.util;

namespace warnings.components
{

    /* Abstract for automatic refactoring. */
    internal abstract class AutoRefactoringComponent : IFactorComponent, ILoggerKeeper
    {
        /* Work queue for scheduling an automatic refactoring. */
        private WorkQueue queue;

        /* Current logger*/
        protected Logger logger { get; private set; }

        protected AutoRefactoringComponent()
        {
            // Initialize work queue with single thread.
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;
            queue.FailedWorkItem += onFailedWorkItem;

            // Initialize the logger.
            logger = GetLogger();
        }

        private void onFailedWorkItem(object sender, WorkItemEventArgs workItemEventArgs)
        {
            logger.Fatal("WorkItem Failed.");
        }


        public void Enqueue(IWorkItem item)
        {
            if (queue.Count == 0)
            {
                logger.Info("Enqueue");
                queue.Add(item);
            }
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }

        public abstract string GetName();

        public abstract Logger GetLogger();
    }


    public abstract class AutoRefactoringWorkItem : WorkItem, ILoggerKeeper
    {

        protected readonly Logger logger;

        /* to where the refactoring shall be performed. */
        protected readonly IDocument document;

        protected AutoRefactoringWorkItem(IDocument document)
        {
            this.logger = GetLogger();
            this.document = document;
        }

        public abstract Logger GetLogger();
    }
}
