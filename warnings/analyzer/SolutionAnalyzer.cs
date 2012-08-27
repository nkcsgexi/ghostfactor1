using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services;

namespace warnings.analyzer
{
    /* An analyzer for basic solution struture. */
    public interface ISolutionAnalyzer
    {
        IEnumerable GetProjects();
        IEnumerable GetDocuments(IProject project);
        String DumpSolutionStructure();
    }
    
    
    public class SolutionAnalyzer : ISolutionAnalyzer
    {
        private readonly ISolution solution;

        public SolutionAnalyzer(ISolution solution)
        {
            this.solution = solution;
        }

        public IEnumerable GetProjects()
        {
            return solution.Projects;
        }

        public IEnumerable GetDocuments(IProject project)
        {
            return project.Documents;
        }

        public string DumpSolutionStructure()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append("\n");
            sb.AppendLine("solution");

            foreach (IProject project in GetProjects())
            {
                sb.AppendLine("\t" + project.Name);
                foreach(IDocument document in project.Documents)
                {
                    sb.AppendLine("\t\t" + document.Name);
                }
            }

            return sb.ToString();
        }
    }
}
