using System;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceImplementationMarker]
    public class SolutionFolderPathConventions : ISolutionFolderPathConventions,IServiceImplementation
    {
        public const string SolutionFolderPathSeparator = "/";


        public string GetSolutionFolderSeparator()
        {
            return SolutionFolderPathConventions.SolutionFolderPathSeparator;
        }
    }
}
