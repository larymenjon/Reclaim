using System;
using System.Collections.Generic;
using Reclaim.Survival.Core;
using Reclaim.Survival.Families;
using UnityEngine;

namespace Reclaim.Survival.Systems
{
    public class MoraleSystem : MonoBehaviour
    {
        [Serializable]
        private struct TimedModifier
        {
            public float ValuePerDay;
            public float RemainingDays;

            public TimedModifier(float valuePerDay, float remainingDays)
            {
                ValuePerDay = valuePerDay;
                RemainingDays = remainingDays;
            }
        }

        [SerializeField, Range(0f, 100f)] private float initialGlobalHope = 60f;
        [SerializeField] private float hopeDriftSpeedPerDay = 10f;
        [SerializeField] private float lowResourcesHopePenalty = 20f;
        [SerializeField] private float starvationHopePenalty = 16f;
        [SerializeField] private float sicknessHopePenalty = 12f;
        [SerializeField] private float averageMoraleWeight = 0.6f;

        private readonly List<TimedModifier> _timedModifiers = new List<TimedModifier>();
        private FamilyManager _familyManager;
        private float _globalHope;

        public event Action<float> OnGlobalHopeChanged;
        public float GlobalHope => _globalHope;

        public void Initialize(FamilyManager familyManager)
        {
            _familyManager = familyManager;
            _globalHope = initialGlobalHope;
        }

        public void AddImmediateHope(float delta)
        {
            SetGlobalHope(_globalHope + delta);
        }

        public void AddTimedHopeModifier(float valuePerDay, float durationDays)
        {
            if (Mathf.Approximately(valuePerDay, 0f) || durationDays <= 0f)
            {
                return;
            }

            _timedModifiers.Add(new TimedModifier(valuePerDay, durationDays));
        }

        public void ProcessTick(TickContext context, NeedsTickReport needsReport, GameStateSnapshot snapshot)
        {
            float averageFamilyMorale = _familyManager != null ? _familyManager.GetAverageMorale() : 0f;
            float targetHope = averageFamilyMorale * averageMoraleWeight + _globalHope * (1f - averageMoraleWeight);

            if (snapshot.FoodFill01 < 0.2f || snapshot.WaterFill01 < 0.2f)
            {
                targetHope -= lowResourcesHopePenalty;
            }

            if (needsReport.StarvingFamilies > 0)
            {
                float starvingRatio = _familyManager.FamilyCount <= 0 ? 0f : (float)needsReport.StarvingFamilies / _familyManager.FamilyCount;
                targetHope -= starvationHopePenalty * starvingRatio;
            }

            if (needsReport.SickFamilies > 0)
            {
                float sickRatio = _familyManager.FamilyCount <= 0 ? 0f : (float)needsReport.SickFamilies / _familyManager.FamilyCount;
                targetHope -= sicknessHopePenalty * sickRatio;
            }

            for (int i = _timedModifiers.Count - 1; i >= 0; i--)
            {
                TimedModifier modifier = _timedModifiers[i];
                targetHope += modifier.ValuePerDay * context.DeltaDays;

                modifier.RemainingDays -= context.DeltaDays;
                if (modifier.RemainingDays <= 0f)
                {
                    _timedModifiers.RemoveAt(i);
                }
                else
                {
                    _timedModifiers[i] = modifier;
                }
            }

            float smoothedHope = Mathf.MoveTowards(_globalHope, Mathf.Clamp(targetHope, 0f, 100f), hopeDriftSpeedPerDay * context.DeltaDays);
            SetGlobalHope(smoothedHope);
        }

        private void SetGlobalHope(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, 100f);
            if (Mathf.Approximately(clamped, _globalHope))
            {
                return;
            }

            _globalHope = clamped;
            OnGlobalHopeChanged?.Invoke(_globalHope);
        }
    }
}
