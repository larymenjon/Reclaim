using System;
using System.Collections.Generic;
using Reclaim.Economy;
using Reclaim.Population;
using UnityEngine;

namespace Reclaim.Events
{
    /// <summary>Efeito de uma escolha sobre as famílias.</summary>
    [Serializable]
    public struct EventFamilyImpact
    {
        [Tooltip("Quantas famílias são afetadas. 0 ou menos afeta todas.")]
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

    /// <summary>
    /// Uma opção de um evento. Valores negativos em Resource Deltas são o custo exibido no botão.
    /// </summary>
    [Serializable]
    public class EventChoice
    {
        [SerializeField] private string choiceId = "choice_01";
        [SerializeField] private string label = "Aceitar";
        [SerializeField, TextArea] private string outcomeText = "";
        [SerializeField] private List<ResourceAmount> resourceDeltas = new List<ResourceAmount>();
        [SerializeField] private EventFamilyImpact familyImpact;
        [SerializeField, Range(-100f, 100f)] private float globalHopeDelta;
        [SerializeField] private float timedHopeDeltaPerDay;
        [SerializeField, Min(0f)] private float timedHopeDurationDays;
        [SerializeField] private FamilyPresetData joinerPreset;
        [SerializeField, Min(0)] private int joinerFamilyCount;

        public string ChoiceId => choiceId;
        public string Label => label;
        public string OutcomeText => outcomeText;
        public IReadOnlyList<ResourceAmount> ResourceDeltas => resourceDeltas;
        public EventFamilyImpact FamilyImpact => familyImpact;
        public float GlobalHopeDelta => globalHopeDelta;
        public float TimedHopeDeltaPerDay => timedHopeDeltaPerDay;
        public float TimedHopeDurationDays => timedHopeDurationDays;
        public FamilyPresetData JoinerPreset => joinerPreset;
        public int JoinerFamilyCount => joinerFamilyCount;

        /// <summary>Escolha só pode ser feita se houver estoque para os custos.</summary>
        public bool IsAffordable(ResourceStock stock)
        {
            for (int i = 0; i < resourceDeltas.Count; i++)
            {
                ResourceAmount delta = resourceDeltas[i];
                if (delta.Amount < 0f && stock.GetAmount(delta.ResourceType) < -delta.Amount)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
