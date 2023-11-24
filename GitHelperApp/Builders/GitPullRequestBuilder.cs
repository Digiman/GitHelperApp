using GitHelperApp.Helpers;
using Microsoft.TeamFoundation.Core.WebApi;
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

    public GitPullRequestBuilder(string title, string description, string sourceBranch, string destinationBranch)
    {
        _pullRequest = new GitPullRequest
        {
            Title = title,
            Description = description,
            TargetRefName = GitBranchHelper.GetRefNameForAzure(destinationBranch),
            SourceRefName = GitBranchHelper.GetRefNameForAzure(sourceBranch)
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
            "Hydrogen", "Oxygen"
        };
        _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        }).ToArray();

        return this;
    }

    public GitPullRequestBuilder WthDefaultReviewersForMain()
    {
        var userNames = new[] { "Admiral" };
        _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
        {
            Id = Constants.Users[x]
        }).ToArray();

        return this;
    }

    public GitPullRequestBuilder WthReviewers(List<string> userNames)
    {
        if (userNames.Any())
        {
            _pullRequest.Reviewers = userNames.Select(x => new IdentityRefWithVote
            {
                Id = Constants.Users[x]
            }).ToArray();
        }

        return this;
    }

    public GitPullRequestBuilder WithWorkItems(List<WorkItem> workItems)
    {
        if (workItems.Any())
        {
            _pullRequest.WorkItemRefs = workItems.Select(x => new ResourceRef
            {
                Id = x.Id.ToString(),
                Url = x.Url
            }).ToArray();
        }

        return this;
    }

    public GitPullRequestBuilder WithTags(string[] tags)
    {
        if (tags.Any())
        {
            _pullRequest.Labels = tags.Select(tag => new WebApiTagDefinition
            {
                Name = tag
            }).ToArray();
        }

        return this;
    }
    
    /// <summary>
    /// Configure PR to set as auto-complete with default merge strategy.
    /// </summary>
    /// <param name="userName">Username to use as committer for auto completed PR.</param>
    /// <returns>Returns builder instance.</returns>
    public GitPullRequestBuilder WithAutocomplete(string userName)
    {
        _pullRequest.CompletionOptions = new GitPullRequestCompletionOptions
        {
            MergeStrategy = GitPullRequestMergeStrategy.NoFastForward
        };
        _pullRequest.AutoCompleteSetBy = new IdentityRef
        {
            Id = Constants.Users[userName]
        }; 

        return this;
    }

    /// <summary>
    /// Return the final result with the pull request created.
    /// </summary>
    /// <returns>Returns the pull request to use.</returns>
    public GitPullRequest Build()
    {
        return _pullRequest;
    }
}