using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitSharp;
using NLog;
using warnings.refactoring.detection;
using warnings.source.history;
using warnings.util;

namespace GitRevisionAnalyzer
{
    /* Detect refactoring by a given ICodeHistoryRecord instance to look back.*/
    public class RecordRefactoringDetector
    {
        /* Look back count. */ 
        private static readonly int SEARCH_DEPTH = 20;

        /* Several detectors.*/ 
        private static readonly IExternalRefactoringDetector EMDetector = 
            RefactoringDetectorFactory.CreateExtractMethodDetector();

        private static readonly IExternalRefactoringDetector CMSDetector =
            RefactoringDetectorFactory.CreateChangeMethodSignatureDetector();

        private readonly Logger logger;
       

        public RecordRefactoringDetector()
        {
            this.logger = NLoggerUtil.GetNLogger(typeof (RecordRefactoringDetector));
        }

        public void DetectRefactorings(ICodeHistoryRecord record)
        {
            // Current source is the source code after.
            var sourceAfter = record.getSource();

            // Look back until no parent or reach the search depth.
            for (int i = 0; i < SEARCH_DEPTH && record.hasPreviousRecord(); i++)
            {
                // Get the previous record and its source code.
                record = record.getPreviousRecord();
                var sourceBefore = record.getSource();

                // Detect refactorings by using detectors.
                try
                {
                    DetectRefactoringByDetector(sourceBefore, sourceAfter, EMDetector);
                    DetectRefactoringByDetector(sourceBefore, sourceAfter, CMSDetector);
                }
                catch (Exception e)
                {
                    logger.Fatal(e);
                    logger.Fatal("Source Before:\n" + sourceBefore);
                    logger.Fatal("Source After:\n" + sourceAfter);
                }
            }
        }

        private void DetectRefactoringByDetector(string before, string after, IExternalRefactoringDetector detector)
        {
            // Set source before and after. 
            detector.setSourceBefore(before);
            detector.setSourceAfter(after);

            // If a refactoring is detected.
            if (detector.hasRefactoring())
            {
                // Get the detected refactorings, and log them.
                var refactorings = detector.getRefactorings();
                foreach (var refactoring in refactorings)
                {
                    logger.Info("Source Before:\n" + before);
                    logger.Info("Source After:\n" + after);
                    logger.Info(refactoring.ToString());
                }
            }
        }
    }
}
