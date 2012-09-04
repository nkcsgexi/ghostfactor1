using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.refactoring.refactoring.manual
{
    public class ManualRefactoringFactory
    {
        public static IManualExtractMethodRefactoring CreateManualExtractMethodRefactoring(SyntaxNode declaration, 
            SyntaxNode invocation, IEnumerable<SyntaxNode> statements)
        {
            return new ManualExtractMethodRefactoring(declaration, invocation, statements);
        }
    }
}
