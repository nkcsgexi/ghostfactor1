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
            return RefactoringDetectorFactory.createExtractMethodDetector();
        }

        protected override int getSearchDepth()
        {
            return 30;
        }

        protected override void onRefactoringDetected(IExternalRefactoringDetector detector)
        {
            logger.Info("\n Extract Method dectected.");
            logger.Info("\n Before: \n" + detector.getSourceBefore());
            logger.Info("\n After: \n" + detector.getSourceAfter());

            // Get the first refactoring detected.
            IManualRefactoring refactoring = detector.getRefactorings().First();

            // Convert before and after source as IDocument.
            var converter = new String2IDocumentConverter();
            var beforeDoc = (IDocument)converter.Convert(detector.getSourceBefore(), null, null, null);
            var afterDoc = (IDocument)converter.Convert(detector.getSourceAfter(), null, null, null);

            // Enqueue workitem for conditions checking component.
            GhostFactorComponents.conditionCheckingComponent.Enqueue(new ConditionCheckWorkItem(beforeDoc, afterDoc, refactoring));
        }

        protected override void onNoRefactoringDetected()
        {
            logger.Info("No extract method detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof(SearchExtractMethodWorkitem));
        }
    }
}
