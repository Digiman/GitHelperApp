using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHelperApp.Services;

/// <summary>
/// Service to work with the work items on Azure DevOps.
/// </summary>
public sealed class WorkItemsService : BaseSharedService, IWorkItemsService
{
    private readonly ILogger<WorkItemsService> _logger;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly WorkItemFilterConfig _workItemFilterConfig;
    
    public WorkItemsService(ILogger<WorkItemsService> logger, IAzureDevOpsService azureDevOpsService,
        IOptions<RepositoriesConfig> repositoriesConfig, IOptions<WorkItemFilterConfig> workItemFilterConfig)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _repositoriesConfig = repositoriesConfig.Value;
        _workItemFilterConfig = workItemFilterConfig.Value;
    }

    /// <inheritdoc />
    public async Task<List<WorkItemSearchResult>> SearchWorkItemsAsync(List<CompareResult> compareResults, bool isFilter = false)
    {
        var result = new List<WorkItemSearchResult>();
        
        foreach (var compareResult in compareResults.Where(x => x.ChangesCount > 0))
        {
            _logger.LogInformation($"Processing PR for repository - {compareResult.RepositoryName}...");
            
            var repoInfo = _repositoriesConfig.GetRepositoryConfig(compareResult.RepositoryName);
            
            var prResult = await SearchWorkItemsAsync(compareResult.RepositoryName, repoInfo.TeamProject,
                repoInfo.SourceBranch, repoInfo.DestinationBranch, isFilter);
            
            result.Add(prResult);
        }

        return result;
    }

    #region Helpers.

    private async Task<WorkItemSearchResult> SearchWorkItemsAsync(string repositoryName, string teamProject,
        string sourceBranch, string destinationBranch, bool isFilter)
    {
        var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryName, teamProject);
        
        var gitCommits = await _azureDevOpsService.GetCommitsDetailsAsync(repo, sourceBranch, destinationBranch);

        if (gitCommits.Count > 0)
        {
            var workItems = await _azureDevOpsService.GetWorkItemsAsync(gitCommits);

            workItems = ProcessWorkItems(workItems, _workItemFilterConfig, isFilter);

            var result = new WorkItemSearchResult
            {
                RepositoryName = repositoryName,
                WorkItems = workItems
                    .Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString())))
                    .ToList(),
            };

            return result;
        }

        return new WorkItemSearchResult
        {
            RepositoryName = repositoryName,
            WorkItems = Enumerable.Empty<WorkItemModel>().ToList()
        };
    }
    
    #endregion
}