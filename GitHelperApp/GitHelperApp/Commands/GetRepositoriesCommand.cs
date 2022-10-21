using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to work with list of repositories.
/// </summary>
public sealed class GetRepositoriesCommand : ICustomCommand
{
    private readonly ILogger<GetRepositoriesCommand> _logger;
    private readonly IRepositoryService _repositoryService;
    private readonly IOutputService _outputService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Team project", ShortName = "tp")]
    private string TeamProject { get; }
    
    public GetRepositoriesCommand(ILogger<GetRepositoriesCommand> logger, IRepositoryService repositoryService,
        IOutputService outputService)
    {
        _logger = logger;
        _repositoryService = repositoryService;
        _outputService = outputService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start searching for repositories Azure DevOps...");
            
            var (runId, directory) = _outputService.InitializeOutputBatch("GetRepositories");

            // 1. Do search
            List<RepositoryModel> results;
            if (string.IsNullOrEmpty(TeamProject))
            {
                results = await _repositoryService.GetRepositoriesListAsync();
            }
            else
            {
                results = await _repositoryService.GetRepositoriesListAsync(TeamProject);
            }

            _logger.LogInformation("Search for repositories is finished");
            
            _logger.LogInformation("Output results...");
            
            // 2. Process the results
            _outputService.OutputRepositoriesResults(results, runId, directory, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during searching for repositories on Azure DevOps");
            
            throw;
        }
    }
}