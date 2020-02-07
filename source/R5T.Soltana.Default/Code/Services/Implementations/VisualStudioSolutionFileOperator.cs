using System;
using System.Collections.Generic;
using System.Linq;

using R5T.Cambridge.Types;
using R5T.Hladir;
using R5T.Koping;
using R5T.Lombardy;
using R5T.Solgene;


namespace R5T.Soltana.Default
{
    public class VisualStudioSolutionFileOperator : IVisualStudioSolutionFileOperator
    {
        private IVisualStudioSolutionFileGenerator VisualStudioSolutionFileGenerator { get; }
        private IVisualStudioSolutionFolderProjectTypeGuidProvider VisualStudioSolutionFolderProjectTypeGuidProvider { get; }
        private IVisualStudioProjectFileNameConventions VisualStudioProjectFileNameConventions { get; }
        private IStringlyTypedPathOperator StringlyTypedPathOperator { get; }


        public VisualStudioSolutionFileOperator(
            IVisualStudioSolutionFileGenerator visualStudioSolutionFileGenerator,
            IVisualStudioSolutionFolderProjectTypeGuidProvider visualStudioSolutionFolderProjectTypeGuidProvider,
            IVisualStudioProjectFileNameConventions visualStudioProjectFileNameConventions,
            IStringlyTypedPathOperator stringlyTypedPathOperator)
        {
            this.VisualStudioSolutionFileGenerator = visualStudioSolutionFileGenerator;
            this.VisualStudioSolutionFolderProjectTypeGuidProvider = visualStudioSolutionFolderProjectTypeGuidProvider;
            this.VisualStudioProjectFileNameConventions = visualStudioProjectFileNameConventions;
            this.StringlyTypedPathOperator = stringlyTypedPathOperator;
        }

        public void AddProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, Guid projectTypeGuid)
        {
            var projectFileRelativePath = this.StringlyTypedPathOperator.GetRelativePathFileToFile(solutionFilePath, projectFilePath);

            // Get project name from project file path.
            var projectFileName = this.StringlyTypedPathOperator.GetFileName(projectFilePath);
            var projectName = this.VisualStudioProjectFileNameConventions.GetProjectName(projectFileName);

            var solutionFileProjectReference = SolutionFileProjectReference.New(projectName, projectFileRelativePath, projectTypeGuid);

            solutionFile.SolutionFileProjectReferences.Add(solutionFileProjectReference);
        }

        public SolutionFile CreateNewSolutionFile()
        {
            var solutionFile = this.VisualStudioSolutionFileGenerator.GenerateVisualStudioSolutionFile();
            return solutionFile;
        }

        public IEnumerable<SolutionFileProjectFileReference> ListProjectFileReferences(SolutionFile solutionFile, string solutionFilePath)
        {
            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            foreach (var projectReference in solutionFile.SolutionFileProjectReferences)
            {
                // Filter out solution folders.
                if (projectReference.ProjectTypeGUID == solutionFolderProjectTypeGuid)
                {
                    continue;
                }

                var projectFilePath = this.StringlyTypedPathOperator.Combine(solutionFilePath, projectReference.ProjectFileRelativePathValue);

                var projectFileReference = new SolutionFileProjectFileReference()
                {
                    ProjectFilePathValue = projectFilePath,
                    ProjectGUID = projectReference.ProjectGUID,
                    ProjectName = projectReference.ProjectName,
                    ProjectTypeGUID = projectReference.ProjectTypeGUID,
                };
                yield return projectFileReference;
            }
        }

        public bool RemoveProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath)
        {
            var hasProjectFile = this.HasProjectFile(solutionFile, solutionFilePath, projectFilePath, out var projectReference);
            if (hasProjectFile)
            {
                solutionFile.SolutionFileProjectReferences.Remove(projectReference);

                return true;
            }

            // Not present (maybe already removed). Do not error, just indicate a lack of success.
            return false;
        }

        private bool HasProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, out SolutionFileProjectReference projectReference)
        {
            var projectFileRelativePath = this.StringlyTypedPathOperator.GetRelativePathFileToFile(solutionFilePath, projectFilePath);

            projectReference = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectFileRelativePathValue == projectFileRelativePath).SingleOrDefault();

            var hasProjectFile = projectReference != default;
            return hasProjectFile;
        }

        public bool HasProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath)
        {
            var hasProjectReference = this.HasProjectFile(solutionFile, solutionFilePath, projectFilePath, out _);
            return hasProjectReference;
        }
    }
}
