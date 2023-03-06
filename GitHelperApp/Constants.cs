namespace GitHelperApp;

public readonly struct Constants
{
    // TODO: think here hot to optimize the data - maybe call to Azure DevOps or move to configuration file???!!!

    /// <summary>
    /// List of user identities to sue in the application for PRs and others.
    /// </summary>
    public static readonly Dictionary<string, string> Users = new Dictionary<string, string>
    {
        // Oxagile team - Oxygen
        { "Andrey Kukharenko", "b9f8187f-14e4-486b-95ab-a063c9c26d51" },
        { "Ivan Grishkov", "b8f8d496-2f40-4e2b-97ff-6f0c4655e71f" },
        { "Oleg Solonko", "bf5de8e1-aaa1-4ec8-a160-d7bf9146a3f4" },
        { "Konstantin Bondarenko", "52003d12-ec51-41af-87f2-766905b64912" },
        { "Stas Ivanousky", "c936e1e8-6a0b-4dfe-91db-92b539dba1ce" },
        { "Yulia Yakovlevich", "9400c153-1aba-4c41-82ea-12b340a5ee21" },

        // Admiral team
        { "Xabier Hernandez", "a859462e-0137-4397-ad3a-c06b6498c2fd" },
        { "Emilio deLeon", "68b98354-0a0c-4539-b72a-11e31233b061" },
        { "Dominic Rzepecki", "8476d6d6-de66-41a1-8aa6-fe2441e08b41" },

        // Matrix team
        { "Brian Bober", "ac32f9a8-fb46-44a5-9d75-e9ead69965e0" },
        { "Haygood, Justin", "66e00245-fb58-4e93-aee3-cd589bdfbfb9" },

        // Oxagile DevOps
        { "Rufat Aliyev", "738dedc7-0df0-4cbe-af68-e6a45cf986d5" },
        { "Anatoliy Zbirovskiy", "016b66a2-d530-452a-bbbd-07079ed8e778" },

        // Hydrogen team
        { "Arafat Hossain", "90b09987-019a-433a-9145-6344ff4c9a2a" },
        { "Nazmus Sakib", "2c17c81c-aae6-4e9a-8650-a6717e9c128b" },
        { "Adan Jauregui", "8289fb73-b378-46ca-ad24-f1e50e7bc261" },
        { "Abhishek Roy", "c90b6fb8-ce8a-487c-bc62-e5c82e45e4ce" },
        { "Mohammad Rezaul Karim", "bc8112e5-1201-43c3-8fac-1cc3539dff8b" },

        // Teams
        { "Admiral", "a6cc9965-0b11-4c04-980a-055c98314119" },
        { "Enosis", "4dbf7be8-101c-4566-a7ff-a6f6970494e3" },
        { "Hydrogen", "c9710410-58a2-4079-bdc3-5cfb3b1019a3" },
        { "Oxygen", "a79067fc-bf1d-4128-8b1c-f66608c1a722" },
        { "DevOps", "60b977d8-adff-4943-af2a-1d5a2b2c6ef7" },
        { "MSG Team", "654d9cbc-e760-4f83-9b30-21ba3eb2bc3d" },

        // Other team members
        { "April Larson", "90989d06-b9fd-4631-bd14-b338ff0dd003" },
        { "Tasnia Nujhat", "41ccdeb7-8bd4-42a1-a435-35ea3266e328" }
    };
}