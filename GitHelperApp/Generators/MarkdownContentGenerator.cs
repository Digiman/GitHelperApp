using GitHelperApp.Configuration;
using GitHelperApp.Generators.Interfaces;
using GitHelperApp.Models;

namespace GitHelperApp.Generators;

/// <summary>
/// Generator to create the lines with string in Markdown format to print the results.
/// </summary>
public sealed class MarkdownContentGenerator : BaseContentGenerator, IContentGenerator
{
    public List<string> ProcessCompareResults(RepositoriesConfig repositoriesConfig, IReadOnlyCollection<CompareResult> results)
    {
        var lines = new List<string>();

        lines.Add($"**Repositories count: {repositoriesConfig.Repositories.Count}**");

        lines.Add(Environment.NewLine);

        // no changes
        var noChanges = results.Where(x => x.ChangesCount == 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"**Repositories without changes ({noChanges.Count}):**");
        var index = 1;
        foreach (var compareResult in noChanges)
        {
            lines.Add($"{index}. Repository: *'{compareResult.RepositoryName}'*. No any changes between **'{compareResult.SourceBranch}'** and **'{compareResult.DestinationBranch}'**");
            index++;
        }

        lines.Add(Environment.NewLine);

        // with any changes
        index = 1;
        var withChanges = results.Where(x => x.ChangesCount > 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"**Repositories with changes ({withChanges.Count}):**");
        foreach (var compareResult in withChanges)
        {
            lines.Add($"{index}. Repository: *'{compareResult.RepositoryName}'*. There are changes between **'{compareResult.SourceBranch}'** and **'{compareResult.DestinationBranch}'**. Changes count = {compareResult.ChangesCount}. Commits count: {compareResult.Commits.Count}.");
            index++;
        }

        lines.Add(Environment.NewLine);
        
        return lines;
    }

    public List<string> ProcessPrResults(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();

        // 1. Details for each PR.
        var index = 1;
        foreach (var pullRequestResult in prResults)
        {
            lines.Add($"**{index}: {pullRequestResult.RepositoryName}:**");
            lines.Add($"PR was created with Id [{pullRequestResult.PullRequestId}]({pullRequestResult.Url}). Work items count: {pullRequestResult.WorkItems.Count}.");
            lines.Add("Work items:");
            lines.AddRange(pullRequestResult.WorkItems.Select(workItemModel => $"* Work Item Id: [{workItemModel.Id}]({workItemModel.Url})"));

            lines.Add(Environment.NewLine);
            index++;
        }

        lines.Add(Environment.NewLine);

        // 2. PR summary
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            lines.Add($"**Pull Requests summary:**");
            lines.AddRange(prResults.Where(x => x.PullRequestId != 0).Select(pr => $"* [{pr.PullRequestId}]({pr.Url})"));

            lines.Add(Environment.NewLine);
        }

        // 3. Process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);

        lines.Add($"**Work items summary ({workItems.Count}):**");
        lines.AddRange(workItems.Select(workItemModel => $"* Work Item Id: [{workItemModel.Id}]({workItemModel.Url})"));

        return lines;
    }

    public List<string> ProcessPullRequestsSummary(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();
        lines.Add($"**Pull Requests summary:**");
        lines.AddRange(prResults.Where(x => x.PullRequestId != 0)
            .Select(pr => $"* PullRequestId: [{pr.PullRequestId}]({pr.Url})"));

        return lines;
    }

    public List<string> ProcessWorkItemsSummary(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();

        // process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);

        lines.Add($"**Work items summary ({workItems.Count}):**");
        lines.AddRange(workItems.Select(workItemModel => $"* Work Item Id: [{workItemModel.Id}]({workItemModel.Url})"));

        return lines;
    }

    public List<string> ProcessPullRequestSearchResult(List<PullRequestSearchResult> prResults)
    {
        var lines = new List<string>();
        lines.Add($"**Pull Requests:**");

        var groups = prResults.GroupBy(x => x.RepositoryName);
        foreach (var group in groups)
        {
            lines.Add($"* Repository name: {group.Key}. Pull Requests ({group.Count()}):");
            lines.AddRange(group.Where(x => x.PullRequestId != 0).Select(pr =>
                $"    * Title: *{pr.Title}*. PullRequestId: [{pr.PullRequestId}]({pr.Url}). From: *'{pr.SourceBranch}'*. To: *'{pr.DestinationBranch}'*."));
            lines.Add(Environment.NewLine);
        }

        return lines;
    }

    public List<string> ProcessWorkItemsSearchResults(List<WorkItemSearchResult> witResults)
    {
        var lines = new List<string>();
        
        lines.Add($"**Work items:**");

        var groups = witResults.GroupBy(x => x.RepositoryName);
        foreach (var group in groups)
        {
            var workItems = group.SelectMany(x => x.WorkItems);
            lines.Add($"* Repository name: {group.Key}. Work items ({workItems.Count()}):");
            lines.AddRange(workItems.Select(wit =>
                $"    * Title: *{wit.Title}*. State: *{wit.State}*. WorkItemId: [{wit.Id}]({wit.Url}). Area Path: *{wit.AreaPath}*. Iteration Path: *{wit.IterationPath}*."));
            lines.Add(Environment.NewLine);
        }

        return lines;
    }
}