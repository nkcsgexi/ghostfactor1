using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.source;
using warnings.source.history;
using warnings.util;

namespace warnings.components
{
    /* This component is for detecting manual extract method refactoring in the code history. */
    public class SearchExtractMethodComponent : SearchRefactoringComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        private SearchExtractMethodComponent()
        {
        }

        public override string GetName()
        {
            return "SearchExtractMethodComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchExtractMethodComponent));
        }
    }


    /* The kind of work item for search extract method component. */
    public class SearchExtractMethodWorkitem : SearchRefactoringWorkitem
    {
        public SearchExtractMethodWorkitem(ICodeHistoryRecord latestRecord) : base(latestRecord)
        {
        }

        protected override IExternalRefactoringDetector getRefactoringDetector()
        {
            return RefactoringDetectorFactory.CreateExtractMethodDetector();
        }

        protected override int getSearchDepth()
        {
            return 30;
        }

        protected override void onRefactoringDetected(ICodeHistoryRecord before, ICodeHistoryRecord after, 
            IEnumerable<IManualRefactoring> refactorings)
        {
            logger.Info("\n Extract Method dectected.");
            logger.Info("\n Before: \n" + before.getSource());
            logger.Info("\n After: \n" + after.getSource());

            // Get the first refactoring detected.
            IManualRefactoring refactoring = refactorings.First();

            // Enqueue workitem for conditions checking component.
            GhostFactorComponents.conditionCheckingComponent.Enqueue
                (new ConditionCheckWorkItem(before, after, refactoring));
        }

        protected override void onNoRefactoringDetected(ICodeHistoryRecord record)
        {
            logger.Info("No extract method detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof(SearchExtractMethodWorkitem));
        }
    }
}
