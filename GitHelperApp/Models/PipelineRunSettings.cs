namespace GitHelperApp.Models;

/// <summary>
/// Settings to run the pipeline from specific branch to the specific environment.
/// </summary>
public sealed class PipelineRunSettings
{
    public string Branch { get; set; }
    public string Environment { get; set; }
}