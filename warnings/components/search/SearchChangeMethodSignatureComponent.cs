using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.conditions;
using warnings.quickfix;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.retriever;
using warnings.source;
using warnings.source.history;
using warnings.util;

namespace warnings.components.search
{
    class SearchChangeMethodSignatureComponent : SearchRefactoringComponent
    {
        private static readonly IFactorComponent instance = new SearchChangeMethodSignatureComponent();

        public static IFactorComponent GetInstance()
        {
            return instance;
        }

        public override string GetName()
        {
            return "SearchChangeMethodSignatureComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchChangeMethodSignatureComponent));
        }
    }

    class SearchChangeMethodSignatureWorkItem : SearchRefactoringWorkitem
    {
        public SearchChangeMethodSignatureWorkItem(ICodeHistoryRecord latestRecord) : base(latestRecord)
        {
        }

        protected override IExternalRefactoringDetector getRefactoringDetector()
        {
            return RefactoringDetectorFactory.CreateChangeMethodSignatureDetector();
        }

        protected override int getSearchDepth()
        {
            return 10;
        }

        protected override void onRefactoringDetected(ICodeHistoryRecord before, ICodeHistoryRecord after,
            IEnumerable<IManualRefactoring> refactorings)
        {
            logger.Info("Change Method Signature Detected.");
            logger.Info("Before:\n" + before.getSource());
            logger.Info("After:\n" + after.getSource());

            // Get all the method declarations in the detected refactorings.
            var declarations = refactorings.Select(r => (IChangeMethodSignatureRefactoring) r).
                Select(r => r.ChangedMethodDeclaration);

            // Initiate the computer and add to the issue component.
            var computer = new UnchangedMethodInvocationComputer(declarations);
            GhostFactorComponents.refactoringIssuedNodeComponent.Enqueue(new ComputeTracedNodeWorkItem(computer, 
                GhostFactorComponents.refactoringIssuedNodeComponent));
        }

        protected override void onNoRefactoringDetected(ICodeHistoryRecord record)
        {
            logger.Info("No change method signature detected.");
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (SearchChangeMethodSignatureWorkItem));
        }

        /* The computer for calculating the unchanged method invocations. */
        private class UnchangedMethodInvocationComputer : ITracedNodesComputer
        {
            private readonly IEnumerable<SyntaxNode> declarations;

            public UnchangedMethodInvocationComputer(IEnumerable<SyntaxNode> declarations)
            {
                this.declarations = declarations;
            }

            public IEnumerable<IIssueTracedNode> ComputeIssuedNodes(IDocument document)
            {
                var list = new List<IIssueTracedNode>();

                // Get the key for the given real IDocument.
                var analyzer = AnalyzerFactory.GetDocumentAnalyzer();
                analyzer.SetDocument(document);
                var key = analyzer.GetKey();

                // Retrievers for method invocations.
                var retriever = RetrieverFactory.GetMethodInvocationRetriever();
                retriever.SetDocument(document);

                // For every declaration, check whether its inovcations in current document exist.
                foreach (SyntaxNode declaration in declarations)
                {
                    // Get all the invocations in the current document.
                    retriever.SetMethodDeclaration(declaration);
                    var invocations = retriever.GetInvocations();

                    // Convert all the invocations to issued nodes and add them all
                    // to the list.
                    list.AddRange(invocations.Select(i => new IssueTracedNode(key, i, new NeedUpdateInvocation())));
                }
                return list.AsEnumerable();
            }
        }

        /* Issue of method invocations that should be updated. */
        private class NeedUpdateInvocation : ICheckingResult
        {
            public RefactoringType type
            {
                get { return RefactoringType.CHANGE_METHOD_SIGNATURE; }
            }

            public bool HasProblem()
            {
                return true;
            }

            public string GetProblemDescription()
            {
                return "Method invocation needs to be updated.";
            }
        }

    }
}
