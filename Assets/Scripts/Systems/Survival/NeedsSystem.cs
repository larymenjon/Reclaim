using Reclaim.Survival.Core;
using Reclaim.Survival.Families;
using Reclaim.Survival.Resources;
using UnityEngine;

namespace Reclaim.Survival.Systems
{
    public readonly struct NeedsTickReport
    {
        public int StarvingFamilies { get; }
        public int SickFamilies { get; }
        public int Deaths { get; }
        public int Births { get; }
        public float AverageHunger { get; }

        public NeedsTickReport(int starvingFamilies, int sickFamilies, int deaths, int births, float averageHunger)
        {
            StarvingFamilies = starvingFamilies;
            SickFamilies = sickFamilies;
            Deaths = deaths;
            Births = births;
            AverageHunger = averageHunger;
        }
    }

    public class NeedsSystem : MonoBehaviour
    {
        [Header("Per Member Consumption (per day)")]
        [SerializeField] private float foodPerMemberPerDay = 1f;
        [SerializeField] private float waterPerMemberPerDay = 1.2f;

        [Header("Need Dynamics")]
        [SerializeField] private float hungerGainPerFullDeficitPerDay = 45f;
        [SerializeField] private float hungerRecoveryPerDay = 18f;
        [SerializeField] private float healthRegenPerDay = 5f;
        [SerializeField] private float healthLossFromStarvationPerDay = 20f;
        [SerializeField] private float healthLossFromDehydrationPerDay = 28f;
        [SerializeField] private float healthLossFromColdPerDay = 24f;
        [SerializeField] private float healthLossFromDiseasePerDay = 16f;

        [Header("Morale Pressure")]
        [SerializeField] private float moraleLossFromDeficitPerDay = 22f;
        [SerializeField] private float moraleLossFromColdPerDay = 10f;
        [SerializeField] private float moraleLossFromSicknessPerDay = 9f;
        [SerializeField] private float moraleRecoveryPerDay = 4f;

        [Header("Sickness")]
        [SerializeField] private float baseSicknessChancePerDay = 0.03f;
        [SerializeField] private float sicknessFromHungerMultiplier = 0.45f;
        [SerializeField] private float sicknessFromColdMultiplier = 0.5f;
        [SerializeField] private float recoveryChancePerDay = 0.14f;

        [Header("Mortality and Growth")]
        [SerializeField] private float criticalHealthThreshold = 12f;
        [SerializeField] private float deathChancePerMemberPerDayAtZeroHealth = 0.5f;
        [SerializeField] private float familyGrowthChancePerDay = 0.09f;
        [SerializeField] private float growthMinHealth = 70f;
        [SerializeField] private float growthMinMorale = 65f;
        [SerializeField] private float growthMaxHunger = 25f;

        private FamilyManager _familyManager;
        private ResourceManager _resourceManager;

        public void Initialize(FamilyManager familyManager, ResourceManager resourceManager)
        {
            _familyManager = familyManager;
            _resourceManager = resourceManager;
        }

