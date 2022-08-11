namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration for repository.
/// </summary>
public sealed class RepositoryConfig
{
    public string Name { get; init; }

    /// <summary>
    /// Local pah on he local machine to use in local compare.
    /// </summary>
    public string Path { get; init; }

    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }

    /// <summary>
    /// Default project or specific used for repository.
    /// </summary>
    public string TeamProject { get; set; }
}