namespace GitHelperApp.Models;

public sealed class WorkItemModel
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string AreaPath { get; set; }
    public string IterationPath { get; set; }
    public string State { get; set; }
}