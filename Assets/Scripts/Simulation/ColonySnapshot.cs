namespace Reclaim.Simulation
{
    /// <summary>
    /// Fotografia do estado da colônia em um tick. Lida por eventos (condições de disparo),
    /// pela moral e pela HUD, sem que esses sistemas precisem conhecer uns aos outros.
    /// </summary>
    public readonly struct ColonySnapshot
    {
        public ColonySnapshot(
            int tickIndex,
            int dayIndex,
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
            DayIndex = dayIndex;
            Population = population;
            AverageFamilyMorale = averageFamilyMorale;
            AverageFamilyHealth = averageFamilyHealth;
            GlobalHope = globalHope;
            FoodFill01 = foodFill01;
            WaterFill01 = waterFill01;
            TemperatureCelsius = temperatureCelsius;
            HomelessFamilies = homelessFamilies;
        }

        public int TickIndex { get; }
        public int DayIndex { get; }
        public int Population { get; }
        public float AverageFamilyMorale { get; }
        public float AverageFamilyHealth { get; }
        public float GlobalHope { get; }
        public float FoodFill01 { get; }
        public float WaterFill01 { get; }
        public float TemperatureCelsius { get; }
        public int HomelessFamilies { get; }
    }
}
