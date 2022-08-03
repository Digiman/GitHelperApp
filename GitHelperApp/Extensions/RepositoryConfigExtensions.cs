using GitHelperApp.Configuration;

namespace GitHelperApp.Extensions;

public static class RepositoryConfigExtensions
{
    public static RepositoryConfig GetRepositoryConfig(this RepositoryConfig repositoryConfig,
        RepositoriesConfig repositoriesConfig)
    {
        repositoryConfig.SourceBranch = !string.IsNullOrEmpty(repositoryConfig.SourceBranch)
            ? repositoryConfig.SourceBranch
            : repositoriesConfig.DefaultSourceBranch;
        repositoryConfig.DestinationBranch = !string.IsNullOrEmpty(repositoryConfig.DestinationBranch)
            ? repositoryConfig.DestinationBranch
            : repositoriesConfig.DefaultDestinationBranch;

        return repositoryConfig;
    }
}