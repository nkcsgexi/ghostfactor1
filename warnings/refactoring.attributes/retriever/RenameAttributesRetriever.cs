using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.refactoring.attributes
{
    class RenameAttributesRetriever : IAttributesRetriever
    {
        public IRefactoringAttributes getAttributes()
        {
            throw new NotImplementedException();
        }

        public void setImportantSyntaxNode(SyntaxNode node)
        {
            throw new NotImplementedException();
        }

        public void retrieveAttributes()
        {
            throw new NotImplementedException();
        }
    }
}
