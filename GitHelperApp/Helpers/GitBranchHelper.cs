namespace GitHelperApp.Helpers;

/// <summary>
/// Simple helper for Git.
/// </summary>
public static class GitBranchHelper
{
    public static string GetRefName(string branchName) => $"origin/{branchName}";
    public static string GetRefNameForAzure(string branchName) => $"refs/heads/{branchName}";
    
    /// <summary>
    /// Remove the branch ref header - 'refs/heads/'.
    /// </summary>
    /// <param name="refBranchName">Branch name with full ref from the Azure DevOps.</param>
    /// <returns>Returns the branch name without ref name.</returns>
    public static string RemoveRefName(string refBranchName)
    {
        return refBranchName.AsSpan(11).ToString();
    }
}