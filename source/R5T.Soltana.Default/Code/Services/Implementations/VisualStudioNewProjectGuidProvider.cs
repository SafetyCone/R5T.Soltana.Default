using System;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceImplementationMarker]
    public class VisualStudioNewProjectGuidProvider : IVisualStudioNewProjectGuidProvider,IServiceImplementation
    {
        public Guid GetNewVisualStudioProjectGuid()
        {
            var newProjectGuid = Guid.NewGuid();
            return newProjectGuid;
        }
    }
}
