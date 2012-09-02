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
        private readonly WorkQueue queue;
        private readonly Logger logger;

        ConditionCheckingComponent()
        {
            // A single thread workqueue.
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;

            // Initiate the logger.
            logger = NLoggerUtil.getNLogger(typeof (ConditionCheckingComponent));
        }

        public void Enqueue(IWorkItem item)
        {
            if(item is ConditionCheckWorkItem && queue.Count == 0)
            {
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
        private IManualRefactoring refactoring;
        private IDocument after;
        private IDocument before;

        public ConditionCheckWorkItem (IDocument before, IDocument after, IManualRefactoring refactoring)
        {
            this.before = before;
            this.after = after;
            this.refactoring = refactoring;
        }

        public override void Perform()
        {
            IEnumerable<ICheckingResult> result;
            switch (refactoring.type)
            {
                case RefactoringType.EXTRACT_METHOD:
                    result = RenameConditionsList.GetInstance().CheckAllConditions(before, after, refactoring);
                    break;

                case RefactoringType.RENAME:
                    result = ExtractMethodConditionsList.GetInstance().CheckAllConditions(before, after, refactoring);
                    break;
            }
            // TODO: push result to another component.

        }
    }

}
