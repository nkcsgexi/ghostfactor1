using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.conditions;
using warnings.quickfix;
using warnings.util;

namespace warnings.components
{
    /* A repository from issues can be queried, added, and updated. */
    public interface IIssuedNodesRepository
    {
        void AddIssueTracedNode(IIssueTracedNode tracedNode);

        CodeIssue GetCodeIssue(SyntaxNode node);

        // TODO: when to remove a issued node.

    }

    /* The componenet itself is a issue repository and also is a Factor component. */
    public interface IRefactoringIssuedNodesComponent :  IFactorComponent, IIssuedNodesRepository
    {
        
    }

    internal class RefactoringIssuedNodesComponent : IRefactoringIssuedNodesComponent
    {
        /* Singleton this component. */
        private static readonly IRefactoringIssuedNodesComponent intance = new RefactoringIssuedNodesComponent();

        public static IRefactoringIssuedNodesComponent GetInstance()
        {
            return intance;
        }

        /* A list containing all the issuedNodes with issues. */
        private List<IIssueTracedNode> issuedNodes;
        
        /* A single thread workqueue. */
        private WorkQueue queue;

        private Logger logger;

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
            if(queue.Count == 0)
            {
                queue.Add(item);
            }
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
        public void AddIssueTracedNode(IIssueTracedNode tracedNode)
        {
            // First lock the issuedNodes, cannot read.
            lock(issuedNodes){
                // Whether the issued node is already there.
                if (!issuedNodes.Contains(tracedNode))
                {
                    issuedNodes.Add(tracedNode);
                    logger.Info("IIssueTracedNode is added. Count: " + issuedNodes.Count);
                }
            }
        }


        public CodeIssue GetCodeIssue(SyntaxNode node)
        {
            // First lock the issued nodes.
            lock (issuedNodes)
            {
                foreach (var iNode in issuedNodes)
                {
                    if(iNode.IsIssuedAt(node))
                        return new CodeIssue(CodeIssue.Severity.Warning, node.Span, iNode.GetCheckResult().GetProblemDescription());
                }
                return null;
            }
        }
    }

    /* Work item to add a new issue traced node to the repository. */
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
                repository.AddIssueTracedNode(node);
            }catch(Exception e)
            {
               logger.Fatal(e);
            }
            
        }
    } 
}
