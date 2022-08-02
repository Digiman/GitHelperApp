// See https://aka.ms/new-console-template for more information

using GitHelperApp;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.SourceControl.WebApi;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// unique run id to use in file names
var runId = Guid.NewGuid().ToString("N");

// ----------------------------------------------------------------
// 1. Compare all repositories and get changes details.
var reposConfig = config.GetSection("RepositoryConfig").Get<RepositoriesConfig>();

var compareResults = AppHelper.DoCompare(reposConfig);

// print original compare results
AppHelper.OutputCompareResults(reposConfig, compareResults, runId, true, true);

// ----------------------------------------------------------------
// 2. Get and process details with Azure DevOps to crete needed PRs

// work with the Azure DevOps
var azureConfig = config.GetSection("AzureDevOps").Get<AzureDevOpsConfig>();

// simple model with the data for PR to be created - can be loaded from the file or from command options
var prModel = new PullRequestModel
{
    Title = "Sprint 8 Draft Release Automated",
    Description = "Automated PR from the tool"
};

var prResults = await AppHelper.CreatePullRequestsAsync(reposConfig, compareResults, azureConfig, prModel);

Console.WriteLine($"PR processed: {prResults.Count}");

// process the full result and print to file and console
AppHelper.ProcessFullResult(reposConfig, compareResults, prResults, runId, true, true);

// need to somehow to merge the results and generate final file with results

// ----------------------------------------------------------------

// await GetPullRequestsDetails();
//
// async Task GetPullRequestsDetails()
// {
//     var azureConfig = config.GetSection("AzureDevOps").Get<AzureDevOpsConfig>();
//     
//     var helper = new AzureDevOpsHelper(azureConfig);
//
//     // var repositoryName = "featureflag-service";
//     var repositoryName = "buyers-platform";
//     var repo = await helper.GetRepositoryByNameAsync(repositoryName, azureConfig.TeamProject);
//     var prs = await helper.GetPullRequestsAsync(repo, PullRequestStatus.Completed);
//
//     var createdBy = prs.Select(x => x.CreatedBy).ToList();
// }