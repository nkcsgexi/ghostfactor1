using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BlackHen.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.components.ui;
using warnings.conditions;
using warnings.quickfix;
using warnings.util;

namespace warnings.components
{
    /* Used for any listens to get the warning messages of the entire solution. */
    public delegate void GlobalRefactoringWarningsReady(IEnumerable<IRefactoringWarningMessage> messages, bool isAdded);

    /* A repository for issue computers to be queried, added, and deleted. */
    public interface ICodeIssueComputersRepository
    {
        event GlobalRefactoringWarningsReady globalWarningsReady;
        void AddCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers);
        void RemoveCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers);
        IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node);
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

        /* Used for any listener to the event of code issue computers added or removed. */
        private delegate void CodeIssueComputersChanged(ICodeIssueComputersChangedArg arg);
        
        /* Used as the parameter for any listeners to the code issue computers changed. */
        private interface ICodeIssueComputersChangedArg
        {
            bool IsAdding();
            IEnumerable<ICodeIssueComputer> GetChangedCodeIssueComputers();
        }


        /* Event when the code issues are changed. */
        private event CodeIssueComputersChanged changeEvent;

        /* Event when a new global refactoring warnings are ready. */
        public event GlobalRefactoringWarningsReady globalWarningsReady;

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

            changeEvent += OnCodeIssueComputersChanged;
        }

        /* When code issue computers are changed, this method will be called. */
        private void OnCodeIssueComputersChanged(ICodeIssueComputersChangedArg codeIssueComputersChangedArg)
        {
            var solution = GhostFactorComponents.searchRealDocumentComponent.GetSolution();

            // Create a work item for this task.
            var item = new GetSolutionRefactoringWarningsWorkItem(solution, codeIssueComputersChangedArg, globalWarningsReady);
            queue.Add(item);
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
            queue.Add(new AddCodeIssueComputersWorkItem(codeIssueComputers, computers, changeEvent));
        }

        /* Remove a list of code issue computers from the current list. */
        public void RemoveCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers)
        {
            queue.Add(new RemoveCodeIssueComputersWorkItem(codeIssueComputers, computers, changeEvent));
        }

        /* Get the code issues in the given node of the given document. */
        public IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node)
        {
            // Create a work item for this task.
            var item = new GetDocumentNodeCodeIssueWorkItem(document, node, codeIssueComputers);
            new WorkItemSynchronizedExecutor(item, queue).Execute();
            return item.GetCodeIssues();
        }

        /* 
         * An abstract implementation of ICodeIssueComputersChangedArg that leaves only whether adding or removint
         * to be implemented.
         */
        internal abstract class CodeIssueComputersChangedArg : ICodeIssueComputersChangedArg
        {
            private readonly IEnumerable<ICodeIssueComputer> computers;

            protected CodeIssueComputersChangedArg(IEnumerable<ICodeIssueComputer> computers)
            {
                this.computers = computers;
            }

            public abstract bool IsAdding();

            public IEnumerable<ICodeIssueComputer> GetChangedCodeIssueComputers()
            {
                return computers;
            }
        }

        /* Work item to add new issue computers to the repository. */
        private class AddCodeIssueComputersWorkItem : WorkItem
        {
            private readonly IList<ICodeIssueComputer> currentComputers;
            private readonly IEnumerable<ICodeIssueComputer> newComputers;
            private readonly CodeIssueComputersChanged changeEvent;
            private readonly Logger logger;

            public AddCodeIssueComputersWorkItem(IList<ICodeIssueComputer> currentComputers,
                IEnumerable<ICodeIssueComputer> newComputers, CodeIssueComputersChanged changeEvent)
            {
                this.currentComputers = currentComputers;
                this.newComputers = newComputers;
                this.changeEvent = changeEvent;
                this.logger = NLoggerUtil.GetNLogger(typeof (AddCodeIssueComputersWorkItem));
            }

            public override void Perform()
            {
                var addedComputers = new List<ICodeIssueComputer>();

                // For every computer in the new computers list. 
                foreach (var computer in newComputers)
                {
                    // If a computer is not already in the list, add it.
                    if (!currentComputers.Contains(computer))
                    {
                        currentComputers.Add(computer);
                        addedComputers.Add(computer);
                    }
                }
                changeEvent(new CodeIssueComputersAddedArg(addedComputers.AsEnumerable()));
            }

            /* Used as the argument to inform listeners that there are added computers. */
            private class CodeIssueComputersAddedArg : CodeIssueComputersChangedArg
            {
                public CodeIssueComputersAddedArg(IEnumerable<ICodeIssueComputer> computers) : base(computers)
                {
                }

                public override bool IsAdding()
                {
                    return true;
                }
            }
        }

        /* Work item to remove computers from the given computer list. */
        private class RemoveCodeIssueComputersWorkItem: WorkItem
        {
            private readonly IList<ICodeIssueComputer> currentComputers;
            private readonly IEnumerable<ICodeIssueComputer> toRemoveComputers;
            private readonly CodeIssueComputersChanged changeEvent;
            private readonly Logger logger; 

            internal RemoveCodeIssueComputersWorkItem(IList<ICodeIssueComputer> currentComputers,
                IEnumerable<ICodeIssueComputer> toRemoveComputers, CodeIssueComputersChanged changeEvent)
            {
                this.currentComputers = currentComputers;
                this.toRemoveComputers = toRemoveComputers;
                this.changeEvent = changeEvent;
                this.logger = NLoggerUtil.GetNLogger(typeof (RemoveCodeIssueComputersWorkItem));
            }

            public override void Perform()
            {
                foreach (ICodeIssueComputer computer in toRemoveComputers)
                {
                    currentComputers.Remove(computer);
                }
                changeEvent(new CodeIssueComputersRemovedArg(toRemoveComputers));
            }

            private class CodeIssueComputersRemovedArg : CodeIssueComputersChangedArg
            {
                internal CodeIssueComputersRemovedArg(IEnumerable<ICodeIssueComputer> computers) : base(computers)
                {
                }

                public override bool IsAdding()
                {
                    return false;
                }
            }
        }

        /* Work item for getting code issues in a given syntax node. */
        private class GetDocumentNodeCodeIssueWorkItem : WorkItem
        {
            private readonly IDocument document;
            private readonly SyntaxNode node;
            private readonly IEnumerable<ICodeIssueComputer> computers;
            private readonly Logger logger;
            private IEnumerable<CodeIssue> results;
            private bool IsReady;


            internal GetDocumentNodeCodeIssueWorkItem(IDocument document, SyntaxNode node, 
                IEnumerable<ICodeIssueComputer> computers)
            {
                this.document = document;
                this.node = node;
                this.computers = computers;
                this.logger = NLoggerUtil.GetNLogger(typeof (GetDocumentNodeCodeIssueWorkItem));
                this.IsReady = false;
            }

            public override void Perform()
            {
                results = computers.SelectMany(c => c.ComputeCodeIssues(document, node));
                this.IsReady = true;
            }

            public bool IsResultReady()
            {
                return IsReady;
            }

            public IEnumerable<CodeIssue> GetCodeIssues()
            {
                return results;
            }
        }

     
        /* Work item for getting all the refactoring warnings in a given solution and a set of computers. */
        private class GetSolutionRefactoringWarningsWorkItem : WorkItem
        {
            private readonly IEnumerable<ICodeIssueComputer> computers;
            private readonly ISolution solution;
            private readonly Logger logger;
            private readonly GlobalRefactoringWarningsReady warningsReady;
            private readonly bool isAdding;


            internal GetSolutionRefactoringWarningsWorkItem(ISolution solution, ICodeIssueComputersChangedArg changedArg, 
                GlobalRefactoringWarningsReady warningsReady)
            {
                this.solution = solution;
                this.computers = changedArg.GetChangedCodeIssueComputers();
                this.isAdding = changedArg.IsAdding();
                this.logger = NLoggerUtil.GetNLogger(typeof (GetSolutionRefactoringWarningsWorkItem));
                this.warningsReady = warningsReady;
            }

            public override void Perform()
            {
                var messagesList = new List<IRefactoringWarningMessage>();

                // Get all the documents.
                var analyzer = AnalyzerFactory.GetSolutionAnalyzer();
                analyzer.SetSolution(solution);
                var documents = analyzer.GetAllDocuments();

                // For each of the document.
                foreach (IDocument document in documents)
                {
                    // Get all the decendant nodes. 
                    var nodes = ((SyntaxNode) document.GetSyntaxRoot()).DescendantNodes();
                    
                    // For each computer in the given list.
                    foreach (ICodeIssueComputer computer in computers)
                    {
                        // Find all the issues in the document. 
                        var issues = nodes.SelectMany(n => computer.ComputeCodeIssues(document, n));
                        
                        // For each code issue in the document, create a warning message and add it to the list.
                        foreach (CodeIssue issue in issues)
                        {
                            var warningMessage = RefactoringWarningMessageFactory.
                                CreateRefactoringWarningMessage(document, issue, computer);
                            messagesList.Add(warningMessage);
                            logger.Info("Create a refactoring warning.");
                        }
                    }
                }
               
                // Inform all the listeners that new messages are available.
                warningsReady(messagesList.AsEnumerable(), isAdding);
            }
        }
    }
}
