using Reclaim.Calendar;
using Reclaim.Economy;
using UnityEngine;

namespace Reclaim.Population
{
    /// <summary>Resumo do que aconteceu com as necessidades das famílias em um tick.</summary>
    public readonly struct NeedsTickReport
    {
        public NeedsTickReport(int starvingFamilies, int sickFamilies, int deaths, int births)
        {
            StarvingFamilies = starvingFamilies;
            SickFamilies = sickFamilies;
            Deaths = deaths;
            Births = births;
        }

        public int StarvingFamilies { get; }
        public int SickFamilies { get; }
        public int Deaths { get; }
        public int Births { get; }
    }

    /// <summary>
    /// Regras de sobrevivência por família: consumo de comida e água, fome, frio, doença,
    /// morte e nascimento. A comida disponível é dividida igualmente entre todas as famílias.
    /// </summary>
    public class NeedsSystem : MonoBehaviour
    {
        [Header("Consumo por morador por dia")]
        [SerializeField, Min(0f)] private float foodPerMemberPerDay = 1f;
        [SerializeField, Min(0f)] private float waterPerMemberPerDay = 1.2f;

        [Header("Dinâmica das necessidades (por dia)")]
        [SerializeField] private float hungerGainPerFullDeficit = 45f;
        [SerializeField] private float hungerRecovery = 18f;
        [SerializeField] private float healthRegen = 5f;
        [SerializeField] private float healthLossFromStarvation = 20f;
        [SerializeField] private float healthLossFromDehydration = 28f;
        [SerializeField] private float healthLossFromCold = 24f;
        [SerializeField] private float healthLossFromDisease = 16f;
        [Tooltip("Fome a partir da qual a saúde começa a cair.")]
        [SerializeField, Range(0f, 100f)] private float starvationHungerThreshold = 60f;
        [Tooltip("Quantos graus abaixo da tolerância da família equivalem a frio máximo.")]
        [SerializeField, Min(1f)] private float coldGapForMaxPressure = 25f;

        [Header("Pressão na moral (por dia)")]
        [SerializeField] private float moraleLossFromDeficit = 22f;
        [SerializeField] private float moraleLossFromCold = 10f;
        [SerializeField] private float moraleLossFromSickness = 9f;
        [SerializeField] private float moraleRecovery = 4f;

        [Header("Doença (chance por dia)")]
        [SerializeField, Range(0f, 1f)] private float baseSicknessChance = 0.03f;
        [SerializeField] private float sicknessFromHungerMultiplier = 0.45f;
        [SerializeField] private float sicknessFromColdMultiplier = 0.5f;
        [SerializeField, Range(0f, 1f)] private float recoveryChance = 0.14f;

        [Header("Mortalidade e crescimento")]
        [SerializeField, Range(0f, 100f)] private float criticalHealthThreshold = 12f;
        [SerializeField, Range(0f, 1f)] private float deathChancePerMemberAtZeroHealth = 0.5f;
        [SerializeField, Range(0f, 1f)] private float familyGrowthChance = 0.09f;
        [SerializeField, Range(0f, 100f)] private float growthMinHealth = 70f;
        [SerializeField, Range(0f, 100f)] private float growthMinMorale = 65f;
        [SerializeField, Range(0f, 100f)] private float growthMaxHunger = 25f;

        private FamilyManager _familyManager;
        private ResourceStock _resourceStock;

        public void Initialize(FamilyManager familyManager, ResourceStock resourceStock)
        {
            _familyManager = familyManager;
            _resourceStock = resourceStock;
        }

        /// <summary>Quanto a população atual consome por dia. Usado para calcular dias de reserva.</summary>
        public float GetDailyDemand(ResourceType type)
        {
            int population = _familyManager != null ? _familyManager.TotalPopulation : 0;
            switch (type)
            {
                case ResourceType.Food: return population * foodPerMemberPerDay;
                case ResourceType.Water: return population * waterPerMemberPerDay;
                default: return 0f;
            }
        }

        public NeedsTickReport ProcessTick(TickContext context)
        {
            if (_familyManager == null || _resourceStock == null)
            {
                return default;
            }

            float foodDeficit = RationDeficit(ResourceType.Food, context.DeltaDays);
            float waterDeficit = RationDeficit(ResourceType.Water, context.DeltaDays);

            int starving = 0;
            int sick = 0;
            int deaths = 0;
            int births = 0;

            for (int i = 0; i < _familyManager.Families.Count; i++)
            {
                Family family = _familyManager.Families[i];
                if (!family.IsAlive)
                {
                    continue;
                }

                float coldPressure = GetColdPressure(family, context.TemperatureCelsius);
                if (ApplyNutrition(family, foodDeficit, waterDeficit, context.DeltaDays))
                {
                    starving++;
                }

                ApplyHealthPressure(family, waterDeficit, coldPressure, foodDeficit, context.DeltaDays);
                UpdateSickness(family, coldPressure, context.DeltaDays);
                if (family.IsSick)
                {
                    sick++;
                }

                deaths += ApplyMortality(family, context.DeltaDays);
                births += ApplyGrowth(family, context.DeltaDays);
            }

            return new NeedsTickReport(starving, sick, deaths, births);
        }

