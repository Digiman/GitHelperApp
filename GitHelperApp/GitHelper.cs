using LibGit2Sharp;

namespace GitHelperApp;

public static class GitHelper
{
    public static List<string> GetBranchesList(string repoPath)
    {
        List<Branch> allBranches;
        
        using (var repository = new Repository(repoPath))
        {
            allBranches = repository.Branches.ToList();
        }

        return allBranches.Select(x => x.FriendlyName).ToList();
    }
    
    public static (bool isChanges, int count, List<string> commits) CompareBranches(string repoPath, string source, string destination)
    {
        using (var repository = new Repository(repoPath))
        {
            var changes = repository.Diff.Compare<TreeChanges>(
                repository.Branches[source].Tip.Tree,
                repository.Branches[destination].Tip.Tree);

            // var t1 = repository.Branches[source].Commits.FirstOrDefault();
            // var t2 = repository.Branches[destination].Commits.FirstOrDefault();

            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time,
                ExcludeReachableFrom = repository.Branches[destination].Tip,
                IncludeReachableFrom = repository.Branches[source].Tip
            };
            
            var commitLog = repository.Commits.QueryBy(filter);
            
            if (changes.Count > 0)
            {
                var commits = commitLog.Select(x => x.Id.Sha).ToList();
                return (true, changes.Count, commits);
            }
        }

        return (false, 0, new List<string>());
    }
    
    public static string GetRefName(string branchName) => $"origin/{branchName}";
}