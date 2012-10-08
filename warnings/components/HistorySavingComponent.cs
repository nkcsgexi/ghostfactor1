using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.analyzer;
using warnings.components.search;
using warnings.configuration;
using warnings.refactoring;
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
        private readonly int TIME_INTERVAL = GlobalConfigurations.GetSnapshotTakingInterval();

        private HistorySavingComponent()
        {
            // Initialize the workqueue.
            this.queue = new WorkQueue();

            // Disallow the concurrency for this component.
            this.queue.ConcurrentLimit = 1;

            // Log the event if an item failed.
            this.queue.FailedWorkItem += onFailedWorkItem;

            // Initiate the component timer.
            this.timer = new ComponentTimer( TIME_INTERVAL, TimeUpHandler);

            // Initialize the logger used in this component.
            logger = NLoggerUtil.GetNLogger(typeof (HistorySavingComponent));
        }

        /* Add a new work item to the queue. */
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
            // If no active document is not null, continue with saving.
            if (activeDocument != null)
            {
                lock (activeDocument)
                {
                    // When timer is triggered, save current active file to the versions. 
                    queue.Add(new HistorySavingWorkItem(activeDocument));
                }
            }
        }

        /* Method called when a workitem failed. */
        private void onFailedWorkItem(object sender, WorkItemEventArgs workItemEventArgs)
        {
            logger.Fatal("WorkItem failed.");
        }

        /* The work item supposed to added to HistorySavingComponent. */
        internal class HistorySavingWorkItem : WorkItem
        {
            private readonly String solutionName;
            private readonly String namespaceName;
            private readonly String fileName;
            private readonly String code;
            private readonly Logger logger;

            /* Retrieve all the properties needed to save this new record. */
            internal HistorySavingWorkItem(IDocument document)
            {
                fileName = document.Name;
                namespaceName = document.Project.Name;

                // TODO: can we get the real solution name?
                solutionName = "solution";
                code = document.GetText().GetText();
                logger = NLoggerUtil.GetNLogger(typeof(HistorySavingWorkItem));
            }

          

            public override void Perform()
            {
                try
                {
                    logger.Info(solutionName + "," + namespaceName + "," + fileName);

                    // Log the saved code if needed.
                    // logger.Info(Environment.NewLine + code);

                    // Add the new IDocuemnt to the code history.
                    CodeHistory.GetInstance().addRecord(solutionName, namespaceName, fileName, code);
                    
                    // Add work item to search component.
                    AddSearchRefactoringWorkItem();
                }
                catch (Exception e)
                {
                    // Stacktrace of Exception will be logged.
                    logger.Fatal(e.StackTrace);
                }
            }

            private void AddSearchRefactoringWorkItem()
            {
                // Get the latest record of the file just editted.    
                ICodeHistoryRecord record = CodeHistory.GetInstance().GetLatestRecord(solutionName, namespaceName, fileName);

                if (GlobalConfigurations.IsSupported(RefactoringType.EXTRACT_METHOD))
                {
                    // Search for extract method refactoring.
                    GhostFactorComponents.searchExtractMethodComponent.Enqueue(new SearchExtractMethodWorkitem(record));
                }
                
                if(GlobalConfigurations.IsSupported(RefactoringType.RENAME))
                {
                    // Search for rename refacotoring.
                    GhostFactorComponents.searchRenameComponent.Enqueue(new SearchRenameWorkItem(record));
                }

               if(GlobalConfigurations.IsSupported(RefactoringType.CHANGE_METHOD_SIGNATURE))
               {
                   // Search for change method signature refactorings.
                   GhostFactorComponents.searchChangeMethodSignatureComponent.Enqueue(
                       new SearchChangeMethodSignatureWorkItem(record));
               }
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
