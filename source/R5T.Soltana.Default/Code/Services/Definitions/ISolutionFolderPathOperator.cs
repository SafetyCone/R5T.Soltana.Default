using System;
using System.Collections.Generic;


namespace R5T.Soltana.Default
{
    public interface ISolutionFolderPathOperator
    {
        IEnumerable<string> GetSolutionFolderPathParts(string solutionFolderPath);

        string GetSolutionFolderPath(IEnumerable<string> solutionFolderPathParts);
    }
}
