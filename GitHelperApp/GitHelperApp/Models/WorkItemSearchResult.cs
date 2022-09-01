namespace GitHelperApp.Models;

/// <summary>
/// Model with the work items for repository.
/// </summary>
public sealed class WorkItemSearchResult
{
    /// <summary>
    /// Repository name.
    /// </summary>
    public string RepositoryName { get; set; }

    /// <summary>
    /// Work items related to the repository.
    /// </summary>
    public List<WorkItemModel> WorkItems { get; set; }
}