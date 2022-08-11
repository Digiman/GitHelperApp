using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to do compare for repositories between branches on Azure DevOps with APIs.
/// </summary>
public sealed class CompareAzureCommand: ICustomCommand
{
    private readonly ILogger<CompareAzureCommand> _logger;
    private readonly ICompareService _compareService;
    private readonly IOutputService _outputService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    public CompareAzureCommand(ILogger<CompareAzureCommand> logger, ICompareService compareService, IOutputService outputService)
    {
        _logger = logger;
        _compareService = compareService;
        _outputService = outputService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start local comparing for repositories on Azure DevOps...");
            
            var (runId, directory) = _outputService.InitializeOutputBatch("CompareAzure");

            // 1. Do compare for repositories and branches from configuration file locally with LibGit2Sharp
            var results = await _compareService.CompareAzureAsync();

            _logger.LogInformation("Compare was finished");
            
            _logger.LogInformation("Output compare results...");
            
            // 2. Process the results
            _outputService.OutputCompareResults(results, runId, directory, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing comparing for repositories on Azure DevOps");
            
            throw;
        }
    }
}