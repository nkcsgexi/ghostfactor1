using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.configuration;
using warnings.ui;
using warnings.util;

namespace warnings.components.ui
{
    /* delegate for update a control component. */
    public delegate void ControlUpdate(Object o);

    /*
     * This the view in the MVC pattern. It registers to the event of code issue changes. When code issues change, this component
     * will ask the latest issues and update the form.
     */
    internal class RefactoringFormViewComponent : IFactorComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new RefactoringFormViewComponent();

        public static IFactorComponent GetInstance()
        {
            return instance;
        }

        /* A work queue for long running task, such as keeping the windows displaying. */
        private WorkQueue longRunningQueue;

        /* A work queue for short running task, such as updating items to the form. */
        private WorkQueue shortTaskQueue;
    
        /* The form instance where new warnings should be added to. */
        private RefactoringWariningsForm form;

        private RefactoringFormViewComponent()
        {
            form = new RefactoringWariningsForm();
            longRunningQueue = new WorkQueue() {ConcurrentLimit = 1};
            shortTaskQueue = new WorkQueue(){ConcurrentLimit = 1};
            GhostFactorComponents.RefactoringCodeIssueComputerComponent.changeEvent += RefactoringIssuesChanged;
         }

        private void RefactoringIssuesChanged()
        {
            shortTaskQueue.Add(new UpdateWarningWorkItem(form));
        }

        public void Enqueue(IWorkItem item)
        {
            shortTaskQueue.Add(item);
        }

        public string GetName()
        {
            return "Refactoring Form Component";
        }

        public int GetWorkQueueLength()
        {
            return shortTaskQueue.Count;
        }

        public void Start()
        {
            // Create an work item for showing dialog and add this work item
            // to the work longRunningQueue.
            longRunningQueue.Add(new ShowingFormWorkItem(form));
        }

        /* Work item for updating workitem in the form. */
        private class UpdateWarningWorkItem : WorkItem
        {
            private readonly RefactoringWariningsForm form;
            private readonly Logger logger;
            
            internal UpdateWarningWorkItem(RefactoringWariningsForm form)
            {
                this.form = form;
                this.logger = NLoggerUtil.GetNLogger(typeof (UpdateWarningWorkItem));
            }

            public override void Perform()
            {
                // Get the messages in the entire solution. 
                var solution = GhostFactorComponents.searchRealDocumentComponent.GetSolution();
                var messages = GhostFactorComponents.RefactoringCodeIssueComputerComponent.
                    GetRefactoringWarningMessages(solution);

                // Add messages to the form. 
                form.Invoke(new ControlUpdate(AddRefactoringWarnings), messages);
            }

            private void AddRefactoringWarnings(object messages)
            {
                logger.Info("Add warnings to the warning form.");
                form.AddRefactoringWarnings((IEnumerable<IRefactoringWarningMessage>)messages);    
            }
        }

        /* Work item for showing the form, unlike other workitem, this work item does not stop. */
        private class ShowingFormWorkItem : WorkItem
        {
            private readonly Form form;

            internal ShowingFormWorkItem(Form form)
            {
                this.form = form;
            }

            public override void Perform()
            {
                form.ShowDialog();
            }
        }
    }
}
