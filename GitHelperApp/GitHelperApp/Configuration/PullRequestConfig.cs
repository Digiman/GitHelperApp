namespace GitHelperApp.Configuration;

/// <summary>
/// Model to use to create the Pr automatically.
/// </summary>
public sealed class PullRequestConfig
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsDraft { get; set; }
    public string Author { get; set; }
    public string[] Tags { get; set; }
}