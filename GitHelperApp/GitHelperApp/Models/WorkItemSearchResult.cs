namespace GitHelperApp.Models;

public sealed class WorkItemSearchResult
{
    public string RepositoryName { get; set; }
    public List<WorkItemModel> WorkItems { get; set; }
}