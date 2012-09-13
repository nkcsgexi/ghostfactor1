﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using warnings.conditions;
using warnings.conditions.CheckResults;
using warnings.quickfix;
using warnings.refactoring;
using warnings.util;

namespace warnings.components
{
    /* The component to handle condition checkings for all the refactoring types. */
    internal class ConditionCheckingComponent : IFactorComponent
    {
        private static IFactorComponent instance = new ConditionCheckingComponent();
       
        public static IFactorComponent GetInstance()
        {
            return instance;
        }

   
        private readonly WorkQueue queue;
        private readonly Logger logger;

        private ConditionCheckingComponent()
        {
            // A single thread workqueue.
            queue = new WorkQueue();
            queue.ConcurrentLimit = 1;
            
            // Set listener for failed work item.
            queue.FailedWorkItem += onFailedItem;

            // Initiate the logger.
            logger = NLoggerUtil.getNLogger(typeof (ConditionCheckingComponent));
        }

        private void onFailedItem(object sender, WorkItemEventArgs workItemEventArgs)
        {
            logger.Fatal("Work item failed.");
        }

        public void Enqueue(IWorkItem item)
        {
            if(queue.Count == 0)
            {
                logger.Info("enqueue");
                queue.Add(item);
            }
        }

        public string GetName()
        {
            return "Conditions Checking Component";
        }

        public int GetWorkQueueLength()
        {
            return queue.Count;
        }

        public void Start()
        {
        }
    }

    /* The work item to be pushed to the condition checking component. */
    class ConditionCheckWorkItem : WorkItem
    {
        // The refactoring instance from detector. 
        private readonly IManualRefactoring refactoring;
        
        // A document instance whose code is identical the detector's before code.
        private readonly IDocument after;
        
        // A document instance whose code is identical to the detector's after code.
        private readonly IDocument before;
        
        private readonly Logger logger;

        public ConditionCheckWorkItem (IDocument before, IDocument after, IManualRefactoring refactoring)
        {
            this.before = before;
            this.after = after;
            this.refactoring = refactoring;
            logger = NLoggerUtil.getNLogger(typeof (ConditionCheckWorkItem));
           
        }

        public override void Perform()
        {
            try
            {
                // The refactoring instance was generated from pure strings, need to map them to real docuement to facilitate semantic
                // analyze.
                refactoring.MapToDocuments(before, after);
                IEnumerable<ICheckingResult> results = Enumerable.Empty<ICheckingResult>();
                switch (refactoring.type)
                {
                    // Checking all conditions for extract method.
                    case RefactoringType.EXTRACT_METHOD:
                        logger.Info("Checking conditions for extract method.");
                        results = ConditionCheckingFactory.GetExtractMethodConditionsList().CheckAllConditions(before, after, refactoring);
                        
                        // Add founded issues to the issue tracking component.
                        AddIssuesToRefactoringIssueComponent(refactoring, results);
                        break;

                    // Checking all conditions for rename.
                    case RefactoringType.RENAME:
                        logger.Info("Checking conditions for rename.");
                        results = ConditionCheckingFactory.GetRenameConditionsList().CheckAllConditions(before, after, refactoring);
                        
                        // Add founded issues to the issue tracking component.
                        AddIssuesToRefactoringIssueComponent(refactoring, results);
                        break;

                    default:
                        logger.Fatal("Unknown refactoring type for conditions checking.");
                        break;
                }
                 
            }catch(Exception e)
            {
                // All exception shall go to the fatal log.
                logger.Fatal(e);
            }
        }

        /* When some condition checkings are failed, add an issue to the issue tracking component. */
        private void AddIssuesToRefactoringIssueComponent(IManualRefactoring refactoring, IEnumerable<ICheckingResult> results)
        {
            // Combine all the checking results into one.
            ICheckingResult combined = new CombinedCheckingResult(results);
            logger.Info("Combined Result: hasProblem = " + combined.HasProblem() + "; description = " +combined.GetProblemDescription());

            // If having problems, added to the issue componet
            if (combined.HasProblem())
            {
                // Create an issued node.
                var issuedNode = new IssueTracedNode(refactoring.GetIssuedNode(null), combined);

                // Add an add issue item to the component.
                GhostFactorComponents.refactoringIssuedNodeComponent.Enqueue(new AddIssueTracedNodeWorkItem(issuedNode, 
                    GhostFactorComponents.refactoringIssuedNodeComponent));
            }
        }
    }
}


