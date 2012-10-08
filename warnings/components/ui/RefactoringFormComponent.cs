using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using BlackHen.Threading;
using NLog;
using warnings.ui;
using warnings.util;

namespace warnings.components.ui
{
    /*
     * A public interface for refactoring error form component, where new refactoring messages
     * can be added.
     */
    public interface IRefactoringFormComponent : IFactorComponent
    {
        void AddWarning(IEnumerable<string> messages);
    }

    internal class RefactoringFormComponent : IRefactoringFormComponent
    {
        /* Singleton this component. */
        private static IRefactoringFormComponent instance = new RefactoringFormComponent();

        public static IRefactoringFormComponent GetInstance()
        {
            return instance;
        }
        
        /* A workqueue with single thread. */
        private WorkQueue queue;
        private Logger logger;

        /* The form instance where new warnings should be added to. */
        private RefactoringWariningsForm form;


        private RefactoringFormComponent()
        {
            queue = new WorkQueue() {ConcurrentLimit = 1};
            logger = NLoggerUtil.GetNLogger(typeof (RefactoringFormComponent));
            form = new RefactoringWariningsForm();
        }

        public void Enqueue(IWorkItem item)
        {   
            queue.Add(item);
        }

        public string GetName()
        {
            return "Refactoring Form Component";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
            // Create a new thread that is dedicated to the dialog.
            var uiThread = new Thread(ShowDialog);
            uiThread.Start();
        }


        private void ShowDialog()
        {
            // Open the form instance.
            // ATTENTION: use show dialogue instead of show, this will block this thread, no need
            // to have an infinite loop.
            form.ShowDialog();
        }

        public void AddWarning(IEnumerable<string> messages)
        {
            // Enqueue a work item for adding message to the form.
            Enqueue(new AddRefactoringFormItemWorkItem(form, messages));
        }

        /* 
         * Work items to added to this work queue. This workitem will add a warning to the 
         * list view. 
         */
        private class AddRefactoringFormItemWorkItem : WorkItem
        {
            private readonly RefactoringWariningsForm form;
            private readonly IEnumerable<string> messages;
            private delegate void AddItemCallback();
       
            internal AddRefactoringFormItemWorkItem(RefactoringWariningsForm form, 
                IEnumerable<string> messages)
            {
                this.form = form;
                this.messages = messages;
            }

            public override void Perform()
            {
                // Use invoke to tell form to invoke the method in the UI thread when idle.
                form.Invoke(new AddItemCallback(Callback));
            }

            /* This callback function is passed to the UI thread and let the form add items. */
            private void Callback()
            {
                form.AddRefactoringWarning(messages);
                form.Invalidate();
            }
        }
    }
}
