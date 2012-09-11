﻿using System;
using System.Collections.Generic;
using System.Linq;  
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.retriever;
using warnings.source;
using warnings.util;

namespace warnings.refactoring.detection
{
    /*
     * This is a detector for mamual rename refactoring. Setting the source code before and after a time interval, 
     * this detector should be able to tell whether a rename refactoring was performed.
     */
    internal class RenameDetector : IExternalRefactoringDetector
    {
        /* The code beforeSource. */
        private String beforeSource;

        /* The code afterSource certain time interval. */
        private String afterSource;

        /* Syntax tree root of the tree beforeSource. */
        private SyntaxNode rootAfter;

        /* Syntax tree root of the tree afterSource. */
        private SyntaxNode rootBefore;

        /* The detected refactoring if any. */
        private IManualRenameRefactoring refactoring;

        internal RenameDetector()
        {
        }


        public void setSourceBefore(String source)
        {
            this.beforeSource = source;
            this.rootBefore = ASTUtil.getSyntaxTreeFromSource(beforeSource).GetRoot();
        }

        public string getSourceBefore()
        {
            return beforeSource;
        }

        public void setSourceAfter(String source)
        {
            this.afterSource = source;
            this.rootAfter = ASTUtil.getSyntaxTreeFromSource(afterSource).GetRoot();
            
        }

        public string getSourceAfter()
        {
            return afterSource;
        }

        public bool hasRefactoring()
        {
            // Get the renamable retriever and retrieving all the identifiers in the before and after 
            // trees.
            var retriever = RetrieverFactory.GetRenamableRetriever();
            retriever.SetRoot(rootBefore);
            var beforeTokens = retriever.GetIdentifierTokens();
            retriever.SetRoot(rootAfter);
            var afterTokens = retriever.GetIdentifierTokens();

            // For all the identifiers in before and after treees, rename shall result in one identifier change
            // and leaves anything else untouched.
            if(beforeTokens.Count() == afterTokens.Count())
            {
                // A list containing all the nodes whose names are changed.
                var changedID = new List<SyntaxToken>();
                
                // Save the new name.
                String newName = null;

                for (int i = 0; i < beforeTokens.Count(); i++)
                {
                    var beforeId = beforeTokens.ElementAt(i);
                    var afterId = afterTokens.ElementAt(i);

                    // If the value of the name is changed, add the token in before tree to
                    // the list. 
                    if(!beforeId.ValueText.Equals(afterId.ValueText))
                    {
                        changedID.Add(beforeId);
                        newName = afterId.ValueText;
                    }
                }

                // If only one name is changed, a rename refactoring is detected. 
                if(changedID.Count() == 1)
                {
                    refactoring = ManualRefactoringFactory.CreateManualRenameRefactoring(changedID.First(), newName);
                    return true;
                }
            }
            return false;

        }

        public IEnumerable<IManualRefactoring> getRefactorings()
        {
            // Return the refactoring if detected. 
            yield return refactoring;
        }
    }
}
