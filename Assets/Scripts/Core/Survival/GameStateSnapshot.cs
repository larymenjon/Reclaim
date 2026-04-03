namespace Reclaim.Survival.Core
{
    public readonly struct GameStateSnapshot
    {
        public int TickIndex { get; }
        public int Population { get; }
        public float AverageFamilyMorale { get; }
        public float AverageFamilyHealth { get; }
        public float GlobalHope { get; }
        public float FoodFill01 { get; }
        public float WaterFill01 { get; }
        public float TemperatureCelsius { get; }
        public int HomelessFamilies { get; }

        public GameStateSnapshot(
            int tickIndex,
            int population,
            float averageFamilyMorale,
            float averageFamilyHealth,
            float globalHope,
            float foodFill01,
            float waterFill01,
            float temperatureCelsius,
            int homelessFamilies)
        {
            TickIndex = tickIndex;
            Population = population;
            AverageFamilyMorale = averageFamilyMorale;
            AverageFamilyHealth = averageFamilyHealth;
            GlobalHope = globalHope;
            FoodFill01 = foodFill01;
            WaterFill01 = waterFill01;
            TemperatureCelsius = temperatureCelsius;
            HomelessFamilies = homelessFamilies;
        }
    }
}
