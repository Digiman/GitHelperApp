﻿using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to run the process to create PRs for all repositories with changes.
/// </summary>
public sealed class CreatePrCommand : ICustomCommand
{
    private readonly ILogger<CreatePrCommand> _logger;
    private readonly ICompareService _compareService;
    private readonly IOutputService _outputService;
    private readonly IPullRequestService _pullRequestService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Dry run", ShortName = "d")]
    private bool DryRun { get; }
    
    public CreatePrCommand(ILogger<CreatePrCommand> logger, ICompareService compareService, IOutputService outputService, IPullRequestService pullRequestService)
    {
        _logger = logger;
        _compareService = compareService;
        _outputService = outputService;
        _pullRequestService = pullRequestService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start local comparing for repositories...");

            var runId = _outputService.InitializeOutputBatch();

            // 1. Do compare for repositories and branches from configuration file locally with LibGit2Sharp
            var results = _compareService.CompareLocal();

            _logger.LogInformation("Compare was finished");
            
            // 2. Do processing to create the PRs
            _logger.LogInformation("Start creating PR for all repository changes...");
            
            var prResults = await _pullRequestService.CreatePullRequestsAsync(results, DryRun);
            
            _logger.LogInformation($"PR processed: {prResults.Count}");
            
            // 3. Process the results - output
            _logger.LogInformation("Output compare results...");
            
            _outputService.OutputFullResult(results, prResults, runId, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing the repositories and creating PRs (compare and create PR on Azure DevOps)");

            throw;
        }
    }
}