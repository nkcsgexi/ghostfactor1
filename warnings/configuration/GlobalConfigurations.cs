using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring;

namespace warnings.configuration
{
    /* Global configurations for GhostFactor.*/
    public class GlobalConfigurations
    {
        /* Whether a given refactoring type is supported by GhostFactor. */
        public static bool IsSupported(RefactoringType type)
        {
            switch (type)
            {
                case RefactoringType.RENAME:
                    return false;
                case RefactoringType.EXTRACT_METHOD:
                    return true;
                case RefactoringType.CHANGE_METHOD_SIGNATURE:
                    return false;
                default:
                    throw new Exception("Unknown Refactoring Type.");
            }
        }
    }
}
