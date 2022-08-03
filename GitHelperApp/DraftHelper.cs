using GitHelperApp.Configuration;

namespace GitHelperApp;

public static class DraftHelper
{
    public static void GetAllBranchesList(List<RepositoryConfig> repositoryConfigs)
    {
        foreach (var repositoryConfig in repositoryConfigs)
        {
            var branches = GitHelper.GetBranchesList(repositoryConfig.Path);

            if (branches.Any())
            {
                Console.WriteLine($"Repository: {repositoryConfig.Name}. Branches:");
                foreach (var branch in branches)
                {
                    Console.WriteLine($" - {branch}");
                }
            }
        }
    }
}