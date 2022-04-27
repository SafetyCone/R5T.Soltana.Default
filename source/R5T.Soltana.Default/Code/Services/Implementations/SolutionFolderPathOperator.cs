using System;
using System.Collections.Generic;

using R5T.Magyar.Extensions;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceImplementationMarker]
    public class SolutionFolderPathOperator : ISolutionFolderPathOperator,IServiceImplementation
    {
        private ISolutionFolderPathConventions SolutionFolderPathConventions { get; }


        public SolutionFolderPathOperator(ISolutionFolderPathConventions solutionFolderPathConventions)
        {
            this.SolutionFolderPathConventions = solutionFolderPathConventions;
        }

        public string GetSolutionFolderPath(IEnumerable<string> solutionFolderPathParts)
        {
            var solutionFolderPathSeparator = this.SolutionFolderPathConventions.GetSolutionFolderSeparator();

            var solutionFolderPath = String.Join(solutionFolderPathSeparator, solutionFolderPathParts);
            return solutionFolderPath;
        }

        public IEnumerable<string> GetSolutionFolderPathParts(string solutionFolderPath)
        {
            var solutionFolderPathSeparator = this.SolutionFolderPathConventions.GetSolutionFolderSeparator();

            var solutionFolderPathParts = solutionFolderPath.Split(solutionFolderPathSeparator);
            return solutionFolderPathParts;
        }
    }
}
