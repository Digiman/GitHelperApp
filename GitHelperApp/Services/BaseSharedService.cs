using GitHelperApp.Configuration;
using GitHelperApp.Helpers;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Services;

public abstract class BaseSharedService
{
    protected static List<WorkItem> ProcessWorkItems(List<WorkItem> workItems, WorkItemFilterConfig workItemFilterConfig, bool isFilter)
    {
        // filter work items by type and area path to use only correct ones
        if (isFilter)
        {
            workItems = WorkItemsHelper.FilterWorkItems(workItems, workItemFilterConfig);
        }

        // we need to process here all the WI to exclude duplicates and etc.
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItem>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }

            return result;
        }

        return workItems;
    }
}