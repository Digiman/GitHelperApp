namespace GitHelperApp.Configuration;

public sealed class WorkItemFilterConfig
{
    public string[] Types { get; set; }
    public string[] Areas { get; set; }
    public string[] Iterations { get; set; }
}