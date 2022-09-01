using GitHelperApp.Services.Interfaces;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitHelperApp.Services;

/// <summary>
/// Service for using the LibGit2Sharp for working with local repositories.
/// Learn more here: https://github.com/libgit2/libgit2sharp
/// </summary>
public sealed class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public List<string> GetBranchesList(string repoPath)
    {
        List<Branch> allBranches;
        
        using (var repository = new Repository(repoPath))
        {
            allBranches = repository.Branches.ToList();
        }

        return allBranches.Select(x => x.FriendlyName).ToList();
    }

    /// <inheritdoc />
    public (bool isChanges, int count, List<string> commits) CompareBranches(string repoPath, string source, string destination)
    {
        using (var repository = new Repository(repoPath))
        {
            if (repository.Branches[source] != null && repository.Branches[destination] != null)
            {
                var changes = repository.Diff.Compare<TreeChanges>(
                    repository.Branches[source].Tip.Tree,
                    repository.Branches[destination].Tip.Tree);

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
            else
            {
                _logger.LogWarning("Branches can't be compared because some of them or both not exists!");
                
                return (false, 0, new List<string>());
            }
        }

        return (false, 0, new List<string>());
    }
}