using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring.detection
{
    class ChangeMethodSignatureDetector : IExternalRefactoringDetector
    {
        private readonly Logger logger = NLoggerUtil.getNLogger(typeof (ChangeMethodSignatureDetector));

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
            private readonly Logger logger = NLoggerUtil.getNLogger(typeof (InMethodExtraceMethodDectector));

            private readonly SyntaxNode beforeMethod;
            private readonly SyntaxNode afterMethod;
           
            private readonly IParameterAnalyzer paraAnalzyer;
            private readonly IMethodAnalyzer methodAnalyzer;

            private IManualRefactoring refactoring;
           

            internal InMethodChangeSignatureDetector(SyntaxNode beforeMethod, SyntaxNode afterMethod)
            {
                this.beforeMethod = beforeMethod;
                this.afterMethod = afterMethod;
                this.methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();
                this.paraAnalzyer = AnalyzerFactory.GetParameterAnalyzer();
            }

            public bool hasRefactoring()
            {
                // Mapping parameters in before and after version, for example, f(int a, int b) and f(int b, int a)
                // 's mapper shall be <0,1><1,0>>
                var parametersMap = new List<Tuple<int, int>>();

                // Combine the type of parameters in before and after method declaration as a string.
                var typeStringBefore = GetParameterTypeCombined(beforeMethod);
                var typeStringAfter = GetParameterTypeCombined(afterMethod);

                // If the strings are not equal, compiler warnings are enough to detect unchanged method signatures.
                if(typeStringBefore.Equals(typeStringAfter))
                {
                    // Get indexes for parameters in the before and after version.
                    var beforeParaUsageIndexes = GetParameterUsagesIndexes(beforeMethod);
                    var afterParaUsageIndexes = GetParameterUsagesIndexes(afterMethod);

                    // Iterate usages for each parameter in the after version
                    for (int i = 0; i < afterParaUsageIndexes.Count(); i++ )
                    {
                        // Retrieve the usage indexes for this parameter.
                        var afterIndex = afterParaUsageIndexes.ElementAt(i);

                        // Iterate usages for each parameter in the before version.
                        for (int j = 0; j < beforeParaUsageIndexes.Count(); j++)
                        {
                            // Retrieve the usage indexes for current parameter.
                            var beforeIndex = beforeParaUsageIndexes.ElementAt(j);

                            // If all the indexes are equal, finding a map.
                            if(AreAllElemenetsEqual(afterIndex, beforeIndex))
                            {
                                logger.Info("Para " + j + " in before version's usage indexes:" + 
                                    StringUtil.ConcatenateAll(",", beforeIndex.Select(integer => integer.ToString())));
                                logger.Info("Para " + i + " in after version's usage indexes:" + 
                                    StringUtil.ConcatenateAll(",", afterIndex.Select(integer => integer.ToString())));
                                logger.Info("Parameter mapping: " + j + "=>" + i);
                                parametersMap.Add(Tuple.Create(j, i));
                            }
                        }
                    }
                    
                    // Search for all the tuples in the map.
                    foreach (var pair in parametersMap)
                    {
                        // If a tuple has values that are different, positions of parameters are changed.
                        if (pair.Item1 != pair.Item2)
                        {
                            // Order the map by indexes in before versions.
                            var orderedList = new List<Tuple<int,int>>();
                            orderedList.AddRange(parametersMap.OrderBy(t => t.Item1));
                            refactoring = ManualRefactoringFactory.
                                CreateManualChangeMethodSignatureRefactoring(afterMethod, orderedList);
                            return true;
                        }
                    }
                }
               
                return false;
            }

            public IEnumerable<IManualRefactoring> getRefactorings()
            {
                yield return refactoring;
            }


            /* 
             * Given a method declaration, get the node index of usages of each parameter in all 
             * the parameter usages. 
             */
            private IEnumerable<IEnumerable<int>> GetParameterUsagesIndexes(SyntaxNode method)
            {
                // List of usages of all parameters.
                var list = new List<IEnumerable<int>>();

                // Get usages for each parameter.
                methodAnalyzer.SetMethodDeclaration(method);
                var usages = methodAnalyzer.GetParameterUsages();
                
                // Combine usages, and sort them by the start position.
                var combinedUsages = CombineNodesGroups(usages);
                combinedUsages = combinedUsages.OrderBy(n => n.Span.Start);
                logger.Info("Combined usages:" + StringUtil.ConcatenateAll(",", combinedUsages.Select(n => n.Span.ToString())));

                // for each parameter
                foreach(var group in usages)
                {
                    logger.Info("Group usages:" + StringUtil.ConcatenateAll(",", group.Select(n => n.Span.ToString())));

                    // Get the indexes of its usages in the combined pool, and sort them.
                    var indexes = GetNodesIndexes(group, combinedUsages).OrderBy(i => i);
                    logger.Info("Indexes are: " + StringUtil.ConcatenateAll(",", indexes.Select(i => i.ToString())));
                    list.Add(indexes);
                }
                return list.AsEnumerable();
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
            private IEnumerable<SyntaxNode> CombineNodesGroups(IEnumerable<IEnumerable<SyntaxNode>> groups)
            {
                var list = new List<SyntaxNode>();
                foreach (var g in groups)
                {
                    list.AddRange(g);
                    logger.Info("Group: " + StringUtil.ConcatenateAll(",", g.Select(n => n.Span.ToString())));
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
                    for(int i = 0; i < allNodes.Count(); i++)
                    {
                        var node2 = allNodes.ElementAt(i);
                        if (node1.Span.Equals(node2.Span))
                            list.Add(i);
                    }
                }
                return list.AsEnumerable();
            }

            /* Given two list of int, whether two elements of the same index are equal. */
            private bool AreAllElemenetsEqual(IEnumerable<int> list1, IEnumerable<int> list2)
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
