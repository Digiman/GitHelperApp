namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration for repository.
/// </summary>
public sealed class RepositoryConfig
{
    /// <summary>
    /// Name of teh repository. Uses to search in Azure DevOps and all other identifications.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Local pah on he local machine to use in local compare.
    /// </summary>
    public string Path { get; init; }

    /// <summary>
    /// Source branch - to override the default.
    /// </summary>
    public string SourceBranch { get; set; }
    
    /// <summary>
    /// Destination branch - to override the default
    /// </summary>
    public string DestinationBranch { get; set; }

    /// <summary>
    /// Default project or specific used for repository.
    /// </summary>
    public string TeamProject { get; set; }
    
    /// <summary>
    /// Id of the build pipeline used to build and deploy applications from repository.
    /// </summary>
    public int PipelineId { get; init; }
}