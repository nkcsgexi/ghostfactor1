using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.refactoring.attributes;
using warnings.source;

namespace warnings.refactoring.refactoring.auto
{
    class AutoExtraceMethodRefactoring : IAutoRefactoring, IRefactoring
    {
        /* Original code. */
        private string code;
        /* Refactored code. */
        private string refactoredCode;
        /* Extracted method declaration node. */
        private MethodDeclarationSyntax extracedMethod;
        /* All the refactoring attributes. */
        private IRefactoringAttributes attributes;
        /* Start index of extracted statements. */
        private int start;
        /* Length of extracted statements. */
        private int length;

        public void setCode(String code)
        {
            this.code = code;
        }

        public void setStart(int start)
        {
            this.start = start;
        }

        public void setLength(int length)
        {
            this.length = length;
        }

        public bool checkConditions()
        {
            ICodeRefactoringProvider p;
            IDocument d;
            IWorkspaceDiscoveryService s;
       
            
            
            throw new NotImplementedException();
        }

        public void performRefactoring()
        {
            throw new NotImplementedException();
        }

        public string getCodeAfterRefactoring()
        {
            return refactoredCode;
        }

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }

        public IRefactoringAttributes getAttributes()
        {
            // Lazy way of getting the attributes for this refactoring.
            if(attributes == null)
            {
                IAttributesRetriever retriever = new ExtractMethodAttributesRetriever();
                // In this case, set the new method declaration node.
                retriever.setImportantSyntaxNode(extracedMethod);
                // Perform the computation of all attributes.
                retriever.retrieveAttributes();
                attributes = retriever.getAttributes();
            }
            return attributes;
        }
    }
}
