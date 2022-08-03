namespace GitHelperApp.Configuration;

public sealed class AzureDevOpsConfig
{
    public string Token { get; set; }
    public string CollectionUrl { get; set; }
    public string TeamProject { get; set; }
}