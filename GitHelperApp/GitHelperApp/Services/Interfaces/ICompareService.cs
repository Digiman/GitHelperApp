using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

/// <summary>
/// Special service to do the compare between branched in the repositories.
/// </summary>
public interface ICompareService
{
    List<CompareResult> CompareLocal();
    Task<List<CompareResult> > CompareAzureAsync();
}