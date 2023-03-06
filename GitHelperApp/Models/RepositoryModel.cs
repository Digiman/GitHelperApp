namespace GitHelperApp.Models;

public sealed class RepositoryModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string RemoteUrl { get; set; }
}