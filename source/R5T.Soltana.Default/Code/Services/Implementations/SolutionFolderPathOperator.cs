using System;
using System.Collections.Generic;

using R5T.Magyar.Extensions;


namespace R5T.Soltana.Default
{
    public class SolutionFolderPathOperator : ISolutionFolderPathOperator
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
