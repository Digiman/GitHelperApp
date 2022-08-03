using GitHelperApp.Models;

namespace GitHelperApp.Services.Interfaces;

public interface ICompareService
{
    List<CompareResult> CompareLocal();
    Task<List<CompareResult> > CompareAzureAsync();
}