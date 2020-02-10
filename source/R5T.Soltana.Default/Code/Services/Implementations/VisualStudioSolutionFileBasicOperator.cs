using System;

using System.Collections.Generic;

using R5T.Cambridge.Types;


namespace R5T.Soltana.Default
{
    public class VisualStudioSolutionFileBasicOperator : IVisualStudioSolutionFileBasicOperator
    {
        public void AddSolutionFolder(SolutionFile solutionFile, string solutionFolder)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SolutionFileProjectReference> GetRootSolutionFolders(SolutionFile solutionFile)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SolutionFileProjectReference> GetSolutionFolders(SolutionFile solutionFile)
        {
            throw new NotImplementedException();
        }
    }
}
