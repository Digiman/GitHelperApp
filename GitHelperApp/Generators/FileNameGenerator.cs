using GitHelperApp.Configuration;
using GitHelperApp.Generators.Interfaces;
using Microsoft.Extensions.Options;

namespace GitHelperApp.Generators;

/// <summary>
/// Simple generator for the file names uses to save results.
/// </summary>
public sealed class FileNameGenerator : IFileNameGenerator
{
    private readonly AppConfig _appConfig;

    public FileNameGenerator(IOptions<AppConfig> appConfig)
    {
        _appConfig = appConfig.Value;
    }

    public string CreateFilenameForCompareResults(string directory, string runId) =>
        Path.Combine(_appConfig.OutputDirectory, directory, $"Result-{runId}.{GetExtension(_appConfig.OutputFormat)}");

    public string CreateFilenameForFullResults(string directory, string runId) =>
        Path.Combine(_appConfig.OutputDirectory, directory, $"ResultFull-{runId}.{GetExtension(_appConfig.OutputFormat)}");

    public string CreateFileNameForPrIds(string directory, string runId) =>
        Path.Combine(_appConfig.OutputDirectory, directory, $"Prs-{runId}.{GetExtension(_appConfig.OutputFormat)}");

    public string CreateFileNameForWorkItems(string directory, string runId) =>
        Path.Combine(_appConfig.OutputDirectory, directory, $"Wit-{runId}.{GetExtension(_appConfig.OutputFormat)}");

    private static string GetExtension(string format)
    {
        return format switch
        {
            "text" => "txt",
            "markdown" => "md",
            _ => "txt"
        };
    }
}
