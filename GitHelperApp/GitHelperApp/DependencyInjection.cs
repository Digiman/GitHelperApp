using GitHelperApp.Configuration;
using GitHelperApp.Generators;
using GitHelperApp.Generators.Interfaces;
using GitHelperApp.Services;
using GitHelperApp.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GitHelperApp;

public static class DependencyInjection
{
    /// <summary>
    /// Configure the application dependencies.
    /// </summary>
    /// <param name="services">Services collections.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Returns updated services collection.</returns>
    public static IServiceCollection InitializeDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // register configuration
        services.Configure<AzureDevOpsConfig>(configuration.GetSection(nameof(AzureDevOpsConfig)));
        services.Configure<RepositoriesConfig>(configuration.GetSection(nameof(RepositoriesConfig)));
        services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
        services.Configure<PullRequestConfig>(configuration.GetSection(nameof(PullRequestConfig)));
        services.Configure<WorkItemFilterConfig>(configuration.GetSection(nameof(WorkItemFilterConfig)));
        services.Configure<CustomPrConfig>(configuration.GetSection(nameof(CustomPrConfig)));
        
        // register services
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<IAzureDevOpsService, AzureDevOpsService>();
        services.AddSingleton<ICompareService, CompareService>();
        services.AddSingleton<IOutputService, OutputService>();
        services.AddSingleton<IPullRequestService, PullRequestService>();
        services.AddSingleton<IWorkItemsService, WorkItemsService>();
        
        // add content generators
        services.AddSingleton<IContentGeneratorFactory, ContentGeneratorFactory>();
        
        // add generators
        services.AddSingleton<IFileNameGenerator, FileNameGenerator>();
        
        return services;
    }
}