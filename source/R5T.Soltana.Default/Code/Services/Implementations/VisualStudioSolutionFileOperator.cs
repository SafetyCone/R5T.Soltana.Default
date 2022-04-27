using System;
using System.Collections.Generic;
using System.Linq;

using R5T.Cambridge.Extensions;
using R5T.Cambridge.Types;
using R5T.Hladir;
using R5T.Koping;
using R5T.Lombardy;
using R5T.Magyar.Extensions;
using R5T.Solgene;using R5T.T0064;


namespace R5T.Soltana.Default
{[ServiceImplementationMarker]
    public class VisualStudioSolutionFileOperator : IVisualStudioSolutionFileOperator,IServiceImplementation
    {
        #region Static

        private static IEnumerable<Guid> GetChildSolutionFolderGUIDs(NestedProjectsGlobalSection nestedProjectsGlobalSection, Guid parentSolutionFolderProjectGuid)
        {
            var childProjectGUIDs = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == parentSolutionFolderProjectGuid).Select(x => x.ChildProjectGUID);
            return childProjectGUIDs;
        }

        private static void VerifyNonEmptySolutionFolderPath(IEnumerable<string> solutionFolderPathParts)
        {
            var solutionFolderPathPartCount = solutionFolderPathParts.Count();
            if (solutionFolderPathPartCount < 1)
            {
                throw new Exception("Empty solution folder path provider.");
            }
        }

        private static bool HasSolutionFolderProject(SolutionFile solutionFile, string solutionFolderName, IEnumerable<Guid> solutionFolderGUIDs, out SolutionFileProjectReference solutionFolderProject)
        {
            solutionFolderProject = solutionFile.SolutionFileProjectReferences.Where(x => solutionFolderGUIDs.Contains(x.ProjectGUID) && x.ProjectName == solutionFolderName).SingleOrDefault();

            var solutionFolderExists = solutionFolderProject != default;
            return solutionFolderExists;
        }

        #endregion


        private IVisualStudioSolutionFileGenerator VisualStudioSolutionFileGenerator { get; }
        private IVisualStudioSolutionFolderProjectTypeGuidProvider VisualStudioSolutionFolderProjectTypeGuidProvider { get; }
        private IVisualStudioNewProjectGuidProvider VisualStudioNewProjectGuidProvider { get; }
        private IVisualStudioProjectFileNameConventions VisualStudioProjectFileNameConventions { get; }
        private ISolutionFolderPathOperator SolutionFolderPathOperator { get; }
        private IStringlyTypedPathOperator StringlyTypedPathOperator { get; }


        public VisualStudioSolutionFileOperator(
            IVisualStudioSolutionFileGenerator visualStudioSolutionFileGenerator,
            IVisualStudioSolutionFolderProjectTypeGuidProvider visualStudioSolutionFolderProjectTypeGuidProvider,
            IVisualStudioNewProjectGuidProvider visualStudioNewProjectGuidProvider,
            IVisualStudioProjectFileNameConventions visualStudioProjectFileNameConventions,
            ISolutionFolderPathOperator solutionFolderPathOperator,
            IStringlyTypedPathOperator stringlyTypedPathOperator)
        {
            this.VisualStudioSolutionFileGenerator = visualStudioSolutionFileGenerator;
            this.VisualStudioSolutionFolderProjectTypeGuidProvider = visualStudioSolutionFolderProjectTypeGuidProvider;
            this.VisualStudioNewProjectGuidProvider = visualStudioNewProjectGuidProvider;
            this.VisualStudioProjectFileNameConventions = visualStudioProjectFileNameConventions;
            this.SolutionFolderPathOperator = solutionFolderPathOperator;
            this.StringlyTypedPathOperator = stringlyTypedPathOperator;
        }

