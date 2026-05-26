using System;
using System.Collections.Generic;

namespace Reclaim.Survival.Events
{
    public static class EventBus
    {
        private static readonly Dictionary<string, Action<object>> Channels = new Dictionary<string, Action<object>>();

        public static void Subscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrWhiteSpace(eventName) || handler == null)
            {
                return;
            }

            if (Channels.TryGetValue(eventName, out Action<object> existing))
            {
                Channels[eventName] = existing + handler;
            }
            else
            {
                Channels[eventName] = handler;
            }
        }

        public static void Unsubscribe(string eventName, Action<object> handler)
        {
            if (string.IsNullOrWhiteSpace(eventName) || handler == null)
            {
                return;
            }

            if (!Channels.TryGetValue(eventName, out Action<object> existing))
            {
                return;
            }

            existing -= handler;
            if (existing == null)
            {
                Channels.Remove(eventName);
            }
            else
            {
                Channels[eventName] = existing;
            }
        }

        public static void Emit(string eventName, object payload = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            if (Channels.TryGetValue(eventName, out Action<object> handlers))
            {
                handlers?.Invoke(payload);
            }
        }
    }
}
