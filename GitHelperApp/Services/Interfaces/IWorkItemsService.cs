using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

public interface IWorkItemsService
{
    Task<List<WorkItemSearchResult>> SearchWorkItemsAsync(List<CompareResult> compareResults, bool isFilter = false);
}