        public void AddProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, Guid projectTypeGuid, Guid projectGuid)
        {
            // Special non-relative formatting.
            var projectFileRelativePath = this.StringlyTypedPathOperator.GetSolutionFileProjectFileRelativePath(solutionFilePath, projectFilePath);

            // Get project name from project file path.
            var projectFileName = this.StringlyTypedPathOperator.GetFileName(projectFilePath);
            var projectName = this.VisualStudioProjectFileNameConventions.GetProjectName(projectFileName);

            var solutionFileProjectReference = SolutionFileProjectReference.New(projectName, projectFileRelativePath, projectTypeGuid, projectGuid);

            this.AddProjectReference(solutionFile, solutionFileProjectReference);
        }

        private void AddProjectReference(SolutionFile solutionFile, SolutionFileProjectReference solutionFileProjectReference)
        {
            solutionFile.SolutionFileProjectReferences.Add(solutionFileProjectReference);

            // Acquire SolutionConfigurationPlatforms global section, adding all default SolutionBuildConfigurationPlatforms if need be.
            var solutionConfigurationPlatforms = solutionFile.GlobalSections.AcquireSolutionConfigurationPlatformsGlobalSection(SolutionConfigurationPlatformsGlobalSection.NewAddDefaultSolutionBuildConfigurationPlatforms);

            var projectConfigurationPlatforms = solutionFile.GlobalSections.AcquireProjectConfigurationPlatformsGlobalSection();

            projectConfigurationPlatforms.AddProjectConfigurations(solutionFileProjectReference.ProjectGUID, solutionConfigurationPlatforms);
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

                var projectFilePath = this.StringlyTypedPathOperator.GetProjectFilePath(solutionFilePath, projectReference.ProjectFileRelativePathValue);

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

        private bool RemoveProjectReference(SolutionFile solutionFile, SolutionFileProjectReference projectReference)
        {
            // Is the project reference in a nested solution folder?
            var hasNestedProjectsGlobalSection = solutionFile.GlobalSections.HasNestedProjectsGlobalSection(out var nestedProjectsGlobalSection);
            if (hasNestedProjectsGlobalSection)
            {
                nestedProjectsGlobalSection.ProjectNestings.RemoveAll(x => x.ChildProjectGUID == projectReference.ProjectGUID);
            }

            // Remove the project configuration platform entries.
            var hasProjectConfigurationPlatformsGlobalSection = solutionFile.GlobalSections.HasProjectConfigurationPlatformsGlobalSection(out var projectConfigurationPlatformsGlobalSection);
            if (hasProjectConfigurationPlatformsGlobalSection)
            {
                projectConfigurationPlatformsGlobalSection.ProjectBuildConfigurationMappings.RemoveAll(x => x.ProjectGUID == projectReference.ProjectGUID);
            }

            solutionFile.SolutionFileProjectReferences.Remove(projectReference);
            return true;
        }

        public bool RemoveProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath)
        {
            var hasProjectFile = this.HasProjectReference(solutionFile, solutionFilePath, projectFilePath, out var projectReference);
            if (!hasProjectFile)
            {
                // Not present (maybe already removed). Do not error, just indicate a lack of success.
                return false;
            }

            var output = this.RemoveProjectReference(solutionFile, projectReference);
            return output;
        }

        private bool HasProjectReference(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, out SolutionFileProjectReference projectReference)
        {
            var projectFileRelativePath = this.StringlyTypedPathOperator.GetSolutionFileProjectFileRelativePath(solutionFilePath, projectFilePath);

            projectReference = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectFileRelativePathValue == projectFileRelativePath).SingleOrDefault();

            var hasProjectFile = projectReference != default;
            return hasProjectFile;
        }

        public bool HasProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath)
        {
            var hasProjectReference = this.HasProjectReference(solutionFile, solutionFilePath, projectFilePath, out _);
            return hasProjectReference;
        }

        public bool HasProjectFile(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, out SolutionFileProjectFileReference projectFileReference)
        {
            var hasProjectFile = this.HasProjectReference(solutionFile, solutionFilePath, projectFilePath, out var projectReference);

            var computedProjectFilePath = this.StringlyTypedPathOperator.GetProjectFilePath(solutionFilePath, projectReference.ProjectFileRelativePathValue);

            if(computedProjectFilePath != projectFilePath)
            {
                throw new Exception($"Mismatch in project file paths:\nComputed: {computedProjectFilePath}\nInput: {projectFilePath}");
            }

            projectFileReference = new SolutionFileProjectFileReference()
            {
                ProjectFilePathValue = computedProjectFilePath,
                ProjectName = projectReference.ProjectName,
                ProjectGUID = projectReference.ProjectGUID,
                ProjectTypeGUID = projectReference.ProjectTypeGUID,
            };

            return hasProjectFile;
        }

        private SolutionFileProjectReference CreateNewSolutionFolder(string solutionFolderName)
        {
            var solutionFolderProject = new SolutionFileProjectReference()
            {
                ProjectTypeGUID = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid(),
                ProjectGUID = this.VisualStudioNewProjectGuidProvider.GetNewVisualStudioProjectGuid(),
                ProjectName = solutionFolderName,
                ProjectFileRelativePathValue = solutionFolderName,
            };
            return solutionFolderProject;
        }

        private IEnumerable<SolutionFileProjectReference> GetRootSolutionFolderProjects(SolutionFile solutionFile)
        {
            var rootSolutionFolderGUIDs = this.GetRootSolutionFolderGUIDs(solutionFile);

            var rootSolutionFolderProjects = solutionFile.SolutionFileProjectReferences.Where(x => rootSolutionFolderGUIDs.Contains(x.ProjectGUID));
            return rootSolutionFolderProjects;
        }

        private bool HasRootSolutionFolderProject(SolutionFile solutionFile, string solutionFolderName, out SolutionFileProjectReference rootSolutionFolder)
        {
            var rootSolutionFolderProjects = this.GetRootSolutionFolderProjects(solutionFile);

            rootSolutionFolder = rootSolutionFolderProjects.Where(x => x.ProjectName == solutionFolderName).SingleOrDefault();

            var rootSolutionFolderExists = rootSolutionFolder != default;
            return rootSolutionFolderExists;
        }

        private SolutionFileProjectReference AcquireAndAddRootSolutionFolderProject(SolutionFile solutionFile, string solutionFolderName)
        {
            var rootSolutionFolderExists = this.HasRootSolutionFolderProject(solutionFile, solutionFolderName, out var rootSolutionFolder);
            if (!rootSolutionFolderExists)
            {
                // Create it and add it to the solution file.
                rootSolutionFolder = this.CreateNewSolutionFolder(solutionFolderName);

                solutionFile.SolutionFileProjectReferences.Add(rootSolutionFolder);
            }

            return rootSolutionFolder;
        }

        private SolutionFileProjectReference AcquireAndAddChildSolutionFolderProject(SolutionFile solutionFile, string solutionFolderName, SolutionFileProjectReference parentSolutionFolderProject)
        {
            var solutionFolderExists = this.HasChildSolutionFolderProject(solutionFile, solutionFolderName, parentSolutionFolderProject, out var solutionFolderProject);
            if (!solutionFolderExists)
            {
                // Solution folder does NOT exist. Create it.
                solutionFolderProject = this.CreateNewSolutionFolder(solutionFolderName);

                // Add the project reference.
                solutionFile.SolutionFileProjectReferences.Add(solutionFolderProject);

                // But also add a nested project.
                var solutionFolderNesting = new ProjectNesting()
                {
                    ChildProjectGUID = solutionFolderProject.ProjectGUID,
                    ParentProjectGUID = parentSolutionFolderProject.ProjectGUID,
                };

                var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

                nestedProjectsGlobalSection.ProjectNestings.Add(solutionFolderNesting);
            }

            return solutionFolderProject;
        }

        private bool HasChildSolutionFolderProject(SolutionFile solutionFile, string solutionFolderName, SolutionFileProjectReference parentSolutionFolderProject, out SolutionFileProjectReference solutionFolderProject)
        {
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var childSolutionFolderGUIDs = VisualStudioSolutionFileOperator.GetChildSolutionFolderGUIDs(nestedProjectsGlobalSection, parentSolutionFolderProject.ProjectGUID);

            solutionFolderProject = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectName == solutionFolderName && childSolutionFolderGUIDs.Contains(x.ProjectGUID)).SingleOrDefault();

            var hasChildSolutionFolderProject = solutionFolderProject != default;
            return hasChildSolutionFolderProject;
        }

        private SolutionFileProjectReference AcquireAndAddSolutionFolderProject(SolutionFile solutionFile, string solutionFolderPath)
        {
            var solutionFolderPathParts = this.SolutionFolderPathOperator.GetSolutionFolderPathParts(solutionFolderPath);

            VisualStudioSolutionFileOperator.VerifyNonEmptySolutionFolderPath(solutionFolderPathParts);

            var rootSolutionFolderName = solutionFolderPathParts.First();

            var rootSolutionFolderProject = this.AcquireAndAddRootSolutionFolderProject(solutionFile, rootSolutionFolderName);

            var solutionFolderPathPartCount = solutionFolderPathParts.Count();
            if(solutionFolderPathPartCount < 2)
            {
                // There is only a root.
                return rootSolutionFolderProject;
            }

            // Recurse down the solution folders.
            var currentSolutionFolderProject = rootSolutionFolderProject;
            foreach (var solutionFolderName in solutionFolderPathParts.Skip(1)) // Skip the root.
            {
                currentSolutionFolderProject = this.AcquireAndAddChildSolutionFolderProject(solutionFile, solutionFolderName, currentSolutionFolderProject);
            }

            return currentSolutionFolderProject;
        }

        public void AddSolutionFolder(SolutionFile solutionFile, string solutionFolderPath)
        {
            this.AcquireAndAddSolutionFolderProject(solutionFile, solutionFolderPath);
        }

        public void AccumulateSolutionFolderContents(SolutionFile solutionFile, Guid solutionFolderGuid, List<SolutionFileProjectReference> solutionFolders, List<SolutionFileProjectReference> projectReferences)
        {
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var childProjectGUIDs = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolderGuid).Select(x => x.ChildProjectGUID);
            var allChildReferences = solutionFile.SolutionFileProjectReferences.Where(x => childProjectGUIDs.Contains(x.ProjectGUID));

            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            var childSolutionFolders = allChildReferences.Where(x => x.ProjectTypeGUID == solutionFolderProjectTypeGuid);
            var childProjectReferences = allChildReferences.Where(x => x.ProjectTypeGUID != solutionFolderProjectTypeGuid);

            solutionFolders.AddRange(childSolutionFolders);
            projectReferences.AddRange(childProjectReferences);

            foreach (var childSolutionFolder in childSolutionFolders)
            {
                this.AccumulateSolutionFolderContents(solutionFile, childSolutionFolder.ProjectGUID, solutionFolders, projectReferences);
            }
        }

        public bool RemoveSolutionFolderAndContents(SolutionFile solutionFile, string solutionFolderPath)
        {
            var hasSolutionFolder = this.HasSolutionFolder(solutionFile, solutionFolderPath, out var solutionFolder);
            if(hasSolutionFolder)
            {
                var solutionFoldersToRemove = new List<SolutionFileProjectReference>
                {
                    solutionFolder
                };

                var projectReferencesToRemove = new List<SolutionFileProjectReference>();

                this.AccumulateSolutionFolderContents(solutionFile, solutionFolder.ProjectGUID, solutionFoldersToRemove, projectReferencesToRemove);

                var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();
                foreach (var solutionFolderToRemove in solutionFoldersToRemove)
                {
                    // Remove project nestings where the solution folder is a parent
                    var projectNestingsToRemove = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolderToRemove.ProjectGUID).ToArray();
                    nestedProjectsGlobalSection.ProjectNestings.RemoveAll(projectNestingsToRemove);

                    // Remove the solution folder project reference.
                    solutionFile.SolutionFileProjectReferences.Remove(solutionFolderToRemove);
                }

                foreach (var projectReferenceToRemove in projectReferencesToRemove)
                {
                    this.RemoveProjectReference(solutionFile, projectReferenceToRemove);
                }

                return true;
            }
            else
            {
                // Charitably, might have already been removed.
                return false;
            }
        }

        public bool HasSolutionFolder(SolutionFile solutionFile, string solutionFolderPath, out SolutionFileProjectReference solutionFolder)
        {
            var solutionFolderPathParts = this.SolutionFolderPathOperator.GetSolutionFolderPathParts(solutionFolderPath);

            VisualStudioSolutionFileOperator.VerifyNonEmptySolutionFolderPath(solutionFolderPathParts);

            var rootSolutionFolderName = solutionFolderPathParts.First();

            var hasRootSolutionFolder = this.HasRootSolutionFolderProject(solutionFile, rootSolutionFolderName, out var rootSolutionFolder);
            if(!hasRootSolutionFolder)
            {
                solutionFolder = default;
                return false;
            }

            var solutionFolderPathPartCount = solutionFolderPathParts.Count();
            if (solutionFolderPathPartCount < 2)
            {
                // There is only a root.
                solutionFolder = rootSolutionFolder;
                return hasRootSolutionFolder;
            }

            // Recurse down the solution folders.
            var currentSolutionFolderProject = rootSolutionFolder;
            foreach (var solutionFolderName in solutionFolderPathParts.Skip(1)) // Skip the root.
            {
                var hasChildSolutionFolder = this.HasChildSolutionFolderProject(solutionFile, solutionFolderName, currentSolutionFolderProject, out currentSolutionFolderProject);
                if(!hasChildSolutionFolder)
                {
                    solutionFolder = default;
                    return false;
                }
            }

            solutionFolder = currentSolutionFolderProject;
            return true;
        }

        private IEnumerable<Guid> GetAllSolutionFolderGUIDs(SolutionFile solutionFile)
        {
            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            var solutionFolderGUIDs = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectTypeGUID == solutionFolderProjectTypeGuid).Select(x => x.ProjectGUID);
            return solutionFolderGUIDs;
        }

        private IEnumerable<Guid> GetRootSolutionFolderGUIDs(SolutionFile solutionFile)
        {
            var allSolutionFolderGUIDs = this.GetAllSolutionFolderGUIDs(solutionFile);

            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var childSolutionFolderGUIDs = nestedProjectsGlobalSection.ProjectNestings.Select(x => x.ChildProjectGUID).Distinct();

            var rootSolutionFolderGUIDs = allSolutionFolderGUIDs.Except(childSolutionFolderGUIDs);
            return rootSolutionFolderGUIDs;
        }

        private IEnumerable<Guid> GetNonRootSolutionFolderGUIDs(SolutionFile solutionFile)
        {
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var nonRootProjectGUIDs = nestedProjectsGlobalSection.ProjectNestings.Select(x => x.ChildProjectGUID).Distinct();
            return nonRootProjectGUIDs;
        }

        public void MoveProjectFileIntoSolutionFolder(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, string solutionFolderPath)
        {
            var hasProjectReference = this.HasProjectReference(solutionFile, solutionFilePath, projectFilePath, out var projectReference);
            if(!hasProjectReference)
            {
                throw new Exception($"Project does not exist: {projectFilePath}");
            }

            var hasSolutionFolder = this.HasSolutionFolder(solutionFile, solutionFolderPath, out var solutionFolder);
            if(!hasSolutionFolder)
            {
                throw new Exception($"Solution folder does not exist: {solutionFolderPath}");
            }

            // Test if project file is already in the solution folder.
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var solutionFolderContainsProjectFile = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolder.ProjectGUID && x.ChildProjectGUID == projectReference.ProjectGUID).Any();
            if(solutionFolderContainsProjectFile)
            {
                throw new Exception($"Solution folder already contains project file.\nSolution Folder:{solutionFolder.ProjectName}\nProject File: {projectFilePath}");
            }

            var projectNesting = new ProjectNesting()
            {
                ParentProjectGUID = solutionFolder.ProjectGUID,
                ChildProjectGUID = projectReference.ProjectGUID,
            };
            nestedProjectsGlobalSection.ProjectNestings.Add(projectNesting);
        }

        public void MoveProjectFileOutOfSolutionFolder(SolutionFile solutionFile, string solutionFilePath, string projectFilePath, string solutionFolderPath)
        {
            var hasProjectReference = this.HasProjectReference(solutionFile, solutionFilePath, projectFilePath, out var projectReference);
            if (!hasProjectReference)
            {
                throw new Exception($"Project does not exist: {projectFilePath}");
            }

            var hasSolutionFolder = this.HasSolutionFolder(solutionFile, solutionFolderPath, out var solutionFolder);
            if (!hasSolutionFolder)
            {
                throw new Exception($"Solution folder does not exist: {solutionFolderPath}");
            }

            // Test if project file is even in the solution folder.
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var projectNesting = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolder.ProjectGUID && x.ChildProjectGUID == projectReference.ProjectGUID).SingleOrDefault();

            var solutionFolderContainsProjectFile = projectNesting == default;
            if (solutionFolderContainsProjectFile)
            {
                throw new Exception($"Solution folder does not contain project file.\nSolution Folder:{solutionFolder.ProjectName}\nProject File: {projectFilePath}");
            }

            nestedProjectsGlobalSection.ProjectNestings.Remove(projectNesting);
        }

        public IEnumerable<SolutionFileProjectFileReference> ListSolutionFolderProjectFiles(SolutionFile solutionFile, string solutionFilePath, string solutionFolderPath)
        {
            var hasSolutionFolder = this.HasSolutionFolder(solutionFile, solutionFolderPath, out var solutionFolder);
            if (!hasSolutionFolder)
            {
                throw new Exception($"Solution folder path does not exist: {solutionFolderPath}");
            }

            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var childProjectGUIDs = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolder.ProjectGUID).Select(x => x.ChildProjectGUID);

            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            var childProjectReferences = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectTypeGUID != solutionFolderProjectTypeGuid && childProjectGUIDs.Contains(x.ProjectGUID));

            foreach (var childProjectReference in childProjectReferences)
            {
                var projectFilePath = this.StringlyTypedPathOperator.GetProjectFilePath(solutionFilePath, childProjectReference.ProjectFileRelativePathValue);

                var childProjectFileReference = new SolutionFileProjectFileReference()
                {
                    ProjectFilePathValue = projectFilePath,
                    ProjectName = childProjectReference.ProjectName,
                    ProjectGUID = childProjectReference.ProjectGUID,
                    ProjectTypeGUID = childProjectReference.ProjectTypeGUID,
                };
                yield return childProjectFileReference;
            }
        }

        public IEnumerable<SolutionFileProjectReference> ListSolutionFolderSolutionFolders(SolutionFile solutionFile, string solutionFolderPath)
        {
            var hasSolutionFolder = this.HasSolutionFolder(solutionFile, solutionFolderPath, out var solutionFolder);
            if(!hasSolutionFolder)
            {
                throw new Exception($"Solution folder path does not exist: {solutionFolderPath}");
            }

            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var childProjectGUIDs = nestedProjectsGlobalSection.ProjectNestings.Where(x => x.ParentProjectGUID == solutionFolder.ProjectGUID).Select(x => x.ChildProjectGUID);

            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            var childSolutionFolders = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectTypeGUID == solutionFolderProjectTypeGuid && childProjectGUIDs.Contains(x.ProjectGUID));
            return childSolutionFolders;
        }

        public IEnumerable<SolutionFileProjectReference> ListRootSolutionFolders(SolutionFile solutionFile)
        {
            var rootSolutionFolderGUIDs = this.GetRootSolutionFolderGUIDs(solutionFile);

            var rootSolutionFolders = solutionFile.SolutionFileProjectReferences.Where(x => rootSolutionFolderGUIDs.Contains(x.ProjectGUID));
            return rootSolutionFolders;
        }

        public IEnumerable<SolutionFileProjectFileReference> ListRootProjectFiles(SolutionFile solutionFile, string solutionFilePath)
        {
            // Select all non solution-folder guids.
            var solutionFolderProjectTypeGuid = this.VisualStudioSolutionFolderProjectTypeGuidProvider.GetVisualStudioSolutionFolderProjectTypeGuid();

            var allNonSolutionFolderGUIDs = solutionFile.SolutionFileProjectReferences.Where(x => x.ProjectTypeGUID != solutionFolderProjectTypeGuid).Select(x => x.ProjectGUID);

            // Select all project GUIDs that are in a solution folder.
            var nestedProjectsGlobalSection = solutionFile.GlobalSections.AcquireNestedProjectsGlobalSection();

            var allChildGUIDs = nestedProjectsGlobalSection.ProjectNestings.Select(x => x.ChildProjectGUID).Distinct();

            var rootProjectGUIDs = allNonSolutionFolderGUIDs.Except(allChildGUIDs);

            var rootProjects = solutionFile.SolutionFileProjectReferences.Where(x => rootProjectGUIDs.Contains(x.ProjectGUID)).Select(x =>
            {
                var projectFilePath = this.StringlyTypedPathOperator.GetProjectFilePath(solutionFilePath, x.ProjectFileRelativePathValue);

                var projectFileReference = new SolutionFileProjectFileReference()
                {
                    ProjectFilePathValue = projectFilePath,
                    ProjectGUID = x.ProjectGUID,
                    ProjectName = x.ProjectName,
                    ProjectTypeGUID = x.ProjectTypeGUID,
                };
                return projectFileReference;
            });

            return rootProjects;
        }
    }
}
