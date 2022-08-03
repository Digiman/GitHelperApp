using GitHelperApp.Configuration;

namespace GitHelperApp.Extensions;

public static class RepositoriesConfigExtensions
{
    public static RepositoryConfig GetRepositoryConfig(this RepositoriesConfig repositoriesConfig, string repositoryName)
    {
        var repoInfo = repositoriesConfig.Repositories.SingleOrDefault(x => x.Name == repositoryName);
        if (repoInfo != null)
        {
            repoInfo.SourceBranch = !string.IsNullOrEmpty(repoInfo.SourceBranch)
                ? repoInfo.SourceBranch
                : repositoriesConfig.DefaultSourceBranch;
            repoInfo.DestinationBranch = !string.IsNullOrEmpty(repoInfo.DestinationBranch)
                ? repoInfo.DestinationBranch
                : repositoriesConfig.DefaultDestinationBranch;
        }

        return repoInfo;
    }
}