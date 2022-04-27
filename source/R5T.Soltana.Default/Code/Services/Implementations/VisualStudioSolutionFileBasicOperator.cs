using System;

using System.Collections.Generic;

using R5T.Cambridge.Types;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceImplementationMarker]
    public class VisualStudioSolutionFileBasicOperator : IVisualStudioSolutionFileBasicOperator,IServiceImplementation
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
