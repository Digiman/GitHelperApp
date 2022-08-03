namespace GitHelperApp;

public static class FileNameHelper
{
    public static string CreateFilenameForCompareResults(string id) => $"C:\\Temp\\GitHelperApp\\Result-{id}.txt";
    public static string CreateFilenameForFullResults(string id) => $"C:\\Temp\\GitHelperApp\\ResultFull-{id}.txt";
    public static string CreateFileNameForPrIds(string id) => $"C:\\Temp\\GitHelperApp\\Prs-{id}.txt";
    public static string CreateFileNameForWorkItems(string id) => $"C:\\Temp\\GitHelperApp\\Wit-{id}.txt";
}