using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp;

/// <summary>
/// Helper logic to work with the Azure DevOps via client and REST API.
/// </summary>
public class AzureDevOpsHelper
{
    private readonly GitHttpClient _gitClient;
    private readonly AzureDevOpsConfig _config;
    private readonly WorkItemTrackingHttpClient _workItemTrackingHttpClient;
    
    public AzureDevOpsHelper(AzureDevOpsConfig config)
    {
        _config = config;
        
        var vstsCollectionUrl = config.CollectionUrl;
        var teamProject = config.TeamProject;

        var creds = new VssBasicCredential(string.Empty, config.Token);
        var connection = new VssConnection(new Uri(vstsCollectionUrl), creds); 

        _gitClient = connection.GetClient<GitHttpClient>();
        _workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();
    }

    public async Task<List<string>> GetRepositoriesAsync(string teamProject)
    {
        var result = new List<string>();

        var repos = await _gitClient.GetRepositoriesAsync(teamProject);
        foreach (var repo in repos)
        {
            result.Add(repo.Name);
        }

        return result;
    }
    
    public async Task<List<string>> GetRepositoriesAsync()
    {
        var result = new List<string>();

        var repos = await _gitClient.GetRepositoriesAsync(_config.TeamProject);
        foreach (var repo in repos)
        {
            result.Add(repo.Name);
        }

        return result;
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

    public async Task<List<GitCommitRef>> GetCommitsDetailsAsync(GitRepository repository, List<string> commits,
        string source, string destination)
    {
        var actualCommits = await _gitClient.GetCommitsBatchAsync(new GitQueryCommitsCriteria
        {
            CompareVersion = new GitVersionDescriptor { Version = source },
            ItemVersion = new GitVersionDescriptor { Version = destination },
            IncludeWorkItems = true,
            Top = 100
        }, repository.Id);
        
        // var actualCommits = await _gitClient.GetCommitsBatchAsync(new GitQueryCommitsCriteria
        // {
        //     IncludeWorkItems = true,
        //     HistoryMode = GitHistoryMode.FullHistory,
        //     FromCommitId = commits.First(),
        //     ToCommitId = commits.Last()
        // }, repository.Id);

        return actualCommits;
    }

    public async Task<List<WorkItem>> ProcessWorkItemsAsync(List<GitCommitRef> commits)
    {
        // var types = await _workItemTrackingHttpClient.GetWorkItemTypesAsync(_config.TeamProject);
        
        var workItemIds  = commits.SelectMany(x => x.WorkItems).Select(x => int.Parse(x.Id)).Distinct();
        var wits = await _workItemTrackingHttpClient.GetWorkItemsBatchAsync(new WorkItemBatchGetRequest
        {
            Ids = workItemIds
        });
        return wits;
    }
    
    public async Task<List<ResourceRef>> GetPullRequestDetailsAsync(GitRepository repository, int pullRequestId)
    {
        var result = await _gitClient.GetPullRequestWorkItemRefsAsync(repository.Id.ToString(), pullRequestId);
        return result;
    }
    
    public string BuildPullRequestUrl(string teamProject, string repositoryName, int pullRequestId)
    {
        return $"{_config.CollectionUrl}/{teamProject}/_git/{repositoryName}/pullrequest/{pullRequestId}";
    }
    
    public string BuildWorkItemUrl(string teamProject, string workItemId)
    {
        return $"{_config.CollectionUrl}/{teamProject}/_workitems/edit/{workItemId}";
    }
}