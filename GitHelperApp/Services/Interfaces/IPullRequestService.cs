using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

public interface IPullRequestService
{
    Task<List<PullRequestResult>> CreatePullRequestsAsync(List<CompareResult> compareResults, bool isDryRun = false);
}