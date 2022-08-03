﻿using GitHelperApp.Builders;
using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Services;

public sealed class PullRequestService : IPullRequestService
{
    private readonly ILogger<PullRequestService> _logger;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly AzureDevOpsConfig _azureDevOpsConfig;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly PullRequestConfig _pullRequestConfig;

    public PullRequestService(ILogger<PullRequestService> logger, IOptions<RepositoriesConfig> repositoriesConfig,
        IOptions<AzureDevOpsConfig> azureDevOpsConfig, IAzureDevOpsService azureDevOpsService,
        IOptions<PullRequestConfig> pullRequestModel)
    {
        _logger = logger;
        _azureDevOpsConfig = azureDevOpsConfig.Value;
        _azureDevOpsService = azureDevOpsService;
        _pullRequestConfig = pullRequestModel.Value;
        _repositoriesConfig = repositoriesConfig.Value;
    }

    /// <summary>
    /// Create the PR for all repositories with changes exists.
    /// </summary>
    /// <param name="compareResults">Compare results from the first step.</param>
    /// <param name="isDryRun">Run in rey run mode without actual PR creation.</param>
    /// <returns>Returns the PR result with details on each PR created/existed.</returns>
    public async Task<List<PullRequestResult>> CreatePullRequestsAsync(List<CompareResult> compareResults, bool isDryRun = false)
    {
        var result = new List<PullRequestResult>();
        
        foreach (var compareResult in compareResults.Where(x => x.ChangesCount > 0))
        {
            _logger.LogInformation($"Processing PR for repository - {compareResult.RepositoryName}...");
            
            var repoInfo = _repositoriesConfig.GetRepositoryConfig(compareResult.RepositoryName);
            
            var prResult = await CreatePullRequestAsync(compareResult.RepositoryName, repoInfo.TeamProject,
                repoInfo.SourceBranch, repoInfo.DestinationBranch, _pullRequestConfig, isDryRun);
            
            result.Add(prResult);
        }

        return result;
    }
    
    private async Task<PullRequestResult> CreatePullRequestAsync(string repositoryName, string teamProject, 
        string sourceBranch, string destinationBranch, PullRequestConfig pullRequestModel, bool isDryRun = false)
    {
        var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryName, teamProject);
        var prs = await _azureDevOpsService.GetPullRequestsAsync(repo, PullRequestStatus.Active);

        var prTitle = pullRequestModel.Title;

        var actualPr = SearchForPrCreated(prs, prTitle, sourceBranch, destinationBranch);
        if (actualPr == null)
        {
            var gitCommits = await _azureDevOpsService.GetCommitsDetailsAsync(repo, sourceBranch, destinationBranch);

            if (gitCommits.Count == 0)
            {
                _logger.LogWarning("Something goes wrong and no changes found for PR!");
            }
            else
            {
                var workItems = await _azureDevOpsService.GetWorkItemsAsync(gitCommits);

                workItems = ProcessWorkItems(workItems);

                var builder = new GitPullRequestBuilder(prTitle, pullRequestModel.Description, sourceBranch, destinationBranch);
                builder = builder
                    .WithAuthor(pullRequestModel.Author)
                    .WithWorkItems(workItems)
                    .WthDefaultReviewers();
                if (pullRequestModel.IsDraft)
                {
                    builder = builder.AsDraft();
                }

                var pr = builder.Build();
                if (isDryRun)
                {
                    _logger.LogInformation($"PR is not created because of Dry Run. Work items count: {workItems.Count}.");

                    return new PullRequestResult
                    {
                        PullRequestId = 0,
                        RepositoryName = repositoryName,
                        Url = string.Empty,
                        WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList()
                    };
                }

                var prCreated = await _azureDevOpsService.CreatePullRequestAsync(pr, repo);

                _logger.LogInformation($"PR was created with Id {prCreated.PullRequestId}. Url: {prCreated.Url}. Work items count: {workItems.Count}.");

                return new PullRequestResult
                {
                    PullRequestId = prCreated.PullRequestId,
                    RepositoryName = repositoryName,
                    Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, prCreated.PullRequestId),
                    WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList()
                };
            }
        }

        var workItemsForActualPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, actualPr.PullRequestId);

        _logger.LogInformation($"PR already created with Id {actualPr.PullRequestId}. Url: {actualPr.Url}. Work items count: {workItemsForActualPr.Count}.");

        return new PullRequestResult
        {
            PullRequestId = actualPr.PullRequestId,
            RepositoryName = repositoryName,
            Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, actualPr.PullRequestId),
            WorkItems = workItemsForActualPr.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id))).ToList()
        };
    }
    
    private static List<WorkItem> ProcessWorkItems(List<WorkItem> workItems)
    {
        // we need to process here all the WI to exclude duplicates and etc.
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItem>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }

        return workItems;
    }

    private static GitPullRequest SearchForPrCreated(List<GitPullRequest> pullRequests, string title, string source, string destination)
    {
        return pullRequests.FirstOrDefault(x =>
            x.Title == title && x.SourceRefName == GitPullRequestBuilder.GetRefName(source)
                             && x.TargetRefName == GitPullRequestBuilder.GetRefName(destination));
    }
}