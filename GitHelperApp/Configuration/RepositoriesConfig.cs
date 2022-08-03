namespace GitHelperApp.Configuration;

public sealed class RepositoriesConfig
{
    public List<RepositoryConfig> Repositories { get; set; }
    public string DefaultSourceBranch { get; set; }
    public string DefaultDestinationBranch { get; set; }
}