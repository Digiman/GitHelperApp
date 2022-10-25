using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Simple service to work with repositories in Azure DevOps.
/// </summary>
public interface IRepositoryService
{
    /// <summary>
    /// Get list of repositories by the default team project from configuration file.
    /// </summary>
    /// <returns>Returns list of git repositories info.</returns>
    Task<List<RepositoryModel>> GetRepositoriesListAsync();
    
    /// <summary>
    /// Get list of repositories by the team project.
    /// </summary>
    /// <param name="teamProject">Team project name.</param>
    /// <returns>Returns list of git repositories info.</returns>
    Task<List<RepositoryModel>> GetRepositoriesListAsync(string teamProject);
}