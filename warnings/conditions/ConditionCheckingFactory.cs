﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.conditions
{
    public class ConditionCheckingFactory
    {
        public static IRefactoringConditionsList GetExtractMethodConditionsList()
        {
            return ExtractMethodConditionsList.GetInstance();
        }

        public static IRefactoringConditionsList GetRenameConditionsList()
        {
            return RenameConditionsList.GetInstance();
        }
    }
}
