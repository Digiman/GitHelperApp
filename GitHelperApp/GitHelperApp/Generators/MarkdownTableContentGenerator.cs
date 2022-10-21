using GitHelperApp.Configuration;
using GitHelperApp.Generators.Interfaces;
using GitHelperApp.Generators.Markdown;
using GitHelperApp.Models;

namespace GitHelperApp.Generators;

/// <summary>
/// Generator to create the lines with string in Markdown format to print the results.
/// </summary>
public sealed class MarkdownTableContentGenerator : BaseContentGenerator, IContentGenerator
{
    public List<string> ProcessCompareResults(RepositoriesConfig repositoriesConfig, IReadOnlyCollection<CompareResult> results)
    {
        var lines = new List<string>();

        lines.Add("# Repositories details");
        lines.Add($"**Repositories count: {repositoriesConfig.Repositories.Count}**");

        lines.Add(Environment.NewLine);

        // no changes
        var noChanges = results.Where(x => x.ChangesCount == 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"## Repositories without changes ({noChanges.Count}):");
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
        lines.Add($"## Repositories with changes ({withChanges.Count}):");
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

        lines.Add("# Pull Request details");
        
        // 1. Details for each PR.
        var index = 1;
        foreach (var pullRequestResult in prResults)
        {
            lines.Add($"## {index} {pullRequestResult.RepositoryName}:");
            lines.Add($"PR was created with Id [{pullRequestResult.PullRequestId}]({pullRequestResult.Url}). Title: **{pullRequestResult.Title}**. Work items count: {pullRequestResult.WorkItems.Count}.");
            lines.Add("Work items:");
            lines.AddRange(CreateWorkItemsTable(pullRequestResult.WorkItems));

            lines.Add(Environment.NewLine);
            index++;
        }

        lines.Add(Environment.NewLine);

        // 2. PR summary
        if (prResults.Any(x => x.PullRequestId != 0))
        {
            lines.Add($"# Pull Requests summary");
            lines.AddRange(CreatePullRequestTable(prResults));

            lines.Add(Environment.NewLine);
        }

        // 3. Process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);

        lines.Add($"# Work items summary ({workItems.Count})");
        lines.AddRange(CreateWorkItemsTable(workItems));

        return lines;
    }

    public List<string> ProcessPullRequestsSummary(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();
        lines.Add($"**Pull Requests summary:**");
        lines.AddRange(CreatePullRequestList(prResults));

        return lines;
    }

    public List<string> ProcessWorkItemsSummary(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();

        // process the list of Work Items to have unique list at the end of log file
        var workItems = ProcessUniqueWorkItems(prResults);

        lines.Add($"**Work items summary ({workItems.Count}):**");
        lines.AddRange(CreateWorkItemsTable(workItems));

        return lines;
    }

    public List<string> ProcessPullRequestSearchResult(List<PullRequestSearchResult> prResults)
    {
        var lines = new List<string>();
        lines.Add($"**Pull Requests:**");

        var groups = prResults.GroupBy(x => x.RepositoryName);
        foreach (var group in groups)
        {
            lines.Add($"Repository name: **{group.Key}**. Pull Requests ({group.Count()}):");
            lines.AddRange(CreatePullRequestExtendedTable(group.ToList()));
            
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
            var workItems = group.SelectMany(x => x.WorkItems).ToList();
            lines.Add($"Repository name: **{group.Key}**. Work items ({workItems.Count()}):");
            lines.AddRange(CreateWorkItemsTable(workItems));

            lines.Add(Environment.NewLine);
        }

        return lines;
    }

    public List<string> ProcessSummaryTableResult(List<ReleaseSummaryModel> aggregatedResult)
    {
        var lines = new List<string>();

        lines.Add(Environment.NewLine);

        lines.Add("# Summary (for Release Notes page)");
        lines.AddRange(CreateSummaryTable(aggregatedResult));

        return lines;
    }

    public List<string> ProcessRepositoriesResult(List<RepositoryModel> repositoryModels)
    {
        var lines = new List<string>();

        lines.Add($"# Repositories list (Count = {repositoryModels.Count})");
        lines.AddRange(CreateRepositoriesTable(repositoryModels));
        
        return lines;
    }

    private IEnumerable<string> CreateRepositoriesTable(List<RepositoryModel> repositoryModels)
    {
        return repositoryModels
            .OrderBy(x => x.Name)
            .Select((x, index) => new
            {
                Index = index + 1,
                Title = $"[{x.Name}]({x.RemoteUrl})",
            })
            .ToMarkdownTable(new[] { "#", "Title" });
    }

    #region Helpers.

    private static IEnumerable<string> CreatePullRequestList(List<PullRequestResult> prResults)
    {
        return prResults.Where(x => x.PullRequestId != 0)
            .OrderBy(x => x.PullRequestId)
            .Select(pr => $"* [{pr.PullRequestId}]({pr.Url})");
    }

    private static IEnumerable<string> CreatePullRequestTable(List<PullRequestResult> prResults)
    {
        return prResults.Where(x => x.PullRequestId != 0)
            .OrderBy(x => x.PullRequestId)
            .Select(x => new
            {
                Url = $"[{x.PullRequestId}]({x.Url})",
                x.Title
            })
            .ToMarkdownTable(new[] { "Id", "Title" });
    }

    private static IEnumerable<string> CreatePullRequestExtendedTable(List<PullRequestSearchResult> prResults)
    {
        return prResults.Where(x => x.PullRequestId != 0)
            .OrderBy(x => x.PullRequestId)
            .Select(x => new
            {
                Url = $"[{x.PullRequestId}]({x.Url})",
                x.Title,
                From = x.SourceBranch,
                To = x.DestinationBranch
            })
            .ToMarkdownTable(new[] { "Id", "Title", "From", "To" });
    }

    private static IEnumerable<string> CreateWorkItemsTable(List<WorkItemModel> workItems)
    {
        return workItems
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Id)
            .ThenBy(x => x.Id).Select(x => new
            {
                Url = $"[{x.Id}]({x.Url})",
                x.Title,
                x.Type,
                x.State,
                x.AreaPath,
                x.IterationPath
            })
            .ToMarkdownTable(new[] { "Id", "Title", "Type", "State", "Area Path", "Iteration Path" });
    }
    
    private static IEnumerable<string> CreateSummaryTable(List<ReleaseSummaryModel> aggregatedResult)
    {
        return aggregatedResult
            .Select(x => new
            {
                x.Index,
                Reposiory = $"[{x.RepositoryName}]({x.RepositoryUrl})",
                Build = $"[Pipeline]({x.PipelineUrl})",
                PullRequest = x.PullRequestId == 0 ? "PR" : $"[PR {x.PullRequestId}]({x.PullRequestUrl})",
                x.WorkItemsCount
            })
            .ToMarkdownTable(new[] { "#", "Repository", "Build Pipeline", "PR", "Work Items Count" });
    }

    #endregion
}