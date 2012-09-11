using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using warnings.refactoring.detection;
using warnings.source;
using warnings.util;

namespace warnings.components
{
    /* Component for detecting manually conducted rename refactoring. */
    class SearchRenameComponent : SearchRefactoringComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new SearchRenameComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        private SearchRenameComponent()
        {
            
        }

        public override string GetName()
        {
            return "SearchRenameComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchRenameComponent));
        }
    }

    /* Item to be schedule to rename searching component. */
    public class SearchRenameWorkItem : SearchRefactoringWorkitem
    {
        public SearchRenameWorkItem(ICodeHistoryRecord latestRecord) : base(latestRecord)
        {
        }

        protected override IExternalRefactoringDetector getRefactoringDetector()
        {
            return RefactoringDetectorFactory.createRenameDetector();
        }

        protected override int getSearchDepth()
        {
            return 3;
        }

        protected override void onRefactoringDetected(IExternalRefactoringDetector detector)
        {
            logger.Info("Rename dectected.");
            logger.Info("\nBefore: \n" + detector.getSourceBefore());
            logger.Info("\nAfter: \n" + detector.getSourceAfter());

            // Get the 
            var refactoring = detector.getRefactorings().FirstOrDefault();
        }

        protected override void onNoRefactoringDetected()
        {
            logger.Info("No Rename Detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchRenameWorkItem));
        }
    }
}
