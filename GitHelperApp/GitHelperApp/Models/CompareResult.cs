namespace GitHelperApp.Models;

public sealed class CompareResult
{
    public int ChangesCount { get; set; }
    public string RepositoryName { get; set; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
    public List<string> Commits { get; set; }
}