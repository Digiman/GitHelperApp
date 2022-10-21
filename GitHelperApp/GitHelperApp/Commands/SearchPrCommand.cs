using GitHelperApp.Commands.Interfaces;
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
    private string Status { get; }

    [Option(CommandOptionType.SingleValue, Description = "Count", ShortName = "c")]
    private int Count { get; set; }

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
            _logger.LogInformation($"Searching PR in the repositories (Status: {Status})");

            var (runId, directory) = _outputService.InitializeOutputBatch("SearchPr");

            // 2. Do processing to create the PRs
            _logger.LogInformation("Start searching the PRs...");

            if (Count == 0)
            {
                Count = 10; // set as internal default value (as draft for now)
            }

            var prResults = await _pullRequestService.SearchPullRequestsAsync(Status, Count);

            _logger.LogInformation($"PR processed: {prResults.Count}");

            // 3. Process the results - output
            _logger.LogInformation("Output search results");

            _outputService.OutputPullRequestsResult(prResults, runId, directory, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during searching the PR on Azure DevOps");

            throw;
        }
    }
}