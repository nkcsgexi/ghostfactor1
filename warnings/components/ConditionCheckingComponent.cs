using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using warnings.conditions;
using warnings.refactoring;
using warnings.util;

namespace warnings.components
{
    internal class ConditionCheckingComponent : IFactorComponent
    {
        private static IFactorComponent instance = new ConditionCheckingComponent();
       
        public static IFactorComponent GetInstance()
        {
            return instance;
        }

   
        private readonly WorkQueue queue;
        private readonly Logger logger;

        private ConditionCheckingComponent()
        {
            // A single thread workqueue.
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;
            
            // Set listener for failed work item.
            queue.FailedWorkItem += onFailedItem;

            // Initiate the logger.
            logger = NLoggerUtil.getNLogger(typeof (ConditionCheckingComponent));
        }

        private void onFailedItem(object sender, WorkItemEventArgs workItemEventArgs)
        {
            logger.Fatal("Work item failed.");
        }

        public void Enqueue(IWorkItem item)
        {
            if(queue.Count == 0)
            {
                logger.Info("enqueue");
                queue.Add(item);
            }
        }

        public string GetName()
        {
            return "Condition Checking Component";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }
    }

    class ConditionCheckWorkItem : WorkItem
    {
        private readonly IManualRefactoring refactoring;
        private readonly IDocument after;
        private readonly IDocument before;
        private readonly Logger logger;

        public ConditionCheckWorkItem (IDocument before, IDocument after, IManualRefactoring refactoring)
        {
            this.before = before;
            this.after = after;
            this.refactoring = refactoring;
            logger = NLoggerUtil.getNLogger(typeof (ConditionCheckWorkItem));
           
        }

        public override void Perform()
        {
            try
            {
                refactoring.MapToDocuments(before, after);
                IEnumerable<ICheckingResult> result = Enumerable.Empty<ICheckingResult>();
                switch (refactoring.type)
                {
                    case RefactoringType.EXTRACT_METHOD:
                        logger.Info("Checking conditions for extract method.");
                        result = ConditionCheckingFactory.GetExtractMethodConditionsList().CheckAllConditions(before, after, refactoring);
                        break;

                    case RefactoringType.RENAME:
                        logger.Info("Checking conditions for rename.");
                        result = ConditionCheckingFactory.GetRenameConditionsList().CheckAllConditions(before, after, refactoring);
                        break;

                    default:
                        logger.Fatal("Unknown refactoring type for conditions checking.");
                        break;
                }
            }catch(Exception e)
            {
                logger.Fatal(e);
            }
        }
    }

}
