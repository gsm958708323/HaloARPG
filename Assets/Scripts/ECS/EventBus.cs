using System;
using System.Collections.Generic;
public interface IEvent { }

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _listeners = new();

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!_listeners.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _listeners[type] = list;
        }
        list.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        if (_listeners.TryGetValue(typeof(T), out var list))
        {
            list.Remove(handler);
        }
    }

    public void Publish<T>(T evt) where T : IEvent
    {
        if (_listeners.TryGetValue(typeof(T), out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                ((Action<T>)list[i]).Invoke(evt);
            }
        }
    }
}