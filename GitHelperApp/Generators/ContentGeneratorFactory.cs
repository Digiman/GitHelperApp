using GitHelperApp.Generators.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GitHelperApp.Generators;

public sealed class ContentGeneratorFactory : IContentGeneratorFactory
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<string, Type> _generators = new Dictionary<string, Type>
    {
        { "text", typeof(TextFileContentGenerator) },
        { "markdown", typeof(MarkdownContentGenerator) }
    };

    public ContentGeneratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IContentGenerator GetContentGenerator(string type)
    {
        var generatorType = _generators[type];
        return (IContentGenerator)ActivatorUtilities.CreateInstance(_serviceProvider, generatorType);
    }
}