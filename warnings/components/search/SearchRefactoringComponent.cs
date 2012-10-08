using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.source;
using warnings.source.history;
using warnings.util;

namespace warnings.components
{
    /* Component for searching a manual refactoring in the code history. */
    public abstract class SearchRefactoringComponent : IFactorComponent, ILoggerKeeper
    {
        /* Queue for handling all the detection */
        private readonly WorkQueue queue;

        /* Logger for this detection component. */
        protected readonly Logger logger;

        protected SearchRefactoringComponent()
        {
            // Single thread workqueue. 
            queue = new WorkQueue {ConcurrentLimit = 1};
            queue.FailedWorkItem += onFailedWorkItem;

            // Initialize the logger.
            logger = GetLogger();
        }

        private void onFailedWorkItem(object sender, WorkItemEventArgs e)
        {
            logger.Fatal("WorkItem failed.");
        }

        public void Enqueue(IWorkItem item)
        {
            logger.Info("enqueue");
            queue.Add(item);
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }

        /* Get the name of current component. */
        public abstract string GetName();

        /* Get the logger of this class. */
        public abstract Logger GetLogger();
    }

    /* The kind of work item for search refactoring component. */
    public abstract class SearchRefactoringWorkitem : WorkItem, ILoggerKeeper
    {
        /* The latest code history record from where the detector trace back. */
        private readonly ICodeHistoryRecord latestRecord;

        protected readonly Logger logger;

        protected SearchRefactoringWorkitem(ICodeHistoryRecord latestRecord)
        {
            this.latestRecord = latestRecord;
            this.logger = GetLogger();
        }

        public override void Perform()
        {
            try
            {
                // get the detector, a detector compare two versions of a single file.
                IExternalRefactoringDetector detector = getRefactoringDetector();

                // The detector shall always have latestRecord as the source after.
                detector.SetSourceAfter(latestRecord.GetSource());

                // The current record shall be latestRecord initially.
                ICodeHistoryRecord currentRecord = latestRecord;

                // we only look back up to the bound given by getSearchDepth()
                for (int i = 0; i < getSearchDepth(); i++)
                {
                    // No record before current, then break.s
                    if (!currentRecord.HasPreviousRecord())
                        break;

                    // get the record before current record
                    currentRecord = currentRecord.GetPreviousRecord();

                    // Set the source before
                    detector.SetSourceBefore(currentRecord.GetSource());

                    // Detect manual refactoring.
                    if (detector.HasRefactoring())
                    {
                        onRefactoringDetected(currentRecord, latestRecord, detector.GetRefactorings());

                        // If refactoring detected, return directly.
                        return;
                    }
                }
                onNoRefactoringDetected(latestRecord);
            }
            catch (Exception e)
            {
                logger.Fatal(e);
            }
        }
        
        protected abstract IExternalRefactoringDetector getRefactoringDetector();

        /* Get the number of record we look back to find a manual refactoring. */
        protected abstract int getSearchDepth();
        
        /* Called when manual refactoring is detected. */
        protected abstract void onRefactoringDetected(ICodeHistoryRecord before, ICodeHistoryRecord after, 
            IEnumerable<IManualRefactoring> refactorings);
        
        /* Called when no manual refactoring is detected. */
        protected abstract void onNoRefactoringDetected(ICodeHistoryRecord after);
        
        public abstract Logger GetLogger();
    }

}
