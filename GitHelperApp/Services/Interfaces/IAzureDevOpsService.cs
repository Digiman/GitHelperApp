﻿using GitHelperApp.Models;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// The service to work with Azure DevOps via API to do some stuff from application.
/// </summary>
public interface IAzureDevOpsService
{
    Task<List<string>> GetRepositoriesAsync(string teamProject);
    Task<List<string>> GetRepositoriesAsync();
    Task<GitRepository> GetRepositoryAsync(Guid repositoryId);
    Task<GitRepository> GetRepositoryByNameAsync(string name, string teamProject);
    Task<GitRepository> GetRepositoryByNameAsync(string name);
    Task<GitPullRequest> CreatePullRequestAsync(GitPullRequest pullRequest, GitRepository repository);
    Task<List<GitPullRequest>> GetPullRequestsAsync(GitRepository repository, PullRequestStatus status);
    Task<List<GitPullRequest>> GetPullRequestsWithOptionsAsync(GitRepository repository, PullRequestStatus status, int top, string source = null, string destination = null);
    Task<List<GitCommitRef>> GetCommitsDetailsAsync(GitRepository repository, string source, string destination);
    Task<GitCommitDiffs> GetCommitsDiffsAsync(GitRepository repository, string teamProject, string source, string destination);
    Task<List<WorkItem>> GetWorkItemsAsync(List<GitCommitRef> commits);
    Task<List<ResourceRef>> GetPullRequestDetailsAsync(GitRepository repository, int pullRequestId);
    Task<WebApiTagDefinition> CreatePullRequestLabelAsync(GitRepository repository, string teamProject, string name, int pullRequestId);
    Task<Build> GetBuildDetailsAsync(string teamProject, int buildId);
    Task<Pipeline> GetPipelineAsyncAsync(string teamProject, int pipelineId);
    Task<Run> RunPipelineAsyncAsync(string teamProject, int pipelineId, PipelineRunSettings settings, bool isDryRun = false);
    string BuildPullRequestUrl(string teamProject, string repositoryName, int pullRequestId);
    string BuildWorkItemUrl(string teamProject, string workItemId);
}