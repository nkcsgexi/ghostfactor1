using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.conditions;

namespace warnings.quickfix
{
    /* Interface to keep track of a node who has a refactoring issue related to it. */
    public interface IIssueTracedNode : IEquatable<IIssueTracedNode>
    {
        void SetNewDocument(IDocument document);
        bool IsIssueResolved();
        CodeIssue GetCodeIssue();
    }
 
    internal class IssueTracedNode : IIssueTracedNode
    {
        // The node where the refactoring issue was first detected. 
        private readonly SyntaxNode originalNode;

        // The problem detected.
        private readonly ICheckingResult result;

        // When new document comes, the updated node in the new doc where the issue is. 
        private SyntaxNode updatedNode;

        public IssueTracedNode(SyntaxNode originalNode, ICheckingResult result)
        {
            this.originalNode = originalNode;
            this.result = result;
            this.updatedNode = originalNode;
        }

        public void SetNewDocument(IDocument document)
        {
            var root = (SyntaxNode) document.GetSyntaxRoot();

            // Find the exactly same node in the new document and it should be the updated node.
            updatedNode = GetIdenticalDecendent(root, updatedNode);
        }

        public bool IsIssueResolved()
        {
            // If no updated node was found, means no issue there any more. 
            return updatedNode == null;
        }

        /* Under which condition the traced nodes are equivalent to each other.*/
        public bool Equals(IIssueTracedNode o)
        {
            var another = (IssueTracedNode) o;
            // First the original nodes should be having same code.
            return another.originalNode.GetText().Equals(originalNode.GetText())
                    // And also the description of problems should be same. 
                   && another.result.GetProblemDescription().Equals(result.GetProblemDescription());
        }


        public CodeIssue GetCodeIssue()
        {
            if (!IsIssueResolved())
                return new CodeIssue(CodeIssue.Severity.Warning, updatedNode.Span, result.GetProblemDescription());
            else
                return null;
        }

        private SyntaxNode GetIdenticalDecendent(SyntaxNode root, SyntaxNode decendent)
        {
            var allIdenticals = root.DescendantNodes().Where(d => d.GetText().Equals(decendent));
            if (allIdenticals.Any())
                return allIdenticals.First();
            return null;
        }

    }
}
