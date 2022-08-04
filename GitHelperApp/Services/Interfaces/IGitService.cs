namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Service for using the LibGit2Sharp for working with local repositories.
/// Learn more here: https://github.com/libgit2/libgit2sharp
/// </summary>
public interface IGitService
{
    List<string> GetBranchesList(string repoPath);
    (bool isChanges, int count, List<string> commits) CompareBranches(string repoPath, string source, string destination);
}