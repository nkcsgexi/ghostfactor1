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
    class SearchExtractMethodComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        /* The maximum number of record it should look back to. */
        private readonly static int SEARCH_DEPTH = 40;

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

        internal class SearchExtractMethodWorkitem : WorkItem
        {
            private readonly ICodeHistoryRecord latestRecrod;
            private readonly Logger logger;

            SearchExtractMethodWorkitem(ICodeHistoryRecord latestRecord)
            {
                this.latestRecrod = latestRecrod;
                
                // Initilize the logger.
                this.logger = NLoggerUtil.getNLogger(typeof(SearchExtractMethodWorkitem));
            }

            public override void Perform()
            {
                IExtractMethodDetector detector = new ExtractMethodDetector();

                // The detector shall always have latestRecord as the source after.
                detector.setSourceAfter(latestRecrod.getSource());

                // The current record shall be latestRecord initially.
                ICodeHistoryRecord currentRecord = latestRecrod;

                for (int i = 0; i < SEARCH_DEPTH; i++)
                {
                    if (!currentRecord.hasPreviousRecord())
                        break;
                    currentRecord = currentRecord.getPreviousRecord();
                    
                    // Set the source before
                    detector.setSourceBefore(currentRecord.getSource());

                    // Detect manual refactoring.
                    if(detector.hasRefactoring())
                        logger.Info("Extract Method dectected.");
                }
            }
        }
    }
}
