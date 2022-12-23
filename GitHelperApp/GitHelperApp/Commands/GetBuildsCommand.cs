using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Special command to process the builds and get latest build runs.
/// </summary>
public sealed class GetBuildsCommand : ICustomCommand
{
    private readonly ILogger<GetBuildsCommand> _logger;
    private readonly IPipelineService _pipelineService;
    private readonly IOutputService _outputService;

    [Option(CommandOptionType.SingleValue, Description = "Print to console", ShortName = "pc")]
    private bool IsPrintToConsole { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Print to file", ShortName = "pf")]
    private bool IsPrintToFile { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Branch", ShortName = "b")]
    private string Branch { get; }
    
    [Option(CommandOptionType.SingleValue, Description = "Environment", ShortName = "e")]
    private string Environment { get; }

    public GetBuildsCommand(ILogger<GetBuildsCommand> logger, IPipelineService pipelineService, IOutputService outputService)
    {
        _logger = logger;
        _pipelineService = pipelineService;
        _outputService = outputService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            var (runId, directory) = _outputService.InitializeOutputBatch("GetBuilds");
            
            // start pipelines
            _logger.LogInformation("Start searching for builds...");

            var settings = new PipelineRunSettings
            {
                Branch = Branch,
                Environment = Environment
            };

            var buildResults = await _pipelineService.GetBuildDetailsAsync(settings);

            _logger.LogInformation($"Builds processed: {buildResults.Count}");

            // output the results
            _logger.LogInformation("Output build run results...");

            _outputService.OutputBuildDetailsResult(buildResults, runId, directory, IsPrintToConsole, IsPrintToFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while searching builds on Azure DevOps");

            throw;
        }
    }
}