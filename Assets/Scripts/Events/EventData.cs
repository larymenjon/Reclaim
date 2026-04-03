using System.Collections.Generic;
using Reclaim.Survival.Core;
using UnityEngine;

namespace Reclaim.Survival.Events
{
    public enum EventTriggerMode
    {
        RandomWeighted = 0,
        ConditionalCritical = 1
    }

    [CreateAssetMenu(fileName = "EventData", menuName = "Reclaim/Survival/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string eventId = "event_01";
        [SerializeField] private string title = "Critical Situation";
        [SerializeField, TextArea] private string description = "An event requires your decision.";

        [Header("Trigger Settings")]
        [SerializeField] private EventTriggerMode triggerMode = EventTriggerMode.RandomWeighted;
        [SerializeField] private float baseWeight = 1f;
        [SerializeField] private int cooldownTicks = 72;
        [SerializeField] private int minDayToTrigger = 0;
        [SerializeField] private bool oneShot;

        [Header("Conditions")]
        [SerializeField] private bool requireLowFood;
        [SerializeField, Range(0f, 1f)] private float lowFoodThreshold01 = 0.2f;
        [SerializeField] private bool requireLowMorale;
        [SerializeField, Range(0f, 100f)] private float lowMoraleThreshold = 45f;
        [SerializeField] private bool requireLowHope;
        [SerializeField, Range(0f, 100f)] private float lowHopeThreshold = 40f;
        [SerializeField] private bool requireColdWeather;
        [SerializeField] private float coldTemperatureThreshold = -10f;
        [SerializeField] private int minPopulation = 0;
        [SerializeField] private int maxPopulation = 100000;

        [Header("Choices")]
        [SerializeField] private List<EventChoice> choices = new List<EventChoice>();

        public string EventId => eventId;
        public string Title => title;
        public string Description => description;
        public EventTriggerMode TriggerMode => triggerMode;
        public float BaseWeight => Mathf.Max(0.01f, baseWeight);
        public int CooldownTicks => Mathf.Max(0, cooldownTicks);
        public bool OneShot => oneShot;
        public IReadOnlyList<EventChoice> Choices => choices;

        public bool CanTrigger(GameStateSnapshot snapshot, int currentDay, int currentTick, int lastTriggerTick, HashSet<string> firedOneShots)
        {
            if (oneShot && firedOneShots.Contains(eventId))
            {
                return false;
            }

            if (currentDay < minDayToTrigger)
            {
                return false;
            }

            if (lastTriggerTick >= 0 && currentTick - lastTriggerTick < CooldownTicks)
            {
                return false;
            }

            if (snapshot.Population < minPopulation || snapshot.Population > maxPopulation)
            {
                return false;
            }

            if (requireLowFood && snapshot.FoodFill01 > lowFoodThreshold01)
            {
                return false;
            }

            if (requireLowMorale && snapshot.AverageFamilyMorale > lowMoraleThreshold)
            {
                return false;
            }

            if (requireLowHope && snapshot.GlobalHope > lowHopeThreshold)
            {
                return false;
            }

            if (requireColdWeather && snapshot.TemperatureCelsius > coldTemperatureThreshold)
            {
                return false;
            }

            return choices.Count > 0;
        }
    }
}
