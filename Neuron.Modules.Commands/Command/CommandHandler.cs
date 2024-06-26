﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core.Logging;
using Neuron.Modules.Commands.Event;
using Ninject;

namespace Neuron.Modules.Commands.Command;

/// <summary>
/// Handle commands in the following manner, any command register which has the name or alias
/// of the command being executed will be executed.
/// </summary>
public class CommandHandler : ICommandHandler
{
    private IKernel _kernel;
    private NeuronLogger _neuronLogger;
    private ILogger _logger;

    public CommandHandler(IKernel kernel, NeuronLogger neuronLogger)
    {
        _kernel = kernel;
        _neuronLogger = neuronLogger;
        _logger = _neuronLogger.GetLogger<CommandHandler>();
    }

    private List<ICommand> _commands = new();
    public ReadOnlyCollection<ICommand> Commands => _commands.AsReadOnly();

    public void Raise(CommandEvent commandEvent)
    {
        if (commandEvent.IsHandled) return;

        foreach (var command in Commands)
        {
            var meta = command.Meta;

            var names = meta.Aliases.ToList();
            names.Add(meta.CommandName);

            foreach (var name in names)
            {
                if (!name.Equals(commandEvent.Context.Command, StringComparison.OrdinalIgnoreCase)) continue;

                var pre = command.InternalPreExecute(commandEvent.Context);
                if (pre != null)
                {
                    commandEvent.Result = pre;
                    commandEvent.IsHandled = true;
                    continue;
                }

                var handled = command.InternalExecute(commandEvent.Context);
                if (handled == null) continue;

                commandEvent.Result = handled;
                commandEvent.IsHandled = true;
            }
        }
    }

    public void RegisterCommand(Type type)
        => RegisterCommand(type, type.GetCustomAttribute<CommandAttribute>());

    public void RegisterCommand(Type type, CommandAttribute meta)
    {
        if (meta == null) return;
        if (!typeof(ICommand).IsAssignableFrom(type)) return;
        var command = (ICommand)_kernel.Get(type);
        command.Meta = meta;
        _commands.Add(command);
    }

    public void RegisterCommand<TCommand>() where TCommand : ICommand => RegisterCommand(typeof(TCommand));

    public void RegisterCommand<TCommand>(TCommand command) where TCommand : ICommand
        => RegisterCommand(command, typeof(TCommand).GetCustomAttribute<CommandAttribute>());

    public void RegisterCommand<TCommand>(TCommand command, CommandAttribute meta) where TCommand : ICommand
    {
        if (meta == null) return;
        command.Meta = meta;
        _commands.Add(command);
    }

    public void UnregisterAllCommands()
    {
        _commands.Clear();
    }
}