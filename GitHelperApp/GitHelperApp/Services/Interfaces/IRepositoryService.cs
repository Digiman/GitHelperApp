using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Simple service to work with repositories in Azure DevOps.
/// </summary>
public interface IRepositoryService
{
    Task<List<RepositoryModel>> GetRepositoriesListAsync();
    Task<List<RepositoryModel>> GetRepositoriesListAsync(string teamProject);
}