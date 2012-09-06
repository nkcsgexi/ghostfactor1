using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows;
using NLog;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using Roslyn.Services.Host;
using warnings.analyzer;
using warnings.components;
using warnings.quickfix;
using warnings.util;

namespace warnings
{
    [ExportSyntaxNodeCodeIssueProvider("CodeIssue", LanguageNames.CSharp)]
    class CodeIssueProvider : ICodeIssueProvider
    {
        private readonly Logger logger = NLoggerUtil.getNLogger(typeof(CodeIssueProvider));

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            initialize(document, false);

            // Add the new record to the history component.
            GhostFactorComponents.historyComponent.Enqueue(new DocumentWorkItem(document));


            ICodeAction action = new RefactoringQuickFix(document);
            ICodeAction[] actions = new ICodeAction[] {action};

            var tokens = from nodeOrToken in node.ChildNodesAndTokens()
                         where nodeOrToken.IsToken
                         select nodeOrToken.AsToken();

            foreach (var token in tokens)
            {
                var tokenText = token.GetText();

                if (tokenText.Contains('a'))
                {
                    var issueDescription = "warnings is running.";

                    yield return new CodeIssue(CodeIssue.Severity.Warning, token.Span, actions);
                }
            }
        }

        private bool initialized = false;

        /* Code runs only once when getIssues is called. */
        private void initialize(IDocument document, bool show)
        {
            if (!initialized)
            {
                // Retrieve the refactoring services to initialize ServiceArchive.
                retrieveService(document, show);

                // Start all the components.
                GhostFactorComponents.StartAllComponents();
                initialized = true;
            }
        }



        /* Initialize the service archive. */
        private void retrieveService(IDocument document, bool show)
        {
            ServiceArchive instance = ServiceArchive.getInstance();
            instance.ExtractMethodService = document.LanguageServices.GetService<IExtractMethodService>();
            if (show)
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
