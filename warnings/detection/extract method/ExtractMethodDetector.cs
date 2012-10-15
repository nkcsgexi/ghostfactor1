using System;
using System.Collections;
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
     * This is a detector for extract method refactoring. After setting the code before and after some time interval, 
     * the detector should be able to tell whether there is a reafactoring performed. 
     */
    internal class ExtractMethodDetector : IExternalRefactoringDetector
    {
        /* Source code before. */
        private String before;

        /* Source code later. */
        private String after;
        
        /* Detected manual refactorings.*/
        private IEnumerable<IManualRefactoring> refactorings;

        public void SetSourceBefore(String before)
        {
            this.before = before;
        }

        public string GetSourceBefore()
        {
            return before;
        }

        public void SetSourceAfter(String after)
        {
            this.after = after;
        }

        public string GetSourceAfter()
        {
            return after;
        }

        public bool HasRefactoring()
        {
            refactorings = Enumerable.Empty<IManualRefactoring>();

            SyntaxTree treeBefore = SyntaxTree.ParseCompilationUnit(before);
            SyntaxTree treeAfter = SyntaxTree.ParseCompilationUnit(after);

            // Get the classes in the code before and after.
            var classesBefore = treeBefore.GetRoot().DescendantNodes().Where(n => n.Kind == SyntaxKind.ClassDeclaration);
            var classesAfter = treeAfter.GetRoot().DescendantNodes().Where(n => n.Kind == SyntaxKind.ClassDeclaration);
            
            // Get the pairs of class declaration in the code before and after
            var paris = GetClassDeclarationPairs(classesBefore, classesAfter);
            foreach (var pair in paris)
            {
                var detector = new InClassExtractMethodDetector((ClassDeclarationSyntax)pair.Key, (ClassDeclarationSyntax)pair.Value);
                detector.SetSyntaxTreeBefore(treeBefore);
                detector.SetSyntaxTreeAfter(treeAfter);
                if(detector.HasRefactoring())
                {
                    refactorings = refactorings.Union(detector.GetRefactorings());
                    return true;
                }
            }
            return false;
        }

        /* Get the definition of same classes before and after. */
        private IEnumerable<KeyValuePair<SyntaxNode, SyntaxNode>> GetClassDeclarationPairs
            (IEnumerable<SyntaxNode> classesBefore, IEnumerable<SyntaxNode> classesAfter)
        {
            var pairs = new List<KeyValuePair<SyntaxNode, SyntaxNode>>();
            foreach (ClassDeclarationSyntax b in classesBefore)
            {
                foreach (ClassDeclarationSyntax a in classesAfter)
                {
                    if(b.Identifier.Value.Equals(a.Identifier.Value))
                    {
                        pairs.Add(new KeyValuePair<SyntaxNode, SyntaxNode>(b, a));
                        break;
                    }
                }
            }
            return pairs;
        }

        public IEnumerable<IManualRefactoring> GetRefactorings()
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
            logger = NLoggerUtil.GetNLogger(typeof (InClassExtractMethodDetector));
        }

        public Boolean HasRefactoring()
        {
            refactorings = Enumerable.Empty<IManualRefactoring>();

            // Build the call graphs of classes before and after.
            CallGraph callGraphBefore = new CallGraphBuilder(classBefore, treeBefore).BuildCallGraph();
            CallGraph callGraphAfter = new CallGraphBuilder(classAfter, treeAfter).BuildCallGraph();
            
            // Get the methods that are newly created.
            var addedMethods = callGraphAfter.getVerticesNotIn(callGraphBefore);

            // Get the common methods between the classes before and after.
            var commonMethods = callGraphAfter.getCommonVertices(callGraphBefore);

            // Find the suspicious pairs of callers and callees; callers are common; callees are added;  
            foreach (var caller in commonMethods)
            {
                foreach (var callee in addedMethods)
                {
                    if(ASTUtil.IsInvoking(caller, callee, treeAfter))
                    {
                        // Get what does the caller look like before. 
                        var callerBefore = callGraphBefore.getVertice((String)caller.Identifier.Value);

                        // Create in method detector;
                        var detector = new InMethodExtractMethodDectector(callerBefore, caller, callee);

                        // Set the trees to the detector.
                        detector.SetSyntaxTreeBefore(treeBefore);
                        detector.SetSyntaxTreeAfter(treeAfter);

                        // Start to detect.
                        if (detector.HasRefactoring())
                        {
                            refactorings = refactorings.Union(detector.GetRefactorings());
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public IEnumerable<IManualRefactoring> GetRefactorings()
        {
            return this.refactorings;
        }

        public void SetSyntaxTreeBefore(SyntaxTree before)
        {
            this.treeBefore = before;
        }

        public void SetSyntaxTreeAfter(SyntaxTree after)
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

        public bool HasRefactoring()
        {
            // Get the first invocation of callee in the caller method body.
            var invocation = ASTUtil.GetAllInvocationsInMethod(callerAfter, calleeAfter, treeAfter).First();

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
                // If similar enough, a manual refactoring instance is likely to be detected and created.
                var analyzer = RefactoringAnalyzerFactory.CreateManualExtractMethodAnalyzer();
                analyzer.SetMethodDeclarationBeforeExtracting(callerBefore);
                analyzer.SetExtractedMethodDeclaration(calleeAfter);
                analyzer.SetInvocationExpression(invocation);

                // If the analyzer can get a refactoring from the given information, get the refactoring 
                // and return true.
                if(analyzer.CanGetRefactoring())
                {
                    refactoring = analyzer.GetRefactoring();
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<IManualRefactoring> GetRefactorings()
        {
            yield return refactoring;
        }

        public void SetSyntaxTreeBefore(SyntaxTree before)
        {
            this.treeBefore = before;
        }

        public void SetSyntaxTreeAfter(SyntaxTree after)
        {
            this.treeAfter = after;
        }
    }
}
