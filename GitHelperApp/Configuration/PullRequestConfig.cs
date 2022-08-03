namespace GitHelperApp.Configuration;

public sealed class PullRequestConfig
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsDraft { get; set; }
    public string Author { get; set; }
}