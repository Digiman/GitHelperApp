using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Services;

/// <summary>
/// Simple service to work with the work items on Azure DevOps.
/// </summary>
public sealed class WorkItemsService : IWorkItemsService
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
        
        var workItems = await _azureDevOpsService.GetWorkItemsAsync(gitCommits);

        workItems = ProcessWorkItems(workItems, isFilter);

        var result = new WorkItemSearchResult
        {
            RepositoryName = repositoryName,
            WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList(),
        };

        return result;
    }
    
    private List<WorkItem> ProcessWorkItems(List<WorkItem> workItems, bool isFilter)
    {
        // filter work items by type and area path to use only correct ones
        if (isFilter)
        {
            workItems = WorkItemsHelper.FilterWorkItems(workItems, _workItemFilterConfig);
        }

        // we need to process here all the WI to exclude duplicates and etc.
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItem>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }

            return result;
        }

        return workItems;
    }

    #endregion
}