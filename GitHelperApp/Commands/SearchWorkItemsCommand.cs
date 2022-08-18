using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to do the compare repositories and search for work items for the difference.
/// </summary>
public sealed class SearchWorkItemsCommand : ICustomCommand
{
    private readonly ILogger<SearchWorkItemsCommand> _logger;
    private readonly ICompareService _compareService;
    private readonly IOutputService _outputService;
    private readonly IWorkItemsService _workItemsService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Is apply filter or not?", ShortName = "f")]
    private bool IsFilter { get; }

    public SearchWorkItemsCommand(ILogger<SearchWorkItemsCommand> logger, ICompareService compareService,
        IOutputService outputService, IWorkItemsService workItemsService)
    {
        _logger = logger;
        _compareService = compareService;
        _outputService = outputService;
        _workItemsService = workItemsService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start local comparing for repositories...");

            var (runId, directory) = _outputService.InitializeOutputBatch("SearchWorkItems");

            // 1. Do compare for repositories and branches from configuration file locally with LibGit2Sharp
            var results = _compareService.CompareLocal();

            _logger.LogInformation("Compare was finished");
            
            // 2. Do processing to create the PRs
            _logger.LogInformation("Start searching the work items for changes...");
            
            var witResults = await _workItemsService.SearchWorkItemsAsync(results, IsFilter);
            
            // 3. Process the results - output
            _logger.LogInformation("Output work items search results...");
            
            _outputService.OutputWorkItemsSearchResult(results, witResults, runId, directory, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during searching the Work Items for the changes");

            throw;
        }
    }
}