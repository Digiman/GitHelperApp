using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Helpers;

// TODO: extend this functionality if needed and also some of the values can be moved to the configuration file!

public static class WorkItemsHelper
{
    public static List<WorkItem> FilterWorkItems(List<WorkItem> wits)
    {
        var result = wits.FilterByType().FilterByAreaPath().FilterByIterationPath();
        return result;
    }

    public static List<WorkItem> FilterByType(this List<WorkItem> wits)
    {
        var types = new string[] { "Story", "Task", "Bug" }; // maybe "Feature" can be added here later!?

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.WorkItemType"))
            .Where(x => types.Contains(x.Fields["System.WorkItemType"])).ToList();

        return filtered;
    }
    
    public static List<WorkItem> FilterByAreaPath(this List<WorkItem> wits)
    {
        var areas = new string[] { "MSG\\Admiral" }; 

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.AreaPath"))
            .Where(x => areas.Contains(x.Fields["System.AreaPath"])).ToList();

        return filtered;
    }
    
    public static List<WorkItem> FilterByIterationPath(this List<WorkItem> wits)
    {
        var iterations = new string[] { "MSG\\Sprint 09 (8-3 to 8-16)", "MSG\\Sprint 10 (8-17 to 8-30)" };

        var filtered = wits
            .Where(x => x.Fields.ContainsKey("System.IterationPath"))
            .Where(x => iterations.Contains(x.Fields["System.IterationPath"])).ToList();

        return filtered;
    }
}