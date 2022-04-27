using System;
using System.Collections.Generic;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceDefinitionMarker]
    public interface ISolutionFolderPathOperator:IServiceDefinition
    {
        IEnumerable<string> GetSolutionFolderPathParts(string solutionFolderPath);

        string GetSolutionFolderPath(IEnumerable<string> solutionFolderPathParts);
    }
}
