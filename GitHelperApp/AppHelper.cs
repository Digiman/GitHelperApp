using Microsoft.TeamFoundation.SourceControl.WebApi;

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

    // public static async Task TestAzureAsync(List<CompareResult> results, AzureDevOpsConfig azureConfig)
    // {
    //     var helper = new AzureDevOpsHelper(azureConfig);
    //
    //     await helper.GetRepositoriesAsync(azureConfig.TeamProject);
    //
    //     var repo = await helper.GetRepositoryByNameAsync("ratings");
    //     var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Active);
    //
    //     var builder = new GitPullRequestBuilder("Sprint 8 Draft Release Automated", "Automated PR from the tool",
    //         "dev", "release");
    //
    //     var pr = builder.WithAuthor("Andrey Kukharenko").AsDraft().Build();
    //     var prCreated = await helper.CreatePullRequestAsync(pr, repo);
    //
    //     Console.WriteLine($"PR was created with Id {prCreated.PullRequestId}");
    //
    //     // foreach (var compareResult in results)
    //     // {
    //     //     var repo = await helper.GetRepositoryByNameAsync(compareResult.RepositoryName);
    //     //     if (repo != null)
    //     //     {
    //     //         var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Active);
    //     //         Console.WriteLine($"Exists - Name: {repo.Name} Id: {repo.Id}");
    //     //     }
    //     //     else
    //     //     {
    //     //         Console.WriteLine($"Not found - Name: {compareResult.RepositoryName}");
    //     //         
    //     //         var repo2 = await helper.GetRepositoryByNameAsync(compareResult.RepositoryName, "Videa Git");
    //     //         Console.WriteLine($"Exists - Name: {repo2.Name} Id: {repo2.Id}");
    //     //     }
    //     // }
    // }
    //
    // public static async Task TestAzureAsync2(List<CompareResult> results, AzureDevOpsConfig azureConfig)
    // {
    //     var helper = new AzureDevOpsHelper(azureConfig);
    //
    //     await helper.GetRepositoriesAsync(azureConfig.TeamProject);
    //
    //     var repo = await helper.GetRepositoryByNameAsync("ratings");
    //     var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Active);
    //
    //     var actualPr = AppHelper.SearchForPrCreated(prs, "Sprint 8 Draft Release Automated", "dev", "release");
    //     if (actualPr == null)
    //     {
    //         var builder = new GitPullRequestBuilder("Sprint 8 Draft Release Automated",
    //             "Automated PR from the tool", "dev", "release");
    //         var pr = builder.WithAuthor("Andrey Kukharenko").AsDraft().Build();
    //         var prCreated = await helper.CreatePullRequestAsync(pr, repo);
    //
    //         Console.WriteLine($"PR was created with Id {prCreated.PullRequestId}. Url: {prCreated.Url}");
    //     }
    //     else
    //     {
    //         Console.WriteLine($"PR already with Id {actualPr.PullRequestId}. Url: {actualPr.Url}");
    //     }
    // }

    /// <summary>
    /// Create the PR for all repositories with changes exists.
    /// </summary>
    /// <param name="repositoriesConfig">Repositories config.</param>
    /// <param name="compareResults">Compare results from the first step.</param>
    /// <param name="azureConfig">Azure configuration settings.</param>
    /// <param name="pullRequestModel">Model with information for PR to be created.</param>
    /// <returns>Returns the PR result with details on each PR created/existed.</returns>
    public static async Task<List<PullRequestResult>> CreatePullRequestsAsync(RepositoriesConfig repositoriesConfig,
        List<CompareResult> compareResults, AzureDevOpsConfig azureConfig, PullRequestModel pullRequestModel)
    {
        var result = new List<PullRequestResult>();

        var helper = new AzureDevOpsHelper(azureConfig);

        // foreach (var compareResult in compareResults.Where(x => x.RepositoryName == "featureflag-service"))
        foreach (var compareResult in compareResults.Where(x => x.ChangesCount > 0))
        {
            var repoInfo = repositoriesConfig.GetRepositoryConfig(compareResult.RepositoryName);
            
            var prResult = await CreatePullRequestAsync(helper, compareResult.RepositoryName, repoInfo.TeamProject,
                repoInfo.SourceBranch, repoInfo.DestinationBranch, compareResult.Commits, pullRequestModel);
            result.Add(prResult);
        }

        return result;
    }

    private static async Task<PullRequestResult> CreatePullRequestAsync(AzureDevOpsHelper helper, string repositoryName,
        string teamProject, string sourceBranch, string destinationBranch, List<string> commits,
        PullRequestModel pullRequestModel)
    {
        var repo = await helper.GetRepositoryByNameAsync(repositoryName, teamProject);
        var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Active);

        var prTitle = pullRequestModel.Title;

        var actualPr = SearchForPrCreated(prs, prTitle, sourceBranch, destinationBranch);
        if (actualPr == null)
        {
            var gitCommits = await helper.GetCommitsDetailsAsync(repo, commits, sourceBranch, destinationBranch);

            var workItems = await helper.ProcessWorkItemsAsync(gitCommits);

            var builder = new GitPullRequestBuilder(prTitle, pullRequestModel.Description, sourceBranch, destinationBranch);
            var pr = builder
                .WithAuthor("Andrey Kukharenko")
                .WithWorkItems(workItems)
                .WthDefaultReviewers()
                .AsDraft()
                .Build();
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

    public static GitPullRequest SearchForPrCreated(List<GitPullRequest> pullRequests, string title, string source, string destination)
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
        
        if (isPrintToConsole)
        {
            OutputHelper.OutputResultToConsole(lines);
        }

        if (isPrintToFile)
        {
            OutputHelper.OutputResultToFile(lines, FileNameHelper.CreateFilenameForFullResults(id));

            var prIds = prResults.Select(x => x.PullRequestId.ToString()).ToList();
            OutputHelper.OutputResultToFile(prIds, FileNameHelper.CreateFileNameForPrIds(id));
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
            lines.AddRange(pullRequestResult.WorkItems.Select(workItemModel => $"Work Item Id: {workItemModel.Id}. Url: {workItemModel.Url}"));

            lines.Add(Environment.NewLine);

            index++;
        }

        return lines;
    }
}