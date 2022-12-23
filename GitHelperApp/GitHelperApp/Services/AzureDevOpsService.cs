using System.Text.Json;
using GitHelperApp.Configuration;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;
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
    private readonly BuildHttpClient _buildHttpClient;
    private readonly PipelinesHttpClient _pipelinesHttpClient;

    public AzureDevOpsService(IOptions<AzureDevOpsConfig> config)
    {
        _config = config.Value;

        var vstsCollectionUrl = _config.CollectionUrl;

        var creds = new VssBasicCredential(string.Empty, _config.Token);
        var connection = new VssConnection(new Uri(vstsCollectionUrl), creds);

        _gitClient = connection.GetClient<GitHttpClient>();
        _workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();
        _buildHttpClient = connection.GetClient<BuildHttpClient>();

        // TODO: here is not the best solution to create instance of the client because used in other way than other clients(
        _pipelinesHttpClient = new PipelinesHttpClient(new Uri(_config.CollectionUrl), creds);
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

    public async Task<List<GitCommitRef>> GetLastCommitAsync(GitRepository repository, string branch)
    {
        var result = await _gitClient.GetCommitsAsync(repository.Id,
            new GitQueryCommitsCriteria { ItemVersion = new GitVersionDescriptor { Version = branch } }, top: 1);
        return result;
    }
    
    public async Task<Build> GetLastBuildDetailsAsync(string teamProject, int buildId)
    {
        var tmp = await _buildHttpClient.GetBuildsAsync(teamProject, new[] { buildId }, top: 1);
        
        return tmp.FirstOrDefault();
    }
    
    public async Task<Build> GetLastBuildDetailsAsync(string teamProject, int buildId, string branchName)
    {
        var azureBranch = GitBranchHelper.GetRefNameForAzure(branchName);
        var builds = await _buildHttpClient.GetBuildsAsync(teamProject, new[] { buildId }, branchName: azureBranch, top: 1);

        return builds.FirstOrDefault();
    }

    public async Task<List<Build>> GetBuildDetailsAsync(string teamProject, int buildId, int top = 10)
    {
        var builds = await _buildHttpClient.GetBuildsAsync(teamProject, new[] { buildId }, top: top);

        return builds;
    }
    
    public async Task<List<Build>> GetBuildDetailsAsync(string teamProject, int buildId, string branchName, int top = 10)
    {
        var azureBranch = GitBranchHelper.GetRefNameForAzure(branchName);
        var builds = await _buildHttpClient.GetBuildsAsync(teamProject, new[] { buildId }, branchName: azureBranch, top: top);

        return builds;
    }

    public async Task<Pipeline> GetPipelineAsyncAsync(string teamProject, int pipelineId)
    {
        var pipeline = await _pipelinesHttpClient.GetPipelineAsync(teamProject, pipelineId);

        // var runs = await _pipelinesHttpClient.ListRunsAsync(teamProject, pipelineId);
        // var run = await _pipelinesHttpClient.GetRunAsync(teamProject, pipelineId, runs.First().Id);
        
        return pipeline;
    }
    
    public async Task<Run> RunPipelineAsyncAsync(string teamProject, int pipelineId, PipelineRunSettings settings, bool isDryRun = false)
    {
        // TODO: this functionality is now working so it needed to wait for final API to be working :(
        
        // var repositories = new Dictionary<string, RepositoryResourceParameters>
        // {
        //     { "self", new RepositoryResourceParameters { RefName = GetRefName(settings.Branch) } }
        // };

        var tmp = await _pipelinesHttpClient.ListRunsAsync(teamProject, pipelineId);
        
        // var resourcesString = $@"{{'repositories': {{'self': {{'refName': '{GetRefName(settings.Branch)}'}}}}}}";
        var pipelineParameters = new RunPipelineParameters
        {
            PreviewRun = isDryRun,
            TemplateParameters = new Dictionary<string, string>
            {
                { "Environment", settings.Environment }
            }
        };
        var result = await _pipelinesHttpClient.RunPipelineAsync(pipelineParameters, teamProject, pipelineId);
        return result;
    }

    public string BuildPullRequestUrl(string teamProject, string repositoryName, int pullRequestId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(repositoryName)}/pullrequest/{pullRequestId}";
    }

    public string BuildWorkItemUrl(string teamProject, string workItemId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_workitems/edit/{workItemId}";
    }

    public string BuildRepositoryUrl(string teamProject, string repositoryName)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(repositoryName)}";
    }

    public string BuildPipelineUrl(string teamProject, int pipelineId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_build?definitionId={pipelineId}";
    }

    public string BuildBuildResultUrl(string teamProject, int buildId)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_build/results?buildId={buildId}&view=results";
    }

    public string BuildRepositoryCommitUrl(string teamProject, string repositoryName, string commit)
    {
        return $"{_config.CollectionUrl}/{Uri.EscapeDataString(teamProject)}/_git/{Uri.EscapeDataString(repositoryName)}/commit/{commit}";
    }

    private RunResourcesParameters DeserializeRunResourcesParameters(string value)
    {
        return JsonSerializer.Deserialize<RunResourcesParameters>(value);
    }
}