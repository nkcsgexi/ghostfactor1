using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services;

namespace warnings.analyzer
{



    internal class DataFlowAnalyzer
    {
        private IDocument document;

        void SetDocument(IDocument document)
        {
            this.document = document;
        }
    }
}
