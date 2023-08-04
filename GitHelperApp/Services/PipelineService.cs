using GitHelperApp.Configuration;
using GitHelperApp.Extensions;
using GitHelperApp.Helpers;
using GitHelperApp.Models;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;

namespace GitHelperApp.Services;

/// <summary>
/// Service to work with pipelines and other builds information.
/// </summary>
public sealed class PipelineService : IPipelineService
{
    private readonly ILogger<PipelineService> _logger;
    private readonly RepositoriesConfig _repositoriesConfig;
    private readonly IAzureDevOpsService _azureDevOpsService;

    public PipelineService(ILogger<PipelineService> logger, IAzureDevOpsService azureDevOpsService,
        IOptions<RepositoriesConfig> repositoriesConfig)
    {
        _logger = logger;
        _azureDevOpsService = azureDevOpsService;
        _repositoriesConfig = repositoriesConfig.Value;
    }

    // TODO: implement later this method later if needed
    /// <inheritdoc />
    public async Task<List<PipelineResult>> RunPipelineAsync(PipelineRunSettings settings, bool isDryRun = false)
    {
        var result = new List<PipelineResult>();

        foreach (var repositoryConfig in _repositoriesConfig.Repositories)
        {
            _logger.LogInformation($"Running the pipeline for {repositoryConfig.Name}...");

            repositoryConfig.GetRepositoryConfig(_repositoriesConfig);
            
            // var pipeline = await _azureDevOpsService.GetPipelineAsyncAsync(repositoryConfig.TeamProject, repositoryConfig.PipelineId);
            //
            // var run = await _azureDevOpsService.RunPipelineAsyncAsync(repositoryConfig.TeamProject, repositoryConfig.PipelineId, settings, isDryRun);
            //
            // result.Add(new PipelineResult
            // {
            //     Id = run.Id,
            //     Name = run.Name,
            //     Url = run.Url
            // });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<BuildDetails>> GetBuildDetailsAsync(PipelineRunSettings settings)
    {
        var tasks = new List<Task>(_repositoriesConfig.Repositories.Count);
        
        foreach (var repositoryConfig in _repositoriesConfig.Repositories)
        {
            _logger.LogInformation($"Get the build details for {repositoryConfig.Name}...");

            repositoryConfig.GetRepositoryConfig(_repositoriesConfig);
            
            tasks.Add(GetLastBuildDetailsForEnvironmentAsync(repositoryConfig.TeamProject,
                _repositoriesConfig.DefaultTeamProject, repositoryConfig.Name,
                repositoryConfig.PipelineId, settings.Branch, settings.Environment));
        }

        await Task.WhenAll(tasks);

        return tasks.Select(task => ((Task<BuildDetails>)task).Result).ToList();
    }
    
    #region Helpers and main logic.
    
    // private async Task GetPipelineDetailsAsync(string teamProject, int pipelineId, string branchName)
    // {
    //     var pipeline = await _azureDevOpsService.GetPipelineAsyncAsync(teamProject, pipelineId);
    //
    //     var build = await _azureDevOpsService.GetLastBuildDetailsAsync(teamProject, pipelineId);
    //
    //     var buildBranch = await _azureDevOpsService.GetLastBuildDetailsAsync(teamProject, pipelineId, branchName);
    // }
    
    private async Task<BuildDetails> GetLastBuildDetailsForEnvironmentAsync(string teamProject, string defaultTeamProject,
        string repositoryName, int pipelineId, string branchName, string environmentName)
    {
        // get repository details
        var repo = await _azureDevOpsService.GetRepositoryByNameAsync(repositoryName, teamProject);
        var commits = await _azureDevOpsService.GetLastCommitAsync(repo, branchName);
        
        // get all build details first
        var build = await GetFromAllBuildsAsync(defaultTeamProject, pipelineId, environmentName);
        
        // try to search builds based on the branch
        if (build == null)
        {
            build = await GetFromBranchBuildsAsync(defaultTeamProject, pipelineId, environmentName, branchName);
        }

        return build != null
            ? CreateBuildDetails(build, teamProject, defaultTeamProject, repositoryName, branchName, commits.FirstOrDefault().CommitId)
            : CreateEmptyBuildDetails(teamProject, defaultTeamProject, repositoryName, environmentName, commits.FirstOrDefault().CommitId, pipelineId);
    }
    
    private async Task<Build> GetFromAllBuildsAsync(string teamProject, int buildId, string environmentName)
    {
        var builds = await _azureDevOpsService.GetBuildDetailsAsync(teamProject, buildId, 20);
        var build = builds.FirstOrDefault(x => ExtractEnvironmentName(x.TemplateParameters) == environmentName);
        return build;
    }

    private async Task<Build> GetFromBranchBuildsAsync(string teamProject, int buildId, string environmentName, string branchName)
    {
        var builds = await _azureDevOpsService.GetBuildDetailsAsync(teamProject, buildId, branchName, 20);
        var build = builds.FirstOrDefault(x => ExtractEnvironmentName(x.TemplateParameters) == environmentName);
        return build;
    }
    
    private BuildDetails CreateBuildDetails(Build build, string teamProject, string defaultTeamProject, string repositoryName, string branchName, string commit)
    {
        var buildDetails =  new BuildDetails
        {
            RepositoryName = repositoryName,
            RepositoryUrl = _azureDevOpsService.BuildRepositoryUrl(teamProject, repositoryName),
            BuildId = build.Id,
            RequestedFor = build.RequestedFor.DisplayName,
            FinishTime = build.FinishTime.GetValueOrDefault(),
            StartTime = build.StartTime.GetValueOrDefault(),
            Status = build.Status.GetValueOrDefault().ToString(),
            SourceBranch = GitBranchHelper.RemoveRefName(build.SourceBranch),
            SourceVersion = build.SourceVersion,
            SourceCommitLink = _azureDevOpsService.BuildRepositoryCommitUrl(teamProject, repositoryName, build.SourceVersion),
            Environment = ExtractEnvironmentName(build.TemplateParameters),
            BuildLink = _azureDevOpsService.BuildBuildResultUrl(defaultTeamProject, build.Id),
            CurrentCommit = commit,
            CurrentCommitLink = _azureDevOpsService.BuildRepositoryCommitUrl(teamProject, repositoryName, commit)
        };

        var isLatestCommit = String.Compare(build.SourceVersion, commit, StringComparison.InvariantCultureIgnoreCase) == 0;

        string statusMessage;
        if (buildDetails.SourceBranch == branchName)
        {
            if (isLatestCommit)
            {
                statusMessage = $"Same as {branchName}";
            }
            else
            {
                statusMessage = $"Behind the {branchName}";
            }
        }
        else
        {
            statusMessage = $"{branchName} isn't deployed";
        }

        buildDetails.Message = statusMessage;
        
        return buildDetails;
    }
    
    private BuildDetails CreateEmptyBuildDetails(string teamProject, string defaultTeamProject, string repositoryName, string environmentName, string commit, int pipelineId)
    {
        return new BuildDetails
        {
            Environment = environmentName,
            RepositoryName = repositoryName,
            RepositoryUrl = _azureDevOpsService.BuildRepositoryUrl(teamProject, repositoryName),
            CurrentCommit = commit,
            CurrentCommitLink = _azureDevOpsService.BuildRepositoryCommitUrl(teamProject, repositoryName, commit),
            Status = "None",
            BuildLink = _azureDevOpsService.BuildPipelineUrl(defaultTeamProject, pipelineId)
        };
    }
    
    private static string ExtractEnvironmentName(IReadOnlyDictionary<string, string> templateParameters)
    {
        if (templateParameters.ContainsKey("environment"))
        {
            return templateParameters["environment"];
        }
        if (templateParameters.ContainsKey("Environment"))
        {
            return templateParameters["Environment"];
        }

        return String.Empty;
    }
    
    #endregion
}