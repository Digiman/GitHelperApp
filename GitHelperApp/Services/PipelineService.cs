using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHelperApp.Services;

public sealed class PipelineService : IPipelineService
{
    private readonly ILogger<PipelineService> _logger;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly IAzureDevOpsService _azureDevOpsService;

    public PipelineService(ILogger<PipelineService> logger, IAzureDevOpsService azureDevOpsService,
        IOptions<RepositoriesConfig> repositoriesConfig)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _repositoriesConfig = repositoriesConfig.Value;
    }

    public async Task<List<PipelineResult>> RunPipelineAsync(PipelineRunSettings settings, bool isDryRun = false)
    {
        var result = new List<PipelineResult>();

        foreach (var repositoryConfig in _repositoriesConfig.Repositories)
        {
            _logger.LogInformation($"Running the pipeline for {repositoryConfig.Name}...");

            repositoryConfig.GetRepositoryConfig(_repositoriesConfig);

            var pipeline = await _azureDevOpsService.GetPipelineAsyncAsync(repositoryConfig.TeamProject, repositoryConfig.PipelineId);
            
            var run = await _azureDevOpsService.RunPipelineAsyncAsync(repositoryConfig.TeamProject, repositoryConfig.PipelineId, settings, isDryRun);
            
            result.Add(new PipelineResult
            {
                Id = run.Id,
                Name = run.Name,
                Url = run.Url
            });
        }

        return result;
    }
}