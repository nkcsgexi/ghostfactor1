using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring.detection
{
    class ChangeMethodSignatureDetector : IExternalRefactoringDetector
    {
        private string beforeSource;

        private SyntaxNode beforeRoot;
        
        private string afterSource;

        private SyntaxNode afterRoot;

        private IEnumerable<IManualRefactoring> refactorings; 

        public bool hasRefactoring()
        {
            // Hosting all the detected refactorings.
            var detectedRefactorings = new List<IManualRefactoring>();

            // Convert string to IDocument instance.
            var converter = new String2IDocumentConverter();
            var documentBefore = (IDocument)converter.Convert(beforeSource, null, null, null);
            var documentAfter = (IDocument)converter.Convert(afterSource, null, null, null);

            // Group mehthos by scopes, a scope is defined by namespace name + class name.
            var analyzer = AnalyzerFactory.GetDocumentAnalyzer();
            analyzer.SetDocument(documentBefore);
            var methodsGroupsBefore = GetMethodInSameScope(analyzer);
            analyzer.SetDocument(documentAfter);
            var methodsGroupsAfter = GetMethodInSameScope(analyzer);

            // Get the copes that are common in both before and after document.
            var commonKeys = methodsGroupsAfter.Keys.Intersect(methodsGroupsBefore.Keys);

            // For each common scope
            foreach (var key in commonKeys)
            {
                IEnumerable<SyntaxNode> methodsBefore, methodsAfter;
 
                // Get methods in the before document within this scope.
                methodsGroupsBefore.TryGetValue(key, out methodsBefore);

                // Get methods in the after documetn within this scope
                methodsGroupsAfter.TryGetValue(key, out methodsAfter);

                foreach (MethodDeclarationSyntax methodbefore in methodsBefore)
                {
                    foreach (MethodDeclarationSyntax methodAfter in methodsAfter)
                    {
                        // Consider two methods are before and after version if they are in the same scope
                        // and also have the same identifier (method name).
                        if(methodbefore.Identifier.ValueText.Equals(methodAfter.Identifier.ValueText))
                        {
                            // Get an in-method detector.
                            var detector = new InMethodChangeSignatureDetector(methodbefore, methodAfter);
                            if(detector.hasRefactoring())
                            {
                                // Add the detected refactorings
                                detectedRefactorings.AddRange(detector.getRefactorings());
                            }
                        }
                    }
                }
            }
            if(detectedRefactorings.Any())
            {
                this.refactorings = detectedRefactorings.AsEnumerable();
                return true;
            }

            return false;
        }

       

        public IEnumerable<IManualRefactoring> getRefactorings()
        {
            return refactorings;
        }

        public void setSourceBefore(string source)
        {
            this.beforeSource = source;
            this.beforeRoot = ASTUtil.getSyntaxTreeFromSource(beforeSource).GetRoot();
        }

        public string getSourceBefore()
        {
            return beforeSource;
        }

        public void setSourceAfter(string source)
        {
            this.afterSource = source;
            this.afterRoot = ASTUtil.getSyntaxTreeFromSource(afterSource).GetRoot();
        }

        public string getSourceAfter()
        {
            return afterSource;
        }

        /* Get all the method declarations in the document and group them by scope, i.e., namespace + class.*/
        private Dictionary<String, IEnumerable<SyntaxNode>> GetMethodInSameScope(IDocumentAnalyzer analyzer)
        {
            // Dictionary for using namespace name + class name to get all the methods.
            var dictionary = new Dictionary<String, IEnumerable<SyntaxNode>>();

            // Get all the namespace
            var namespaces = analyzer.GetNamespaceDecalarations();
            foreach (NamespaceDeclarationSyntax space in namespaces)
            {
                string namespaceName = space.Name.GetText();

                // In each namespace, get all the class declarations.
                var classes = analyzer.GetClassDeclarations(space);
                foreach (ClassDeclarationSyntax cla in classes)
                {
                    string className = cla.Identifier.ValueText;

                    // In each class declaration, get all the method delcarations, they are in the same
                    // scope.
                    dictionary.Add(namespaceName + className, analyzer.GetMethodDeclarations(cla));
                }
            }
            return dictionary;
        }


        private class InMethodChangeSignatureDetector : IRefactoringDetector
        {
            private readonly SyntaxNode beforeMethod;
            private readonly SyntaxNode afterMethod;
           
            private readonly IParameterAnalyzer paraAnalzyer;
            private readonly IMethodAnalyzer methodAnalyzer;

            private IEnumerable<IManualRefactoring> refactorings;
           

            internal InMethodChangeSignatureDetector(SyntaxNode beforeMethod, SyntaxNode afterMethod)
            {
                this.beforeMethod = beforeMethod;
                this.afterMethod = afterMethod;
                this.methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();
                this.paraAnalzyer = AnalyzerFactory.GetParameterAnalyzer();
            }

            public bool hasRefactoring()
            {
                var typeStringBefore = GetParameterTypeCombined(beforeMethod);
                var typeStringAfter = GetParameterTypeCombined(afterMethod);
                if(typeStringBefore.Equals(typeStringAfter))
                {
                    methodAnalyzer.SetMethodDeclaration(beforeMethod);
                    var beforeUsages = methodAnalyzer.GetParameterUsages();
                    methodAnalyzer.SetMethodDeclaration(afterMethod);
                    var afterUsages = methodAnalyzer.GetParameterUsages();
                }
                return false;
            }

            public IEnumerable<IManualRefactoring> getRefactorings()
            {
                return refactorings;
            }

            /* 
             * Combine the type of parameters in a method declaration as a string, deleting all the white 
             * space among the combined string.
             */
            private string GetParameterTypeCombined(SyntaxNode method)
            {
                var sb = new StringBuilder();
                
                // Get all the parameters in the method.
                methodAnalyzer.SetMethodDeclaration(method);
                var paras = methodAnalyzer.GetParameters();

                // For each parameter, get its type and combined to the string builder
                foreach (SyntaxNode para in paras)
                {
                    paraAnalzyer.SetParameter(para);
                    sb.Append(paraAnalzyer.GetParameterType().GetText());
                }

                // Return the combined string, replacing all trivial space in it.
                return sb.ToString().Replace(" ", "");
            }

            /* Combine all the nodes in a nodes' gourp to one group of nodes. */
            IEnumerable<SyntaxNode> CombineNodesGroups(IEnumerable<IEnumerable<SyntaxNode>> groups)
            {
                var list = new List<SyntaxNode>();
                foreach (var group in groups)
                {
                    list.AddRange(group);
                }
                return list.AsEnumerable();
            }

            /* 
             * For a given list of nodes interested, and a pool of nodes, get the list of indexes of these interested
             * nodes in the pool.
             */
            private IEnumerable<int> GetNodesIndexes(IEnumerable<SyntaxNode> nodes, IEnumerable<SyntaxNode> allNodes)
            {
                var list = new List<int>();
                foreach (var node1 in nodes)
                {
                    for(int i=0; i< allNodes.Count(); i++)
                    {
                        var node2 = allNodes.ElementAt(i);
                        if (node1.Span.Equals(node2.Span))
                            list.Add(i);
                    }
                }
                return list.AsEnumerable();
            }

            /* Given two list of int, whether two elements of the same index are equal. */
            private bool AreAllElemenetEqual(IEnumerable<int> list1, IEnumerable<int> list2)
            {
                if(list1.Count() == list2.Count())
                {
                    for (int i = 0; i < list1.Count(); i++)
                    {
                        if (list1.ElementAt(i) != list2.ElementAt(i))
                            return false;
                    }
                    return true;
                }
                return false;
            }

        }
    }



}
