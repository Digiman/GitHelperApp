using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp;

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

public static class WorkItemExtensions
{
    public static WorkItemModel ToModel(this WorkItem workItem)
    {
        return new WorkItemModel
        {
            Id = workItem.Id.ToString(),
            Url = workItem.Url
        };
    }
}

public static class ResourceRefExtensions
{
    public static WorkItemModel ToModel(this ResourceRef resourceRef)
    {
        return new WorkItemModel
        {
            Id = resourceRef.Id,
            Url = resourceRef.Url
        };
    }
}

