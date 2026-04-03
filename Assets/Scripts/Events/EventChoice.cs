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
        public ResourceType resourceType;
        public float amount;
    }

    [Serializable]
    public struct EventFamilyImpact
    {
        [Tooltip("If <= 0, impact all families.")]
        public int affectedFamilies;
        public int memberDelta;
        [Range(-100f, 100f)] public float hungerDelta;
        [Range(-100f, 100f)] public float healthDelta;
        [Range(-100f, 100f)] public float moraleDelta;
        [Range(0f, 1f)] public float sicknessChance;
        [Range(0f, 1f)] public float cureChance;
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
