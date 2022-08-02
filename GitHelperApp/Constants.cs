namespace GitHelperApp;

public readonly struct Constants
{
    /// <summary>
    /// List of user identities to sue in the application for PRs and others.
    /// </summary>
    public static readonly Dictionary<string, string> Users = new Dictionary<string, string>
    {
        // Oxagile team
        { "Andrey Kukharenko", "b9f8187f-14e4-486b-95ab-a063c9c26d51" },
        { "Ivan Grishkov", "b8f8d496-2f40-4e2b-97ff-6f0c4655e71f" },
        { "Oleg Solonko", "bf5de8e1-aaa1-4ec8-a160-d7bf9146a3f4" },
        { "Konstantin Bondarenko", "52003d12-ec51-41af-87f2-766905b64912" },
        { "Stas Ivanousky", "c936e1e8-6a0b-4dfe-91db-92b539dba1ce" },
        { "Yulia Yakovlevich", "9400c153-1aba-4c41-82ea-12b340a5ee21" },
        // Admiral team
        { "Xabier Hernandez", "a859462e-0137-4397-ad3a-c06b6498c2fd" },
        { "Nazmus Sakib", "2c17c81c-aae6-4e9a-8650-a6717e9c128b" },
        { "Adan Jauregui", "8289fb73-b378-46ca-ad24-f1e50e7bc261" },
        { "Emilio deLeon", "68b98354-0a0c-4539-b72a-11e31233b061" },
        { "Dominic Rzepecki", "8476d6d6-de66-41a1-8aa6-fe2441e08b41" },
        // Matrix team
        { "Brian Bober", "ac32f9a8-fb46-44a5-9d75-e9ead69965e0" },
        { "Haygood, Justin", "66e00245-fb58-4e93-aee3-cd589bdfbfb9" },
        // Oxagile DevOps
        { "Rufat Aliyev", "738dedc7-0df0-4cbe-af68-e6a45cf986d5" },
        { "Anatoliy Zbirovskiy", "016b66a2-d530-452a-bbbd-07079ed8e778" },
        // teams
        { "Admiral", "a6cc9965-0b11-4c04-980a-055c98314119" }
    };
}