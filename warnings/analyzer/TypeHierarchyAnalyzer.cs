using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;

namespace warnings.analyzer
{
    /* Analyzer to get the entire type hierarchy of a given type. */
    public interface ITypeHierarchyAnalyzer
    {
        void SetSemanticModel(ISemanticModel model);
        void SetDeclaration(SyntaxNode declaration);
        IEnumerable<INamedTypeSymbol> GetBaseTypes();
        IEnumerable<INamedTypeSymbol> GetImplementedInterfaces();
        IEnumerable<INamedTypeSymbol> GetContainingTypes();
        IEnumerable<INamedTypeSymbol> GetContainedTypes();
    }

    internal class TypeHierarchyAnalyzer : ITypeHierarchyAnalyzer
    {
        private ISemanticModel model;
        private SyntaxNode declaration;

        /* Set the semantic model. */
        public void SetSemanticModel(ISemanticModel model)
        {
            this.model = model;
        }

        /* Set the type declaration. */
        public void SetDeclaration(SyntaxNode declaration)
        {
            this.declaration = declaration;
        }

        /* Get all the base type hierarchy. */
        public IEnumerable<INamedTypeSymbol> GetBaseTypes()
        {
            var type = model.GetTypeInfo(declaration).Type;
            var baseTypes = new List<INamedTypeSymbol>();

            // Iteratively get all the base types hierarchy.
            for (var currentType = type; currentType.BaseType != null; currentType = currentType.BaseType)
            {
                baseTypes.Add(currentType.BaseType);
            }
            return baseTypes.AsEnumerable();
        }

        /* Get all the interfaces implemented by this type. */
        public IEnumerable<INamedTypeSymbol> GetImplementedInterfaces()
        {
            var type = model.GetTypeInfo(declaration).Type;
            return type.Interfaces.AsEnumerable();
        }

        /* Get all the types that are containing this type declaration. */
        public IEnumerable<INamedTypeSymbol> GetContainingTypes()
        {
            var type = model.GetTypeInfo(declaration).Type;
            var containingTypeList = new List<INamedTypeSymbol>();

            // Iteratively get all the containing types.
            for (var currentType = type; currentType.ContainingType != null; currentType = currentType.ContainingType)
            {
                containingTypeList.Add(currentType.ContainingType);
            }
            return containingTypeList.AsEnumerable();
        }

        /* Get all the type declarations that are in this type declaration. */
        public IEnumerable<INamedTypeSymbol> GetContainedTypes()
        {
            var type = model.GetTypeInfo(declaration).Type;
            return type.GetTypeMembers().AsEnumerable();
        }
    }
}
