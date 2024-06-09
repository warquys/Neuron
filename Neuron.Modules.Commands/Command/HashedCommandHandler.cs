using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Neuron.Core;
using Neuron.Core.Logging;
using Neuron.Modules.Commands.Event;
using Ninject;

namespace Neuron.Modules.Commands.Command;

public class HashedCommandHandler : ICommandHandler
{
    private IKernel _kernel;
    private NeuronLogger _neuronLogger;
    private ILogger _logger;

    public readonly bool overrideName;

    /// <param name="kernel" />
    /// <param name="neuronLogger" />
    /// <param name="overrideName">
    /// if <see langword="true"/> a register command with a same name than a other other command allready register
    /// will replace the last register. 
    /// <br>else a <see cref="ModuleOrPluginConflictException"/> get throw </br> 
    /// </param>
    public HashedCommandHandler(IKernel kernel, NeuronLogger neuronLogger, bool overrideName = false)
    {
        _kernel = kernel;
        _neuronLogger = neuronLogger;
        _logger = _neuronLogger.GetLogger<CommandHandler>();
        this.overrideName = overrideName;
    }

    private List<ICommand> _commands = new();
    public ReadOnlyCollection<ICommand> Commands => _commands.AsReadOnly();

    private Dictionary<string, ICommand> _nameToCommands  = new();
    public ReadOnlyDictionary<string, ICommand> NameToCommands => new ReadOnlyDictionary<string, ICommand>(_nameToCommands);

    private Dictionary<string, ICommand> _aliasToCommands  = new();
    public ReadOnlyDictionary<string, ICommand> AliasToCommands => new ReadOnlyDictionary<string, ICommand>(_aliasToCommands);


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
                break;
            }
        }
    }

    public void RegisterCommand(Type type)
        => RegisterCommand(type, type.GetCustomAttribute<CommandAttribute>());

    public void RegisterCommand(Type type, CommandAttribute meta)
    {
        if (meta == null) return;
        if (!typeof(ICommand).IsAssignableFrom(type)) return;

        if (!overrideName && _nameToCommands.ContainsKey(meta.CommandName))
            return;

        var command = (ICommand)_kernel.Get(type);
        command.Meta = meta;

        _nameToCommands[meta.CommandName] = command;

        foreach (var alias in meta.Aliases)
        {
            if (!_aliasToCommands.ContainsKey(alias))
                _aliasToCommands.Add(alias, command);
        }
        _commands.Add(command);
    }

    public void RegisterCommand<TCommand>() where TCommand : ICommand => RegisterCommand(typeof(TCommand));

    public void RegisterCommand<TCommand>(TCommand command) where TCommand : ICommand
        => RegisterCommand(command, typeof(TCommand).GetCustomAttribute<CommandAttribute>());

    public void RegisterCommand<TCommand>(TCommand command, CommandAttribute meta) where TCommand : ICommand
    {
        if (meta == null) return;
        command.Meta = meta;

        if (!overrideName && _nameToCommands.ContainsKey(meta.CommandName))
            throw ModuleOrPluginConflictException.Build(_nameToCommands[meta.CommandName], command,
                "it is not possible to register two commands with the same name");

        _nameToCommands[meta.CommandName] = command;

        foreach (var alias in meta.Aliases)
            _aliasToCommands[alias] = command;
        _commands.Add(command);
    }

    public void UnregisterAllCommands()
    {
        _commands.Clear();
    }
}
