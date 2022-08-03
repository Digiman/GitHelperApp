using GitHelperApp.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitHelperApp.Extensions;

public static class ResourceRefExtensions
{
    public static WorkItemModel ToModel(this ResourceRef resourceRef, string url)
    {
        return new WorkItemModel
        {
            Id = resourceRef.Id,
            Url = url
        };
    }
}