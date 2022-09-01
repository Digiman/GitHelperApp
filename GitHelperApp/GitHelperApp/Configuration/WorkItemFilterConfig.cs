namespace GitHelperApp.Configuration;

/// <summary>
/// Configuration for use in work items filter in the application.
/// </summary>
public sealed class WorkItemFilterConfig
{
    /// <summary>
    /// Work items types (Bug, Story, Feature, Task, etc.);
    /// </summary>
    public string[] Types { get; set; }

    /// <summary>
    /// Areas in the Azure DevOps.
    /// </summary>
    public string[] Areas { get; set; }

    /// <summary>
    /// Iterations paths in Azure DevOps.
    /// </summary>
    public string[] Iterations { get; set; }
}