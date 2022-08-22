using GitHelperApp.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp.Extensions;

public static class WorkItemExtensions
{
    public static WorkItemModel ToModel(this WorkItem workItem, string url)
    {
        return new WorkItemModel
        {
            Id = workItem.Id.ToString(),
            Url = url,
            Title = workItem.Fields["System.Title"].ToString(),
            Type = workItem.Fields["System.WorkItemType"].ToString(),
            AreaPath = workItem.Fields["System.AreaPath"].ToString(),
            IterationPath = workItem.Fields["System.IterationPath"].ToString(),
            State = workItem.Fields["System.State"].ToString()
        };
    }
}