namespace GitHelperApp;

public readonly struct Constants
{
    // TODO: think here hot to optimize the data - maybe call to Azure DevOps or move to configuration file???!!!

    /// <summary>
    /// List of user identities to sue in the application for PRs and others.
    /// </summary>
    public static readonly Dictionary<string, string> Users = new()
    {
        // Oxagile team
        { "Andrey Kukharenko", "b9f8187f-14e4-486b-95ab-a063c9c26d51" }
    };
}