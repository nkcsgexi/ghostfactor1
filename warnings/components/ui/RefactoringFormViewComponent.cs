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
    public delegate void ControlUpdate();

    /*
     * This the view part in the MVC pattern. It registers to the event of code issue changes. When code issues change, this component
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
            GhostFactorComponents.RefactoringCodeIssueComputerComponent.globalWarningsReady += OnGlobalWarningsReady;
         }

        private void OnGlobalWarningsReady(IEnumerable<IRefactoringWarningMessage> messages, bool isAdded)
        {
            shortTaskQueue.Add(new UpdateWarningWorkItem(form, messages, isAdded));
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
            private readonly IEnumerable<IRefactoringWarningMessage> messages;
            private readonly bool isAdded;

            internal UpdateWarningWorkItem(RefactoringWariningsForm form, 
                IEnumerable<IRefactoringWarningMessage> messages, bool isAdded)
            {
                this.form = form;
                this.messages = messages;
                this.isAdded = isAdded;
                this.logger = NLoggerUtil.GetNLogger(typeof (UpdateWarningWorkItem));
            }

            public override void Perform()
            {
                // Add messages to the form. 
                form.Invoke(new ControlUpdate(UpdateRefactoringWarnings));
            }

            private void UpdateRefactoringWarnings()
            {
                if (isAdded)
                {
                    logger.Info("Adding messages to the form.");
                    form.AddRefactoringWarnings(messages);
                }
                else
                {
                    logger.Info("Removing messages from the form.");
                    form.RemoveRefactoringWarnings(messages);
                }
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
