using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHelperApp.Services;

/// <summary>
/// Special service to do the compare between branched in the repositories.
/// </summary>
public sealed class CompareService : ICompareService
{
    private readonly ILogger<CompareService> _logger;
    private readonly IGitService _gitService;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly IAzureDevOpsService _azureDevOpsService;

    public CompareService(ILogger<CompareService> logger, IGitService gitService,
        IOptions<RepositoriesConfig> repositoriesConfig, IAzureDevOpsService azureDevOpsService)
    {
        _logger = logger;
        _gitService = gitService;
        _azureDevOpsService = azureDevOpsService;
        _repositoriesConfig = repositoriesConfig.Value;
    }

    /// <inheritdoc />
    public List<CompareResult> CompareLocal()
    {
        var compareResults = CompareBranchesLocal(_repositoriesConfig);

        return compareResults;
    }

    /// <inheritdoc />
    public async Task<List<CompareResult>> CompareAzureAsync()
    {
        var compareResult = await CompareBranchesAzureAsync(_repositoriesConfig);

        return compareResult;
    }

    #region Helper functions with the main logic.

    /// <summary>
    /// Run local branches compare for all repositories from config.
    /// </summary>
    /// <param name="repositoriesConfig">Configuration for repositories to compare.</param>
    /// <returns>Returns the compare result.</returns>
    private List<CompareResult> CompareBranchesLocal(RepositoriesConfig repositoriesConfig)
    {
        var result = new List<CompareResult>(repositoriesConfig.Repositories.Count);
        foreach (var repositoryConfig in repositoriesConfig.Repositories)
        {
            if (!Directory.Exists(repositoryConfig.Path))
            {
                _logger.LogInformation($"Repository path is not exists. Skipping processing {repositoryConfig.Name}");
                continue;
            }

            var repoInfo = repositoryConfig.GetRepositoryConfig(repositoriesConfig);

            _logger.LogInformation($"Repository: {repoInfo.Name}. Comparing: {repoInfo.SourceBranch} -> {repoInfo.DestinationBranch}");

            var (isChanges, count, commits) = _gitService.CompareBranches(repositoryConfig.Path,
                GitBranchHelper.GetRefName(repoInfo.SourceBranch),
                GitBranchHelper.GetRefName(repoInfo.DestinationBranch));

            result.Add(new CompareResult
            {
                RepositoryName = repositoryConfig.Name,
                ChangesCount = count,
                SourceBranch = repoInfo.SourceBranch,
                DestinationBranch = repoInfo.DestinationBranch,
                Commits = commits
            });
        }

        return result;
    }

    /// <summary>
    /// Run branches compare for all repositories from config with Azure DevOps API to use internal functionality.
    /// </summary>
    /// <param name="repositoriesConfig">Configuration for repositories to compare.</param>
    /// <returns>Returns the compare result.</returns>
    private async Task<List<CompareResult>> CompareBranchesAzureAsync(RepositoriesConfig repositoriesConfig)
    {
        var result = new List<CompareResult>(repositoriesConfig.Repositories.Count);
        foreach (var repositoryConfig in repositoriesConfig.Repositories)
        {
            // get extended repo info - with the default values if not provided
            var repoInfo = repositoriesConfig.GetRepositoryConfig(repositoryConfig.Name);

            _logger.LogInformation($"Repository: {repoInfo.Name}. Comparing: {repoInfo.SourceBranch} -> {repoInfo.DestinationBranch}");

            var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repoInfo.Name, repoInfo.TeamProject);

            var gitCommits = await _azureDevOpsService.GetCommitsDetailsAsync(repo,
                repoInfo.SourceBranch, repoInfo.DestinationBranch);

            // TODO: need to improve here the logic to calculate the changes count - maybe need to make more calls to get proper details

            var commitDiffs = await _azureDevOpsService.GetCommitsDiffsAsync(repo, repoInfo.TeamProject, repoInfo.SourceBranch,
                repoInfo.DestinationBranch);

            // var changesCount = gitCommits.Select(x => x.ChangeCounts).Sum(x => x.Count); // old logic to calculate the changes that works wrong!(
            var changesCount = commitDiffs.BehindCount ?? 0; // better but locally it works correct not as here(

            result.Add(new CompareResult
            {
                SourceBranch = repoInfo.SourceBranch,
                DestinationBranch = repoInfo.DestinationBranch,
                RepositoryName = repoInfo.Name,
                Commits = gitCommits.Select(x => x.CommitId).ToList(),
                ChangesCount = changesCount
            });
        }

        return result;
    }

    #endregion
}