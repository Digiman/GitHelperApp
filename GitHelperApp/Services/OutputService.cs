using GitHelperApp.Configuration;
using GitHelperApp.Generators.Interfaces;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitHelperApp.Services;

/// <summary>
/// Service to do some processing for results to output to console and file.
/// </summary>
public sealed class OutputService : IOutputService
{
    private readonly ILogger<OutputService> _logger;
    private readonly IContentGeneratorFactory _contentGeneratorFactory;
    private readonly IFileNameGenerator _fileNameGenerator;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly AppConfig _appConfig;

    public OutputService(ILogger<OutputService> logger, IContentGeneratorFactory contentGeneratorFactory,
        IFileNameGenerator fileNameGenerator, IOptions<RepositoriesConfig> repositoriesConfig,
        IOptions<AppConfig> appConfig)
    {
        _logger = logger;
        _contentGeneratorFactory = contentGeneratorFactory;
        _fileNameGenerator = fileNameGenerator;
        _repositoriesConfig = repositoriesConfig.Value;
        _appConfig = appConfig.Value;
    }

    public (string runId, string directory) InitializeOutputBatch(string commandName)
    {
        _logger.LogInformation("Initializing the batch...");
        
        var runId = Guid.NewGuid().ToString("N");
        
        var directoryName = BuildDirectoryName(commandName);
        
        if (!Directory.Exists(_appConfig.ToString()))
        {
            Directory.CreateDirectory(_appConfig.OutputDirectory);
        }

        var batchDirectory = Path.Combine(_appConfig.OutputDirectory, directoryName);
        if (!Directory.Exists(batchDirectory))
        {
            Directory.CreateDirectory(batchDirectory);
        }

        return (runId, directoryName);
    }
    
    public void OutputCompareResults(List<CompareResult> compareResults, string runId, string directory, bool isPrintToConsole = true, bool isPrintToFile = false)
    {
        var contentGenerator = _contentGeneratorFactory.GetContentGenerator(_appConfig.OutputFormat);
        var lines = contentGenerator.ProcessCompareResults(_repositoriesConfig, compareResults);

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, _fileNameGenerator.CreateFilenameForCompareResults(directory, runId));
        }
    }
    
    public void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults, 
        string runId, string directory, bool isPrintToConsole = false, bool isPrintToFile = false)
    {
        var contentGenerator = _contentGeneratorFactory.GetContentGenerator(_appConfig.OutputFormat);
        
        // 1. File with the full results
        // 1.1. Process compare result.
        var lines = contentGenerator.ProcessCompareResults(_repositoriesConfig, compareResults);

        // 1. 2. Process PR result.
        lines.AddRange(contentGenerator.ProcessPrResults(prResults));
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, _fileNameGenerator.CreateFilenameForFullResults(directory, runId));
        }
        
        // 2. Another separate files
        // 2.1. output only PRs to separate file
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            ProcessPrsResult(prResults, runId, directory, isPrintToConsole, isPrintToFile);
        }

        // 2.2. output work items only to separate file
        ProcessWorkItemsResult(prResults, runId, directory, isPrintToConsole, isPrintToFile);
    }

    public void OutputPullRequestsResult(List<PullRequestSearchResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            ProcessPrsResult(prResults, runId, directory, isPrintToConsole, isPrintToFile);
        }    
    }

    #region Helpers.
    
    private static string BuildDirectoryName(string commandName)
    {
        return $"{commandName}-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}";
    }
    
    private void ProcessPrsResult(List<PullRequestResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var contentGenerator = _contentGeneratorFactory.GetContentGenerator(_appConfig.OutputFormat);
        var lines = contentGenerator.ProcessPullRequestsSummary(prResults);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, _fileNameGenerator.CreateFileNameForPrIds(directory, runId));
        }
    }
    
    private void ProcessWorkItemsResult(List<PullRequestResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var contentGenerator = _contentGeneratorFactory.GetContentGenerator(_appConfig.OutputFormat);
        var lines = contentGenerator.ProcessWorkItemsSummary(prResults);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, _fileNameGenerator.CreateFileNameForWorkItems(directory, runId));
        }
    }

    private void ProcessPrsResult(List<PullRequestSearchResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var contentGenerator = _contentGeneratorFactory.GetContentGenerator(_appConfig.OutputFormat);
        var lines = contentGenerator.ProcessPullRequestSearchResult(prResults);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, _fileNameGenerator.CreateFileNameForPrIds(directory, runId));
        }
    }

    #endregion
}