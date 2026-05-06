using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Neuron.Core.Scheduling;
using Xunit;

namespace Neuron.Tests.Core.Scheduling;

public class CoroutineAsync
{
    [Fact]
    public async Task Awaitable()
    {
        var reactor = new LoopingCoroutineReactor();
        Task runningReactor = Task.Run(reactor.Start);
        var coroutine = reactor.StartCoroutineAsync(LongCoroutine());
        await coroutine;
    }

    public IEnumerator<float> LongCoroutine()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return 1f;
        }
    }

    [Fact]
    public async Task DirectAwaitThrowsException()
    {
        var reactor = new StepByStepCoroutineReactor();
        var coroutine = reactor.StartCoroutineAsync(LongCoroutine());
        reactor.Tick();
        Assert.Throws<InvalidOperationException>(() => coroutine.GetAwaiter().GetResult());
    }


    public class StepByStepCoroutineReactor : CoroutineReactor
    {
        public new void Tick() => base.Tick();
    }
}
