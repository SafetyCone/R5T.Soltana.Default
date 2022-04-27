using System;
using System.Collections.Generic;

using R5T.Cambridge.Types;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceDefinitionMarker]
    public interface IVisualStudioSolutionFileBasicOperator:IServiceDefinition
    {
        void AddSolutionFolder(SolutionFile solutionFile, string solutionFolder);

        IEnumerable<SolutionFileProjectReference> GetSolutionFolders(SolutionFile solutionFile);

        IEnumerable<SolutionFileProjectReference> GetRootSolutionFolders(SolutionFile solutionFile);
    }
}
