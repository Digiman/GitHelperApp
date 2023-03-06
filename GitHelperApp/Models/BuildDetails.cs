namespace GitHelperApp.Models;

public sealed class BuildDetails
{
    public string RepositoryName { get; set; }
    public string RepositoryUrl { get; set; }
    public int BuildId { get; set; }
    public string RequestedFor { get; set; }
    public DateTime FinishTime { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; }
    public string SourceBranch { get; set; }
    public string SourceVersion { get; set; }
    public string SourceCommitLink { get; set; }
    
    public string Environment { get; set; }
    
    public string BuildLink { get; set; }

    public string CurrentCommit { get; set; }
    public string CurrentCommitLink { get; set; }
    public string Message { get; set; }
}