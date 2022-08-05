using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service to work with Pull Requests.
/// </summary>
public interface IPullRequestService
{
    /// <summary>
    /// Create the PR for all repositories with changes if exists.
    /// </summary>
    /// <param name="compareResults">Compare results from the first step.</param>
    /// <param name="isDryRun">Run in rey run mode without actual PR creation.</param>
    /// <returns>Returns the PR result with details on each PR created/existed.</returns>
    Task<List<PullRequestResult>> CreatePullRequestsAsync(List<CompareResult> compareResults, bool isDryRun = false);
    
    Task<List<PullRequestSearchResult>> SearchPullRequestsAsync(string status);
}