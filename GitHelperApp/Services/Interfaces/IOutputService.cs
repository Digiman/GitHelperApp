using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service to do some processing for results to output to console and file.
/// </summary>
public interface IOutputService
{
    string InitializeOutputBatch();
    void OutputCompareResults(List<CompareResult> compareResults, string id, bool isPrintToConsole = true, bool isPrintToFile = false);
    void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults,
        string id, bool isPrintToConsole = false, bool isPrintToFile = false);
}