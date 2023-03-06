namespace GitHelperApp.Configuration;

/// <summary>
/// Model to use to create the Pr automatically.
/// </summary>
public sealed class PullRequestConfig
{
    /// <summary>
    /// Title to use for the new Pull Request to be created.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description to add to the new Pull Request.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Is draft pull request?
    /// </summary>
    public bool IsDraft { get; set; }

    /// <summary>
    /// Author - the name as provided in the list in Constants.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// The list of tags to be added to the new Pull Request.
    /// </summary>
    public string[] Tags { get; set; }
    
    /// <summary>
    /// Create with autocomplete option.
    /// </summary>
    public bool Autocomplete { get; set; }
}