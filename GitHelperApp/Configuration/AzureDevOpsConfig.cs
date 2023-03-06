namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration for Azure DevOps.
/// </summary>
public sealed class AzureDevOpsConfig
{
    /// <summary>
    /// Personal Access Token with access to code and work items and PRs.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Collection URL user for API client configuration.
    /// </summary>
    public string CollectionUrl { get; set; }

    /// <summary>
    /// Main and default team project.
    /// </summary>
    /// <remarks>For some of the repository it can be overriden.</remarks>
    public string TeamProject { get; set; }
}