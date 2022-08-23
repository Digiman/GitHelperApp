using GitHelperApp.Configuration;
using GitHelperApp.Models;

namespace GitHelperApp.Generators.Interfaces;

public interface IContentGenerator
{
    List<string> ProcessCompareResults(RepositoriesConfig repositoriesConfig, IReadOnlyCollection<CompareResult> results);
    List<string> ProcessPrResults(List<PullRequestResult> prResults);
    List<string> ProcessPullRequestsSummary(List<PullRequestResult> prResults);
    List<string> ProcessWorkItemsSummary(List<PullRequestResult> prResults);
    List<string> ProcessPullRequestSearchResult(List<PullRequestSearchResult> prResults);
    List<string> ProcessWorkItemsSearchResults(List<WorkItemSearchResult> witResults);
    List<string> ProcessSummaryTableResult(List<ReleaseSummaryModel> aggregatedResult);
}