using System;


namespace R5T.Soltana.Default
{
    public class SolutionFolderPathConventions : ISolutionFolderPathConventions
    {
        public const string SolutionFolderPathSeparator = "/";


        public string GetSolutionFolderSeparator()
        {
            return SolutionFolderPathConventions.SolutionFolderPathSeparator;
        }
    }
}
