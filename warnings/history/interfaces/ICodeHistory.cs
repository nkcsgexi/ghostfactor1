using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.source
{
    public interface ICodeHistory
    {
        void addRecord(String solution, String nameSpace, String file, String source);
        bool hasRecord(String solution, String nameSpace, String file);
        ICodeHistoryRecord getLatestRecord(string solution, string nameSpace, string file);
    }
}
