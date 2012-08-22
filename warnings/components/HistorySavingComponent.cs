using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using warnings.source;
using warnings.source.history;
using warnings.util;

namespace warnings.components
{
    /* Component for recording new version of a source code file.*/
    public class HistorySavingComponent : IFactorComponent
    {
        /* Singleton the instance. */
        private static HistorySavingComponent instance = new HistorySavingComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        /* Internal work queue for handling all the tasks. */
        private readonly WorkQueue queue;

        /* logger for the history saving component. */
        private readonly Logger logger;

        /* Current active document. */
        private IDocument activeDocument;

        /* Timer for triggering saving current version. */
        private readonly ComponentTimer timer;

        /* Timer interval used by timer. */
        private readonly int TIME_INTERVAL = 5000;

        private HistorySavingComponent()
        {
            // Initialize the workqueue.
            this.queue = new WorkQueue();

            // Disallow the concurrency for this component.
            this.queue.ConcurrentLimit = 1;

            // Initiate the component timer.
            this.timer = new ComponentTimer( TIME_INTERVAL, TimeUpHandler);

            // Initialize the logger used in this component.
            logger = NLoggerUtil.getNLogger(typeof (HistorySavingComponent));
        }

        /* Add a new work item to the queue. */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Enqueue(IWorkItem item)
        {
            activeDocument = ((DocumentWorkItem)item).document;
        }

        /* Return the name of this work queue. */
        public string GetName()
        {
            return "HistorySavingComponent";
        }

        /* The length of this work queue. */
        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        /* Start this component by starting the timing thread. */
        public void Start()
        {
            this.timer.start();
        }

        /* handler when time up event is triggered. */
        private void TimeUpHandler(object o, EventArgs args)
        {
            logger.Info("Time up handler.");
            if(activeDocument != null && queue.Count == 0)
            {
                logger.Info("enqueue");

                // When timer is triggered, save current active file to the versions. 
                queue.Add(new HistorySavingWorkItem(activeDocument));
            }
        }

        /* The work item supposed to added to HistorySavingComponent. */
        internal class HistorySavingWorkItem : WorkItem
        {
            private readonly String solutionName;
            private readonly String namespaceName;
            private readonly String fileName;
            private readonly String code;
            private readonly Logger log;

            /* Retrieve all the properties needed to save this new record. */
            internal HistorySavingWorkItem(IDocument document)
                : base()
            {
                fileName = document.DisplayName;
                namespaceName = document.Project.DisplayName;

                // TODO: can we get the real solution name?
                solutionName = "solution";
                code = document.GetText().GetText();
                log = NLoggerUtil.getNLogger(typeof(HistorySavingWorkItem));
            }

            public override void Perform()
            {
                log.Info(solutionName + "," + namespaceName + "," + fileName);
                log.Info(code);

                // Add the new IDocuemnt to the code history.
                CodeHistory.getInstance().addRecord(solutionName, namespaceName, fileName, code);

                // Get the latest record of the file just editted.    
                ICodeHistoryRecord record = CodeHistory.getInstance().getLatestRecord(solutionName, namespaceName, fileName);
                
                // After add the new record, search for extract method refactoring.
                GhostFactorComponents.searchExtractMethodComponent.Enqueue(new SearchExtractMethodWorkitem(record));
               
            }
        }
    }

    /* This is a wrapper for IDocument using a WorkItem abstract class. */
    public class DocumentWorkItem : WorkItem
    {
        public IDocument document { get; private set; }

        public DocumentWorkItem (IDocument document)
        {
            this.document = document;
        }
        public override void Perform()
        {
        }
    }



}
