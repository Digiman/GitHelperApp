using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace GitHelperApp.Services;

/// <summary>
/// Simple service to work with repositories in Azure DevOps.
/// </summary>
public sealed class RepositoryService : IRepositoryService
{
    private readonly IAzureDevOpsService _azureDevOpsService;

    public RepositoryService(IAzureDevOpsService azureDevOpsService)
    {
        _azureDevOpsService = azureDevOpsService;
    }

    /// <inheritdoc />
    public async Task<List<RepositoryModel>> GetRepositoriesListAsync()
    {
        var repositories = await _azureDevOpsService.GetRepositoriesListAsync();

        return repositories.Select(CreateRepositoryModel).ToList();
    }

    /// <inheritdoc />
    public async Task<List<RepositoryModel>> GetRepositoriesListAsync(string teamProject)
    {
        var repositories = await _azureDevOpsService.GetRepositoriesListAsync(teamProject);

        return repositories.Select(CreateRepositoryModel).ToList();
    }

    private static RepositoryModel CreateRepositoryModel(GitRepository repo)
    {
        return new RepositoryModel
        {
            Id = repo.Id,
            Name = repo.Name,
            Url = repo.Url,
            RemoteUrl = repo.RemoteUrl
        };
    }
}