        /// <summary>Consome a demanda do tick e devolve a fração que faltou (0 = todos comeram).</summary>
        private float RationDeficit(ResourceType type, float deltaDays)
        {
            float required = GetDailyDemand(type) * deltaDays;
            if (required <= 0f)
            {
                return 0f;
            }

            float consumed = _resourceStock.ConsumeUpTo(type, required);
            return Mathf.Clamp01((required - consumed) / required);
        }

        private float GetColdPressure(Family family, float temperatureCelsius)
        {
            float coldGap = family.TemperatureTolerance - temperatureCelsius;
            return coldGap > 0f ? Mathf.Clamp01(coldGap / coldGapForMaxPressure) : 0f;
        }

        private bool ApplyNutrition(Family family, float foodDeficit, float waterDeficit, float deltaDays)
        {
            float combinedDeficit = (foodDeficit + waterDeficit) * 0.5f;
            if (combinedDeficit > 0.01f)
            {
                family.ApplyHunger(hungerGainPerFullDeficit * combinedDeficit * deltaDays);
                family.ApplyMorale(-moraleLossFromDeficit * combinedDeficit * deltaDays);
                return true;
            }

            family.ApplyHunger(-hungerRecovery * deltaDays);
            family.ApplyMorale(moraleRecovery * deltaDays);
            return false;
        }

        private void ApplyHealthPressure(Family family, float waterDeficit, float coldPressure, float foodDeficit, float deltaDays)
        {
            float starvationPressure = Mathf.Clamp01((family.Hunger - starvationHungerThreshold) / (100f - starvationHungerThreshold));

            family.ApplyHealth(-healthLossFromStarvation * starvationPressure * deltaDays);
            family.ApplyHealth(-healthLossFromDehydration * waterDeficit * deltaDays);
            family.ApplyHealth(-healthLossFromCold * coldPressure * deltaDays);
            family.ApplyMorale(-moraleLossFromCold * coldPressure * deltaDays);

            if (family.IsSick)
            {
                family.ApplyHealth(-healthLossFromDisease * deltaDays);
                family.ApplyMorale(-moraleLossFromSickness * deltaDays);
                return;
            }

            bool wellSupplied = (foodDeficit + waterDeficit) * 0.5f < 0.1f && coldPressure < 0.1f;
            if (wellSupplied)
            {
                family.ApplyHealth(healthRegen * deltaDays);
            }
        }

        private void UpdateSickness(Family family, float coldPressure, float deltaDays)
        {
            float hunger01 = family.Hunger / 100f;

            if (!family.IsSick)
            {
                float chance = baseSicknessChance
                               * (1f + family.IllnessVulnerability)
                               * (1f + hunger01 * sicknessFromHungerMultiplier + coldPressure * sicknessFromColdMultiplier)
                               * deltaDays;

                if (Random.value < chance)
                {
                    family.SetSick(true);
                }

                return;
            }

            float recover = recoveryChance * (1f - hunger01 * 0.4f) * (1f - coldPressure * 0.35f) * deltaDays;
            if (Random.value < Mathf.Clamp01(recover))
            {
                family.SetSick(false);
            }
        }

        private int ApplyMortality(Family family, float deltaDays)
        {
            if (family.Health > criticalHealthThreshold || criticalHealthThreshold <= 0f)
            {
                return 0;
            }

            float severity = 1f - Mathf.Clamp01(family.Health / criticalHealthThreshold);
            float chance = deathChancePerMemberAtZeroHealth * severity * deltaDays;

            int deaths = 0;
            for (int member = family.MembersCount - 1; member >= 0; member--)
            {
                if (Random.value < chance)
                {
                    deaths++;
                }
            }

            family.LoseMembers(deaths);
            return deaths;
        }

        private int ApplyGrowth(Family family, float deltaDays)
        {
            bool canGrow = !family.IsSick
                           && family.Health >= growthMinHealth
                           && family.Morale >= growthMinMorale
                           && family.Hunger <= growthMaxHunger;

            if (!canGrow || Random.value >= familyGrowthChance * family.GrowthPotential * deltaDays)
            {
                return 0;
            }

            family.AddMembers(1);
            return 1;
        }
    }
}
