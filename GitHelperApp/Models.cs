namespace GitHelperApp;

public sealed class RepositoriesConfig
{
    public List<RepositoryConfig> Repositories { get; set; }
    public string DefaultSourceBranch { get; set; }
    public string DefaultDestinationBranch { get; set; }
}

public sealed class RepositoryConfig
{
    public string Name { get; init; }
    public string Path { get; init; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
    public string TeamProject { get; init; } = "MSG";
}

public sealed class CompareResult
{
    public int ChangesCount { get; set; }
    public string RepositoryName { get; set; }
    public string SourceBranch { get; set; }
    public string DestinationBranch { get; set; }
    public List<string> Commits { get; set; }
}

public sealed class AzureDevOpsConfig
{
    public string Token { get; set; }
    public string CollectionUrl { get; set; }
    public string TeamProject { get; set; }
}

public sealed class PullRequestResult
{
    public int PullRequestId { get; set; }
    public string RepositoryName { get; set; }
    public string Url { get; set; }
    public List<WorkItemModel> WorkItems { get; set; }
}

public sealed class PullRequestModel
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public sealed class WorkItemModel
{
    public string Id { get; set; }
    public string Url { get; set; }
}