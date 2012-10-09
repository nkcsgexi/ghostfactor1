using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BlackHen.Threading;
using Roslyn.Services;
using warnings.components.search;
using warnings.components.ui;

namespace warnings.components
{
    /* All the components in GhostFactor shall be implementing this interface.*/
    public interface IFactorComponent
    {
        void Enqueue(IWorkItem item);
        string GetName();
        int GetWorkQueueLength();
        void Start();
    }

    public class GhostFactorComponents
    {
        /* Component for saving the source code at certain time interval. */
        public static readonly IFactorComponent historyComponent = HistorySavingComponent.getInstance();

        /* Component for traversing the source code history and looking for manual rename refactoring. */
        public static readonly IFactorComponent searchRenameComponent = SearchRenameComponent.getInstance();

        /* Component for traversing the source code history and looking for manual extract method refactorings. */
        public static readonly IFactorComponent searchExtractMethodComponent = SearchExtractMethodComponent.getInstance();

        /* Component searching in the history records for change method signature refactoring that cannot trigger compiler issues. */
        public static readonly IFactorComponent searchChangeMethodSignatureComponent =
            SearchChangeMethodSignatureComponent.GetInstance();
        
        /* Component for checking the conditions of detected manual refactorings. */
        public static readonly IFactorComponent conditionCheckingComponent = ConditionCheckingComponent.GetInstance();

        /* Component for keeping track of all the refactoring issues and posting them to the editor.*/
        public static readonly ICodeIssueComputersRepository RefactoringCodeIssueComputerComponent =
            RefactoringCodeIssueComputersComponent.GetInstance();

        public static IDocumentSearcher searchRealDocumentComponent;

        public static IRefactoringFormComponent refactoringFormComponent = RefactoringFormComponent.GetInstance(); 

        public static void StartAllComponents(ISolution solution)
        {
            // Start the history keeping component.
            historyComponent.Start();
            
            // Start the searching refactoring components.
            searchRenameComponent.Start();
            searchExtractMethodComponent.Start();
            searchChangeMethodSignatureComponent.Start();

            // Start condition checker.
            conditionCheckingComponent.Start();

            // Initiate and start search real document component.
            searchRealDocumentComponent = SearchRealDocumentComponent.GetInstance(solution);

            // Start the refactoring form component, a new window will be displayed.
            refactoringFormComponent.Start();
        }
    }
}
