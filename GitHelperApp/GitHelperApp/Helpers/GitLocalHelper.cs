namespace GitHelperApp.Helpers;

public static class GitLocalHelper
{
    public static string GetRefName(string branchName) => $"origin/{branchName}";
}