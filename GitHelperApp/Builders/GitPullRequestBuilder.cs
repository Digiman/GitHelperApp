using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp.Builders;

/// <summary>
/// Builder logic for Pull Request.
/// </summary>
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
        var userNames = new[]
        {
            "Admiral", "Haygood, Justin", "Brian Bober", // Matrix team
            "Ivan Grishkov", "Oleg Solonko", "Konstantin Bondarenko", "Stas Ivanousky" // Oxagile team
        };
        _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        }).ToArray();
        
        return this;
    }
    
    public GitPullRequestBuilder WthDefaultReviewersForMain()
    {
        var userNames = new[] { "Admiral", "Haygood, Justin", "Brian Bober" };
        _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        }).ToArray();
        
        return this;
    }
    
    public GitPullRequestBuilder WthReviewers(List<string> userNames)
    {
        _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        }).ToArray();
        
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