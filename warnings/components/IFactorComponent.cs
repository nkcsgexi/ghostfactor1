using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;

namespace warnings.components
{
    public interface IFactorComponent
    {
        void Enqueue(object item);
        string GetName();
        int GetWorkQueueLength();
        void Start();
    }

    public class GhostFactorComponents
    {
        /* Component for saving the source code at certain time interval. */
        public static readonly IFactorComponent HistoryComponent = HistorySavingComponent.getInstance();

        /* Component for traversing the source code history and looking for manual rename refactoring. */
        public static readonly IFactorComponent RenameComponent = SearchRenameComponent.getInstance();

        /* Component for traversing the source code history and looking for manual extract method refactorings. */
        public static readonly IFactorComponent ExtractMethodComponent = SearchExtractMethodComponent.getInstance();

        public static void StartAllComponents()
        {
            HistoryComponent.Start();
            RenameComponent.Start();
            ExtractMethodComponent.Start();
        }
    }
}
