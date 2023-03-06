using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service to work with builds and pipelines.
/// </summary>
public interface IPipelineService
{
    Task<List<PipelineResult>> RunPipelineAsync(PipelineRunSettings settings, bool isDryRun = false);
    
    /// <summary>
    /// Get build details - runs, commits, etc.
    /// </summary>
    /// <param name="settings">Settings for builds.</param>
    /// <returns>Returns list with build details for each repository.</returns>
    Task<List<BuildDetails>> GetBuildDetailsAsync(PipelineRunSettings settings);
}