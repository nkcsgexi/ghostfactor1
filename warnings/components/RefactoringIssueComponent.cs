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
        IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node);
    }

    /* The componenet itself is a issue repository and also is a Factor component. */
    public interface IRefactoringCodeIssueComputersComponent :  IFactorComponent, ICodeIssueComputersRepository
    {
            
    }

    internal class RefactoringCodeIssueComputersComponent : IRefactoringCodeIssueComputersComponent
    {
        /* Singleton this component. */
        private static readonly IRefactoringCodeIssueComputersComponent intance = 
            new RefactoringCodeIssueComputersComponent();

        public static IRefactoringCodeIssueComputersComponent GetInstance()
        {
            return intance;
        }

        /* Saving all of the code issue computers. */
        private readonly List<ICodeIssueComputer> codeIssueComputers; 

        /* A single thread workqueue. */
        private WorkQueue queue;

        private Logger logger;

        public ISolution solution { get; set; }

        private RefactoringCodeIssueComputersComponent()
        {
            codeIssueComputers = new List<ICodeIssueComputer>();

            // Single thread workqueue.
            queue = new WorkQueue {ConcurrentLimit = 1};

            // Add a listener for failed work item.
            queue.FailedWorkItem += OnItemFailed;

            logger = NLoggerUtil.getNLogger(typeof (RefactoringCodeIssueComputersComponent));
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


        public void AddCodeIssueComputers(IEnumerable<ICodeIssueComputer> computers)
        {
            lock (codeIssueComputers)
            {
                // Select the computers that are not null computers. 
                var validComputers = computers.Where(c => !c.GetType().Equals(typeof(NullCodeIssueComputer)));
                codeIssueComputers.AddRange(validComputers);
            }
        }

        public IEnumerable<CodeIssue> GetCodeIssues(IDocument document, SyntaxNode node)
        {
            lock (codeIssueComputers)
            {
                var issues = new List<CodeIssue>();
                foreach (var computer in codeIssueComputers)
                {
                    issues.AddRange(computer.ComputeCodeIssues(document, node));
                }
                return issues.AsEnumerable();
            }
        }
    }

    /* Work item to add new issue computers to the repository. */
    internal class AddCodeIssueComputersWorkItem : WorkItem
    {
        private readonly IEnumerable<ICodeIssueComputer> computers;
        private readonly ICodeIssueComputersRepository repository;

        public AddCodeIssueComputersWorkItem(IEnumerable<ICodeIssueComputer> computers, ICodeIssueComputersRepository repository)
        {
            this.computers = computers;
            this.repository = repository;
        }

        public override void Perform()
        {
            repository.AddCodeIssueComputers(computers);
        }
    }
}
