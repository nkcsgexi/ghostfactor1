using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using warnings.configuration;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.source;
using warnings.source.history;
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
            return NLoggerUtil.GetNLogger(typeof (SearchRenameComponent));
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
            return RefactoringDetectorFactory.CreateRenameDetector();
        }

        protected override int getSearchDepth()
        {
            return GlobalConfigurations.GetSearchDepth(RefactoringType.RENAME);
        }

        protected override void onRefactoringDetected(ICodeHistoryRecord before, ICodeHistoryRecord after,
            IEnumerable<IManualRefactoring> refactorings)
        {
            logger.Info("Rename dectected.");
            logger.Info("\nBefore: \n" + before.getSource());
            logger.Info("\nAfter: \n" + after.getSource());
        }

        protected override void onNoRefactoringDetected(ICodeHistoryRecord record)
        {
            //logger.Info("No Rename Detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.GetNLogger(typeof (SearchRenameWorkItem));
        }
    }
}
