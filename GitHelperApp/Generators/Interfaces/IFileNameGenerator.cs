namespace GitHelperApp.Generators.Interfaces;

/// <summary>
/// Simple generator for the file names uses to save results.
/// </summary>
public interface IFileNameGenerator
{
    string CreateFilenameForCompareResults(string directory, string runId);
    string CreateFilenameForFullResults(string directory, string runId);
    string CreateFileNameForPrIds(string directory, string runId);
    string CreateFileNameForWorkItems(string directory, string runId);
    string CreateFileNameForRepositories(string directory, string runId);
}