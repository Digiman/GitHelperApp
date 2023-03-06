using GitHelperApp.Configuration;

namespace GitHelperApp.Helpers;

/// <summary>
/// Simple helper to provide the functionality to print result ot file or console. 
/// </summary>
public static class OutputHelper
{
    public static void OutputResultToConsole(IReadOnlyCollection<string> lines)
    {
        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }
    }

    public static void OutputResultToFile(IReadOnlyCollection<string> lines, string filename)
    {
        File.WriteAllLines(filename, lines);
    }

    public static void OutputRepositoriesList(RepositoriesConfig repositoriesConfig)
    {
        var index = 1;
        foreach (var repo in repositoriesConfig.Repositories.OrderBy(x => x.Name))
        {
            Console.WriteLine($"{index}: {repo.Name}");
            index++;
        }
    }
}