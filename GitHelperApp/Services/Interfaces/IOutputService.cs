using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service to do some processing for results to output to console and file.
/// </summary>
public interface IOutputService
{
    (string runId, string directory) InitializeOutputBatch(string commandName);
    
    void OutputCompareResults(List<CompareResult> compareResults, string runId, 
        string directory, bool isPrintToConsole = true, bool isPrintToFile = false);

    void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults,
        string runId, string directory, bool isPrintToConsole = false, bool isPrintToFile = false);

    void OutputPullRequestsResult(List<PullRequestSearchResult> prResults, string runId, 
        string directory, bool isPrintToConsole, bool isPrintToFile);
}