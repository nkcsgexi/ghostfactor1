using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.conditions;
using warnings.refactoring.attributes;
using warnings.source;

namespace warnings.refactoring
{
    /* Refactoring input that shall be feed in to the checker. */
    public interface IManualRefactoring : IHasRefactoringType
    {
    }

    /* Containing all the information about the extract method information. */
    public class ManualExtractMethodRefactoring : IManualRefactoring
    {
        /* Method declaration node of the extracted method. */
        public SyntaxNode ExtractedMethodDeclaration { private set; get; }

        /* Method invocation node where the extracted method is invoked. */
        public SyntaxNode ExtractMethodInvocation { private set; get; }

        /* Statements to extract in the original code. */
        public IEnumerable<SyntaxNode> ExtractedStatements { private set; get; }

        public RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }

        public ManualExtractMethodRefactoring(SyntaxNode declaration, SyntaxNode invocation, IEnumerable<SyntaxNode> statements )
        {
            ExtractedMethodDeclaration = declaration;
            ExtractMethodInvocation = invocation;
            ExtractedStatements = statements;
        }
    }

    public class ManaulRenameRefactoring : IManualRefactoring
    {
        public RefactoringType type
        {
            get { return RefactoringType.RENAME; }
        }
    }
}
