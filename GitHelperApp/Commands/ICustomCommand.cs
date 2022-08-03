using McMaster.Extensions.CommandLineUtils;

namespace GitHelperApp.Commands;

public interface ICustomCommand
{
    Task OnExecuteAsync(CommandLineApplication command, IConsole console);
}