using GitHelperApp.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Helpers;

/// <summary>
/// Simple helper to apply the filtering for Work Items based on settings.
/// </summary>
public static class WorkItemsHelper
{
    /// <summary>
    /// Filter work items with filter settings.
    /// </summary>
    /// <param name="wits">Work items to be filtered.</param>
    /// <param name="workItemFilterConfig">Configuration for filter to apply.</param>
    /// <returns>Returns work items after filtering.</returns>
    public static List<WorkItem> FilterWorkItems(List<WorkItem> wits, WorkItemFilterConfig workItemFilterConfig)
    {
        var result = wits
            .FilterByType(workItemFilterConfig.Types)
            .FilterByAreaPath(workItemFilterConfig.Areas)
            .FilterByIterationPath(workItemFilterConfig.Iterations);
        return result;
    }

    private static List<WorkItem> FilterByType(this List<WorkItem> wits, string[] types)
    {
        if (!types.Any()) return wits;

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.WorkItemType"))
            .Where(x => types.Contains(x.Fields["System.WorkItemType"])).ToList();

        return filtered;
    }

    private static List<WorkItem> FilterByAreaPath(this List<WorkItem> wits, string[] areas)
    {
        if (!areas.Any()) return wits;

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.AreaPath"))
            .Where(x => areas.Contains(x.Fields["System.AreaPath"])).ToList();

        return filtered;
    }

    private static List<WorkItem> FilterByIterationPath(this List<WorkItem> wits, string[] iterations)
    {
        if (!iterations.Any()) return wits;

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.IterationPath"))
            .Where(x => iterations.Contains(x.Fields["System.IterationPath"])).ToList();

        return filtered;
    }
}