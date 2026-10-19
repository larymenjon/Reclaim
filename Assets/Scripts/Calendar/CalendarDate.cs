using UnityEngine;

namespace Reclaim.Calendar
{
    /// <summary>
    /// Data do jogo derivada de um índice de dia absoluto. Dia 0 é o primeiro dia do ano 1.
    /// Progresso 0 do dia corresponde a 00:00.
    /// </summary>
    public readonly struct CalendarDate
    {
        public const int SeasonsPerYear = 4;

        public CalendarDate(int dayIndex, float dayProgress01, int daysPerSeason)
        {
            DayIndex = Mathf.Max(0, dayIndex);
            DayProgress01 = Mathf.Clamp01(dayProgress01);
            DaysPerSeason = Mathf.Max(1, daysPerSeason);
        }

        public int DayIndex { get; }
        public float DayProgress01 { get; }
        public int DaysPerSeason { get; }

        public int DaysPerYear => DaysPerSeason * SeasonsPerYear;
        public int Year => DayIndex / DaysPerYear + 1;
        public int DayOfYear => DayIndex % DaysPerYear + 1;
        public Season Season => (Season)(DayIndex % DaysPerYear / DaysPerSeason);
        public int DayOfSeason => DayIndex % DaysPerSeason + 1;
        public float SeasonProgress01 => (DayOfSeason - 1 + DayProgress01) / DaysPerSeason;

        public int Hour => Mathf.FloorToInt(DayProgress01 * 24f) % 24;
        public int Minute => Mathf.FloorToInt(DayProgress01 * 24f * 60f) % 60;

        public override string ToString()
        {
            return $"Ano {Year}, dia {DayOfYear} ({Season}) {Hour:00}:{Minute:00}";
        }
    }
}
