using GitHelperApp.Builders;
using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Services;

/// <summary>
/// Service to work with Pull Requests.
/// </summary>
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
    
    /// <inheritdoc />
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

    public async Task<List<PullRequestSearchResult>> SearchPullRequestsAsync(string status)
    {
        var result = new List<PullRequestSearchResult>();

        var prStatus = ConvertStatus(status);
        
        foreach (var repositoryConfig in _repositoriesConfig.Repositories)
        {
            _logger.LogInformation($"Searching for Pull Request in the {repositoryConfig.Name}...");
            
            var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryConfig.Name, repositoryConfig.TeamProject);
            var prs = await _azureDevOpsService.GetPullRequestsAsync(repo, prStatus);

            foreach (var gitPullRequest in prs)
            {
                var workItemsFlorPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, gitPullRequest.PullRequestId);

                var prResult = new PullRequestSearchResult
                {
                    PullRequestId = gitPullRequest.PullRequestId,
                    Title = gitPullRequest.Title,
                    Description = gitPullRequest.Description,
                    RepositoryName = repositoryConfig.Name,
                    Url = _azureDevOpsService.BuildPullRequestUrl(repositoryConfig.TeamProject, repo.Name, gitPullRequest.PullRequestId),
                    WorkItems = workItemsFlorPr.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(repositoryConfig.TeamProject, x.Id))).ToList(),
                    IsNew = false
                };
                
                result.Add(prResult);
            }
        }

        return result;
    }
    
    #region Helpers.
    
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
                _logger.LogWarning("Something goes wrong and no changes found to create the PR! Trying to search completed PR...");
                
                // search for PR that can already created and completed
                var completedPrs = await _azureDevOpsService.GetPullRequestsAsync(repo, PullRequestStatus.Completed);

                var completedPr = SearchForPrCreated(completedPrs, prTitle, sourceBranch, destinationBranch);
                if (completedPr != null)
                {
                    var workItemsForCompletedPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, completedPr.PullRequestId);

                    _logger.LogInformation($"PR already created with Id {completedPr.PullRequestId}. Url: {completedPr.Url}. Work items count: {workItemsForCompletedPr.Count}.");

                    return new PullRequestResult
                    {
                        PullRequestId = completedPr.PullRequestId,
                        RepositoryName = repositoryName,
                        Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, completedPr.PullRequestId),
                        WorkItems = workItemsForCompletedPr.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id))).ToList(),
                        IsNew = false
                    };
                }
            }
            else
            {
                var workItems = await _azureDevOpsService.GetWorkItemsAsync(gitCommits);

                workItems = ProcessWorkItems(workItems);

                var builder = new GitPullRequestBuilder(prTitle, pullRequestModel.Description, sourceBranch, destinationBranch);
                builder
                    .WithAuthor(pullRequestModel.Author)
                    .WithWorkItems(workItems)
                    .WthDefaultReviewers();
                if (pullRequestModel.IsDraft)
                {
                    builder.AsDraft();
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
                        WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList(),
                        IsNew = true
                    };
                }

                var prCreated = await _azureDevOpsService.CreatePullRequestAsync(pr, repo);

                _logger.LogInformation($"PR was created with Id {prCreated.PullRequestId}. Url: {prCreated.Url}. Work items count: {workItems.Count}.");

                return new PullRequestResult
                {
                    PullRequestId = prCreated.PullRequestId,
                    RepositoryName = repositoryName,
                    Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, prCreated.PullRequestId),
                    WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList(),
                    IsNew = true
                };
            }
        }
        else
        {
            var workItemsForActualPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, actualPr.PullRequestId);

            _logger.LogInformation($"PR already created with Id {actualPr.PullRequestId}. Url: {actualPr.Url}. Work items count: {workItemsForActualPr.Count}.");

            return new PullRequestResult
            {
                PullRequestId = actualPr.PullRequestId,
                RepositoryName = repositoryName,
                Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, actualPr.PullRequestId),
                WorkItems = workItemsForActualPr.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id))).ToList(),
                IsNew = false
            };
        }

        // return empty result
        return new PullRequestResult
        {
            PullRequestId = 0,
            RepositoryName = repositoryName,
            Url = string.Empty,
            WorkItems = new List<WorkItemModel>(),
            IsNew = false
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
            (x.Title == title && x.SourceRefName == GitPullRequestBuilder.GetRefName(source)
                              && x.TargetRefName == GitPullRequestBuilder.GetRefName(destination))
            || (x.SourceRefName == GitPullRequestBuilder.GetRefName(source)
                && x.TargetRefName == GitPullRequestBuilder.GetRefName(destination)));
    }
    
    private static PullRequestStatus ConvertStatus(object status)
    {
        return status switch
        {
            "a" => PullRequestStatus.Active,
            "c" => PullRequestStatus.Completed,
            "b" => PullRequestStatus.Abandoned,
            "all" => PullRequestStatus.All,
            _ => PullRequestStatus.NotSet
        };
    }
    
    #endregion
}