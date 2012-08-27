using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;
using NLog;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.util;

namespace warnings.components
{
    /* Component for handling automatic extract method refactoring. */
    internal class AutoExtractMethodComponent : AutoRefactoringComponent
    {
        /* Singleton this component. */
        private static IFactorComponent instance = new AutoExtractMethodComponent();

        public static IFactorComponent getInstance()
        {
            return instance;
        }

        public override string GetName()
        {
           return "AutoExtractMethodComponent";
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (AutoExtractMethodComponent));
        }
    }

    /* Workitem for performing automatic extract method. */
    internal class AutoExtractMethodWorkItem : AutoRefactoringWorkItem
    {
        private readonly IExtractMethodService service;

        public AutoExtractMethodWorkItem(IDocument document) : base(document)
        {
            this.service = ServiceArchive.getInstance().ExtractMethodService;
        }

        public override void Perform()
        {
            
        }

        public override Logger GetLogger()
        {
            return NLoggerUtil.getNLogger(typeof (AutoExtractMethodWorkItem));
        }
    }
}
