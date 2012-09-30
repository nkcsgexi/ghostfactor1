using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using GitSharp;
using GitSharp.Commands;
using NLog;
using warnings.refactoring.detection;
using warnings.source.history;
using warnings.util;

namespace GitRevisionAnalyzer
{
    class GitRevisionAnalyzer
    {
        private WorkQueue queue;

        private GitRevisionAnalyzer()
        {
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;
        }

        private class RefactoringDetectionWorkItem : WorkItem
        {
            private string gitHttp;

            internal RefactoringDetectionWorkItem(string gitHttp)
            {
                this.gitHttp = gitHttp;
            }
            public override void Perform()
            {
                var project = new GitProject(gitHttp);
                project.Clone();
                project.AddCommitsToCodeHistory(CodeHistory.getInstance());
                var records = project.GetHeadHitoryRecords(CodeHistory.getInstance());
                var detector = new RecordRefactoringDetector();
                foreach (var record in records)
                {
                    detector.DetectRefactorings(record);
                }
            }
        }

        public void AddNewGitHttp(string gitHttp)
        {
            queue.Add(new RefactoringDetectionWorkItem(gitHttp));
        }


        static void Main(string[] args)
        {
            var analyzer = new GitRevisionAnalyzer();
            analyzer.AddNewGitHttp("");    
        }

    }
}
