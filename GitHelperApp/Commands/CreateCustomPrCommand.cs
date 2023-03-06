using GitHelperApp.Commands.Interfaces;
using GitHelperApp.Services.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Commands;

/// <summary>
/// Command to create the custom PR (single) with specific settings.
/// </summary>
public sealed class CreateCustomPrCommand : ICustomCommand
{
    private readonly ILogger<CreateCustomPrCommand> _logger;
    private readonly IPullRequestService _pullRequestService;

    [Option(CommandOptionType.SingleValue, Description = "Dry run", ShortName = "d")]
    private bool DryRun { get; }

    public CreateCustomPrCommand(ILogger<CreateCustomPrCommand> logger, IPullRequestService pullRequestService)
    {
        _logger = logger;
        _pullRequestService = pullRequestService;
    }

    public async Task OnExecuteAsync(CommandLineApplication command, IConsole console)
    {
        try
        {
            _logger.LogInformation("Start creating PR for all repository changes...");

            var prResult = await _pullRequestService.CreatePullRequestAsync(DryRun);

            _logger.LogInformation($"PR processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured during processing the repositories and creating PR");

            throw;
        }
    }
}