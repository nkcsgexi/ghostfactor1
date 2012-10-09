using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.conditions;
using warnings.quickfix;
using warnings.util;

namespace warnings.components
{
    /* A repository for issue computers to be queried, added, and deleted. */
    public interface ICodeIssueComputersRepository
    {
        void AddCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers);
        void RemoveCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers);
        IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node);
        IEnumerable<CodeIssue> GetCodeIssues(IDocument document);
        IEnumerable<CodeIssue> GetCodeIssues(ISolution solution);
    }

    internal class RefactoringCodeIssueComputersComponent : IFactorComponent, ICodeIssueComputersRepository
    {
        /* Singleton this component. */
        private static readonly ICodeIssueComputersRepository intance =
            new RefactoringCodeIssueComputersComponent();

        public static ICodeIssueComputersRepository GetInstance()
        {
            return intance;
        }

        /* Saving all of the code issue computers. */
        private readonly List<ICodeIssueComputer> codeIssueComputers;

        /* A single thread workqueue. */
        private WorkQueue queue;

        private Logger logger;

        private RefactoringCodeIssueComputersComponent()
        {
            codeIssueComputers = new List<ICodeIssueComputer>();

            // Single thread workqueue.
            queue = new WorkQueue {ConcurrentLimit = 1};

            // Add a listener for failed work item.
            queue.FailedWorkItem += OnItemFailed;

            // Add a null computer to avoid adding more null computers.
            codeIssueComputers.Add(new NullCodeIssueComputer());
            logger = NLoggerUtil.GetNLogger(typeof (RefactoringCodeIssueComputersComponent));
        }

        private void OnItemFailed(object sender, WorkItemEventArgs workItemEventArgs)
        {
            logger.Fatal("Work item failed.");
        }

        public void Enqueue(IWorkItem item)
        {
            queue.Add(item);
        }

        public string GetName()
        {
            return "Refactoring Issues Componenet.";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }


        /* Add a list of code issue computers to the current list. */
        public void AddCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers)
        {
            // Create a code issue adding work item and push it to the work queue.
            var item = new AddCodeIssueComputersWorkItem(codeIssueComputers, computers);
            queue.Add(item);
        }

        /* Remove a list of code issue computers from the current list. */
        public void RemoveCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers)
        {
            queue.Add(new RemoveCodeIssueComputersWorkItem(codeIssueComputers, computers));
        }

        /* Get the code issues in the given node of the given document. */
        public IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node)
        {
            // Create a work item for this task.
            var item = new GetDocumentNodeCodeIssueWorkItem(document, node, codeIssueComputers);
            queue.Add(item);

            // Busy waiting for the completion of this work item.
            while (item.State != WorkItemState.Completed && item.State == WorkItemState.Failing);
            return item.GetCodeIssues();
        }

        /* Get all the code issues in a given document. */
        public IEnumerable<CodeIssue> GetCodeIssues(IDocument document)
        {
            // Create a work item for this task and add this item to the work queue.
            var item = new GetDocumentCodeIssuesWorkItem(document, codeIssueComputers);
            queue.Add(item);

            // Busy waiting for the completion of this task.
            while (item.State != WorkItemState.Completed && item.State != WorkItemState.Failing);
            return item.GetCodeIssues();
        }

        /* Get all the code issues in a given solution. */
        public IEnumerable<CodeIssue> GetCodeIssues(ISolution solution)
        {
            var item =  new GetSolutionCodeIssueWorkItem(solution, codeIssueComputers.AsEnumerable());
            queue.Add(item);
            while (item.State != WorkItemState.Completed && item.State != WorkItemState.Failing);
            return item.GetCodeIssues();
        }
        

        /* Work item to add new issue computers to the repository. */
        private class AddCodeIssueComputersWorkItem : WorkItem
        {
            private IList<ICodeIssueComputer> currentComputers;
            private IEnumerable<ICodeIssueComputer> newComputers;

            public AddCodeIssueComputersWorkItem(IList<ICodeIssueComputer> currentComputers,
                                                 IEnumerable<ICodeIssueComputer> newComputers)
            {
                this.currentComputers = currentComputers;
                this.newComputers = newComputers;
            }

            public override void Perform()
            {
                // For every computer in the new computers list. 
                foreach (var computer in newComputers)
                {
                    // If a computer is not already in the list, add it.
                    if (!currentComputers.Contains(computer))
                        currentComputers.Add(computer);
                }
            }
        }

        /* Work item to remove computers from the given computer list. */
        private class RemoveCodeIssueComputersWorkItem: WorkItem
        {
            private readonly IList<ICodeIssueComputer> currentComputers;
            private readonly IEnumerable<ICodeIssueComputer> toRemoveComputers;

            internal RemoveCodeIssueComputersWorkItem(IList<ICodeIssueComputer> currentComputers, 
                IEnumerable<ICodeIssueComputer> toRemoveComputers )
            {
                this.currentComputers = currentComputers;
                this.toRemoveComputers = toRemoveComputers;
            }

            public override void Perform()
            {
                foreach (ICodeIssueComputer computer in toRemoveComputers)
                {
                    currentComputers.Remove(currentComputers.First(c => c.Equals(computer)));
                }
            }
        }

        /* Abstract class for all the work item whose aim is to retrieve code issues.*/
        private abstract class GetCodeIssueWorkItem : WorkItem
        {
            public abstract IEnumerable<CodeIssue> GetCodeIssues();

            /* Get code issues by the given document, node and a list of computers. */
            protected IEnumerable<CodeIssue> GetSyntaxNodeIssues(IDocument document, SyntaxNode node, 
                IEnumerable<ICodeIssueComputer> computers)
            {
                return computers.SelectMany(c => c.ComputeCodeIssues(document, node));
            }

            /* Get code issues in the given document by the given list of computers. */
            protected IEnumerable<CodeIssue> GetDocumentIssues(IDocument document, IEnumerable<ICodeIssueComputer> computers)
            {

                // Where to store all the issues. 
                var issues = new List<CodeIssue>();

                // Get all the decendant nodes. 
                var nodes = ((SyntaxNode)document.GetSyntaxRoot()).DescendantNodes();

                // For each of the decendent node, get its issues and add them to the list. 
                foreach (var node in nodes)
                {
                    issues.AddRange(GetSyntaxNodeIssues(document, node, computers));
                }
                return issues;
            }
        }

        /* Work item for getting code issues in a given syntax node. */
        private class GetDocumentNodeCodeIssueWorkItem : GetCodeIssueWorkItem
        {
            private readonly IDocument document;
            private readonly SyntaxNode node;
            private readonly IEnumerable<ICodeIssueComputer> computers;
            private IEnumerable<CodeIssue> results; 

            internal GetDocumentNodeCodeIssueWorkItem(IDocument document, SyntaxNode node, 
                IEnumerable<ICodeIssueComputer> computers)
            {
                this.document = document;
                this.node = node;
                this.computers = computers;
            }

            public override void Perform()
            {
                results = GetSyntaxNodeIssues(document, node, computers);
            }

            public override IEnumerable<CodeIssue> GetCodeIssues()
            {
                return results;
            }
        }

        /* Work item for getting all the code issues in a given document. */
        private class GetDocumentCodeIssuesWorkItem : GetCodeIssueWorkItem
        {
            private readonly IDocument document;
            private readonly IEnumerable<ICodeIssueComputer> computers;
            private IEnumerable<CodeIssue> results; 

            internal GetDocumentCodeIssuesWorkItem(IDocument document, IEnumerable<ICodeIssueComputer> computers)
            {
                this.document = document;
                this.computers = computers;
            }

            public override void Perform()
            {
                results = GetDocumentIssues(document, computers);
            }

            public override IEnumerable<CodeIssue> GetCodeIssues()
            {
                return results;
            }
        }

        /* Work item for getting all the code issues in a given solution. */
        private class GetSolutionCodeIssueWorkItem : GetCodeIssueWorkItem
        {
            private readonly IEnumerable<ICodeIssueComputer> computers;
            private readonly ISolution solution;
            private IEnumerable<CodeIssue> results; 

            internal GetSolutionCodeIssueWorkItem(ISolution solution, IEnumerable<ICodeIssueComputer> computers)
            {
                this.solution = solution;
                this.computers = computers;
            }

            public override void Perform()
            {
                // Where to store all the code issues. 
                var issues = new List<CodeIssue>();

                // Get all the documents in the solution. 
                var solutionAnalyzer = AnalyzerFactory.GetSolutionAnalyzer();
                solutionAnalyzer.SetSolution(solution);
                var documents = solutionAnalyzer.GetAllDocuments();

                // For each document, get its 
                foreach (IDocument document in documents)
                {
                    issues.AddRange(GetDocumentIssues(document, computers));
                }
                results = issues.AsEnumerable();
            }

            public override IEnumerable<CodeIssue> GetCodeIssues()
            {
                return results;
            }
        }

    }

}
