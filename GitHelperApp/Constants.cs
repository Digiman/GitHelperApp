namespace GitHelperApp;

public readonly struct  Constants
{
    /// <summary>
    /// List of user identities to sue in the application for PRs and others.
    /// </summary>
    public static readonly Dictionary<string, string> Users = new Dictionary<string, string>
    {
        { "Andrey Kukharenko", "b9f8187f-14e4-486b-95ab-a063c9c26d51" },
        { "Admiral", "a6cc9965-0b11-4c04-980a-055c98314119" }
    };
}