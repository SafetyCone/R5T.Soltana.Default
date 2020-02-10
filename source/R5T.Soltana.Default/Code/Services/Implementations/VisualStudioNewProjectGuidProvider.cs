using System;


namespace R5T.Soltana.Default
{
    public class VisualStudioNewProjectGuidProvider : IVisualStudioNewProjectGuidProvider
    {
        public Guid GetNewVisualStudioProjectGuid()
        {
            var newProjectGuid = Guid.NewGuid();
            return newProjectGuid;
        }
    }
}
