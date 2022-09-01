namespace GitHelperApp.Models;

// TODO: draft and initial version so can be extended later for the new functionality to be supported

/// <summary>
/// Model with the data for summary table to be used on release page.
/// </summary>
public sealed class ReleaseSummaryModel
{
    public int Index { get; set; }
    public string RepositoryName { get; set; }
    public string RepositoryUrl { get; set; }
    public string PipelineUrl { get; set; }
    public int PullRequestId { get; set; }
    public string PullRequestUrl { get; set; }
    public int WorkItemsCount { get; set; }
}