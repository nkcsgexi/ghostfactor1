using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.analyzer;
using warnings.refactoring;

namespace warnings.conditions
{
    /* All the condition checkers for extract method should implement this. */
    abstract class ExtractMethodConditionChecker : IRefactoringConditionChecker
    {
        public RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }

        public ICodeIssueComputer CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            return CheckCondition(before, after, (IManualExtractMethodRefactoring)input);
        }

        /* Except one symbol list from the other by symbol name. */
        protected IEnumerable<ISymbol> GetSymbolListExceptByName(IEnumerable<ISymbol> original, IEnumerable<ISymbol> except)
        {
            var result = new List<ISymbol>();
            foreach (ISymbol o in original)
            {
                var name = o.Name;
                if (!except.Any(e => e.Name.Equals(name)))
                {
                    result.Add(o);
                }
            }
            return result.AsEnumerable();
        }

        /* Remove 'this' symbol in a list of symbols. */
        protected IEnumerable<ISymbol> RemoveThisSymbol(IEnumerable<ISymbol> original)
        {
            return original.Where(s => !s.Name.Equals("this"));
        }

        /* Get the type name tuples by a given symbol list. */
        protected IEnumerable<Tuple<string, string>> GetTypeNameTuples(IEnumerable<ISymbol> symbols)
        {
            var typeNameTuples = new List<Tuple<string, string>>();
            var symbolAnalyzer = AnalyzerFactory.GetSymbolAnalyzer();
            foreach (var symbol in symbols)
            {
                var symbolName = symbol.Name;
                symbolAnalyzer.SetSymbol(symbol);
                var typeName = symbolAnalyzer.GetSymbolTypeName();
                typeNameTuples.Add(Tuple.Create(typeName, symbolName));
            }
            return typeNameTuples.AsEnumerable();
        }



        protected abstract ICodeIssueComputer CheckCondition(IDocument before, IDocument after, IManualExtractMethodRefactoring input);
    }

    /* Condition list for extract method. */
    internal class ExtractMethodConditionsList : RefactoringConditionsList
    {
        private static Lazy<ExtractMethodConditionsList> instance = new Lazy<ExtractMethodConditionsList>();

        private ExtractMethodConditionsList()
        {
        }

        public static IRefactoringConditionsList GetInstance()
        {
            if(instance.IsValueCreated)
                return instance.Value;
            return new ExtractMethodConditionsList();
        }

        protected override IEnumerable<IRefactoringConditionChecker> GetAllConditionCheckers()
        {
            var checkers = new List<IRefactoringConditionChecker>();
            checkers.Add(new ParametersChecker());
            checkers.Add(new ReturnTypeChecker());
            return checkers.AsEnumerable();
        }

        public override RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }
    }

}
