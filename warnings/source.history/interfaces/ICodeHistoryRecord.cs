using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.source
{
    public interface ICodeHistoryRecord
    {
        String getSolution();
        String getNameSpace();
        String getFile();
        String getSource();
        SyntaxTree getSyntaxTree();
        long getTime();
        bool hasPreviousRecord();
        ICodeHistoryRecord getPreviousRecord();
        ICodeHistoryRecord createNextRecord(string source);
        void delete();
    }
}
