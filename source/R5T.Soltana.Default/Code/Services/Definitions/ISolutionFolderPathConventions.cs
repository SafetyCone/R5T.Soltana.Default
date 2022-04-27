using System;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceDefinitionMarker]
    public interface ISolutionFolderPathConventions:IServiceDefinition
    {
        string GetSolutionFolderSeparator();
    }
}
