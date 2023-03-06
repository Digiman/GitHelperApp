using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

// TODO: implement this command later...

public sealed class RunPipelinesCommand : ICustomCommand
{
    private readonly ILogger<RunPipelinesCommand> _logger;
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
    
    [Option(CommandOptionType.SingleValue, Description = "Dry run", ShortName = "d")]
    private bool DryRun { get; }
    
    public RunPipelinesCommand(ILogger<RunPipelinesCommand> logger, IPipelineService pipelineService, IOutputService outputService)
    {
        _logger = logger;
        _pipelineService = pipelineService;
        _outputService = outputService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            var (runId, directory) = _outputService.InitializeOutputBatch("RunPipelines");
            
            // start pipelines
            _logger.LogInformation("Start pipelines for repositories...");

            var settings = new PipelineRunSettings
            {
                Branch = Branch,
                Environment = Environment
            };
            var runResults = await _pipelineService.RunPipelineAsync(settings, DryRun);
            
            _logger.LogInformation($"Pipelines processed: {runResults.Count}");

            // output the results
            _logger.LogInformation("Output pipelines run results...");

            // TODO: implement the logic to print results
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while running pipelines on Azure DevOps");

            throw;
        }
    }
}