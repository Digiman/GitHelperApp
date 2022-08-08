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

    public List<CompareResult> CompareLocal()
    {
        var compareResults = CompareBranchesLocal(_repositoriesConfig);
        
        return compareResults;
    }

    public async Task<List<CompareResult>> CompareAzureAsync()
    {
        var compareResult = await CompareBranchesAzureAsync(_repositoriesConfig);

        return compareResult;
    }
    
    #region Helper functions with the main logic.

    private List<CompareResult> CompareBranchesLocal(RepositoriesConfig repositoriesConfig)
    {
        var result = new List<CompareResult>(repositoriesConfig.Repositories.Count);
        foreach (var repositoryConfig in repositoriesConfig.Repositories)
        {
            if (!Directory.Exists(repositoryConfig.Path))
            {
                continue;
            }

            _logger.LogInformation($"Repository: {repositoryConfig.Name}. Comparing: {repositoryConfig.SourceBranch} -> {repositoryConfig.DestinationBranch}");
            
            var source = !string.IsNullOrEmpty(repositoryConfig.SourceBranch)
                ? repositoryConfig.SourceBranch
                : repositoriesConfig.DefaultSourceBranch;
            var destination = !string.IsNullOrEmpty(repositoryConfig.DestinationBranch)
                ? repositoryConfig.DestinationBranch
                : repositoriesConfig.DefaultDestinationBranch;

            var (isChanges, count, commits) = _gitService.CompareBranches(repositoryConfig.Path,
                GitLocalHelper.GetRefName(source), GitLocalHelper.GetRefName(destination));

            result.Add(new CompareResult
            {
                RepositoryName = repositoryConfig.Name,
                ChangesCount = count,
                SourceBranch = source,
                DestinationBranch = destination,
                Commits = commits
            });
        }

        return result;
    }

    private async Task<List<CompareResult>> CompareBranchesAzureAsync(RepositoriesConfig repositoriesConfig)
    {
        var result = new List<CompareResult>(repositoriesConfig.Repositories.Count);
        foreach (var repositoryConfig in repositoriesConfig.Repositories)
        {
            // get extended repo info
            var repoInfo = repositoriesConfig.GetRepositoryConfig(repositoryConfig.Name);
            
            var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repoInfo.Name, repoInfo.TeamProject);

            var gitCommits = await _azureDevOpsService.GetCommitsDetailsAsync(repo,
                repoInfo.SourceBranch, repoInfo.DestinationBranch);

            // TODO: need to improve here the logic to calculate the changes count - maybe need to make more calls to get proper details
            
            result.Add(new CompareResult
            {
                SourceBranch = repoInfo.SourceBranch,
                DestinationBranch = repoInfo.DestinationBranch,
                RepositoryName = repoInfo.Name,
                Commits = gitCommits.Select(x => x.CommitId).ToList(),
                ChangesCount = gitCommits.Select(x => x.ChangeCounts).Sum(x => x.Count)
            });
        }

        return result;
    }
    
    #endregion
}