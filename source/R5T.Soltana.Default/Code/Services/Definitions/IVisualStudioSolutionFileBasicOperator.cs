using System;
using System.Collections.Generic;

using R5T.Cambridge.Types;


namespace R5T.Soltana.Default
{
    public interface IVisualStudioSolutionFileBasicOperator
    {
        void AddSolutionFolder(SolutionFile solutionFile, string solutionFolder);

        IEnumerable<SolutionFileProjectReference> GetSolutionFolders(SolutionFile solutionFile);

        IEnumerable<SolutionFileProjectReference> GetRootSolutionFolders(SolutionFile solutionFile);
    }
}
