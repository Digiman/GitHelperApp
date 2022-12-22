namespace GitHelperApp.Helpers;

/// <summary>
/// Simple helper for Git.
/// </summary>
public static class GitBranchHelper
{
    public static string GetRefName(string branchName) => $"origin/{branchName}";
    public static string GetRefNameForAzure(string branchName) => $"refs/heads/{branchName}";
}