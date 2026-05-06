using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Neuron.Core.Logging;
using System.Threading;
using Neuron.Core.Logging.Diagnostics;

namespace Neuron.Core.Scheduling;

/// <summary>
/// Neuron implementation of a MEC like coroutine which is meant to be
/// easily hookable into a lot of environments.
/// </summary>
public abstract class CoroutineReactor
{
    public int MainThreadId { get; protected set; }

    protected CoroutineReactor()
    {
        MainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public ILogger Logger { get; set; }

    protected List<CoroutineRegistration> _coroutines = new();
    protected ConcurrentQueue<CoroutineRegistration> _addCoroutines = new();
    protected ConcurrentQueue<CoroutineRegistration> _removeCoroutines = new();

    /// <summary>
    /// Starts a new coroutine defined by the enumerator.
    /// </summary>
    /// <param name="coroutine">the courtoutine is a method that returns the waiting time in seconds (float) before to continue the code</param>
    /// <returns>the coroutine handle use to stop the coroutine in <see cref="StopCoroutine"/></returns>
    public object StartCoroutine(IEnumerator<float> coroutine)
    {
        if (coroutine == null)
            throw new ArgumentNullException(nameof(coroutine));

        var registration = new CoroutineRegistration(coroutine);
        _addCoroutines.Enqueue(registration);
        return registration;
    }

    /// <summary>
    /// Starts a new coroutine and returns a task completed when the coroutine finishes.
    /// </summary>
    public NonBlockingAwaitable StartCoroutineAsync(IEnumerator<float> coroutine)
    {
        var registration = (CoroutineRegistration)StartCoroutine(coroutine);
        return registration.CompletionAwaitable = new NonBlockingAwaitable(MainThreadId);
    }

    /// <summary>
    /// Stops the coroutine identified by the handle.
    /// </summary>
    /// <param name="handle">the coroutine handle seen by <see cref="StartCoroutine"/></param>
    public void StopCoroutine(object handle)
    {
        if (handle is not CoroutineRegistration registration)
            throw new ArgumentException("Invalid coroutine handle", nameof(handle));

        registration.CompletionAwaitable ??= new NonBlockingAwaitable(MainThreadId);
        registration.CompletionAwaitable.TrySetCanceled();
        _removeCoroutines.Enqueue(registration);
    }

    /// <summary>
    /// returns a task completed when the coroutine finishes.
    /// </summary>
    public NonBlockingAwaitable WaitEndAsync(object handle)
    {
        if (handle is not CoroutineRegistration registration)
            throw new ArgumentException("Invalid coroutine handle", nameof(handle));

        registration.CompletionAwaitable ??= new NonBlockingAwaitable(MainThreadId);
        if (registration.Enumerator == null)
            registration.CompletionAwaitable.TrySetResult();
        
        return registration.CompletionAwaitable;
    }

    protected void Tick()
    {
        while (_addCoroutines.TryDequeue(out var routine)) _coroutines.Add(routine);
        while (_removeCoroutines.TryDequeue(out var routine)) _coroutines.Remove(routine);
        var currentMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (var pair in _coroutines)
        {
            if (pair.ScheduledUpdate > currentMillis) continue;
            var coroutine = pair.Enumerator;
            try
            {
                if (coroutine.MoveNext())
                {
                    var delay = coroutine.Current;
                    pair.ScheduledUpdate += (long) (delay * 1000);
                }
                else
                {
                    if (pair.CompletionAwaitable != null)
                        pair.CompletionAwaitable.TrySetResult();

                    pair.Enumerator = null;
                    _removeCoroutines.Enqueue(pair);
                }
            }
            catch (Exception e)
            {
                pair.CompletionAwaitable ??= new NonBlockingAwaitable(MainThreadId);
                pair.CompletionAwaitable.TrySetException(e);
                _removeCoroutines.Enqueue(pair);
                var error = DiagnosticsError.FromParts(
                    DiagnosticsError.Summary("An error occured while ticking a neuron coroutine"),
                    DiagnosticsError.Description($"The coroutine '{pair.Enumerator}' threw {e.GetType().FullName}: {e.Message}")
                );
                error.Exception = e;
                NeuronDiagnosticHinter.AddExeptionInformationHints(e, error);
                Logger?.Framework(error);
            }
        }
    }
}