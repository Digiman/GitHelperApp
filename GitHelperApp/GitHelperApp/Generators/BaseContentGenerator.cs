using GitHelperApp.Models;

namespace GitHelperApp.Generators;

public abstract class BaseContentGenerator
{
    protected static List<WorkItemModel> ProcessUniqueWorkItems(List<PullRequestResult> prResults)
    {
        var workItems = prResults.SelectMany(x => x.WorkItems).Distinct().ToList();

        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var uniqueWorkItems = new List<WorkItemModel>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                uniqueWorkItems.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }

        return uniqueWorkItems;
    }
}