        public NeedsTickReport ProcessTick(TickContext context)
        {
            if (_familyManager == null || _resourceManager == null)
            {
                return new NeedsTickReport(0, 0, 0, 0, 0f);
            }

            int starvingFamilies = 0;
            int sickFamilies = 0;
            int deaths = 0;
            int births = 0;
            float hungerAccumulator = 0f;

            for (int i = 0; i < _familyManager.Families.Count; i++)
            {
                Family family = _familyManager.Families[i];
                if (!family.IsAlive)
                {
                    continue;
                }

                float requiredFood = family.MembersCount * foodPerMemberPerDay * context.DeltaDays;
                float requiredWater = family.MembersCount * waterPerMemberPerDay * context.DeltaDays;

                float consumedFood = _resourceManager.ConsumeUpTo(ResourceType.Food, requiredFood);
                float consumedWater = _resourceManager.ConsumeUpTo(ResourceType.Water, requiredWater);

                float foodDeficitRatio = requiredFood <= 0f ? 0f : Mathf.Clamp01((requiredFood - consumedFood) / requiredFood);
                float waterDeficitRatio = requiredWater <= 0f ? 0f : Mathf.Clamp01((requiredWater - consumedWater) / requiredWater);
                float combinedDeficit = Mathf.Clamp01((foodDeficitRatio + waterDeficitRatio) * 0.5f);

                if (combinedDeficit > 0.01f)
                {
                    family.ApplyHunger(hungerGainPerFullDeficitPerDay * combinedDeficit * context.DeltaDays);
                    family.ApplyMorale(-moraleLossFromDeficitPerDay * combinedDeficit * context.DeltaDays);
                    starvingFamilies++;
                }
                else
                {
                    family.ApplyHunger(-hungerRecoveryPerDay * context.DeltaDays);
                    family.ApplyMorale(moraleRecoveryPerDay * context.DeltaDays);
                }

                float hunger01 = family.Hunger / 100f;
                float starvationPressure = Mathf.Clamp01((family.Hunger - 60f) / 40f);
                float temperaturePressure = 0f;
                if (context.TemperatureCelsius < family.TemperatureTolerance)
                {
                    float coldGap = family.TemperatureTolerance - context.TemperatureCelsius;
                    temperaturePressure = Mathf.Clamp01(coldGap / 25f);
                }

                family.ApplyHealth(-healthLossFromStarvationPerDay * starvationPressure * context.DeltaDays);
                family.ApplyHealth(-healthLossFromDehydrationPerDay * waterDeficitRatio * context.DeltaDays);
                family.ApplyHealth(-healthLossFromColdPerDay * temperaturePressure * context.DeltaDays);
                family.ApplyMorale(-moraleLossFromColdPerDay * temperaturePressure * context.DeltaDays);

                if (family.IsSick)
                {
                    family.ApplyHealth(-healthLossFromDiseasePerDay * context.DeltaDays);
                    family.ApplyMorale(-moraleLossFromSicknessPerDay * context.DeltaDays);
                }
                else if (combinedDeficit < 0.1f && temperaturePressure < 0.1f)
                {
                    family.ApplyHealth(healthRegenPerDay * context.DeltaDays);
                }

                if (!family.IsSick)
                {
                    float sicknessChance =
                        baseSicknessChancePerDay
                        * (1f + family.IllnessVulnerability)
                        * (1f + hunger01 * sicknessFromHungerMultiplier + temperaturePressure * sicknessFromColdMultiplier)
                        * context.DeltaDays;

                    if (Random.value < sicknessChance)
                    {
                        family.SetSick(true);
                    }
                }
                else
                {
                    float recoverChance = recoveryChancePerDay
                        * (1f - hunger01 * 0.4f)
                        * (1f - temperaturePressure * 0.35f)
                        * context.DeltaDays;

                    if (Random.value < Mathf.Clamp01(recoverChance))
                    {
                        family.SetSick(false);
                    }
                }

                if (family.IsSick)
                {
                    sickFamilies++;
                }

                if (family.Health <= criticalHealthThreshold && family.MembersCount > 0)
                {
                    float severity = 1f - Mathf.Clamp01(family.Health / criticalHealthThreshold);
                    float deathChance = deathChancePerMemberPerDayAtZeroHealth * severity * context.DeltaDays;

                    for (int member = family.MembersCount - 1; member >= 0; member--)
                    {
                        if (Random.value < deathChance)
                        {
                            family.LoseMembers(1);
                            deaths++;
                        }
                    }
                }

                bool canGrow =
                    family.Health >= growthMinHealth &&
                    family.Morale >= growthMinMorale &&
                    family.Hunger <= growthMaxHunger &&
                    !family.IsSick;

                if (canGrow)
                {
                    float growthChance = familyGrowthChancePerDay * family.GrowthPotential * context.DeltaDays;
                    if (Random.value < growthChance)
                    {
                        family.AddMembers(1);
                        births++;
                    }
                }

                hungerAccumulator += family.Hunger;
            }

            float avgHunger = _familyManager.FamilyCount > 0 ? hungerAccumulator / _familyManager.FamilyCount : 0f;
            return new NeedsTickReport(starvingFamilies, sickFamilies, deaths, births, avgHunger);
        }
    }
}
