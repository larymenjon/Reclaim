using System.Collections.Generic;
using System.Text;
using Reclaim.Calendar;
using Reclaim.Economy;
using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>Textos da HUD montados num só lugar, para todas as telas falarem igual.</summary>
    public static class HudFormat
    {
        public const string Separator = " · ";

        private static readonly StringBuilder Builder = new StringBuilder(64);

        /// <summary>Dias inteiros de reserva; -1 quando não há consumo (não faz sentido falar em dias).</summary>
        public static int ReserveDays(float amount, float dailyDemand)
        {
            return dailyDemand > 0f ? Mathf.FloorToInt(amount / dailyDemand) : -1;
        }

        public static string SignedPerDay(float balance)
        {
            int rounded = Mathf.RoundToInt(balance);
            return rounded > 0 ? $"+{rounded}/dia" : $"{rounded}/dia";
        }

        public static string Date(CalendarDate date) => $"Ano {date.Year}{Separator}Dia {date.DayOfYear}";

        public static string TimeOfDay(CalendarDate date) => $"{PeriodName(date.Hour)} {date.Hour:00}:{date.Minute:00}";

        public static string PeriodName(int hour)
        {
            if (hour < 5) return "Madrugada";
            if (hour < 12) return "Manhã";
            if (hour < 18) return "Tarde";
            return "Noite";
        }

        public static string Temperature(float celsius) => $"{Mathf.RoundToInt(celsius)}°C";

        public static string SeasonName(Season season)
        {
            switch (season)
            {
                case Season.Winter: return "Inverno";
                case Season.Spring: return "Primavera";
                case Season.Summer: return "Verão";
                default: return "Outono";
            }
        }

        /// <summary>Custos (deltas negativos) como "4 remédios, 10 comida". Vazio se não houver custo.</summary>
        public static string Costs(IReadOnlyList<ResourceAmount> deltas, ResourceStock stock)
        {
            Builder.Clear();
            for (int i = 0; i < deltas.Count; i++)
            {
                if (deltas[i].Amount >= 0f)
                {
                    continue;
                }

                if (Builder.Length > 0)
                {
                    Builder.Append(", ");
                }

                Builder.Append(Mathf.RoundToInt(-deltas[i].Amount)).Append(' ').Append(ResourceName(deltas[i].ResourceType, stock));
            }

            return Builder.ToString();
        }

        public static string ResourceName(ResourceType type, ResourceStock stock)
        {
            ResourceDefinition definition = stock != null ? stock.GetDefinition(type) : null;
            string name = definition != null && !string.IsNullOrEmpty(definition.DisplayName) ? definition.DisplayName : type.ToString();
            return name.ToLowerInvariant();
        }
    }
}
