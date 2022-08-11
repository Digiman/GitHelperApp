namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration for repositories to work within the application.
/// </summary>
public sealed class RepositoriesConfig
{
    /// <summary>
    /// Specific repositories configuration.
    /// </summary>
    public List<RepositoryConfig> Repositories { get; set; }

    public string DefaultSourceBranch { get; set; }
    public string DefaultDestinationBranch { get; set; }

    /// <summary>
    /// Default team project for the repositories to use (because it possible to have projects from the different ones).
    /// </summary>
    public string DefaultTeamProject { get; set; }
}