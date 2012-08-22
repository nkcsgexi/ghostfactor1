using System;
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
    /* This component is for detecting manual extract method refactoring in the code history. */
    class SearchExtractMethodComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        /* The maximum number of record it should look back to. */
        public readonly static int SEARCH_DEPTH = 40;

        /* The queue handles all the detections. */
        private readonly WorkQueue queue;

        private readonly Logger log;

        private SearchExtractMethodComponent()
        {
            // Single thread workqueue. 
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;

            // Initialize the logger.
            log = NLoggerUtil.getNLogger(typeof (SearchExtractMethodComponent));
        }

        public void Enqueue(IWorkItem item)
        {
            if(queue.Count == 0)
            {
                log.Info("enqueue");
                queue.Add(item);
            }
        }

        public string GetName()
        {
            return "SearchExtractMethodComponent";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
            
        }

    }

    /* The kind of work item for search extract method component. No other kind of component is allowed. */
    public class SearchExtractMethodWorkitem : WorkItem
    {
        private readonly ICodeHistoryRecord latestRecrod;
        private readonly Logger logger;

        public SearchExtractMethodWorkitem(ICodeHistoryRecord latestRecord)
        {
            this.latestRecrod = latestRecrod;

            // Initilize the logger.
            this.logger = NLoggerUtil.getNLogger(typeof (SearchExtractMethodWorkitem));
        }

        public override void Perform()
        {
            IExtractMethodDetector detector = new ExtractMethodDetector();

            // The detector shall always have latestRecord as the source after.
            detector.setSourceAfter(latestRecrod.getSource());

            // The current record shall be latestRecord initially.
            ICodeHistoryRecord currentRecord = latestRecrod;

            for (int i = 0; i < SearchExtractMethodComponent.SEARCH_DEPTH; i++)
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
                    //TODO: posting warnings and quick fix.
                    logger.Info("Extract Method dectected.");
                    logger.Info("Before: \n" + detector.getSourceBefore());
                    logger.Info("After: \n" + detector.getSourceAfter());

                    // If refactoring detected, return directly.
                    return;
                }
            }
            logger.Info("No extract method detected.");
        }
    }
}
