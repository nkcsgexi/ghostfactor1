using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using NLog;
using NLog.Config;
using NLog.Targets;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.components;
using warnings.util;

namespace RefactoringIssues
{
    [ExportSyntaxNodeCodeIssueProvider("CodeIssue", LanguageNames.CSharp)]
    class CodeIssueProvider : ICodeIssueProvider
    {
        [Import]
        private IRenameService renameService { set; get; }

        private readonly Logger logger;

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            // runOnce(document, true);
    
            // Add the new record to the history component.
            // GhostFactorComponents.historyComponent.Enqueue(new DocumentWorkItem(document));
            
           var tokens = from nodeOrToken in node.ChildNodesAndTokens()
                         where nodeOrToken.IsToken
                         select nodeOrToken.AsToken();

            foreach (var token in tokens)
            {
                var tokenText = token.GetText();

                if (tokenText.Contains('a'))
                {
                    var issueDescription = string.Format("'{0}' contains the letter 'a'", tokenText);
                    yield return new CodeIssue(CodeIssue.Severity.Warning, token.Span, issueDescription);
                }
            }
        }

        private bool hasRan = false;

        /* Code runs only once when getIssues is called. */
        private void runOnce(IDocument document, bool show)
        {
            if(!hasRan)
            {
                // Retrieve the refactoring services to initialize ServiceArchive.
                retrieveService(document, show);

                // Start all the components.
                GhostFactorComponents.StartAllComponents();

                hasRan = true;
            }
        }



        /* Initialize the service archive. */
        private void retrieveService(IDocument document, bool show)
        {
            ServiceArchive instance = ServiceArchive.getInstance();
            instance.RenameService = renameService;
            instance.ExtractMethodService = document.LanguageServices.GetService<IExtractMethodService>();
            if(show)
                MessageBox.Show(instance.ToString());
        }

        #region Unimplemented ICodeIssueProvider members

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxTrivia trivia, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
