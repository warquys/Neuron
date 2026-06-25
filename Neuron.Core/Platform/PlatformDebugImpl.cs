using System;
using System.Threading;
using System.Transactions;
using Neuron.Core.Logging;
using Neuron.Core.Modules;
using Neuron.Core.Scheduling;
using Ninject;

namespace Neuron.Core.Platform;

/// <summary>
/// Platform Implementation for test environments like Unit-Tests
/// </summary>
public class PlatformDebugImpl : IPlatform
{
    public PlatformConfiguration Configuration { get; set; } = new PlatformConfiguration();
    public NeuronBase NeuronBase { get; set; }
    public LoopingCoroutineReactor CoroutineReactor = new();
    
    private readonly Thread _coroutineThread;
    private readonly Action<ModuleManager> _loadModules;

    public PlatformDebugImpl(Action<ModuleManager> loadModules) : this()
    {
        _loadModules = loadModules;
    }

    public PlatformDebugImpl() 
    { 
        _coroutineThread = new Thread(CoroutineReactor.Start);
    }

    public void Load()
    {
        Configuration.FileIo = false;
        Configuration.UseGlobals = false;
        Configuration.CoroutineReactor = CoroutineReactor;
        Configuration.ConsoleWidth = 100;
        NeuronBase.Configuration.Logging.FileLogging = false;
        NeuronBase.Configuration.Logging.LogLevel = LogLevel.Debug;
    }

    public void Enable()
    {
        if (_loadModules != null)
            _loadModules(NeuronBase.Kernel.Get<ModuleManager>());
    }

    public void Continue()
    {
        _coroutineThread.Start(); // Start coroutine Reactor in separate Thread for debug purposes
    }

    public void Disable()
    {
        CoroutineReactor.Running = false;
    }
}