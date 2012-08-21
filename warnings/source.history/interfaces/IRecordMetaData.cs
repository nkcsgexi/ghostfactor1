using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.source
{
    public interface IRecordMetaData
    {
        String getPreviousMetaPath();
        String getNameSpace();
        String getSolution();
        String getFile();
        String getSourcePath();
        String getMetaDataPath();
        long getTime();
    }
}
