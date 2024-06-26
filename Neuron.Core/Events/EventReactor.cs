﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Neuron.Core.Logging;
using Neuron.Core.Meta;

namespace Neuron.Core.Events;

/// <summary>
/// .Net event based proxy event handler and invocator.
/// All events used by the class should have mutable public properties for their data.
/// Event subscribers should manipulate the properties of the event they receive to alter data.
/// </summary>
/// <typeparam name="T">type of the event</typeparam>
public class EventReactor<T> : IEventReactor where T : IEvent
{
    private readonly object _locker = new object();
    private readonly List<HandlerRegistration<T>> _registrations = new();
    private readonly HandlerRegistrationComparer<T> _comparer = new ();

    public int Registred => _registrations.Count;
    public bool Used => Registred > 0;


    /// <summary>
    /// Invokes the multicast event system.
    /// See <see cref="EventReactor{T}"/> for details about required property attributes.
    /// </summary>
    /// <param name="evt">the event argument object</param>
    public void Raise(T evt)
    {
        if (!Used) return;

        lock (_locker)
        {
            foreach (var registration in _registrations)
            {
                registration.Handler.Invoke(evt);
            }    
        }
    }

    /// <summary>
    /// Subscribes a delegate to the backing event.
    /// </summary>
    /// <param name="handler">the delegate to subscribe</param>
    /// <param name="priority">the priority of the subscription, the lower take the event first.</param>
    public void Subscribe(EventHandler<T> handler, int priority = 0)
    {
        lock (_locker)
        {
            _registrations.Add(new HandlerRegistration<T>(priority, handler));
            _registrations.Sort(_comparer);
        }
    }

    /// <summary>
    /// Unsubscribes a delegate from the backing event.
    /// </summary>
    /// <param name="handler">the delegate to unsubscribe</param>
    public void Unsubscribe(EventHandler<T> handler)
    {
        lock (_locker)
        { 
            _registrations.RemoveAll(x => x.Handler == handler);
        }
    }

    /// <summary>
    /// Returns the type of the generic <typeparamref name="T"/>.
    /// </summary>
    public Type TypeDelegate() => typeof(T);

    /// <summary>
    /// Invokes the multicast event system.
    /// See <see cref="EventReactor{T}"/> for details about required property attributes.
    /// </summary>
    /// <param name="obj">the delegate boxed as an object</param>
    public void RaiseUnsafe(object obj)
    {
        Raise((T)obj);
    }

    /// <summary>
    /// Subscribes a method to the backing event.
    /// Uses reflections to create method delegates of type <typeparamref name="T"/>.
    /// which can be subscribed normally.
    /// </summary>
    /// <param name="obj">the instance of object which method shall be hooked</param>
    /// <param name="info">the method which shall be hooked</param>
    /// <param name="priority">the priority, the lower priority takes the event first</param>
    public object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0)
    {
        var handler = DelegateUtils.CreateDelegate<EventHandler<T>>(obj, info);
        Subscribe(handler, priority);
        return handler;
    }
    
    /// <summary>
    /// Unsubscribes a delegate from the backing event
    /// </summary>
    /// <param name="subscription">the delegate boxed as an object</param>
    public void UnsubscribeUnsafe(object subscription)
    {
        Unsubscribe(subscription as EventHandler<T>);
    }
}

internal class DelegateUtils
{
    public static T CreateDelegate<T>(MethodInfo info) where T : Delegate
    {
        var delegateType = typeof(T);
        var delegated = info.CreateDelegate(delegateType);
        return (T)delegated;   
    }
        
    public static T CreateDelegate<T>(object instance, MethodInfo info) where T : Delegate
    {
        var delegateType = typeof(T);
        var delegated = info.CreateDelegate(delegateType, instance);
        return (T)delegated;
    }
}

public struct HandlerRegistration<T> where T : IEvent
{
    public int Priority { get; }
    public EventHandler<T> Handler { get; }

    public HandlerRegistration(int priority, EventHandler<T> handler)
    {
        Priority = priority;
        Handler = handler;
    }
}

public class HandlerRegistrationComparer<T> : IComparer<HandlerRegistration<T>> where T: IEvent
{
    public int Compare(HandlerRegistration<T> x, HandlerRegistration<T> y)
    {
        return y.Priority.CompareTo(x.Priority);
    }
}

public static class VoidEventExtension
{

    public static readonly VoidEvent RecyclableEvent = new();
    
    /// <summary>
    /// Invokes the multicast event system.
    /// See <see cref="EventReactor{T}"/> for details about required property attributes.
    /// </summary>
    public static void Raise(this EventReactor<VoidEvent> reactor)
    {
        reactor.Raise(RecyclableEvent);
    }

    /// <summary>
    /// Subscribes a method to the backing event.
    /// Uses reflections to create method delegates of type T
    /// which can be subscribed normally. Uses an inlined delegate
    /// to wrap the action into the matching VoidEvent delegate type.
    /// </summary>
    /// <param name="reactor">the reactor to subscribe to</param>
    /// <param name="obj">the instance of object which method shall be hooked</param>
    /// <param name="info">the method which shall be hooked</param>
    public static object SubscribeAction(this EventReactor<VoidEvent> reactor, object obj, MethodInfo info)
    {
        var action = DelegateUtils.CreateDelegate<Action>(obj, info);
        EventHandler<VoidEvent> handler = _ => action.Invoke();
        reactor.Subscribe(handler);
        return handler;
    }
}

public delegate void EventHandler<in T>(T args) where T : IEvent;

public class VoidEvent : IEvent { }

public interface IEventReactor
{
    Type TypeDelegate();
    void RaiseUnsafe(object obj);
    object SubscribeUnsafe(object obj, MethodInfo info, int priority = 0);
    void UnsubscribeUnsafe(object subscription);
}