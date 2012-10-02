using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitSharp;
using NLog;
using warnings.refactoring;
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

        private static readonly string DETECTED_REFACTORINGS_ROOT = "DetectedRefactorings/";

        /* Several detectors.*/ 
        private static readonly IExternalRefactoringDetector EMDetector = 
            RefactoringDetectorFactory.CreateExtractMethodDetector();

        private static readonly IExternalRefactoringDetector CMSDetector =
            RefactoringDetectorFactory.CreateChangeMethodSignatureDetector();

        private readonly Logger logger;

        private string solutionName;
        private string fileName;

        // The count of all refactorings detected in this record chain.
        private int refactoringsCount;

        public RecordRefactoringDetector()
        {
            this.logger = NLoggerUtil.GetNLogger(typeof (RecordRefactoringDetector));
            refactoringsCount = 0;
        }

         public void DetectRefactorings(ICodeHistoryRecord head)
         {
             // For every record in the record chain, look back to detect refactorings.
             for (var current = head; current.HasPreviousRecord(); current = current.GetPreviousRecord())
             {
                 LookBackToDetectRefactorings(current);
             }
         }

        private void LookBackToDetectRefactorings(ICodeHistoryRecord record)
        {
            // Retriever the solution and file names.
            this.solutionName = record.getSolution();
            this.fileName = record.GetFile();

            // Current source is the source code after.
            var sourceAfter = record.getSource();

            // Look back until no parent or reach the search depth.
            for (int i = 0; i < SEARCH_DEPTH && record.HasPreviousRecord(); i++)
            {
                // Get the previous record and its source code.
                record = record.GetPreviousRecord();
                var sourceBefore = record.getSource();

                // Detect refactorings by using detectors.
                try
                {
                    DetectRefactoringByDetector(sourceBefore, sourceAfter, EMDetector);
                    // DetectRefactoringByDetector(sourceBefore, sourceAfter, CMSDetector);
                }
                catch (Exception e)
                {
                    logger.Fatal(e);
                }
            }
        }

        private void DetectRefactoringByDetector(string before, string after, IExternalRefactoringDetector detector)
        {
            // Set source before and after. 
            detector.SetSourceBefore(before);
            detector.SetSourceAfter(after);

            // If a refactoring is detected.
            if (detector.HasRefactoring())
            {
                // Get the detected refactorings, and log them.
                var refactorings = detector.GetRefactorings();
                foreach (var refactoring in refactorings)
                {
                    var path = HandleDetectedRefactoring(before, after, refactoring);
                    refactoringsCount ++;
                    logger.Info("Refactoring detected! Saved at " + path);
                }
            }
        }

        /* Handle a detected refactoring by saveing it at an independent file. */
        private string HandleDetectedRefactoring(string before, string after, IManualRefactoring refactoring)
        {
            // Get the folder and the file name for this detected refactoring.
            string refactoringDirectory = DETECTED_REFACTORINGS_ROOT + solutionName;
            string refactoringFilePath = refactoringDirectory + "/" + fileName + refactoringsCount + ".txt";

            // If the directory does not exist, create it.
            if(!Directory.Exists(refactoringDirectory))
            {
                Directory.CreateDirectory(refactoringDirectory);
            }

            // Streaming all the needed information to the file.
            var stream = File.CreateText(refactoringFilePath);
            stream.WriteLine("Source Before:");
            stream.WriteLine(before);
            stream.WriteLine("Source After:");
            stream.WriteLine(after);
            stream.WriteLine("Detected Refactoring:");
            stream.WriteLine(refactoring.ToString());
            stream.Flush();
            stream.Close();

            // Return the saved refactoring file path.
            return refactoringFilePath;
        }
    }
}
