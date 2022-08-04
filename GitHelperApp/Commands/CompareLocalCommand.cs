using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to do local compare between branches for local repositories with Lib2gitCsharp.
/// </summary>
public sealed class CompareLocalCommand : ICustomCommand
{
    private readonly ILogger<CompareLocalCommand> _logger;
    private readonly ICompareService _compareService;
    private readonly IOutputService _outputService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    public CompareLocalCommand(ILogger<CompareLocalCommand> logger, ICompareService compareService, IOutputService outputService)
    {
        _logger = logger;
        _compareService = compareService;
        _outputService = outputService;
    }

    public Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start local comparing for repositories...");
            
            var runId = _outputService.InitializeOutputBatch();

            // 1. Do compare for repositories and branches from configuration file locally with LibGit2Sharp
            var results = _compareService.CompareLocal();

            _logger.LogInformation("Compare was finished");
            
            _logger.LogInformation("Output compare results...");
            
            // 2. Process the results
            _outputService.OutputCompareResults(results, runId, IsPrintToConsole, IsPrintToFile);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing local comparing for repositories");
            
            throw;
        }
    }
}