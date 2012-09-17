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
    /* A repository from issues can be queried, added, and updated. */
    public interface IIssuedNodesRepository
    {
        ISolution solution { set; get; }
        void AddSingleIssueTracedNode(IIssueTracedNode tracedNode);
        void AddMultipleIssueTracedNodes(IEnumerable<IIssueTracedNode> nodes);
        CodeIssue GetCodeIssue(IDocument document, SyntaxNode node);
        // TODO: when to remove a issued node.
    }

    /* The componenet itself is a issue repository and also is a Factor component. */
    public interface IRefactoringIssuedNodesComponent :  IFactorComponent, IIssuedNodesRepository
    {
            
    }

    internal class RefactoringIssuedNodesComponent : IRefactoringIssuedNodesComponent
    {
        /* Singleton this component. */
        private static readonly IRefactoringIssuedNodesComponent intance = 
            new RefactoringIssuedNodesComponent();

        public static IRefactoringIssuedNodesComponent GetInstance()
        {
            return intance;
        }

        /* A list containing all the issuedNodes with issues. */
        private List<IIssueTracedNode> issuedNodes;
        
        /* A single thread workqueue. */
        private WorkQueue queue;

        private Logger logger;

        public ISolution solution { get; set; }

        private RefactoringIssuedNodesComponent()
        {
            issuedNodes = new List<IIssueTracedNode>();

            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;

            // Add a listener for failed work item.
            queue.FailedWorkItem += OnItemFailed;

            logger = NLoggerUtil.getNLogger(typeof (RefactoringIssuedNodesComponent));
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

        /* Add a issue traced node to the repository, if such node does not exist. */
        public void AddSingleIssueTracedNode(IIssueTracedNode tracedNode)
        {
            // First lock the issuedNodes, cannot read.
            lock(issuedNodes){
                
                // Whether the issued node is already there.
                if (!issuedNodes.Contains(tracedNode))
                {
                    issuedNodes.Add(tracedNode);
                }
            }
        }

        public void AddMultipleIssueTracedNodes(IEnumerable<IIssueTracedNode> nodes)
        {
            lock(issuedNodes)
            {
                foreach (IIssueTracedNode node in nodes)
                {
                    if(!issuedNodes.Contains(node))
                    {
                        issuedNodes.Add(node);
                    }
                }
            }
        }


        public CodeIssue GetCodeIssue(IDocument document, SyntaxNode node)
        {
            // First lock the issued nodes.
            lock (issuedNodes)
            {
                foreach (var iNode in issuedNodes)
                {
                    if(iNode.IsIssuedAt(node))
                        return new CodeIssue(CodeIssue.Severity.Warning, node.Span, 
                            iNode.GetCheckResult().GetProblemDescription());
                }
                return null;
            }
        }
    }

    /* Work item to directly add a new issue traced node to the repository. */
    internal class AddIssueTracedNodeWorkItem : WorkItem
    {
        private readonly IIssuedNodesRepository repository;
        private readonly IIssueTracedNode node;
        private readonly Logger logger;

        public AddIssueTracedNodeWorkItem(IIssueTracedNode node, IIssuedNodesRepository repository)
        {
            this.node = node;
            this.repository = repository;
            this.logger = NLoggerUtil.getNLogger(typeof (AddIssueTracedNodeWorkItem));
        }

        public override void Perform()
        {
            try
            {
                repository.AddSingleIssueTracedNode(node);
            }catch(Exception e)
            {
               logger.Fatal(e);
            }
            
        }
    }

    /* Indirect way of adding issued nodes.*/
    internal class ComputeTracedNodeWorkItem : WorkItem
    {
        private readonly ITracedNodesComputer computer;
        private readonly IIssuedNodesRepository repository;

        public ComputeTracedNodeWorkItem(ITracedNodesComputer computer, IIssuedNodesRepository repository)
        {
            this.computer = computer;
            this.repository = repository;
        }

        public override void Perform()
        {
            // Get all document in the solution.
            var solutionAnalyzer = AnalyzerFactory.GetSolutionAnalyzer();
            solutionAnalyzer.SetSolution(repository.solution);
            var documents = solutionAnalyzer.GetProjects().SelectMany(solutionAnalyzer.GetDocuments);
            
            // For each document, use the given issued nodes computer to compute issued nodes in the 
            // document.
            var issues = documents.SelectMany(computer.ComputeIssuedNodes);
            repository.AddMultipleIssueTracedNodes(issues);
        }
    }

    /*
     * All the other components that want to use the indirect way of adding issued nodes shall implement their
     * own version of ITracedNodesComputer.
     */
    public interface ITracedNodesComputer
    {
        IEnumerable<IIssueTracedNode> ComputeIssuedNodes(IDocument document);
    }
}
