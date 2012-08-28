using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.util;

namespace warnings.components
{
    /* Component for handling the automatic rename refactoring. */
    internal class AutoRenameComponent : AutoRefactoringComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new AutoRenameComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        public override string GetName()
        {
            return "AutoRenameComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (AutoRenameComponent));
        }
    }

    /* WorkItem to be added to automatic rename component. */
    internal class AutoRenameWorkItem : AutoRefactoringWorkItem
    {
        private readonly IRenameService service;

        public AutoRenameWorkItem(IDocument document, IRenameService service) : base(document)
        {
            this.service = service;
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (AutoRenameWorkItem));
        }

        public override void Perform()
        {
       

            //renameService.RenameSymbol(w, s, GetFirstLocalVariable(document), "somenewname"); 
        }

    }
}
