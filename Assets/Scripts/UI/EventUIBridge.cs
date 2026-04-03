using System;
using Reclaim.Survival.Events;
using UnityEngine;

namespace Reclaim.Survival.UI
{
    /// <summary>
    /// UI hook layer for event presentation.
    /// Connect buttons to ResolveChoice(index).
    /// </summary>
    public class EventUIBridge : MonoBehaviour
    {
        [SerializeField] private EventManager eventManager;

        public event Action<ActiveEvent> OnEventShown;
        public event Action<ActiveEvent, EventChoice> OnEventClosed;

        private void Awake()
        {
            if (eventManager == null)
            {
                eventManager = FindFirstObjectByType<EventManager>();
            }

            if (eventManager != null)
            {
                eventManager.OnEventTriggered += HandleEventTriggered;
                eventManager.OnEventResolved += HandleEventResolved;
            }
        }

        private void OnDestroy()
        {
            if (eventManager == null)
            {
                return;
            }

            eventManager.OnEventTriggered -= HandleEventTriggered;
            eventManager.OnEventResolved -= HandleEventResolved;
        }

        public bool ResolveChoice(int choiceIndex)
        {
            return eventManager != null && eventManager.ResolveCurrentEvent(choiceIndex);
        }

        private void HandleEventTriggered(ActiveEvent activeEvent)
        {
            OnEventShown?.Invoke(activeEvent);
        }

        private void HandleEventResolved(ActiveEvent activeEvent, EventChoice choice)
        {
            OnEventClosed?.Invoke(activeEvent, choice);
        }
    }
}
