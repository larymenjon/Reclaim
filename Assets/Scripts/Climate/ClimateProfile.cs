using Reclaim.Calendar;
using UnityEngine;

namespace Reclaim.Climate
{
    /// <summary>
    /// Curva de temperatura de um mapa. Cada estação tem uma média; a transição entre estações
    /// é suave e a variação diária segue um cosseno com mínimo de madrugada.
    /// </summary>
    [CreateAssetMenu(fileName = "ClimateProfile", menuName = "Reclaim/Clima/Perfil de Clima")]
    public class ClimateProfile : ScriptableObject
    {
        [Header("Média por estação (°C)")]
        [SerializeField] private float winterMean = -10f;
        [SerializeField] private float springMean = 4f;
        [SerializeField] private float summerMean = 18f;
        [SerializeField] private float autumnMean = 3f;

        [Header("Variação diária")]
        [Tooltip("Diferença entre a hora mais quente e a mais fria do dia.")]
        [SerializeField, Min(0f)] private float dailyRange = 8f;
        [SerializeField, Range(0f, 23f)] private float coldestHour = 5f;

        public float EvaluateCelsius(CalendarDate date)
        {
            return EvaluateSeasonalMean(date) + EvaluateDailyOffset(date.DayProgress01);
        }

        private float EvaluateSeasonalMean(CalendarDate date)
        {
            // Posição contínua no ano medida em estações, com o centro de cada estação em x.5.
            float yearPosition = (date.DayOfYear - 1 + date.DayProgress01) / date.DaysPerSeason - 0.5f;
            int fromIndex = Mathf.FloorToInt(yearPosition);
            float t = yearPosition - fromIndex;
            float smoothT = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);

            float from = GetMean(fromIndex);
            float to = GetMean(fromIndex + 1);
            return Mathf.Lerp(from, to, smoothT);
        }

        private float EvaluateDailyOffset(float dayProgress01)
        {
            float hoursFromColdest = dayProgress01 * 24f - coldestHour;
            return -Mathf.Cos(hoursFromColdest / 24f * Mathf.PI * 2f) * dailyRange * 0.5f;
        }

        private float GetMean(int seasonIndex)
        {
            int wrapped = ((seasonIndex % CalendarDate.SeasonsPerYear) + CalendarDate.SeasonsPerYear) % CalendarDate.SeasonsPerYear;
            switch ((Season)wrapped)
            {
                case Season.Winter: return winterMean;
                case Season.Spring: return springMean;
                case Season.Summer: return summerMean;
                default: return autumnMean;
            }
        }
    }
}
