using System;
using System.Collections.Generic;
using Reclaim.Survival.Families;
using Reclaim.Survival.Resources;
using UnityEngine;

namespace Reclaim.Survival.Events
{
    [Serializable]
    public struct EventResourceDelta
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private float amount;

        public ResourceType ResourceType => resourceType;
        public float Amount => amount;
    }

    [Serializable]
    public struct EventFamilyImpact
    {
        [Tooltip("If <= 0, impact all families.")]
        [SerializeField] private int affectedFamilies;
        [SerializeField] private int memberDelta;
        [SerializeField, Range(-100f, 100f)] private float hungerDelta;
        [SerializeField, Range(-100f, 100f)] private float healthDelta;
        [SerializeField, Range(-100f, 100f)] private float moraleDelta;
        [SerializeField, Range(0f, 1f)] private float sicknessChance;
        [SerializeField, Range(0f, 1f)] private float cureChance;

        public int AffectedFamilies => affectedFamilies;
        public int MemberDelta => memberDelta;
        public float HungerDelta => hungerDelta;
        public float HealthDelta => healthDelta;
        public float MoraleDelta => moraleDelta;
        public float SicknessChance => sicknessChance;
        public float CureChance => cureChance;
    }

    [Serializable]
    public class EventChoice
    {
        [SerializeField] private string choiceId = "choice_01";
        [SerializeField] private string label = "Accept";
        [SerializeField, TextArea] private string outcomeText = "Consequence applied.";
        [SerializeField] private List<EventResourceDelta> resourceDeltas = new List<EventResourceDelta>();
        [SerializeField] private EventFamilyImpact familyImpact;
        [SerializeField, Range(-100f, 100f)] private float globalHopeDelta;
        [SerializeField] private float timedHopeDeltaPerDay;
        [SerializeField] private float timedHopeDurationDays;
        [SerializeField] private FamilyPresetData joinerPreset;
        [SerializeField] private int joinerFamilyCount;

        public string ChoiceId => choiceId;
        public string Label => label;
        public string OutcomeText => outcomeText;
        public IReadOnlyList<EventResourceDelta> ResourceDeltas => resourceDeltas;
        public EventFamilyImpact FamilyImpact => familyImpact;
        public float GlobalHopeDelta => globalHopeDelta;
        public float TimedHopeDeltaPerDay => timedHopeDeltaPerDay;
        public float TimedHopeDurationDays => timedHopeDurationDays;
        public FamilyPresetData JoinerPreset => joinerPreset;
        public int JoinerFamilyCount => joinerFamilyCount;
    }
}
