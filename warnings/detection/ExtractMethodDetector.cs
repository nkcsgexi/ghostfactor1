using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NLog;
using Roslyn.Compilers.CSharp;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring.detection
{

    /* 
     * This is a detector for extract method refactoring. After setting the code before and after some time interval, the detector should be able to 
     * tell whether there is a reafactoring performed. 
     */
    internal class ExtractMethodDetector : IExternalRefactoringDetector
    {
        /* Source code before. */
        private String before;

        /* Source code later. */
        private String after;
        
        /* Detected manual refactorings.*/
        private IEnumerable<IManualRefactoring> refactorings = Enumerable.Empty<IManualRefactoring>();

        public void setSourceBefore(String before)
        {
            this.before = before;
        }

        public string getSourceBefore()
        {
            return before;
        }

        public void setSourceAfter(String after)
        {
            this.after = after;
        }

        public string getSourceAfter()
        {
            return after;
        }

        public bool hasRefactoring()
        {
            SyntaxTree treeBefore = SyntaxTree.ParseCompilationUnit(before);
            SyntaxTree treeAfter = SyntaxTree.ParseCompilationUnit(after);

            // Get the classes in the code before and after.
            var classesBefore = treeBefore.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            var classesAfter = treeAfter.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            
            // Get the pairs of class declaration in the code before and after
            var paris = getClassDeclarationPairs(classesBefore, classesAfter);
            foreach (var pair in paris)
            {
                var detector = new InClassExtractMethodDetector(pair.Key, pair.Value);
                detector.setSyntaxTreeBefore(treeBefore);
                detector.setSyntaxTreeAfter(treeAfter);
                if(detector.hasRefactoring())
                {
                    refactorings = refactorings.Union(detector.getRefactorings());
                }
            }
            return refactorings.Any();
        }

        /* Get the definition of same classes before and after. */
        private IList<KeyValuePair<ClassDeclarationSyntax, ClassDeclarationSyntax>> getClassDeclarationPairs
            (IEnumerable<ClassDeclarationSyntax> classesBefore, IEnumerable<ClassDeclarationSyntax> classesAfter)
        {
            var pairs = new List<KeyValuePair<ClassDeclarationSyntax, ClassDeclarationSyntax>>();
            foreach(var b in classesBefore)
            {
                foreach (var a in classesAfter)
                {
                    if(b.Identifier.Value.Equals(a.Identifier.Value))
                    {
                        pairs.Add(new KeyValuePair<ClassDeclarationSyntax, ClassDeclarationSyntax>(b, a));
                        break;
                    }
                }
            }
            return pairs;
        }

        public IEnumerable<IManualRefactoring> getRefactorings()
        {
            return this.refactorings;
        }
    }


    /* Extract method detector for same classes before and after. */
    class InClassExtractMethodDetector : IRefactoringDetector, IBeforeAndAfterSyntaxTreeKeeper
    {
        private readonly ClassDeclarationSyntax classBefore;
        private readonly ClassDeclarationSyntax classAfter;

        /* Entire trees of the files containing the class definitions above. */
        private SyntaxTree treeBefore;
        private SyntaxTree treeAfter;

        /* The detected refactorings. */
        private IEnumerable<IManualRefactoring> refactorings;

        private Logger logger;


        public InClassExtractMethodDetector (ClassDeclarationSyntax classBefore,
            ClassDeclarationSyntax classAfter)
        {
            this.classBefore = classBefore;
            this.classAfter = classAfter;
            this.refactorings = Enumerable.Empty<IManualRefactoring>();
            logger = NLoggerUtil.GetNLogger(typeof (InClassExtractMethodDetector));
        }

        public Boolean hasRefactoring()
        {
            // Build the call graphs of classes before and after.
            CallGraph callGraphBefore = new CallGraphBuilder(classBefore, treeBefore).buildCallGraph();
            CallGraph callGraphAfter = new CallGraphBuilder(classAfter, treeAfter).buildCallGraph();
            
            // Get the methods that are newly created.
            var addedMethods = callGraphAfter.getVerticesNotIn(callGraphBefore);

            // Get the common methods between the classes before and after.
            var commonMethods = callGraphAfter.getCommonVertices(callGraphBefore);

            // Find the suspicious pairs of callers and callees; callers are common; callees are added;  
            foreach (var caller in commonMethods)
            {
                foreach (var callee in addedMethods)
                {
                    if(ASTUtil.isInvoking(caller, callee, treeAfter))
                    {
                        // Get what does the caller look like before. 
                        var callerBefore = callGraphBefore.getVertice((String)caller.Identifier.Value);

                        // Create in method detector;
                        var detector = new InMethodExtractMethodDectector(callerBefore, caller, callee);

                        // Set the trees to the detector.
                        detector.setSyntaxTreeBefore(treeBefore);
                        detector.setSyntaxTreeAfter(treeAfter);

                        // Start to detect.
                        if (detector.hasRefactoring())
                        {
                            refactorings = refactorings.Union(detector.getRefactorings());
                        }
                    }
                }
            }
            return refactorings.Any();
        }

        public IEnumerable<IManualRefactoring> getRefactorings()
        {
            return this.refactorings;
        }

        public void setSyntaxTreeBefore(SyntaxTree before)
        {
            this.treeBefore = before;
        }

        public void setSyntaxTreeAfter(SyntaxTree after)
        {
            this.treeAfter = after;
        }
    }
    /* Extract method detector for a given caller and an added callee. */
    class InMethodExtractMethodDectector : IRefactoringDetector, IBeforeAndAfterSyntaxTreeKeeper
    {
        /* The caller declaration before. */
        private readonly MethodDeclarationSyntax callerBefore;

        /* The caller declaration after. */
        private readonly MethodDeclarationSyntax callerAfter;
        
        /* The callee that is called after but never called before. */
        private readonly MethodDeclarationSyntax calleeAfter;
        
        /* The entire trees before and after. */
        private SyntaxTree treeAfter;
        private SyntaxTree treeBefore;

        /* The detected refactoring if any. */
        private IManualRefactoring refactoring;
       
        private readonly Logger logger;

        public InMethodExtractMethodDectector(MethodDeclarationSyntax callerBefore, MethodDeclarationSyntax callerAfter, 
            MethodDeclarationSyntax calleeAfter)
        {
            this.callerBefore = callerBefore;
            this.callerAfter = callerAfter;
            this.calleeAfter = calleeAfter;
            logger = NLoggerUtil.GetNLogger(typeof (InMethodExtractMethodDectector));
        }

        public bool hasRefactoring()
        {
            // Get the first invocation of callee in the caller method body.
            var invocation = ASTUtil.getAllInvocationsInMethod(callerAfter, calleeAfter, treeAfter)[0];
            // Precondition  
            Contract.Requires(invocation!= null);
            
            /* Flatten the caller after by replacing callee invocation with the code in the calle method body. */
            String callerAfterFlattenned = ASTUtil.flattenMethodInvocation(callerAfter, calleeAfter, invocation);
            
            // logger.Info("Caller Before:\n" + callerBefore.GetText());
            // logger.Info("Caller After:\n" + callerAfter.GetText());
            // logger.Info("Callee after:\n" + calleeAfter.GetText());
            // logger.Info("Flattened Caller:\n" + callerAfterFlattenned);

            var beforeWithoutSpace = callerBefore.GetFullText().Replace(" ", "");

            // The distance between flattened caller after and the caller before.
            int dis1 = StringUtil.GetStringDistance(callerAfterFlattenned.Replace(" ", ""), beforeWithoutSpace);
           
            // The distance between caller after and the caller before.
            int dis2 = StringUtil.GetStringDistance(callerAfter.GetFullText().Replace(" ", ""), beforeWithoutSpace);
            logger.Info("Distance Gain by Flattening:" + (dis2 - dis1));
            
            // Check whether the distance is shortened by flatten. 
            if (dis2 > dis1)
            {
                // If similar enough, a manual refactoring instance is detected and created.
                var analyzer = RefactoringAnalyzerFactory.CreateManualExtractMethodAnalyzer();
                analyzer.SetMethodDeclarationBeforeExtracting(callerBefore);
                analyzer.SetExtractedMethodDeclaration(calleeAfter);
                analyzer.SetInvocationExpression(invocation);
                refactoring = analyzer.GetRefactoring();
                return true;
            }
            else
                return false;
        }

        public IEnumerable<IManualRefactoring> getRefactorings()
        {
            yield return refactoring;
        }

        public void setSyntaxTreeBefore(SyntaxTree before)
        {
            this.treeBefore = before;
        }

        public void setSyntaxTreeAfter(SyntaxTree after)
        {
            this.treeAfter = after;
        }
    }


}
