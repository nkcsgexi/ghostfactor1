using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.conditions;
using warnings.quickfix;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.retriever;
using warnings.source;
using warnings.source.history;
using warnings.util;

namespace warnings.components.search
{
    class SearchChangeMethodSignatureComponent : SearchRefactoringComponent
    {
        private static readonly IFactorComponent instance = new SearchChangeMethodSignatureComponent();

        public static IFactorComponent GetInstance()
        {
            return instance;
        }

        public override string GetName()
        {
            return "SearchChangeMethodSignatureComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchChangeMethodSignatureComponent));
        }
    }

    class SearchChangeMethodSignatureWorkItem : SearchRefactoringWorkitem
    {
        public SearchChangeMethodSignatureWorkItem(ICodeHistoryRecord latestRecord) : base(latestRecord)
        {
        }

        protected override IExternalRefactoringDetector getRefactoringDetector()
        {
            return RefactoringDetectorFactory.CreateChangeMethodSignatureDetector();
        }

        protected override int getSearchDepth()
        {
            return 10;
        }

        protected override void onRefactoringDetected(ICodeHistoryRecord before, ICodeHistoryRecord after,
            IEnumerable<IManualRefactoring> refactorings)
        {
            logger.Info("Change Method Signature Detected.");
            logger.Info("Before:\n" + before.getSource());
            logger.Info("After:\n" + after.getSource());

            // Enqueue the condition checking process for this detected refactoring.
            GhostFactorComponents.conditionCheckingComponent.Enqueue(
                new ConditionCheckWorkItem(before, after, refactorings.First()));
        }

        protected override void onNoRefactoringDetected(ICodeHistoryRecord record)
        {
            logger.Info("No change method signature detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchChangeMethodSignatureWorkItem));
        }

        

     

    }
}
