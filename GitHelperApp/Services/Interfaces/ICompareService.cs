using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Special service to do the compare between branched in the repositories.
/// </summary>
public interface ICompareService
{
    /// <summary>
    /// Compare the branches locally for all th repositories from config.
    /// </summary>
    /// <returns>Returns the compare result.</returns>
    List<CompareResult> CompareLocal();

    /// <summary>
    /// Compare the branches on Azure DevOps with API for all th repositories from config.
    /// </summary>
    /// <returns>Returns the compare result.</returns>
    Task<List<CompareResult>> CompareAzureAsync();
}