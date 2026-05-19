using System;
using System.Collections.Concurrent;
using System.Threading;
using Neuron.Core.Logging;
using Neuron.Core.Logging.Diagnostics;

namespace Neuron.Core.Scheduling;

/// <summary>
/// Dispatches actions to be executed on the thread that owns this dispatcher.
/// </summary>
public abstract class ThreadDispatcher
{
    private readonly ConcurrentQueue<Action> _actions = new();

    public ILogger Logger { get; set; }

    /// <summary>
    /// Gets the managed thread id that owns this dispatcher.
    /// </summary>
    public int ThreadId { get; protected set; }

    /// <summary>
    /// Returns whether the current thread is the owning thread.
    /// </summary>
    public bool IsCurrentThread => Thread.CurrentThread.ManagedThreadId == ThreadId;

    public ThreadDispatcher()
    {
        ThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Enqueues an action to be executed on the owning thread.
    /// </summary>
    public void Post(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (IsCurrentThread)
            action();
        else
            _actions.Enqueue(action);
    }

    /// <summary>
    /// Executes all queued actions. This method should be called on the owning thread.
    /// </summary>
    protected void ExecutePending()
    {
        while (_actions.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                var error = DiagnosticsError.FromParts(
                    DiagnosticsError.Summary("An error occured while ticking a neuron coroutine"),
                    DiagnosticsError.Description($"The action '{action.Method.Name}' threw {e.GetType().FullName}: {e.Message}")
                );
                error.Exception = e;
                NeuronDiagnosticHinter.AddExeptionInformationHints(e, error);
                Logger?.Framework(error);
            }
        }
    }

    protected void VerifyMainThread()
    {
        if (!IsCurrentThread)
            throw new InvalidOperationException("ThreadDispatcher.ExecutePending must be called from the owning thread.");
    }
}
