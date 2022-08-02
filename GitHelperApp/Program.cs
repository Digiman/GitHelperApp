// See https://aka.ms/new-console-template for more information

using GitHelperApp;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var reposConfig = config.GetSection("RepositoryConfig").Get<RepositoriesConfig>();

// DraftHelper.GetAllBranchesList(reposConfig.Repositories);
var compareResults = AppHelper.DoCompare(reposConfig, true, true);
// AppHelper.OutputRepositoriesList(reposConfig);

// work with the Azure DevOps
var azureConfig = config.GetSection("AzureDevOps").Get<AzureDevOpsConfig>();

// await AppHelper.TestAzureAsync(compareResults, azureConfig);
// await AppHelper.TestAzureAsync2(compareResults, azureConfig);

// simple model with the data for PR to be created - can be loaded from the file or from command options
var prModel = new PullRequestModel
{
    Title = "Sprint 8 Draft Release Automated",
    Description = "Automated PR from the tool"
};

var prResults = await AppHelper.CreatePullRequestsAsync(reposConfig, compareResults, azureConfig, prModel);

Console.WriteLine($"PR processed: {prResults.Count}");

// need to somehow to merge the results and generate final file with results


// ----------------------------------------------------------------



// ----------------------------------------------------------------
