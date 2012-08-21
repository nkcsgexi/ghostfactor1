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
    }

    public class GhostFactorComponents
    {
        public static readonly IFactorComponent HistoryComponent = HistorySavingComponent.getInstance();
        

    }
}
