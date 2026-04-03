namespace Reclaim.Survival.Core
{
    public readonly struct TickContext
    {
        public int TickIndex { get; }
        public int DayIndex { get; }
        public float DayProgress01 { get; }
        public float DeltaSeconds { get; }
        public float DeltaDays { get; }
        public float TemperatureCelsius { get; }

        public TickContext(int tickIndex, int dayIndex, float dayProgress01, float deltaSeconds, float deltaDays, float temperatureCelsius)
        {
            TickIndex = tickIndex;
            DayIndex = dayIndex;
            DayProgress01 = dayProgress01;
            DeltaSeconds = deltaSeconds;
            DeltaDays = deltaDays;
            TemperatureCelsius = temperatureCelsius;
        }
    }
}
