using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.refactoring.detection
{
    internal class RefactoringDetectionUtils
    {
        private static readonly IComparer<SyntaxNode> classNameComparer = new ClassNameComparer();
        private static readonly IComparer<SyntaxNode> methodNameComparer = new MethodNameComparer();

        /* A comparer between two class declarations, return 0 if they have the same identifier. */
        private class ClassNameComparer : IComparer<SyntaxNode>
        {
            public int Compare(SyntaxNode x, SyntaxNode y)
            {
                var classX = (ClassDeclarationSyntax) x;
                var classY = (ClassDeclarationSyntax)y;
                if(classX.Identifier.ValueText.Equals(classY.Identifier.ValueText))
                {
                    return 0;
                }
                return 1;
            }
        }

        /* A comparer between two method declarations, return 0 if they have the same method name. */
        private class MethodNameComparer : IComparer<SyntaxNode>
        {
            public int Compare(SyntaxNode x, SyntaxNode y)
            {
                var methodX = (MethodDeclarationSyntax)x;
                var methodY = (MethodDeclarationSyntax)y;
                if (methodX.Identifier.ValueText.Equals(methodY.Identifier.ValueText))
                {
                    return 0;
                }
                return 1;
            }
        }

        public static IComparer<SyntaxNode> GetClassDeclarationNameComparer()
        {
            return classNameComparer;
        }

        public static IComparer<SyntaxNode> GetMethodDeclarationNameComparer()
        {
            return methodNameComparer;
        }

        /* Get the common node pairs in before and after set of nodes. */
        public static IEnumerable<KeyValuePair<SyntaxNode, SyntaxNode>> GetCommonNodePairs(IEnumerable<SyntaxNode> beforeNodes,
            IEnumerable<SyntaxNode> afterNodes, IComparer<SyntaxNode> comparer)
        {
            var result = new List<KeyValuePair<SyntaxNode, SyntaxNode>>();
            foreach (var before in beforeNodes)
            {
                foreach (var after in afterNodes)
                {
                    if (comparer.Compare(before, after) == 0)
                    {
                        result.Add(new KeyValuePair<SyntaxNode, SyntaxNode>(before, after));
                    }
                }
            }
            return result;
        }

    }
}
