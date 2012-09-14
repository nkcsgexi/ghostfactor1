using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;

namespace warnings.retriever
{
    /* For retrieving the typable identifiers in a given IDocument. */
    public interface ITypablesRetriever
    {
        void SetDocument(IDocument document);
        IEnumerable<Tuple<SyntaxNode, ITypeSymbol>> GetTypableIdentifierTypeTuples();
    }

    internal class TypableRetriever : ITypablesRetriever
    {
        private ISemanticModel model;
        private SyntaxNode root;

        public void SetDocument(IDocument document)
        {
            model = document.GetSemanticModel();
            root = (SyntaxNode) document.GetSyntaxRoot();
        }

        /* Get tuples of node and type. */
        public IEnumerable<Tuple<SyntaxNode, ITypeSymbol>> GetTypableIdentifierTypeTuples()
        {
            var typedIdentifiers = new List<Tuple<SyntaxNode, ITypeSymbol>>();

            // Get all identifiers.
            var identifiers = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.IdentifierName);
            foreach(SyntaxNode id in identifiers)
            {
                // Query type information of an identifier.
                var info = model.GetTypeInfo(id);

                // If type is retrieved, add to the result.
                if(info.Type != null)
                    typedIdentifiers.Add(Tuple.Create(id, info.Type));
            }
            return typedIdentifiers.AsEnumerable();
        }
    }
}
