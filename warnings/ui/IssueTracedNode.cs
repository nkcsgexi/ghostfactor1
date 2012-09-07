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
        bool IsIssuedAt(SyntaxNode another);
        SyntaxNode GetSyntaxNode();
        ICheckingResult GetCheckResult();
    }
 
    internal class IssueTracedNode : IIssueTracedNode
    {
        // The node where the refactoring issue was first detected. 
        private readonly SyntaxNode node;

        // The problem detected.
        private readonly ICheckingResult result;

        public IssueTracedNode(SyntaxNode node, ICheckingResult result)
        {
            this.node = node;
            this.result = result;
        }

        /* Under which condition the traced nodes are equivalent to each other.*/
        public bool Equals(IIssueTracedNode o)
        {
            var another = (IssueTracedNode) o;
            // First the original nodes should be having same code.
            return another.node.GetText().Equals(node.GetText())
                    // And also the description of problems should be same. 
                   && another.result.GetProblemDescription().Equals(result.GetProblemDescription());
        }


        public bool IsIssuedAt(SyntaxNode another)
        {
            // TODO: more sofisticated way to determine whether it is the right node
            // What if they are slightly different, say a space is added. 
            return another.GetText().Equals(node.GetText());
        }

        public SyntaxNode GetSyntaxNode()
        {
            return node;
        }

        public ICheckingResult GetCheckResult()
        {
            return result;
        }

    }
}
