using System;
using System.Collections.Generic;

internal delegate void CustomEventHandler<T>(T @event);

public interface IEvent
{
    object sender { get; set; }
}

public interface IEventHandler<T> where T : IEvent
{
    // Classes that want to catch events will have to define how to handle them
    // The '@' before event is just because event is a reserved keyword, we could have given another name to the variable
    void Handle(T @event);
}

public abstract class Event : IEvent
{
    public object sender { get; set; }

    protected Event() { } // This makes users not having to implement a default constructor
}

public static class IEventHandlerExtensions
{
    // We want these methods to be accessible only to the classes who inherit from IEventHandler<T>

    /// <summary>
    /// Don't forget to unsubscribe when this object is destroyed.
    /// </summary>
    public static void Subscribe<T>(this IEventHandler<T> handler) where T : IEvent
    {
        EventHandlersManager<T>.Add(new CustomEventHandler<T>(handler.Handle));
    }

    public static void UnSubscribe<T>(this IEventHandler<T> handler) where T : IEvent
    {
        EventHandlersManager<T>.Remove(new CustomEventHandler<T>(handler.Handle));
    }
}

public static class ObjectExtensions
{
    // We want this method to be accessible from any object

    public static void Publish<T>(this object sender, T @event = default(T)) where T : IEvent, new() // Ensure the event has a parameterless constructor by using 'new()' constraint
    {
        if (EqualityComparer<T>.Default.Equals(@event, default(T))) // We gave no constructor for this event
        {
            @event = new T(); // We construct the event with its default constructor
        }
        @event.sender = sender;
        EventHandlersManager<T>.RaiseEvent(@event);
    }
}

internal class EventHandlersManager<T>
{
    private static Dictionary<Type, EventElement<T>> eventHandlers = new Dictionary<Type, EventElement<T>>();


    public static void Add(CustomEventHandler<T> handler)
    {
        Type typeT = typeof(T);

        if (!eventHandlers.ContainsKey(typeT))
        {
            eventHandlers.Add(typeT, new EventElement<T>());
        }

        eventHandlers[typeT].EventRaiser += handler;
    }

    public static void Remove(CustomEventHandler<T> handler)
    {
        Type typeT = typeof(T);

        if (eventHandlers.ContainsKey(typeT))
        {
            eventHandlers[typeT].EventRaiser -= handler;
        }
    }

    public static void RaiseEvent(T @event)
    {
        Type typeT = typeof(T);

        if (eventHandlers.ContainsKey(typeT))
        {
            eventHandlers[typeT].Raise(@event);
        }
    }
}

internal class EventElement<T>
{
    public event CustomEventHandler<T> EventRaiser;

    public void Raise(T @event)
    {
        EventRaiser?.Invoke(@event);
    }
}
