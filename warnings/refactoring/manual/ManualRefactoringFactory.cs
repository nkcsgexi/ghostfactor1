using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.refactoring
{
    public class ManualRefactoringFactory
    {
        /* Create a manual extract method refactoring that extracts statements. */
        public static IManualExtractMethodRefactoring CreateManualExtractMethodRefactoring(SyntaxNode declaration, 
            SyntaxNode invocation, IEnumerable<SyntaxNode> statements)
        {
            return new ManualExtractMethodRefactoring(declaration, invocation, statements);
        }

        /* Create a manual extract method refacoting that extracts a expression. */
        public static IManualExtractMethodRefactoring CreateManualExtractMethodRefactoring( SyntaxNode declaration, 
            SyntaxNode invocation, SyntaxNode expression)
        {
            return new ManualExtractMethodRefactoring(declaration, invocation, expression);
        }

        /* 
         * Create a manual rename refactoring, the token (of type identifier token) is where the rename is performed on,
         * the new name is the name given to the identifier. Token is in the before version. 
         */
        public static IManualRenameRefactoring CreateManualRenameRefactoring(SyntaxNode node, string newName)
        {
            return new ManualRenameRefactoring(node, newName);
        }
    }
}
