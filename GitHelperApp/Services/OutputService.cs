using GitHelperApp.Configuration;
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
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly AppConfig _appConfig;

    public OutputService(ILogger<OutputService> logger, IOptions<RepositoriesConfig> repositoriesConfig, IOptions<AppConfig> appConfig)
    {
        _logger = logger;
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
        var lines = ProcessCompareResults(_repositoriesConfig, compareResults);

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFilenameForCompareResults(_appConfig.OutputDirectory, directory, runId));
        }
    }
    
    public void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults, 
        string runId, string directory, bool isPrintToConsole = false, bool isPrintToFile = false)
    {
        // 1. Process compare result.
        var lines = ProcessCompareResults(_repositoriesConfig, compareResults);

        // 2. Process PR result.
        lines.AddRange(ProcessPrResults(prResults));

        // output only PRs to separate file
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            ProcessPrsResult(prResults, runId, directory, isPrintToConsole, isPrintToFile);
        }

        // output work items only to separate file
        ProcessWorkItemsResult(prResults, runId, directory, isPrintToConsole, isPrintToFile);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFilenameForFullResults(_appConfig.OutputDirectory, directory, runId));
        }
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
    
    private static List<string> ProcessCompareResults(RepositoriesConfig repositoriesConfig, IReadOnlyCollection<CompareResult> results)
    {
        var lines = new List<string>();

        lines.Add($"Repositories count: {repositoriesConfig.Repositories.Count}");

        lines.Add(Environment.NewLine);

        // no changes
        var noChanges = results.Where(x => x.ChangesCount == 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"Repositories without changes ({noChanges.Count}):");
        var index = 1;
        foreach (var compareResult in noChanges)
        {
            lines.Add($"{index}: Repository: '{compareResult.RepositoryName}'. No any changes between '{compareResult.SourceBranch}' and '{compareResult.DestinationBranch}'");
            index++;
        }

        lines.Add(Environment.NewLine);

        // with any changes
        index = 1;
        var withChanges = results.Where(x => x.ChangesCount > 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"Repositories with changes ({withChanges.Count}):");
        foreach (var compareResult in withChanges)
        {
            lines.Add($"{index}: Repository: '{compareResult.RepositoryName}'. There are changes between '{compareResult.SourceBranch}' and '{compareResult.DestinationBranch}'. Changes count = {compareResult.ChangesCount}. Commits count: {compareResult.Commits.Count}.");
            index++;
        }

        return lines;
    }
    
    private static IEnumerable<string> ProcessPrResults(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();
        
        // 1. Details for each PR.
        var index = 1;
        foreach (var pullRequestResult in prResults)
        {
            lines.Add($"{index}: {pullRequestResult.RepositoryName}:");
            lines.Add($"PR was created with Id {pullRequestResult.PullRequestId}. Url: {pullRequestResult.Url}. Work items count: {pullRequestResult.WorkItems.Count}.");
            lines.Add("Work items:");
            lines.AddRange(pullRequestResult.WorkItems.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));

            lines.Add(Environment.NewLine);
            index++;
        }
        
        lines.Add(Environment.NewLine);
        
        // 2. PR summary
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            lines.Add($"Pull Requests summary:");
            lines.AddRange(prResults.Where(x => x.PullRequestId != 0).Select(pr => $"\t{pr.Url}"));

            lines.Add(Environment.NewLine);
        }

        // 3. Process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);
        
        lines.Add($"Work items summary ({workItems.Count}):");
        lines.AddRange(workItems.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));
        
        return lines;
    }
    
    private void ProcessPrsResult(List<PullRequestResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var lines = new List<string>();
        lines.Add($"Pull Requests summary:");
        lines.AddRange(prResults.Where(x => x.PullRequestId != 0).Select(pr => $"\tPullRequestId: {pr.PullRequestId}. Url: {pr.Url}"));
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFileNameForPrIds(_appConfig.OutputDirectory, directory, runId));
        }
    }
    
    private void ProcessWorkItemsResult(List<PullRequestResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var lines = new List<string>();
        
        // process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);
        
        lines.Add($"Work items summary ({workItems.Count}):");
        lines.AddRange(workItems.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFileNameForWorkItems(_appConfig.OutputDirectory, directory, runId));
        }
    }

    private static List<WorkItemModel> ProcessUniqueWorkItems(List<PullRequestResult> prResults)
    {
        var workItems = prResults.SelectMany(x => x.WorkItems).Distinct().ToList();
        
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var uniqueWorkItems = new List<WorkItemModel>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                uniqueWorkItems.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }

        return uniqueWorkItems;
    }
    
    private void ProcessPrsResult(List<PullRequestSearchResult> prResults, string runId, string directory, bool isPrintToConsole, bool isPrintToFile)
    {
        var lines = new List<string>();
        lines.Add($"Pull Requests:");

        var groups = prResults.GroupBy(x => x.RepositoryName);
        foreach (var group in groups)
        {
            lines.Add($"  Repository name: {group.Key}. Pull Requests ({group.Count()}):");
            lines.AddRange(group.Where(x => x.PullRequestId != 0).Select(pr => $"    PullRequestId: {pr.PullRequestId}. Title: {pr.Title}. Url: {pr.Url}"));
            lines.Add(Environment.NewLine);
        }
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFileNameForPrIds(_appConfig.OutputDirectory, directory, runId));
        }
    }

    private static string CreateFilenameForCompareResults(string outputPath, string directory, string runId) => Path.Combine(outputPath, directory, $"Result-{runId}.txt");

    private static string CreateFilenameForFullResults(string outputPath, string directory, string runId) => Path.Combine(outputPath, directory, $"ResultFull-{runId}.txt");

    private static string CreateFileNameForPrIds(string outputPath, string directory, string runId) => Path.Combine(outputPath, directory, $"Prs-{runId}.txt");

    private static string CreateFileNameForWorkItems(string outputPath, string directory, string runId) => Path.Combine(outputPath, directory, $"Wit-{runId}.txt");

    #endregion
}