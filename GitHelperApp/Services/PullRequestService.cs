﻿using GitHelperApp.Builders;
using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace GitHelperApp.Services;

/// <summary>
/// Service to work with Pull Requests.
/// </summary>
public sealed class PullRequestService : BaseSharedService, IPullRequestService
{
    private readonly ILogger<PullRequestService> _logger;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly IAzureDevOpsService _azureDevOpsService;
    private readonly PullRequestConfig _pullRequestConfig;
    private readonly WorkItemFilterConfig _workItemFilterConfig;
    private readonly CustomPrConfig _customPrConfig;

    public PullRequestService(ILogger<PullRequestService> logger, IAzureDevOpsService azureDevOpsService,
        IOptions<RepositoriesConfig> repositoriesConfig, IOptions<PullRequestConfig> pullRequestConfig,
        IOptions<WorkItemFilterConfig> workItemFilterConfig, IOptions<CustomPrConfig> customPrConfig)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _customPrConfig = customPrConfig.Value;
        _pullRequestConfig = pullRequestConfig.Value;
        _repositoriesConfig = repositoriesConfig.Value;
        _workItemFilterConfig = workItemFilterConfig.Value;
    }

    /// <inheritdoc />
    public async Task<List<PullRequestResult>> CreatePullRequestsAsync(List<CompareResult> compareResults, bool isFilter, bool isDryRun = false)
    {
        var result = new List<PullRequestResult>();

        foreach (var compareResult in compareResults.Where(x => x.ChangesCount > 0))
        {
            _logger.LogInformation($"Processing PR for repository - {compareResult.RepositoryName}...");

            var repoInfo = _repositoriesConfig.GetRepositoryConfig(compareResult.RepositoryName);

            var prResult = await CreatePullRequestAsync(compareResult.RepositoryName, repoInfo.TeamProject,
                repoInfo.SourceBranch, repoInfo.DestinationBranch, _pullRequestConfig, isFilter, isDryRun);

            result.Add(prResult);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<PullRequestSearchResult>> SearchPullRequestsAsync(string status, int count)
    {
        var result = new List<PullRequestSearchResult>();

        var prStatus = ConvertStatus(status);

        foreach (var repositoryConfig in _repositoriesConfig.Repositories)
        {
            _logger.LogInformation($"Searching for Pull Request in the {repositoryConfig.Name}...");

            repositoryConfig.GetRepositoryConfig(_repositoriesConfig);

            var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryConfig.Name, repositoryConfig.TeamProject);
            var prsFromSource = await _azureDevOpsService.GetPullRequestsWithOptionsAsync(repo, prStatus, count,
                source: repositoryConfig.SourceBranch);
            var prsToDestination = await _azureDevOpsService.GetPullRequestsWithOptionsAsync(repo, prStatus, count,
                destination: repositoryConfig.DestinationBranch);

            var prs = new List<GitPullRequest>(prsFromSource.Count + prsToDestination.Count);
            prs.AddRange(prsFromSource);
            prs.AddRange(prsToDestination);

            foreach (var gitPullRequest in prs)
            {
                var workItemsFlorPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, gitPullRequest.PullRequestId);

                var prResult = new PullRequestSearchResult
                {
                    PullRequestId = gitPullRequest.PullRequestId,
                    Title = gitPullRequest.Title,
                    Description = gitPullRequest.Description,
                    SourceBranch = RemoveRefName(gitPullRequest.SourceRefName),
                    DestinationBranch = RemoveRefName(gitPullRequest.TargetRefName),
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

    /// <inheritdoc />
    public async Task<PullRequestResult> CreatePullRequestAsync(bool isDryRun = false)
    {
        var prModel = new PullRequestConfig
        {
            Title = _customPrConfig.Title,
            Description = _customPrConfig.Description,
            IsDraft = _customPrConfig.IsDraft,
            Author = _customPrConfig.Author,
            Tags = Enumerable.Empty<string>().ToArray()
        };

        var result = await CreatePullRequestAsync(_customPrConfig.RepositoryName, _customPrConfig.TeamProject,
            _customPrConfig.SourceBranch, _customPrConfig.DestinationBranch, prModel, false, isDryRun);

        return result;
    }

    #region Helpers.

    private async Task<PullRequestResult> CreatePullRequestAsync(string repositoryName, string teamProject,
        string sourceBranch, string destinationBranch, PullRequestConfig pullRequestConfig, bool isFilter, bool isDryRun = false)
    {
        var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryName, teamProject);
        var prs = await _azureDevOpsService.GetPullRequestsAsync(repo, PullRequestStatus.Active);

        var prTitle = pullRequestConfig.Title;

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
                        IsNew = false,
                        Title = completedPr.Title
                    };
                }
            }
            else
            {
                var workItems = await _azureDevOpsService.GetWorkItemsAsync(gitCommits);

                // add additional required work items from config - for some releases it can be added manually because no PRs or related items for commits
                if (_workItemFilterConfig.WorkItemsToAdd != null && _workItemFilterConfig.WorkItemsToAdd.Any())
                {
                    var witToAdd = await _azureDevOpsService.GetWorkItemsAsync(_workItemFilterConfig.WorkItemsToAdd.ToList());
                    workItems.AddRange(witToAdd);
                }

                workItems = ProcessWorkItems(workItems, _workItemFilterConfig, isFilter);

                var builder = new GitPullRequestBuilder(prTitle, pullRequestConfig.Description, sourceBranch, destinationBranch);
                builder
                    .WithAuthor(pullRequestConfig.Author)
                    .WithWorkItems(workItems)
                    .WthDefaultReviewers()
                    .WithTags(pullRequestConfig.Tags);
                
                // create as draft
                if (pullRequestConfig.IsDraft)
                {
                    builder.AsDraft();
                }
                
                // create with auto-complete
                if (pullRequestConfig.Autocomplete)
                {
                    builder.WithAutocomplete(pullRequestConfig.Author);
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
                        IsNew = true,
                        Title = "EMPTY PULL REQUEST - DRY MODE"
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
                    IsNew = true,
                    Title = prCreated.Title
                };
            }
        }
        else
        {
            var workItemsForActualPr = await _azureDevOpsService.GetPullRequestDetailsAsync(repo, actualPr.PullRequestId);

            var workItems = await _azureDevOpsService.GetWorkItemsAsync(workItemsForActualPr);

            _logger.LogInformation($"PR already created with Id {actualPr.PullRequestId}. Url: {actualPr.Url}. Work items count: {workItemsForActualPr.Count}.");

            return new PullRequestResult
            {
                PullRequestId = actualPr.PullRequestId,
                RepositoryName = repositoryName,
                Url = _azureDevOpsService.BuildPullRequestUrl(teamProject, repositoryName, actualPr.PullRequestId),
                WorkItems = workItems.Select(x => x.ToModel(_azureDevOpsService.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList(),
                IsNew = false,
                Title = actualPr.Title
            };
        }

        // return empty result
        return new PullRequestResult
        {
            PullRequestId = 0,
            RepositoryName = repositoryName,
            Url = string.Empty,
            WorkItems = new List<WorkItemModel>(),
            IsNew = false,
            Title = "EMPTY PULL REQUEST"
        };
    }

    private static GitPullRequest SearchForPrCreated(List<GitPullRequest> pullRequests, string title, string source, string destination)
    {
        return pullRequests.FirstOrDefault(x =>
            (x.Title == title && x.SourceRefName == GitBranchHelper.GetRefNameForAzure(source)
                              && x.TargetRefName == GitBranchHelper.GetRefNameForAzure(destination))
            || (x.SourceRefName == GitBranchHelper.GetRefNameForAzure(source)
                && x.TargetRefName == GitBranchHelper.GetRefNameForAzure(destination)));
    }

    private static PullRequestStatus ConvertStatus(string status)
    {
        return status switch
        {
            "active" => PullRequestStatus.Active,
            "completed" => PullRequestStatus.Completed,
            "abandoned" => PullRequestStatus.Abandoned,
            "all" => PullRequestStatus.All,
            _ => PullRequestStatus.NotSet
        };
    }

    /// <summary>
    /// Remove the branch ref header - 'refs/heads/'.
    /// </summary>
    /// <param name="refBranchName">Branch name with full ref from the Azure DevOps.</param>
    /// <returns>Returns the branch name without ref name.</returns>
    private static string RemoveRefName(string refBranchName)
    {
        return refBranchName.AsSpan(11).ToString();
    }

    #endregion
}