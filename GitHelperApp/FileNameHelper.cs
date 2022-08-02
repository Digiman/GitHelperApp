namespace GitHelperApp;

public static class FileNameHelper
{
    public static string CreateFilenameForCompareResults(string id) => $"C:\\Temp\\Result-{id}.txt";
    public static string CreateFilenameForFullResults(string id) => $"C:\\Temp\\ResultFull-{id}.txt";
    public static string CreateFileNameForPrIds(string id) => $"C:\\Temp\\Prs-{id}.txt";
}