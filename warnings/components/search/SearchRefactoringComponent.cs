﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using warnings.refactoring.detection;
using warnings.source;
using warnings.util;

namespace warnings.components
{
    public abstract class SearchRefactoringComponent : IFactorComponent, ILoggerKeeper
    {
        /* Queue for handling all the detection */
        private readonly WorkQueue queue;

        /* Logger for this detection component. */
        protected readonly Logger logger;

        protected SearchRefactoringComponent()
        {
            // Single thread workqueue. 
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;
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
            if (queue.Count == 0)
            {
                logger.Info("enqueue");
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

    /* The kind of work item for search refactoring component. */
    public abstract class SearchRefactoringWorkitem : WorkItem, ILoggerKeeper
    {
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
               IExternalRefactoringDetector detector = getRefactoringDetector();

                // The detector shall always have latestRecord as the source after.
                detector.setSourceAfter(latestRecord.getSource());

                // The current record shall be latestRecord initially.
                ICodeHistoryRecord currentRecord = latestRecord;

                for (int i = 0; i < getSearchDepth(); i++)
                {
                    // No record before current, then break.s
                    if (!currentRecord.hasPreviousRecord())
                        break;
                    currentRecord = currentRecord.getPreviousRecord();

                    // Set the source before
                    detector.setSourceBefore(currentRecord.getSource());

                    // Detect manual refactoring.
                    if (detector.hasRefactoring())
                    {
                        onRefactoringDetected(detector);

                        // If refactoring detected, return directly.
                        return;
                    }
                }
                onNoRefactoringDetected();
            }
            catch (NullReferenceException e)
            {
                logger.Fatal(e);
            }
        }
        
        protected abstract IExternalRefactoringDetector getRefactoringDetector();

        /* Get the number of record we look back to find a manual refactoring. */
        protected abstract int getSearchDepth();
        
        /* Called when manual refactoring is detected. */
        protected abstract void onRefactoringDetected(IExternalRefactoringDetector detector);
        
        /* Called when no manual refactoring is detected. */
        protected abstract void onNoRefactoringDetected();
        
        public abstract Logger GetLogger();
    }

}