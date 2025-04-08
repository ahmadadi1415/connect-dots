using System;
using System.Collections.Generic;
public static class EventManager
{
    private static readonly Dictionary<Type, Delegate> _eventHandlers = new();

    public static void Subscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (_eventHandlers.TryGetValue(eventType, out var existingDelegate))
        {
            _eventHandlers[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _eventHandlers[eventType] = listener;
        }
    }

    public static void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (_eventHandlers.TryGetValue(eventType, out var existingDelegate))
        {
            Delegate newDelegate = Delegate.Remove(existingDelegate, listener);
            if (newDelegate == null)
            {
                _eventHandlers.Remove(eventType);
            }
            else
            {
                _eventHandlers[eventType] = newDelegate;
            }
        }
    }

    public static void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);

        if (_eventHandlers.TryGetValue(eventType, out var del))
        {
            Action<T> handler = del as Action<T>;
            handler?.Invoke(eventData);
        }
    }
}
