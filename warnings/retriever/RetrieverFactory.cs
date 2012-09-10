using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace warnings.retriever
{
    public class RetrieverFactory
    {
        public static IRenamableRetriever GetRenamableRetriever(SyntaxNode root)
        {
            return new RenamablesRetriever(root);
        }
    }
}
