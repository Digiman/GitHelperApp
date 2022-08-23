using McMaster.Extensions.CommandLineUtils;

namespace GitHelperApp.Commands.Interfaces;

public interface ICustomCommand
{
    Task OnExecuteAsync(CommandLineApplication command, IConsole console);
}