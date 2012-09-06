using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring;

namespace warnings.conditions.CheckResults
{
    public class CombinedCheckingResult : ICheckingResult
    {
        private readonly RefactoringType internal_type;

        private readonly bool hasProblem;

        private readonly string description;

        public CombinedCheckingResult(IEnumerable<ICheckingResult> results)
        {
            hasProblem = false;
            var sb = new StringBuilder();

            // Get the type from one of the sub results.
            internal_type = results.First().type;

            // Iterate all the result
            foreach (var result in results)
            {
                // One of check has problem
                if(result.HasProblem())
                {
                    // The combined check has problem.
                    hasProblem = true;

                    // Combine the descriptions together.
                    sb.AppendLine(result.GetProblemDescription());
                }
            }
            description = sb.ToString();
        }

        public RefactoringType type
        {
            get { return internal_type; }
        }

        public bool HasProblem()
        {
            return hasProblem;
        }

        public string GetProblemDescription()
        {
            return description;
        }
    }
}
