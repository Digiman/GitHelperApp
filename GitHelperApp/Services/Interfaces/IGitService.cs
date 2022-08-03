namespace GitHelperApp.Services.Interfaces;

public interface IGitService
{
    List<string> GetBranchesList(string repoPath);
    (bool isChanges, int count, List<string> commits) CompareBranches(string repoPath, string source, string destination);
}