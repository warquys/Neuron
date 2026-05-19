using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neuron.Core.Scheduling;
using Xunit;

namespace Neuron.Tests.Core.Scheduling;

public class CoroutineAsync
{
    public IEnumerator<float> ThrowingCoroutine()
    {
        yield return 0f;
        throw new TestException();
    }

    public IEnumerator<float> OneTickCoroutine()
    {
        yield return 0f;
    }

    public IEnumerator<float> FiveSecondCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return 1f;
        }
    }

    [Fact]
    public async Task CoroutineCompletesWhenAwaited()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);
        var coroutine = reactor.StartCoroutineAsync(FiveSecondCoroutine(), out var handler);
        await coroutine;
    }

    [Fact]
    public async Task AwaitingOnMainThreadThrowsInvalidOperationException()
    {
        var reactor = new StepByStepCoroutineReactor();
        var coroutine = reactor.StartCoroutineAsync(FiveSecondCoroutine(), out _);
        reactor.Tick();
        Assert.Throws<InvalidOperationException>(() => coroutine.GetAwaiter().GetResult());
    }

    [Fact]
    public async Task CancelledCoroutineThrowsOperationCanceledException()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);
        var coroutine = reactor.StartCoroutineAsync(FiveSecondCoroutine(), out var handler);
        reactor.StopCoroutine(handler);
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await coroutine);
    }

    [Fact]
    public async Task CancelingAfterCompletionStillAllowsAwait()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);
        var coroutine = reactor.StartCoroutineAsync(OneTickCoroutine(), out var handler);
        await Task.Delay(1500);
        reactor.StopCoroutine(handler);
        await coroutine;
    }

    [Fact]
    public async Task StoppingThrowingCoroutinePropagatesException()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);
        var coroutine = reactor.StartCoroutineAsync(ThrowingCoroutine(), out var handler);
        await Task.Delay(1500);
        reactor.StopCoroutine(handler);
        await Assert.ThrowsAsync<TestException>(async () => await coroutine);
    }

    [Fact]
    public async Task ContinuationDoesNotResumeOnMainThread()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);

        int continuationThreadId = await CaptureContinuationThreadId(
            reactor.StartCoroutineAsync(OneTickCoroutine(), out _));

        Assert.NotEqual(reactor.MainThreadId, continuationThreadId);
    }

    private static async Task<int> CaptureContinuationThreadId(NonBlockingAwaitable coroutine)
    {
        await coroutine;
        return Thread.CurrentThread.ManagedThreadId;
    }

    public class TestException : Exception
    {

    }

    public class StepByStepCoroutineReactor : CoroutineReactor
    {
        public new void Tick() => base.Tick();
    }
}
