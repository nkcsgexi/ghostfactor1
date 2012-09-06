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
    public interface IIssuedNodesRepository
    {
        void AddIssueTracedNode(IIssueTracedNode tracedNode);

        IEnumerable<CodeIssue> GetCodeIssues();

        void UpdateIssues(IDocument document);
    }

    public interface IRefactoringIssuedNodesComponent :  IFactorComponent, IIssuedNodesRepository
    {
        
    }

    internal class RefactoringIssuedNodesComponent : IRefactoringIssuedNodesComponent
    {
        private static readonly IRefactoringIssuedNodesComponent intance = new RefactoringIssuedNodesComponent();

        public static IRefactoringIssuedNodesComponent GetInstance()
        {
            return intance;
        }

        /* A list containing all the issuedNodes with issues. */
        private List<IIssueTracedNode> issuedNodes;

        /* All the issues. */
        /*
         * Reads and writes of the following data types are atomic: bool, char, byte, sbyte, short, 
         * ushort, uint, int, float, and reference types.
         */
        private IEnumerable<CodeIssue> issues; 
        
        /* A single thread workqueue. */
        private WorkQueue queue;

        private Logger logger;

        private RefactoringIssuedNodesComponent()
        {
            issuedNodes = new List<IIssueTracedNode>();
            issues = Enumerable.Empty<CodeIssue>();

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
            if(!issuedNodes.Contains(tracedNode))
            {
                issuedNodes.Add(tracedNode);
            }
        }

        /* Get the issues. */
        public IEnumerable<CodeIssue> GetCodeIssues()
        {
            return issues;
        }

        /* Update all the issues given a new document instance. */
        public void UpdateIssues(IDocument document)
        {
            var newIssuedNode = new List<IIssueTracedNode>();
            foreach (var iNode in issuedNodes)
            {
                iNode.SetNewDocument(document);
                if (!iNode.IsIssueResolved())
                {
                    newIssuedNode.Add(iNode);
                }
            }
            issuedNodes = newIssuedNode;
            issues = issuedNodes.Select(n => n.GetCodeIssue());    
        }
    }

    /* Work item to add a new issue traced node to the repository. */
    internal class AddIssueTracedNodeWorkItem : WorkItem
    {
        private readonly IIssuedNodesRepository repository;
        private readonly IIssueTracedNode node;

        public AddIssueTracedNodeWorkItem(IIssueTracedNode node, IIssuedNodesRepository repository)
        {
            this.node = node;
            this.repository = repository;
        }

        public override void Perform()
        {
            repository.AddIssueTracedNode(node);
        }
    }

    /* Work item to update the existing issues by giving a new document. */
    internal class UpdateIssuesWorkItem : WorkItem
    {
        private readonly IIssuedNodesRepository repository;
        private readonly IDocument document;

        public UpdateIssuesWorkItem(IDocument document, IIssuedNodesRepository repository)
        {
            this.document = document;
            this.repository = repository;
        }

        public override void Perform()
        {
            repository.UpdateIssues(document);
        }
    }
  


}
