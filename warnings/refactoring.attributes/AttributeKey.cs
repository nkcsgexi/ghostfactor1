using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    public enum AttributeKey
    {
        //Rename:
        RENAME_ORIGINAL_NAME,
        RENAME_NEW_NAME,

        //extract method:
        EXTRACT_METHOD_ACCESSABILITY,
        EXTRACT_METHOD_METHOD_NAME,
        EXTRACT_METHOD_MODIFIERS,
        EXTRACT_METHOD_PARAMETERS
    }
}
