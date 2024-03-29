﻿using GitHelperApp.Configuration;
using GitHelperApp.Helpers;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp.Services;

/// <summary>
/// The service to work with Azure DevOps via API to do some stuff from application.
/// </summary>
public sealed class AzureDevOpsService : IAzureDevOpsService
{
    private readonly GitHttpClient _gitClient;
    private readonly AzureDevOpsConfig _config;
    private readonly WorkItemTrackingHttpClient _workItemTrackingHttpClient;

    public AzureDevOpsService(IOptions<AzureDevOpsConfig> config)
    {
        _config = config.Value;

        var vstsCollectionUrl = _config.CollectionUrl;

        var creds = new VssBasicCredential(string.Empty, _config.Token);
        var connection = new VssConnection(new Uri(vstsCollectionUrl), creds);

        _gitClient = connection.GetClient<GitHttpClient>();
        _workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();
    }

    public async Task<GitRepository> GetRepositoryAsync(Guid repositoryId)
    {
        var repo = await _gitClient.GetRepositoryAsync(repositoryId);
        return repo;
    }

    public async Task<GitRepository> GetRepositoryByNameAsync(string name, string teamProject)
    {
        var repos = await _gitClient.GetRepositoriesAsync(teamProject);
        var repo = repos.SingleOrDefault(x => x.Name == name);
        return repo;
    }

    public async Task<GitRepository> GetRepositoryByNameAsync(string name)
    {
        var repos = await _gitClient.GetRepositoriesAsync(_config.TeamProject);
        var repo = repos.SingleOrDefault(x => x.Name == name);
        return repo;
    }

    public async Task<GitPullRequest> CreatePullRequestAsync(GitPullRequest pullRequest, GitRepository repository)
    {
        var result = await _gitClient.CreatePullRequestAsync(pullRequest, repository.Id);
        return result;
    }

    public async Task<List<GitPullRequest>> GetPullRequestsAsync(GitRepository repository, PullRequestStatus status)
    {
        var result = await _gitClient.GetPullRequestsAsync(repository.Id.ToString(),
            new GitPullRequestSearchCriteria { Status = status });
        return result;
    }

    public async Task<List<GitPullRequest>> GetPullRequestsWithOptionsAsync(GitRepository repository,
        PullRequestStatus status, int top, string source = null, string destination = null)
    {
        var criteria = new GitPullRequestSearchCriteria
        {
            Status = status
        };

        if (!string.IsNullOrEmpty(destination))
        {
            criteria.TargetRefName = GitBranchHelper.GetRefNameForAzure(destination);
        }

        if (!string.IsNullOrEmpty(source))
        {
            criteria.SourceRefName = GitBranchHelper.GetRefNameForAzure(source);
        }

        var result = await _gitClient.GetPullRequestsAsync(repository.Id.ToString(), criteria, top: top);
        return result;
    }

    public async Task<List<GitCommitRef>> GetCommitsDetailsAsync(GitRepository repository, string source, string destination)
    {
        // compare branches and search for commits
        var actualCommits = await _gitClient.GetCommitsBatchAsync(new GitQueryCommitsCriteria
        {
            CompareVersion = new GitVersionDescriptor { Version = source },
            ItemVersion = new GitVersionDescriptor { Version = destination },
            IncludeWorkItems = true,
            Top = 100
        }, repository.Id);
        return actualCommits;
    }

    public async Task<GitCommitDiffs> GetCommitsDiffsAsync(GitRepository repository, string teamProject, string source, string destination)
    {
        var result = await _gitClient.GetCommitDiffsAsync(teamProject, repository.Id,
            null, 100, null,
            new GitBaseVersionDescriptor
            {
                Version = source
            }, new GitTargetVersionDescriptor
            {
                Version = destination
            });
        return result;
    }

    public async Task<List<WorkItem>> GetWorkItemsAsync(List<GitCommitRef> commits)
    {
        var workItemIds = commits.SelectMany(x => x.WorkItems).Select(x => int.Parse(x.Id)).Distinct().ToList();
        if (!workItemIds.Any()) return Enumerable.Empty<WorkItem>().ToList();

        var wits = await _workItemTrackingHttpClient.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest
        {
            Ids = workItemIds
        });

        return wits;
    }

    public async Task<List<WorkItem>> GetWorkItemsAsync(List<ResourceRef> resourceRefs)
    {
        var workItemIds = resourceRefs.Select(x => int.Parse(x.Id)).Distinct().ToList();
        if (!workItemIds.Any()) return Enumerable.Empty<WorkItem>().ToList();

        var wits = await _workItemTrackingHttpClient.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest
        {
            Ids = workItemIds
        });

        return wits;
    }

    public async Task<List<WorkItem>> GetWorkItemsAsync(List<int> workItemIds)
    {
        if (!workItemIds.Any()) return Enumerable.Empty<WorkItem>().ToList();

        var wits = await _workItemTrackingHttpClient.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest
        {
            Ids = workItemIds
        });

        return wits;
    }

    public async Task<List<WorkItem>> GetWorkItemsLAsync(string teamProject, List<GitCommitRef> commits)
    {
        var workItemIds = commits.SelectMany(x => x.WorkItems).Select(x => int.Parse(x.Id)).Distinct().ToList();
        if (!workItemIds.Any()) return Enumerable.Empty<WorkItem>().ToList();

        var wits = await _workItemTrackingHttpClient.GetWorkItemsAsync(teamProject, workItemIds);

        return wits;
    }

    public async Task<List<ResourceRef>> GetPullRequestDetailsAsync(GitRepository repository, int pullRequestId)
    {
        var result = await _gitClient.GetPullRequestWorkItemRefsAsync(repository.Id.ToString(), pullRequestId);
        return result;
    }

    public async Task<WebApiTagDefinition> CreatePullRequestLabelAsync(GitRepository repository, string teamProject,
        string name, int pullRequestId)
    {
        return await _gitClient.CreatePullRequestLabelAsync(new WebApiCreateTagRequestData { Name = name },
            teamProject, repository.Id, pullRequestId);
    }

    public async Task<List<GitRepository>> GetRepositoriesListAsync(string teamProject)
    {
        return await _gitClient.GetRepositoriesAsync(teamProject);
    }

    public async Task<List<GitRepository>> GetRepositoriesListAsync()
    {
        return await _gitClient.GetRepositoriesAsync(_config.TeamProject);
    }

    public string BuildPullRequestUrl(string teamProject, string repositoryName, int pullRequestId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(repositoryName)}/pullrequest/{pullRequestId}";
    }

    public string BuildWorkItemUrl(string teamProject, string workItemId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_workitems/edit/{workItemId}";
    }

    public string BuildRepositoryUrl(string teamProject, string name)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(name)}";
    }

    public string BuildPipelineUrl(string teamProject, int pipelineId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_build?definitionId={pipelineId}";
    }
}