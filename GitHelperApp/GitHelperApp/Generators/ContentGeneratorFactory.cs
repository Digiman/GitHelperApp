using GitHelperApp.Generators.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GitHelperApp.Generators;

/// <summary>
/// Simple factory to create the required content generator based on the type.
/// </summary>
public sealed class ContentGeneratorFactory : IContentGeneratorFactory
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, Type> _generators = new Dictionary<string, Type>
    {
        { "text", typeof(TextFileContentGenerator) },
        { "markdown", typeof(MarkdownContentGenerator) },
        { "markdown-table", typeof(MarkdownTableContentGenerator) }
    };

    public ContentGeneratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IContentGenerator GetContentGenerator(string type)
    {
        var generatorType = _generators[type];
        return (IContentGenerator)ActivatorUtilities.CreateInstance(_serviceProvider, generatorType);
    }
}