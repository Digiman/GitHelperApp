using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service to work with the work items on Azure DevOps.
/// </summary>
public interface IWorkItemsService
{
    /// <summary>
    /// Search work items in repositories with the changes available.
    /// </summary>
    /// <param name="compareResults">Compare results from previous step.</param>
    /// <param name="isFilter">Is apply filter?</param>
    /// <returns>Returns the result with work items by repository.</returns>
    Task<List<WorkItemSearchResult>> SearchWorkItemsAsync(List<CompareResult> compareResults, bool isFilter = false);
}