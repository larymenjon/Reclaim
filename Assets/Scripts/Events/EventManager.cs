using System;
using System.Collections.Generic;
using Reclaim.Calendar;
using Reclaim.Economy;
using Reclaim.Morale;
using Reclaim.Population;
using Reclaim.Simulation;
using UnityEngine;

namespace Reclaim.Events
{
    public readonly struct ActiveEvent
    {
        public ActiveEvent(EventData data, int triggerTick)
        {
            Data = data;
            TriggerTick = triggerTick;
        }

        public EventData Data { get; }
        public int TriggerTick { get; }
    }

    /// <summary>
    /// Escolhe qual evento disparar, mantém um evento ativo por vez e aplica a escolha do jogador.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        [SerializeField] private List<EventData> eventDatabase = new List<EventData>();
        [Tooltip("De quantos em quantos ticks o diretor avalia se dispara um evento.")]
        [SerializeField, Min(1)] private int evaluationIntervalTicks = 6;
        [Tooltip("0 gera uma semente diferente a cada partida.")]
        [SerializeField] private int randomSeed;

        private readonly Dictionary<string, int> _lastTriggerTickByEvent = new Dictionary<string, int>();
        private readonly HashSet<string> _firedOneShots = new HashSet<string>();
        private readonly List<EventData> _candidates = new List<EventData>();
        private readonly List<Family> _targets = new List<Family>();

        private System.Random _rng;
        private ResourceStock _resourceStock;
        private FamilyManager _familyManager;
        private MoraleSystem _moraleSystem;
        private ActiveEvent? _activeEvent;

        public bool HasActiveEvent => _activeEvent.HasValue;
        public ActiveEvent? CurrentEvent => _activeEvent;

        public event Action<ActiveEvent> OnEventTriggered;
        public event Action<ActiveEvent, EventChoice> OnEventResolved;

        public void Initialize(ResourceStock resourceStock, FamilyManager familyManager, MoraleSystem moraleSystem)
        {
            _resourceStock = resourceStock;
            _familyManager = familyManager;
            _moraleSystem = moraleSystem;
            _rng = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);

            _activeEvent = null;
            _lastTriggerTickByEvent.Clear();
            _firedOneShots.Clear();
        }

        public void ProcessTick(TickContext context, ColonySnapshot snapshot)
        {
            if (_activeEvent.HasValue || eventDatabase.Count == 0 || context.TickIndex % evaluationIntervalTicks != 0)
            {
                return;
            }

            EventData selected = SelectEvent(snapshot, context.DayIndex, context.TickIndex);
            if (selected == null)
            {
                return;
            }

            _activeEvent = new ActiveEvent(selected, context.TickIndex);
            _lastTriggerTickByEvent[selected.EventId] = context.TickIndex;
            if (selected.OneShot)
            {
                _firedOneShots.Add(selected.EventId);
            }

            OnEventTriggered?.Invoke(_activeEvent.Value);
        }

        public bool CanChoose(int choiceIndex)
        {
            return TryGetChoice(choiceIndex, out EventChoice choice) && choice.IsAffordable(_resourceStock);
        }

        public bool ResolveCurrentEvent(int choiceIndex)
        {
            if (!TryGetChoice(choiceIndex, out EventChoice choice))
            {
                return false;
            }

            if (!choice.IsAffordable(_resourceStock))
            {
                Debug.LogWarning($"[EventManager] Escolha '{choice.ChoiceId}' sem recursos suficientes.", this);
                return false;
            }

            ActiveEvent resolved = _activeEvent.Value;
            ApplyChoice(choice);
            _activeEvent = null;
            OnEventResolved?.Invoke(resolved, choice);
            return true;
        }

        private bool TryGetChoice(int choiceIndex, out EventChoice choice)
        {
            choice = null;
            if (!_activeEvent.HasValue || _resourceStock == null)
            {
                return false;
            }

            IReadOnlyList<EventChoice> choices = _activeEvent.Value.Data.Choices;
            if (choiceIndex < 0 || choiceIndex >= choices.Count)
            {
                return false;
            }

            choice = choices[choiceIndex];
            return choice != null;
        }

        private EventData SelectEvent(ColonySnapshot snapshot, int dayIndex, int tickIndex)
        {
            _candidates.Clear();
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

                if (data.IsCritical)
                {
                    return data;
                }

                _candidates.Add(data);
                totalWeight += data.BaseWeight;
            }

            return PickWeighted(totalWeight);
        }

        private EventData PickWeighted(float totalWeight)
        {
            if (_candidates.Count == 0)
            {
                return null;
            }

            float roll = (float)_rng.NextDouble() * totalWeight;
            for (int i = 0; i < _candidates.Count; i++)
            {
                roll -= _candidates[i].BaseWeight;
                if (roll <= 0f)
                {
                    return _candidates[i];
                }
            }

            return _candidates[_candidates.Count - 1];
        }

        private void ApplyChoice(EventChoice choice)
        {
            IReadOnlyList<ResourceAmount> deltas = choice.ResourceDeltas;
            for (int i = 0; i < deltas.Count; i++)
            {
                _resourceStock.ApplyDelta(deltas[i].ResourceType, deltas[i].Amount);
            }

            ApplyFamilyImpact(choice.FamilyImpact);

            if (_moraleSystem != null)
            {
                _moraleSystem.AddImmediateHope(choice.GlobalHopeDelta);
                _moraleSystem.AddTimedHopeModifier(choice.TimedHopeDeltaPerDay, choice.TimedHopeDurationDays);
            }

            for (int i = 0; i < choice.JoinerFamilyCount && choice.JoinerPreset != null; i++)
            {
                _familyManager.AddFamily(choice.JoinerPreset);
            }
        }

        private void ApplyFamilyImpact(EventFamilyImpact impact)
        {
            if (_familyManager == null)
            {
                return;
            }

            _familyManager.PickRandomFamilies(impact.AffectedFamilies, _targets);
            for (int i = 0; i < _targets.Count; i++)
            {
                ApplyImpactToFamily(_targets[i], impact);
            }
        }

        private void ApplyImpactToFamily(Family family, EventFamilyImpact impact)
        {
            family.ApplyHunger(impact.HungerDelta);
            family.ApplyHealth(impact.HealthDelta);
            family.ApplyMorale(impact.MoraleDelta);

            if (impact.MemberDelta > 0)
            {
                family.AddMembers(impact.MemberDelta);
            }
            else if (impact.MemberDelta < 0)
            {
                family.LoseMembers(-impact.MemberDelta);
            }

            if (impact.SicknessChance > 0f && _rng.NextDouble() < impact.SicknessChance)
            {
                family.SetSick(true);
            }

            if (impact.CureChance > 0f && _rng.NextDouble() < impact.CureChance)
            {
                family.SetSick(false);
            }
        }
    }
}
