namespace GitHelperApp.Generators.Interfaces;

public interface IContentGeneratorFactory
{
    IContentGenerator GetContentGenerator(string type);
}