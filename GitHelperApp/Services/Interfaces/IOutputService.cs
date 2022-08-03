using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

public interface IOutputService
{
    string InitializeOutputBatch();
    void OutputCompareResults(List<CompareResult> compareResults, string id, bool isPrintToConsole = true, bool isPrintToFile = false);
    void OutputFullResult(List<CompareResult> compareResults, List<PullRequestResult> prResults,
        string id, bool isPrintToConsole = false, bool isPrintToFile = false);
}