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

    /// <summary>
    /// Default branch to use as source - will be applied for all repo or can be overriden.
    /// </summary>
    public string DefaultSourceBranch { get; set; }

    /// <summary>
    /// Default branch to use as destination - will be applied for all repo or can be overriden.
    /// </summary>
    public string DefaultDestinationBranch { get; set; }

    /// <summary>
    /// Default team project for the repositories to use (because it possible to have projects from the different ones).
    /// </summary>
    public string DefaultTeamProject { get; set; }
}