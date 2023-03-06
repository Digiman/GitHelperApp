namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service for using the LibGit2Sharp for working with local repositories.
/// Learn more here: https://github.com/libgit2/libgit2sharp
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Get list of all branches for repository.
    /// </summary>
    /// <param name="repoPath">Path to the repository on local machine.</param>
    /// <returns>Returns the list of branches.</returns>
    List<string> GetBranchesList(string repoPath);

    /// <summary>
    /// Compare branched.
    /// </summary>
    /// <param name="repoPath">Path to the repository on local machine.</param>
    /// <param name="source">Source branch.</param>
    /// <param name="destination">Destination branch.</param>
    /// <returns>Returns details after compare: changes count and list of commits.</returns>
    (bool isChanges, int count, List<string> commits) CompareBranches(string repoPath, string source, string destination);
}