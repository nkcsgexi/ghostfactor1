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
    /*
     * A public interface for refactoring error form component, where new refactoring messages
     * can be added and removed.
     */
    public interface IRefactoringFormComponent : IFactorComponent
    {
        void AddWarning(IEnumerable<string> messages);
        void RemoveWarning(IEnumerable<string> messages);
        void UpdateWarnings(IEnumerable<CodeIssue> issues);
    }

    internal class RefactoringFormComponent : IRefactoringFormComponent
    {
        /* Singleton this component. */
        private static IRefactoringFormComponent instance = new RefactoringFormComponent();

        public static IRefactoringFormComponent GetInstance()
        {
            return instance;
        }

        /* 
         * A workqueue with two threads. One is dedicated to show this dialog and another is for handling
         * all the add and remove items from the form.  
         */
        private WorkQueue longRunningQueue;

        private WorkQueue shortTaskQueue;
    
        /* The form instance where new warnings should be added to. */
        private RefactoringWariningsForm form;

        private RefactoringFormComponent()
        {
            form = new RefactoringWariningsForm();
            longRunningQueue = new WorkQueue() {ConcurrentLimit = 2};
            shortTaskQueue = new WorkQueue(){ConcurrentLimit = 1};
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
            longRunningQueue.Add(new CodeIssuesMonitoringWorkItem());

        }

        public void AddWarning(IEnumerable<string> messages)
        {
            // Enqueue a work item for adding message to the form.
            shortTaskQueue.Add(new AddRefactoringFormItemWorkItem(form, messages));
        }

        public void RemoveWarning(IEnumerable<string> messages)
        {
            // Enqueue a work item for removing message from the form.
            shortTaskQueue.Add(new RemoveRefactoringFormItem(form, messages));
        }

        public void UpdateWarnings(IEnumerable<CodeIssue> issues)
        {
            // Enqueue a short task to update the errors on the form. 
            shortTaskQueue.Add(new UpdateWarningWorkItem(form, issues));
        }


        /* 
         * Work items to added to this work longRunningQueue. This workitem will add a warning to the 
         * list view. 
         */
        private class AddRefactoringFormItemWorkItem : WorkItem
        {
            private readonly RefactoringWariningsForm form;
            private readonly IEnumerable<string> messages;
            private readonly Logger logger;

            private delegate void AddItemCallback();

            internal AddRefactoringFormItemWorkItem(RefactoringWariningsForm form,
                IEnumerable<string> messages)
            {
                this.form = form;
                this.messages = messages;
                this.logger = NLoggerUtil.GetNLogger(typeof(AddRefactoringFormItemWorkItem));
            }

            public override void Perform()
            {
                // Use invoke to tell form to invoke the method in the UI thread when idle.
                form.Invoke(new AddItemCallback(Callback));
            }

            /* This callback function is passed to the UI thread and let the form add items. */
            private void Callback()
            {
                logger.Info("Add warning called Back.");
                form.AddRefactoringWarning(messages);
            }
        }

        /* A work item to remove a refactoring warning from the form. */
        private class RemoveRefactoringFormItem : WorkItem
        {
            private readonly Logger logger;
            private readonly RefactoringWariningsForm form;
            private readonly IEnumerable<string> messages;
            private delegate void RemoveItemCallback();

            internal RemoveRefactoringFormItem(RefactoringWariningsForm form,
                                               IEnumerable<string> messages)
            {
                this.form = form;
                this.messages = messages;
                this.logger = NLoggerUtil.GetNLogger(typeof(RemoveRefactoringFormItem));
            }

            public override void Perform()
            {
                form.Invoke(new RemoveItemCallback(Callback));
            }

            private void Callback()
            {
                logger.Info("Remove warning called back.");
                form.RemoveRefactoringWarning(messages);
            }
        }

        private class UpdateWarningWorkItem : WorkItem
        {
            private readonly IEnumerable<CodeIssue> issues;
            private readonly RefactoringWariningsForm form;

            internal UpdateWarningWorkItem(RefactoringWariningsForm form, IEnumerable<CodeIssue> issues)
            {
                this.form = form;
                this.issues = issues;
            }

            public override void Perform()
            {
                
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

        /* Work item for monitoring new code issues. */
        private class CodeIssuesMonitoringWorkItem : WorkItem
        {
            public override void Perform()
            {
                var solution = GhostFactorComponents.searchRealDocumentComponent.GetSolution();
                for (; ; )
                {
                    var issues = GhostFactorComponents.RefactoringCodeIssueComputerComponent.GetCodeIssues(solution);
                    GhostFactorComponents.refactoringFormComponent.UpdateWarnings(issues);
                    Thread.Sleep(GlobalConfigurations.GetRefactoringWarningListUpdateInterval());
                }
            }
        }

    }
}
