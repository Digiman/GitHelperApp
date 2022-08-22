using GitHelperApp.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Helpers;

// TODO: extend this functionality if needed and also some of the values can be moved to the configuration file!

public static class WorkItemsHelper
{
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
        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.WorkItemType"))
            .Where(x => types.Contains(x.Fields["System.WorkItemType"])).ToList();

        return filtered;
    }

    private static List<WorkItem> FilterByAreaPath(this List<WorkItem> wits, string[] areas)
    {
        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.AreaPath"))
            .Where(x => areas.Contains(x.Fields["System.AreaPath"])).ToList();

        return filtered;
    }

    private static List<WorkItem> FilterByIterationPath(this List<WorkItem> wits, string[] iterations)
    {
        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.IterationPath"))
            .Where(x => iterations.Contains(x.Fields["System.IterationPath"])).ToList();

        return filtered;
    }
}