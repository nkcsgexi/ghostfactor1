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

        private SearchRenameComponent() :base()
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
            return 30;
        }

        protected override void onRefactoringDetected(IExternalRefactoringDetector detector)
        {
            logger.Info("\n Rename dectected.");
            logger.Info("\n Before: \n" + detector.getSourceBefore());
            logger.Info("\n After: \n" + detector.getSourceAfter());
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
