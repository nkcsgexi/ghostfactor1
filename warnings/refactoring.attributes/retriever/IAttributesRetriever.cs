using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.refactoring.attributes
{
    public interface IAttributesRetriever
    {
        IRefactoringAttributes getAttributes();
        void setImportantSyntaxNode(SyntaxNode node);
        void retrieveAttributes();
    }

}
