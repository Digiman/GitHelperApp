using GitHelperApp.Configuration;
using GitHelperApp.Services;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHelperApp;

public static class DependencyInjection
{
    public static IServiceCollection InitializeDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // register configuration
        services.Configure<AzureDevOpsConfig>(configuration.GetSection("AzureDevOps"));
        services.Configure<RepositoriesConfig>(configuration.GetSection(nameof(RepositoriesConfig)));
        services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
        services.Configure<PullRequestConfig>(configuration.GetSection(nameof(PullRequestConfig)));
        
        // register services
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();
        services.AddSingleton<ICompareService, CompareService>();
        services.AddSingleton<IOutputService, OutputService>();
        services.AddSingleton<IPullRequestService, PullRequestService>();
        
        return services;
    }
}