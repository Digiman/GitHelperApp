using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

public interface IPipelineService
{
    Task<List<PipelineResult>> RunPipelineAsync(PipelineRunSettings settings, bool isDryRun = false);
}