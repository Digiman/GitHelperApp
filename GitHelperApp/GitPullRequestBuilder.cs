using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp;

public sealed class GitPullRequestBuilder
{
    private readonly GitPullRequest _pullRequest;

    public GitPullRequestBuilder(string title, string description,
        string sourceBranch, string destinationBranch)
    {
        _pullRequest = new GitPullRequest
        {
            Title = title,
            Description = description,
            TargetRefName = GetRefName(destinationBranch),
            SourceRefName = GetRefName(sourceBranch)
        };
    }

    public GitPullRequestBuilder WithAuthorAndrey()
    {
        _pullRequest.CreatedBy = new IdentityRef
        {
            Id = "b9f8187f-14e4-486b-95ab-a063c9c26d51",
            DisplayName = "Andrey Kukharenko",
            UniqueName = @"GatewayDevOpsDa\akukharenko"
        };
        
        return this;
    }

    
    public GitPullRequestBuilder WithAuthor(string userName)
    {
        _pullRequest.CreatedBy = new IdentityRef
        {
            Id = Constants.Users[userName]
        };
        
        return this;
    }
    public GitPullRequestBuilder AsDraft()
    {
        _pullRequest.IsDraft = true;
        return this;
    }

    public GitPullRequestBuilder WthDefaultReviewers()
    {
        _pullRequest.Reviewers = new[]
        {
            new IdentityRefWithVote
            {
                Id = "a6cc9965-0b11-4c04-980a-055c98314119" // Admiral
            }
        };
        
        return this;
    }
    
    public GitPullRequestBuilder WthReviewers(List<string> userName)
    {
        _pullRequest.Reviewers = (IdentityRefWithVote[])userName.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        });
        
        return this;
    }

    public GitPullRequestBuilder WithWorkItems(List<WorkItem> workItems)
    {
        _pullRequest.WorkItemRefs = workItems.Select(x => new ResourceRef
        {
            Id = x.Id.ToString(),
            Url = x.Url
        }).ToArray();
        
        return this;
    }
    
    public static string GetRefName(string branchName) => $"refs/heads/{branchName}";

    public GitPullRequest Build()
    {
        return _pullRequest;
    }
}