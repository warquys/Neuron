using System.Collections.Generic;
using Neuron.Core.Logging;
using Neuron.Core.Meta;
using Neuron.Modules.Commands.Command;
using Neuron.Modules.Commands.Event;
using Ninject;

// ReSharper disable MemberCanBePrivate.Global
namespace Neuron.Modules.Commands;

public class CommandService : Service
{
    private IKernel _kernel;
    private NeuronLogger _neuronLogger;
    private ILogger _logger;

    public CommandHandler GlobalHandler => (CommandHandler)GlobalCommandReactor.Handler;

    public readonly List<CommandReactor> CommandReactors;
    public readonly CommandReactor GlobalCommandReactor;

    public CommandService(IKernel kernel, NeuronLogger neuronLogger)
    {
        _kernel = kernel;
        _neuronLogger = neuronLogger;
        _logger = _neuronLogger.GetLogger<CommandService>();
        GlobalCommandReactor = new CommandReactor(_kernel, _neuronLogger);
        CommandReactors = new List<CommandReactor>();
    }

    /// <summary>
    /// Create a new <see cref="CommandReactor"/> using the default command handler (<see cref="CommandHandler"/>).
    /// And bind the <see cref="CommandReactor"/> to the <see cref="GlobalCommandReactor"/>.
    /// </summary>
    public CommandReactor CreateCommandReactor()
        => CreateCommandReactor(new CommandHandler(_kernel, _neuronLogger));

    /// <summary>
    /// Create a new <see cref="CommandReactor"/> using the cached command handler (<see cref="HashedCommandHandler"/>).
    /// And bind the <see cref="CommandReactor"/> to the <see cref="GlobalCommandReactor"/>.
    /// <br>Note: </br>
    /// <remarks>
    /// <b>Note:</b>
    ///  Cannot manage two commands with the same name
    /// </remarks>
    /// </summary>
    /// <param name="overrideName">
    /// if <see langword="true"/> a register command with a same name than a other other command allready register
    /// will replace the last register. 
    /// <br>else a <see cref="Core.ModuleOrPluginConflictException"/> get throw </br> 
    /// </param>
    public CommandReactor CreateHashedCommandReactor(bool overrideName = false)
        => CreateCommandReactor(new HashedCommandHandler(_kernel, _neuronLogger, overrideName));

    public CommandReactor CreateCommandReactor<TCommandHandler>(TCommandHandler handler) 
        where TCommandHandler : ICommandHandler
    {
        var reactor = new CommandReactor(_kernel, _neuronLogger, handler);
        reactor.Subscribe(CallGlobalReactor);
        CommandReactors.Add(reactor);
        return reactor;
    }

    private void CallGlobalReactor(CommandEvent context) 
        => GlobalCommandReactor.Raise(context);
}