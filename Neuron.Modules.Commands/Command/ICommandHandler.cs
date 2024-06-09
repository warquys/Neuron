using System;
using System.Collections.ObjectModel;
using Neuron.Modules.Commands.Event;

namespace Neuron.Modules.Commands.Command
{
    public interface ICommandHandler
    {
        ReadOnlyCollection<ICommand> Commands { get; }

        void Raise(CommandEvent commandEvent);
        void RegisterCommand(Type type);
        void RegisterCommand(Type type, CommandAttribute meta);
        void RegisterCommand<TCommand>() where TCommand : ICommand;
        void RegisterCommand<TCommand>(TCommand command) where TCommand : ICommand;
        void RegisterCommand<TCommand>(TCommand command, CommandAttribute meta) where TCommand : ICommand;
        void UnregisterAllCommands();
    }
}