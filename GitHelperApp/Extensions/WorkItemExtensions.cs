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
            Url = url
        };
    }
}