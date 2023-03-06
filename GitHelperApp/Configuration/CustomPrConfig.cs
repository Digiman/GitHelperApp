namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration to use to create the single custom PR with the tool.
/// </summary>
public sealed class CustomPrConfig
{
    public string RepositoryName { get; set; }
    public string TeamProject { get; set; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
    public string Author { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsDraft { get; set; }
}