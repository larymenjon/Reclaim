namespace Reclaim.Calendar
{
    /// <summary>
    /// Tudo o que um sistema da simulação precisa saber sobre o tick atual.
    /// Montado pela ColonySimulation a partir do relógio e do clima.
    /// </summary>
    public readonly struct TickContext
    {
        public TickContext(int tickIndex, CalendarDate date, float deltaDays, float temperatureCelsius)
        {
            TickIndex = tickIndex;
            Date = date;
            DeltaDays = deltaDays;
            TemperatureCelsius = temperatureCelsius;
        }

        public int TickIndex { get; }
        public CalendarDate Date { get; }
        public float DeltaDays { get; }
        public float TemperatureCelsius { get; }

        public int DayIndex => Date.DayIndex;
        public float DayProgress01 => Date.DayProgress01;
    }
}
