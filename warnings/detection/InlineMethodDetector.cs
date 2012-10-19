using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring.detection
{
    internal class InlineMethodDetector : IExternalRefactoringDetector
    {
        private readonly Logger logger;

        private string sourceAfter;
        private string sourceBefore;
        private List<IManualRefactoring> refactorings;
 
        internal InlineMethodDetector()
        {
            logger = NLoggerUtil.GetNLogger(typeof (InlineMethodDetector));
            refactorings = new List<IManualRefactoring>();
        }

        public bool HasRefactoring()
        {
            refactorings.Clear();

            // Parse the source code before and after to get the tree structures.
            var treeBefore = ASTUtil.GetSyntaxTreeFromSource(sourceBefore);
            var treeAfter = ASTUtil.GetSyntaxTreeFromSource(sourceAfter);

            // Parse the before and after code and get the roots of these trees.
            var rootBefore = treeBefore.GetRoot();
            var rootAfter = treeAfter.GetRoot();

            // Get classes contained in these root.
            var classesBefore = ASTUtil.GetClassDeclarations(rootBefore);
            var classesAfter = ASTUtil.GetClassDeclarations(rootAfter);

            // Get the class pairs with common class name
            var commonNodePairs = RefactoringDetectionUtils.GetCommonNodePairs(classesBefore, classesAfter,
                RefactoringDetectionUtils.GetClassDeclarationNameComparer());
            var inClassDetector = new InClassInlineMethodDetector(treeBefore, treeAfter);

            // For each pair of common class.
            foreach (var pair in commonNodePairs)
            {
                // Get the common class before and after. 
                var classBefore = pair.Key;
                var classAfter = pair.Value;

                // Invoke in class detector to find refactorings.
                inClassDetector.SetSyntaxNodeBefore(classBefore);
                inClassDetector.SetSyntaxNodeAfter(classAfter);
                if(inClassDetector.HasRefactoring())
                {
                    refactorings.AddRange(inClassDetector.GetRefactorings());
                }
            }
            return refactorings.Any();
        }

        public IEnumerable<IManualRefactoring> GetRefactorings()
        {
            return refactorings;
        }

        public void SetSourceBefore(string source)
        {
            this.sourceBefore = source;
        }

        public string GetSourceBefore()
        {
            return sourceBefore;
        }

        public void SetSourceAfter(string source)
        {
            this.sourceAfter = source;
        }

        public string GetSourceAfter()
        {
            return sourceAfter;
        }

        /* Inline refactoring detector in the class level. */
        private class InClassInlineMethodDetector : IRefactoringDetector, IBeforeAndAfterSyntaxNodeKeeper
        {
            private readonly List<IManualRefactoring> refactorings;
            private readonly SyntaxTree treeBefore;
            private readonly SyntaxTree treeAfter;

            private SyntaxNode beforeClass; 
            private SyntaxNode afterClass;
            

            internal InClassInlineMethodDetector(SyntaxTree treeBefore, SyntaxTree treeAfter)
            {
                refactorings = new List<IManualRefactoring>();
                this.treeBefore = treeBefore;
                this.treeAfter = treeAfter;
            }

            public bool HasRefactoring()
            {
                refactorings.Clear();

                // Get the methods in before and after class.
                var methodsBefore = ASTUtil.GetMethodsDeclarations(beforeClass);
                var methodsAfter = ASTUtil.GetMethodsDeclarations(afterClass);

                // Get the common methods in before and after class. Common means same name.
                var commonMethodsPairs = RefactoringDetectionUtils.GetCommonNodePairs(methodsBefore, methodsAfter,
                    RefactoringDetectionUtils.GetMethodDeclarationNameComparer());

                // Get the methods that are in the before version but are not in the after version.
                var removedMethodsBefore = methodsBefore.Except(commonMethodsPairs.Select(p => p.Key));
                var inMethodDetector = new InMethodInlineRefactoringDetector(treeBefore, treeAfter);

                // For each removed method.
                foreach (var removed in removedMethodsBefore)
                {
                    // For each pair of common methods.
                    foreach (var pair in commonMethodsPairs)
                    {
                        // Get the invocations of the removed method. 
                        var invocations = ASTUtil.GetAllInvocationsInMethod(pair.Key, removed, treeBefore);

                        // If such invocations exist
                        if(invocations.Any())
                        {
                            // Configure the in method detector.
                            inMethodDetector.SetSyntaxNodeBefore(pair.Key);
                            inMethodDetector.SetSyntaxNodeAfter(pair.Value);
                            inMethodDetector.SetRemovedMethod(removed);
                            inMethodDetector.SetRemovedInvocations(invocations);

                            // If a refactoring is detected, add it to the list.
                            if(inMethodDetector.HasRefactoring())
                            {
                                refactorings.AddRange(inMethodDetector.GetRefactorings());
                            }
                        }
                    }
                }
                return refactorings.Any();
            }

            public IEnumerable<IManualRefactoring> GetRefactorings()
            {
                return refactorings;
            }

            public void SetSyntaxNodeBefore(SyntaxNode before)
            {
                this.beforeClass = before;
            }

            public void SetSyntaxNodeAfter(SyntaxNode after)
            {
                this.afterClass = after;
            }

            /* Inline method refactoring detector in the method level. */
            private class InMethodInlineRefactoringDetector : IRefactoringDetector, IBeforeAndAfterSyntaxNodeKeeper
            {
                private readonly static int COUNT_THRESHHOLD = 2;
                private readonly List<IManualRefactoring> refactorings; 
                private readonly SyntaxTree treeBefore;
                private readonly SyntaxTree treeAfter;

                private SyntaxNode methodBefore;
                private SyntaxNode methodAfter;
                private SyntaxNode methodRemoved;
                private IEnumerable<SyntaxNode> invocationsRemoved;
                

                internal InMethodInlineRefactoringDetector(SyntaxTree treeBefore, SyntaxTree treeAfter)
                {
                    this.refactorings = new List<IManualRefactoring>();
                    this.treeBefore = treeBefore;
                    this.treeAfter = treeAfter;
                }

                public bool HasRefactoring()
                {
                    refactorings.Clear();

                    // Get the inlined statements.
                    var inlinedStatements = GetLongestNeigboredStatements(GetCommonStatements(methodAfter, methodRemoved));
                    
                    // If the inlined statements are above threshhold, an inline method refactoring is detected.
                    if (inlinedStatements.Count() > COUNT_THRESHHOLD)
                    {
                        var refactoring = ManualRefactoringFactory.CreateManualInlineMethodRefactoring
                            // Only considering the first invocation.
                            (methodBefore, methodAfter, methodRemoved, invocationsRemoved.First(), inlinedStatements);
                        refactorings.Add(refactoring);
                        return true;
                    }
                    return false;
                }

                public IEnumerable<IManualRefactoring> GetRefactorings()
                {
                    return refactorings;
                }

                public void SetSyntaxNodeBefore(SyntaxNode before)
                {
                    this.methodBefore = before;
                }

                public void SetSyntaxNodeAfter(SyntaxNode after)
                {
                    this.methodAfter = after;
                }

                public void SetRemovedMethod(SyntaxNode methodRemoved)
                {
                    this.methodRemoved = methodRemoved;
                }

                public void SetRemovedInvocations(IEnumerable<SyntaxNode> invocationsRemoved)
                {
                    this.invocationsRemoved = invocationsRemoved;
                }

                private IEnumerable<SyntaxNode> GetCommonStatements(SyntaxNode methodAfter, SyntaxNode inlinedMethod)
                {
                    // Get all the statements in the caller after inlining. 
                    var methodAnalyzer = AnalyzerFactory.GetMethodDeclarationAnalyzer();
                    methodAnalyzer.SetMethodDeclaration(methodAfter);
                    var afterMethodStatements = methodAnalyzer.GetStatements();

                    // Get all the statements in the inlined method.
                    methodAnalyzer.SetMethodDeclaration(inlinedMethod);
                    var inlinedMethodStatements = methodAnalyzer.GetStatements();
                    var commonStatements = new List<SyntaxNode>();
                    
                    // Get the statements in the caller method after inlining that also appear in the
                    // inlined method.
                    foreach (var afterStatement in afterMethodStatements)
                    {
                        foreach (var inlinedStatement in inlinedMethodStatements)
                        {
                            if(IsStatementsSame(afterStatement, inlinedStatement))
                            {
                                commonStatements.Add(afterStatement);
                            }
                        }
                    }
                    return commonStatements;
                }

                private bool IsStatementsSame(SyntaxNode n1, SyntaxNode n2)
                {
                    var s1 = n1.GetText().Replace(" ", "");
                    var s2 = n2.GetText().Replace(" ", "");
                    return s1.Equals(s2);
                }

                private IEnumerable<SyntaxNode> GetLongestNeigboredStatements(IEnumerable<SyntaxNode> statements)
                {
                    var analyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
                    analyzer.SetSyntaxNodes(statements);
                    return analyzer.GetLongestNeighborredNodesGroup();
                }
            }
        }
    }
}
