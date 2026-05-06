using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neuron.Core.Scheduling;

/// <summary>
/// Registration used inside <see cref="CoroutineReactor"/> and returned as handles.
/// </summary>
public class CoroutineRegistration
{
    /// <summary>
    /// Set to null when finish.
    /// </summary>
    public IEnumerator<float> Enumerator;
    public long ScheduledUpdate;
    /// <summary>
    /// Can be null if no one is currently waiting, can be set after completion.
    /// </summary>
    public NonBlockingAwaitable CompletionAwaitable;

    public CoroutineRegistration(IEnumerator<float> enumerator)
    {
        Enumerator = enumerator;
        ScheduledUpdate = 0;
        CompletionAwaitable = null;
    }
}