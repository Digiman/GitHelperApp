using GitHelperApp.Builders;
using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace GitHelperApp;

public static class AppHelper
{
    /// <summary>
    /// Main logic to identify the changes in repositories (do it locally by ue remote branches).
    /// </summary>
    /// <param name="repositoriesConfig">Repositories config.</param>
    /// <returns>Returns compare result.</returns>
    public static List<CompareResult> DoCompare(RepositoriesConfig repositoriesConfig)
    {
        var compareResults = CompareBranches(repositoriesConfig);
        
        return compareResults;
    }

    public static void OutputCompareResults(RepositoriesConfig repositoriesConfig, List<CompareResult> compareResults,
        string id, bool isPrintToConsole = true, bool isPrintToFile = false)
    {
        var lines = ProcessCompareResults(repositoriesConfig, compareResults);

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, FileNameHelper.CreateFilenameForCompareResults(id));
        }
    }
    
    #region Main logic for the applicaion.

    private static List<CompareResult> CompareBranches(RepositoriesConfig repositoriesConfig)
    {
        var result = new List<CompareResult>(repositoriesConfig.Repositories.Count);
        foreach (var repositoryConfig in repositoriesConfig.Repositories)
        {
            if (!Directory.Exists(repositoryConfig.Path))
            {
                continue;
            }

            var source = !string.IsNullOrEmpty(repositoryConfig.SourceBranch)
                ? repositoryConfig.SourceBranch
                : repositoriesConfig.DefaultSourceBranch;
            var destination = !string.IsNullOrEmpty(repositoryConfig.DestinationBranch)
                ? repositoryConfig.DestinationBranch
                : repositoriesConfig.DefaultDestinationBranch;
            
            var (isChanges, count, commits) = GitHelper.CompareBranches(repositoryConfig.Path, GitHelper.GetRefName(source), GitHelper.GetRefName(destination));

            result.Add(new CompareResult
            {
                RepositoryName = repositoryConfig.Name,
                ChangesCount = count,
                SourceBranch = source,
                DestinationBranch = destination,
                Commits = commits
            });
        }

        return result;
    }

    private static List<string> ProcessCompareResults(RepositoriesConfig repositoriesConfig,
        IReadOnlyCollection<CompareResult> results)
    {
        var lines = new List<string>();

        lines.Add($"Repositories count: {repositoriesConfig.Repositories.Count}");

        lines.Add(Environment.NewLine);

        // no changes
        var noChanges = results.Where(x => x.ChangesCount == 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"Repositories without changes ({noChanges.Count}):");
        var index = 1;
        foreach (var compareResult in noChanges)
        {
            lines.Add(
                $"{index}: Repository: '{compareResult.RepositoryName}'. No any changes between '{compareResult.SourceBranch}' and '{compareResult.DestinationBranch}'");
            index++;
        }

        lines.Add(Environment.NewLine);

        // with any changes
        index = 1;
        var withChanges = results.Where(x => x.ChangesCount > 0).OrderBy(x => x.RepositoryName).ToList();
        lines.Add($"Repositories with changes ({withChanges.Count}):");
        foreach (var compareResult in withChanges)
        {
            lines.Add(
                $"{index}: Repository: '{compareResult.RepositoryName}'. There are changes between '{compareResult.SourceBranch}' and '{compareResult.DestinationBranch}'. Changes count = {compareResult.ChangesCount}. Commits count: {compareResult.Commits.Count}.");
            index++;
        }

        return lines;
    }
    
    #endregion
    
    #region Azure DevOps stuff.
    
    /// <summary>
    /// Create the PR for all repositories with changes exists.
    /// </summary>
    /// <param name="repositoriesConfig">Repositories config.</param>
    /// <param name="compareResults">Compare results from the first step.</param>
    /// <param name="azureConfig">Azure configuration settings.</param>
    /// <param name="pullRequestModel">Model with information for PR to be created.</param>
    /// <returns>Returns the PR result with details on each PR created/existed.</returns>
    public static async Task<List<PullRequestResult>> CreatePullRequestsAsync(RepositoriesConfig repositoriesConfig,
        List<CompareResult> compareResults, AzureDevOpsConfig azureConfig, PullRequestConfig pullRequestModel)
    {
        var result = new List<PullRequestResult>();

        var helper = new AzureDevOpsHelper(azureConfig);

        // foreach (var compareResult in compareResults.Where(x => x.RepositoryName == "featureflag-service"))
        foreach (var compareResult in compareResults.Where(x => x.ChangesCount > 0))
        {
            var repoInfo = repositoriesConfig.GetRepositoryConfig(compareResult.RepositoryName);
            
            var prResult = await CreatePullRequestAsync(helper, compareResult.RepositoryName, repoInfo.TeamProject,
                repoInfo.SourceBranch, repoInfo.DestinationBranch, pullRequestModel);
            
            result.Add(prResult);
        }

        return result;
    }

    private static async Task<PullRequestResult> CreatePullRequestAsync(AzureDevOpsHelper helper, string repositoryName,
        string teamProject, string sourceBranch, string destinationBranch, PullRequestConfig pullRequestModel)
    {
        var repo = await helper.GetRepositoryByNameAsync(repositoryName, teamProject);
        var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Active);

        var prTitle = pullRequestModel.Title;

        var actualPr = SearchForPrCreated(prs, prTitle, sourceBranch, destinationBranch);
        if (actualPr == null)
        {
            var gitCommits = await helper.GetCommitsDetailsAsync(repo, sourceBranch, destinationBranch);

            if (gitCommits.Count == 0)
            {
                Console.WriteLine("Something goes wrong and no changes found for PR!");
            }
            else
            {
                var workItems = await helper.GetWorkItemsAsync(gitCommits);

                workItems = ProcessWorkItems(workItems);

                var builder = new GitPullRequestBuilder(prTitle, pullRequestModel.Description, sourceBranch, destinationBranch);
                builder = builder
                    .WithAuthor("Andrey Kukharenko")
                    .WithWorkItems(workItems)
                    .WthDefaultReviewers();
                if (pullRequestModel.IsDraft)
                {
                    builder = builder.AsDraft();
                }

                var pr = builder.Build();
                // var prCreated = await helper.CreatePullRequestAsync(pr, repo);

                // Console.WriteLine($"PR was created with Id {prCreated.PullRequestId}. Url: {prCreated.Url}. Work items count: {workItems.Count}.");

                return new PullRequestResult
                {
                    // PullRequestId = prCreated.PullRequestId,
                    RepositoryName = repositoryName,
                    // Url = helper.BuildPullRequestUrl(teamProject, repositoryName, prCreated.PullRequestId),
                    WorkItems = workItems.Select(x => x.ToModel(helper.BuildWorkItemUrl(teamProject, x.Id.ToString()))).ToList()
                };
            }
        }

        var workItemsForActualPr = await helper.GetPullRequestDetailsAsync(repo, actualPr.PullRequestId);

        Console.WriteLine($"PR already created with Id {actualPr.PullRequestId}. Url: {actualPr.Url}. Work items count: {workItemsForActualPr.Count}.");

        return new PullRequestResult
        {
            PullRequestId = actualPr.PullRequestId,
            RepositoryName = repositoryName,
            Url = helper.BuildPullRequestUrl(teamProject, repositoryName, actualPr.PullRequestId),
            WorkItems = workItemsForActualPr.Select(x => x.ToModel(helper.BuildWorkItemUrl(teamProject, x.Id))).ToList()
        };
    }

    private static List<WorkItem> ProcessWorkItems(List<WorkItem> workItems)
    {
        // we need to process here all the WI to exclude duplicates and etc.
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItem>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }

        return workItems;
    }

    private static GitPullRequest SearchForPrCreated(List<GitPullRequest> pullRequests, string title, string source, string destination)
    {
        return pullRequests.FirstOrDefault(x =>
            x.Title == title && x.SourceRefName == GitPullRequestBuilder.GetRefName(source)
                             && x.TargetRefName == GitPullRequestBuilder.GetRefName(destination));
    }
    
    #endregion
    
    public static void ProcessFullResult(RepositoriesConfig repositoriesConfig, List<CompareResult> compareResults,
        List<PullRequestResult> prResults, string id, bool isPrintToConsole = false,
        bool isPrintToFile = false)
    {
        // 1. Process compare result.
        var lines = ProcessCompareResults(repositoriesConfig, compareResults);

        // 2. Process PR result.
        lines.AddRange(ProcessPrResults(prResults));

        // output only PR IDs to separate file
        ProcessPrsResult(prResults, id, isPrintToConsole, isPrintToFile);

        ProcessWorkItemsResult(prResults, id, isPrintToConsole, isPrintToFile);
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, FileNameHelper.CreateFilenameForFullResults(id));
        }
    }

    private static void ProcessWorkItemsResult(List<PullRequestResult> prResults, string id, bool isPrintToConsole, bool isPrintToFile)
    {
        var lines = new List<string>();
        
        // process the list of Work Items to have unique list at the end of log file
        var workItems = prResults.SelectMany(x => x.WorkItems).Distinct().ToList();
        
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItemModel>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }
        
        lines.Add($"Work items summary ({workItems.Count}):");
        lines.AddRange(result.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));

        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, FileNameHelper.CreateFileNameForWorkItems(id));
        }
    }

    private static void ProcessPrsResult(List<PullRequestResult> prResults, string id, bool isPrintToConsole, bool isPrintToFile)
    {
        var lines = prResults.Select(x => x.PullRequestId.ToString()).ToList();
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, FileNameHelper.CreateFileNameForPrIds(id));
        }
    }

    private static IEnumerable<string> ProcessPrResults(List<PullRequestResult> prResults)
    {
        var lines = new List<string>();
        
        var index = 1;
        foreach (var pullRequestResult in prResults)
        {
            lines.Add($"{index}: {pullRequestResult.RepositoryName}:");
            lines.Add($"PR was created with Id {pullRequestResult.PullRequestId}. Url: {pullRequestResult.Url}. Work items count: {pullRequestResult.WorkItems.Count}.");
            lines.Add("Work items:");
            lines.AddRange(pullRequestResult.WorkItems.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));

            lines.Add(Environment.NewLine);
            index++;
        }
        
        lines.Add(Environment.NewLine);
        
        // process the list of Work Items to have unique list at the end of log file
        var workItems = prResults.SelectMany(x => x.WorkItems).Distinct().ToList();
        
        var uniqueIds = workItems.Select(x => x.Id).Distinct().ToList();
        var result = new List<WorkItemModel>(uniqueIds.Count);
        if (uniqueIds.Count != workItems.Count)
        {
            foreach (var uniqueId in uniqueIds)
            {
                result.Add(workItems.FirstOrDefault(x => x.Id == uniqueId));
            }
        }
        lines.Add($"Work items summary ({result.Count}):");
        lines.AddRange(result.Select(workItemModel => $"\tWork Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));
        
        return lines;
    }
}