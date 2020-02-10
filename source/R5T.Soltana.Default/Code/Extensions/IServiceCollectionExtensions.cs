using System;

using Microsoft.Extensions.DependencyInjection;

using R5T.Dacia;
using R5T.Hladir;
using R5T.Koping;
using R5T.Lombardy;
using R5T.Solgene;


namespace R5T.Soltana.Default
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="VisualStudioSolutionFileOperator"/> implementation of <see cref="IVisualStudioSolutionFileOperator"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static IServiceCollection AddDefaultVisualStudioSolutionFileOperator(this IServiceCollection services,
            ServiceAction<IVisualStudioSolutionFileGenerator> addSolutionFileGenerator,
            ServiceAction<IVisualStudioSolutionFolderProjectTypeGuidProvider> addSolutionFolderProjectTypeGuidProvider,
            ServiceAction<IVisualStudioProjectFileNameConventions> addProjectFileNameConventions,
            ServiceAction<IStringlyTypedPathOperator> addStringlyTypedPathOperator)
        {
            services
                .AddSingleton<IVisualStudioSolutionFileOperator, VisualStudioSolutionFileOperator>()
                .RunServiceAction(addSolutionFileGenerator)
                .RunServiceAction(addSolutionFolderProjectTypeGuidProvider)
                .RunServiceAction(addProjectFileNameConventions)
                .RunServiceAction(addStringlyTypedPathOperator)

                // Extra services.
                .AddSingleton<IVisualStudioNewProjectGuidProvider, VisualStudioNewProjectGuidProvider>()
                .AddSingleton<ISolutionFolderPathOperator, SolutionFolderPathOperator>()
                .AddSingleton<ISolutionFolderPathConventions, SolutionFolderPathConventions>()
                ;

            return services;
        }

        /// <summary>
        /// Adds the <see cref="VisualStudioSolutionFileOperator"/> implementation of <see cref="IVisualStudioSolutionFileOperator"/> as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        public static ServiceAction<IVisualStudioSolutionFileOperator> AddDefaultVisualStudioSolutionFileOperatorAction(this IServiceCollection services,
            ServiceAction<IVisualStudioSolutionFileGenerator> addSolutionFileGenerator,
            ServiceAction<IVisualStudioSolutionFolderProjectTypeGuidProvider> addSolutionFolderProjectTypeGuidProvider,
            ServiceAction<IVisualStudioProjectFileNameConventions> addProjectFileNameConventions,
            ServiceAction<IStringlyTypedPathOperator> addStringlyTypedPathOperator)
        {
            var serviceAction = new ServiceAction<IVisualStudioSolutionFileOperator>(() => services.AddDefaultVisualStudioSolutionFileOperator(
                addSolutionFileGenerator,
                addSolutionFolderProjectTypeGuidProvider,
                addProjectFileNameConventions,
                addStringlyTypedPathOperator));
            return serviceAction;
        }
    }
}
