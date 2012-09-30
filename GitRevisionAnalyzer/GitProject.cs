using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitSharp;
using warnings.refactoring.detection;
using warnings.source.history;

namespace GitRevisionAnalyzer
{
    /* Interface for a project represented by its git repository. */
    public interface IGitProject
    {
        void Clone();
        string GetSourceFolder();
        Repository GetRepository();
        Commit GetCurrentCommit();
        void AddCommitsToCodeHistory(ICodeHistory history);
        IEnumerable<ICodeHistoryRecord> GetHeadHitoryRecords(ICodeHistory history);
    }

    class GitProject : IGitProject
    {
        // Default namespace name for all files.
        private static readonly string NAMESPACE_NAME = "namespace";

        // Remote git http.
        private readonly string gitHttp;

        // Where the local project is saved, equals to project name.
        private readonly string sourceFolder;

        // All file names encountered when visiting the change history.
        private readonly List<string> fileNames; 


        public GitProject(string gitHttp)
        {
            this.gitHttp = gitHttp;

            // The string between the last / and the last . is the name of the project.
            // Also we use it as the source folder for cloning repository.
            int start = gitHttp.LastIndexOf('/');
            int end = gitHttp.LastIndexOf('.');
            this.sourceFolder = gitHttp.Substring(start, end - start + 1);
            this.fileNames = new List<string>();
        }

        public string GetSourceFolder()
        {
            return sourceFolder;
        }

        public Repository GetRepository()
        {
            if(File.Exists(sourceFolder + @"\.git"))
            {
                // The git repository should be the ".git" folder under 
                // the source folder.
                return new Repository(sourceFolder + @"\.git");
            }
            return null;
        }

        public Commit GetCurrentCommit()
        {
            return GetRepository().Head.CurrentCommit;
        }

        /* 
         * Convert all the commits in the repository as code history record and add these records 
         * to the given code history.
         */
        public void AddCommitsToCodeHistory(ICodeHistory codeHistory)
        {
            // All commits in time order.
            var commits = GetAndOrderAllCommits();

            // Iterate all of the commits.
            for (int i = 0; i < commits.Count(); i ++)
            {
                var commit = commits.ElementAt(i);

                // Get the changes that are changing the C# source code.
                var changes = commit.Changes.Where(c => c.Name.EndsWith(".cs") &&
                        c.ChangedObject.IsBlob);

                // If we found such changes.
                if (changes.Any())
                {
                    // For each change in the found changes.
                    foreach (var change in changes)
                    {
                        // Get the file name of where the change is performed on.
                        string fileName = change.Name;

                        // Get the source after change.
                        string source = ((Blob)change.ChangedObject).Data;

                        // Add the source as a new record to the history.
                        codeHistory.addRecord(GetSourceFolder(), NAMESPACE_NAME, fileName, source);
                        
                        // If we did not meet with the file name, add it to the list.
                        if(!fileNames.Contains(fileName))
                        {
                            fileNames.Add(fileName);
                        }
                    }
                }
            }
        }


        private IEnumerable<Commit> GetAndOrderAllCommits()
        {
            var head = GetCurrentCommit();
            return head.Ancestors.OrderBy(c => c.CommitDate);
        }

        /* Get the record heads for this project. */
        public IEnumerable<ICodeHistoryRecord> GetHeadHitoryRecords(ICodeHistory history)
        {
            var records = new List<ICodeHistoryRecord>();
            if (fileNames.Any())
            {
                // For each encountered name in the visiting process. 
                foreach (var fileName in fileNames)
                {
                    // Get the latest record for the file name.
                    var record = history.GetLatestRecord(GetSourceFolder(), NAMESPACE_NAME, fileName);
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
            }
            return records.AsEnumerable();
        }

        public void Clone()
        {
            GitSharp.Git.Clone(gitHttp, sourceFolder);
        }
    }
}
