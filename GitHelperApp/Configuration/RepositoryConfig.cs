namespace GitHelperApp.Configuration;

public sealed class RepositoryConfig
{
    public string Name { get; init; }
    public string Path { get; init; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
    public string TeamProject { get; init; } = "MSG";
}