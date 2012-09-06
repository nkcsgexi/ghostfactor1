using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlackHen.Threading;

namespace warnings.components
{
    /* All the components in GhostFactor shall be implementing this interface.*/
    public interface IFactorComponent
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        void Enqueue(IWorkItem item);
        string GetName();
        int GetWorkQueueLength();
        void Start();
    }

    public class GhostFactorComponents
    {
        /* Component for saving the source code at certain time interval. */
        public static readonly IFactorComponent historyComponent = HistorySavingComponent.getInstance();

        /* Component for traversing the source code history and looking for manual rename refactoring. */
        public static readonly IFactorComponent searchRenameComponent = SearchRenameComponent.getInstance();

        /* Component for traversing the source code history and looking for manual extract method refactorings. */
        public static readonly IFactorComponent searchExtractMethodComponent = SearchExtractMethodComponent.getInstance();

        /* Component for checking the conditions of detected manual refactorings. */
        public static readonly IFactorComponent conditionCheckingComponent = ConditionCheckingComponent.GetInstance();

        public static void StartAllComponents()
        {

            // Start the history keeping component.
            historyComponent.Start();
            
            // Start the searching refactoring components.
            searchRenameComponent.Start();
            searchExtractMethodComponent.Start();

            // Start condition checker.
            conditionCheckingComponent.Start();
        }
    }
}
