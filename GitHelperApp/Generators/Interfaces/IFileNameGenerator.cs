namespace GitHelperApp.Generators.Interfaces;

public interface IFileNameGenerator
{
    string CreateFilenameForCompareResults(string directory, string runId);
    string CreateFilenameForFullResults(string directory, string runId);
    string CreateFileNameForPrIds(string directory, string runId);
    string CreateFileNameForWorkItems(string directory, string runId);
}