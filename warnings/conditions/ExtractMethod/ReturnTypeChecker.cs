using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services;
using warnings.refactoring;

namespace warnings.conditions
{
    class ReturnTypeChecker : ExtractMethodConditionChecker
    {
        public override ICheckingResult CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            throw new NotImplementedException();
        }
    }

    class ReturnTypeCheckingResult : ExtractMethodConditionCheckingResult
    {
        public override bool HasProblem()
        {
            throw new NotImplementedException();
        }

        public override string GetProblemDescription()
        {
            throw new NotImplementedException();
        }
    }
}
