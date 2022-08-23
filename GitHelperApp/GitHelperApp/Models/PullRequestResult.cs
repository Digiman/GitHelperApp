namespace GitHelperApp.Models;

public class PullRequestResult
{
    public int PullRequestId { get; set; }
    public string RepositoryName { get; set; }
    public string Url { get; set; }
    public List<WorkItemModel> WorkItems { get; set; }
    public bool IsNew { get; set; }
}