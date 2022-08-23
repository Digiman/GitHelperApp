namespace GitHelperApp.Configuration;

/// <summary>
/// Application configuration.
/// </summary>
public sealed class AppConfig
{
    /// <summary>
    /// Path to save the files the results.
    /// </summary>
    public string OutputDirectory { get; set; }
    
    /// <summary>
    /// Format for the files with the results.
    /// </summary>
    public string OutputFormat { get; set; }
}