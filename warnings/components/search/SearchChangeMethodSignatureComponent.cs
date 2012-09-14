using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using warnings.refactoring.detection;
using warnings.source;
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

        protected override void onRefactoringDetected(IExternalRefactoringDetector detector)
        {
            logger.Info("Change Method Signature Detected.");
            logger.Info("Before:\n" + detector.getSourceBefore());
            logger.Info("After:\n" + detector.getSourceAfter());
        }

        protected override void onNoRefactoringDetected()
        {
            logger.Info("No change method signature detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchChangeMethodSignatureWorkItem));
        }
    }
}
