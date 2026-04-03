using System;
using System.Collections.Generic;
using Reclaim.Survival.Core;
using Reclaim.Survival.Families;
using Reclaim.Survival.Resources;
using Reclaim.Survival.Systems;
using UnityEngine;

namespace Reclaim.Survival.Events
{
    public readonly struct ActiveEvent
    {
        public EventData Data { get; }
        public int TriggerTick { get; }

        public ActiveEvent(EventData data, int triggerTick)
        {
            Data = data;
            TriggerTick = triggerTick;
        }
    }

    public class EventManager : MonoBehaviour
    {
        [SerializeField] private List<EventData> eventDatabase = new List<EventData>();
        [SerializeField] private int evaluationIntervalTicks = 6;
        [SerializeField] private int randomSeed;

        private readonly Dictionary<string, int> _lastTriggerTickByEvent = new Dictionary<string, int>();
        private readonly HashSet<string> _firedOneShots = new HashSet<string>();
        private System.Random _rng;

        private ResourceManager _resourceManager;
        private FamilyManager _familyManager;
        private MoraleSystem _moraleSystem;
        private ActiveEvent? _activeEvent;

        public event Action<ActiveEvent> OnEventTriggered;
        public event Action<ActiveEvent, EventChoice> OnEventResolved;

        public bool HasActiveEvent => _activeEvent.HasValue;
        public ActiveEvent? CurrentEvent => _activeEvent;

        private void Awake()
        {
            _rng = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);
        }

        public void Initialize(ResourceManager resourceManager, FamilyManager familyManager, MoraleSystem moraleSystem)
        {
            _resourceManager = resourceManager;
            _familyManager = familyManager;
            _moraleSystem = moraleSystem;
        }

        public void ProcessTick(TickContext context, GameStateSnapshot snapshot)
        {
            if (_activeEvent.HasValue || eventDatabase.Count == 0)
            {
                return;
            }

            if (evaluationIntervalTicks > 1 && context.TickIndex % evaluationIntervalTicks != 0)
            {
                return;
            }

            EventData selected = SelectEvent(snapshot, context.DayIndex, context.TickIndex);
            if (selected == null)
            {
                return;
            }

            ActiveEvent active = new ActiveEvent(selected, context.TickIndex);
            _activeEvent = active;
            _lastTriggerTickByEvent[selected.EventId] = context.TickIndex;
            if (selected.OneShot)
            {
                _firedOneShots.Add(selected.EventId);
            }

            OnEventTriggered?.Invoke(active);
        }

        public bool ResolveCurrentEvent(int choiceIndex)
        {
            if (!_activeEvent.HasValue)
            {
                return false;
            }

            ActiveEvent active = _activeEvent.Value;
            if (active.Data.Choices == null || choiceIndex < 0 || choiceIndex >= active.Data.Choices.Count)
            {
                return false;
            }

            EventChoice choice = active.Data.Choices[choiceIndex];
            ApplyChoice(choice);

            _activeEvent = null;
            OnEventResolved?.Invoke(active, choice);
            return true;
        }

        private EventData SelectEvent(GameStateSnapshot snapshot, int dayIndex, int tickIndex)
        {
            List<EventData> candidates = new List<EventData>();
            float totalWeight = 0f;

            for (int i = 0; i < eventDatabase.Count; i++)
            {
                EventData data = eventDatabase[i];
                if (data == null)
                {
                    continue;
                }

                int lastTick = _lastTriggerTickByEvent.TryGetValue(data.EventId, out int value) ? value : -1;
                if (!data.CanTrigger(snapshot, dayIndex, tickIndex, lastTick, _firedOneShots))
                {
                    continue;
                }

                if (data.TriggerMode == EventTriggerMode.ConditionalCritical)
                {
                    return data;
                }

                candidates.Add(data);
                totalWeight += data.BaseWeight;
            }

            if (candidates.Count == 0 || totalWeight <= 0f)
            {
                return null;
            }

            float roll = (float)_rng.NextDouble() * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += candidates[i].BaseWeight;
                if (roll <= cumulative)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private void ApplyChoice(EventChoice choice)
        {
            for (int i = 0; i < choice.ResourceDeltas.Count; i++)
            {
                EventResourceDelta delta = choice.ResourceDeltas[i];
                _resourceManager.ApplyDelta(delta.resourceType, delta.amount);
            }

            ApplyFamilyImpact(choice.FamilyImpact);

            _moraleSystem.AddImmediateHope(choice.GlobalHopeDelta);
            _moraleSystem.AddTimedHopeModifier(choice.TimedHopeDeltaPerDay, choice.TimedHopeDurationDays);

            if (choice.JoinerPreset != null && choice.JoinerFamilyCount > 0)
            {
                for (int i = 0; i < choice.JoinerFamilyCount; i++)
                {
                    _familyManager.AddFamily(choice.JoinerPreset);
                }
            }
        }

        private void ApplyFamilyImpact(EventFamilyImpact impact)
        {
            if (_familyManager == null || _familyManager.FamilyCount == 0)
            {
                return;
            }

            List<Family> targets = new List<Family>();
            if (impact.affectedFamilies <= 0 || impact.affectedFamilies >= _familyManager.FamilyCount)
            {
                for (int i = 0; i < _familyManager.FamilyCount; i++)
                {
                    targets.Add(_familyManager.Families[i]);
                }
            }
            else
            {
                HashSet<int> selectedIndices = new HashSet<int>();
                while (selectedIndices.Count < impact.affectedFamilies && selectedIndices.Count < _familyManager.FamilyCount)
                {
                    selectedIndices.Add(_rng.Next(0, _familyManager.FamilyCount));
                }

                foreach (int index in selectedIndices)
                {
                    targets.Add(_familyManager.Families[index]);
                }
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Family family = targets[i];
                family.ApplyHunger(impact.hungerDelta);
                family.ApplyHealth(impact.healthDelta);
                family.ApplyMorale(impact.moraleDelta);

                if (impact.memberDelta > 0)
                {
                    family.AddMembers(impact.memberDelta);
                }
                else if (impact.memberDelta < 0)
                {
                    family.LoseMembers(-impact.memberDelta);
                }

                if (impact.sicknessChance > 0f && UnityEngine.Random.value < impact.sicknessChance)
                {
                    family.SetSick(true);
                }

                if (impact.cureChance > 0f && UnityEngine.Random.value < impact.cureChance)
                {
                    family.SetSick(false);
                }
            }
        }
    }
}
