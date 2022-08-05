using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to search the PR in the repositories to get the list of them with details.
/// </summary>
public sealed class SearchPrCommand : ICustomCommand
{
    private readonly ILogger<SearchPrCommand> _logger;
    private readonly IPullRequestService _pullRequestService;
    private readonly IOutputService _outputService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "PR status", ShortName = "s")]
    private string Status  { get; }
    
    public SearchPrCommand(ILogger<SearchPrCommand> logger, IPullRequestService pullRequestService, IOutputService outputService)
    {
        _logger = logger;
        _pullRequestService = pullRequestService;
        _outputService = outputService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start local comparing for repositories...");

            var runId = _outputService.InitializeOutputBatch();

            // 2. Do processing to create the PRs
            _logger.LogInformation("Start creating PR for all repository changes...");
            
            var prResults = await _pullRequestService.SearchPullRequestsAsync(Status);
            
            _logger.LogInformation($"PR processed: {prResults.Count}");
            
            // 3. Process the results - output
            _logger.LogInformation("Output compare results...");
            
            _outputService.OutputPullRequestsResult(prResults, runId, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during searching the PR on Azure DevOps");

            throw;
        }
    }
}