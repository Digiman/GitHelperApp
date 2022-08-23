namespace GitHelperApp.Models;

/// <summary>
/// Extended model with the details after search existed PR.
/// </summary>
public sealed class PullRequestSearchResult : PullRequestResult
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
}