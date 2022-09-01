namespace GitHelperApp.Generators.Interfaces;

/// <summary>
/// Simple factory to create the required content generator based on the type.
/// </summary>
public interface IContentGeneratorFactory
{
    /// <summary>
    /// Get instance of the content generator by type.
    /// </summary>
    /// <param name="type">Type of the generator.</param>
    /// <returns>Returns the actual instance of the generator.</returns>
    IContentGenerator GetContentGenerator(string type);
}