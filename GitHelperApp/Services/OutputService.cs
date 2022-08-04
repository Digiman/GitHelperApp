using GitHelperApp.Configuration;
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

    public string InitializeOutputBatch()
    {
        _logger.LogInformation("Initializing the batch...");
        
        var runId = Guid.NewGuid().ToString("N");

        if (!Directory.Exists(_appConfig.ToString()))
        {
            Directory.CreateDirectory(_appConfig.OutputDirectory);
        }

        var batchDirectory = Path.Combine(_appConfig.OutputDirectory, runId);
        if (!Directory.Exists(batchDirectory))
        {
            Directory.CreateDirectory(batchDirectory);
        }
        
        return runId;
    }
    
    public void OutputCompareResults(List<CompareResult> compareResults, string id, bool isPrintToConsole = true, bool isPrintToFile = false)
    {
        var lines = ProcessCompareResults(_repositoriesConfig, compareResults);

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFilenameForCompareResults(_appConfig.OutputDirectory, id));
        }
    }
    
    public void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults, 
        string id, bool isPrintToConsole = false, bool isPrintToFile = false)
    {
        // 1. Process compare result.
        var lines = ProcessCompareResults(_repositoriesConfig, compareResults);

        // 2. Process PR result.
        lines.AddRange(ProcessPrResults(prResults));

        // output only PRs to separate file
        ProcessPrsResult(prResults, id, isPrintToConsole, isPrintToFile);

        // output work items only to separate file
        ProcessWorkItemsResult(prResults, id, isPrintToConsole, isPrintToFile);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, CreateFilenameForFullResults(_appConfig.OutputDirectory, id));
        }
    }
    
    #region Helpers.
    
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
        lines.Add($"Pull Requests summary:");
        lines.AddRange(prResults.Where(x => x.PullRequestId != 0).Select(pr => $"\t{pr.Url}"));
        
        lines.Add(Environment.NewLine);
        
        // 3. Process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);
        
        lines.Add($"Work items summary ({workItems.Count}):");
        lines.AddRange(workItems.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));
        
        return lines;
    }
    
    private void ProcessPrsResult(List<PullRequestResult> prResults, string id, bool isPrintToConsole, bool isPrintToFile)
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
            OutputHelper.OutputResultToFile(lines, CreateFileNameForPrIds(_appConfig.OutputDirectory, id));
        }
    }
    
    private void ProcessWorkItemsResult(List<PullRequestResult> prResults, string id, bool isPrintToConsole, bool isPrintToFile)
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
            OutputHelper.OutputResultToFile(lines, CreateFileNameForWorkItems(_appConfig.OutputDirectory, id));
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

    private static string CreateFilenameForCompareResults(string outputPath, string id) => Path.Combine(outputPath, id, $"Result-{id}.txt");

    private static string CreateFilenameForFullResults(string outputPath, string id) => Path.Combine(outputPath, id, $"ResultFull-{id}.txt");

    private static string CreateFileNameForPrIds(string outputPath, string id) => Path.Combine(outputPath, id, $"Prs-{id}.txt");

    private static string CreateFileNameForWorkItems(string outputPath, string id) => Path.Combine(outputPath, id, $"Wit-{id}.txt");

    #endregion
}