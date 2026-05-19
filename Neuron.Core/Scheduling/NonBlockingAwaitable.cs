using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Neuron.Core.Scheduling;

/// <summary>
/// An awaitable that throws an exception if awaited, preventing soft locks in coroutines.
/// </summary>
public class NonBlockingAwaitable
{
    private int _isCompleted;
    private Exception _exception;
    private bool _isCanceled;
    private readonly int _mainThreadId;
    private Action _continuation;

    public bool IsCompleted => _isCompleted == 1;

    public NonBlockingAwaitable()
    {
        _isCompleted = 0;
        _exception = null;
        _isCanceled = false;
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public NonBlockingAwaitable(int mainThreadId)
    {
        _isCompleted = 0;
        _exception = null;
        _isCanceled = false;
        _mainThreadId = mainThreadId;
    }

    public NonBlockingAwaiter GetAwaiter()
    {
        return new NonBlockingAwaiter(this);
    }

    public async Task AsTask()
    {
        await this;
    }

    internal bool TrySetResult()
    {
        if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) != 0)
            return false;
        var cont = Interlocked.Exchange(ref _continuation, null);
        if (cont != null)
            ThreadPool.QueueUserWorkItem(_ => cont());
        return true;
    }

    internal bool TrySetException(Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) != 0)
            return false;
        _exception = exception;
        var cont = Interlocked.Exchange(ref _continuation, null);
        if (cont != null)
            ThreadPool.QueueUserWorkItem(_ => cont());
        return true;
    }

    internal bool TrySetCanceled()
    {
        if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) != 0)
            return false;
        _isCanceled = true;
        var cont = Interlocked.Exchange(ref _continuation, null);
        if (cont != null)
            ThreadPool.QueueUserWorkItem(_ => cont());
        return true;
    } 
    
    
    public struct NonBlockingAwaiter : INotifyCompletion
    {
        private readonly NonBlockingAwaitable _awaitable;

        public NonBlockingAwaiter(NonBlockingAwaitable awaitable)
        {
            _awaitable = awaitable;
        }

        public bool IsCompleted => _awaitable.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            if (continuation == null) throw new ArgumentNullException(nameof(continuation));

            var prev = Interlocked.Exchange(ref _awaitable._continuation, continuation);
            if (prev != null)
            {
                ThreadPool.QueueUserWorkItem(_ => prev());
            }

            if (_awaitable._isCompleted == 1)
            {
                ThreadPool.QueueUserWorkItem(_ => continuation());
            }
        }

        public void GetResult()
        {
            if (!_awaitable.IsCompleted && Thread.CurrentThread.ManagedThreadId == _awaitable._mainThreadId)
                throw new InvalidOperationException("Synchronous waiting on a coroutine from the main thread is not allowed and would block the main loop. Use await from an async method or WaitEndAsync.");

            if (_awaitable._isCanceled)
                throw new OperationCanceledException("The coroutine was canceled.");
            if (_awaitable._exception != null)
                throw _awaitable._exception;
        }
    }
}
