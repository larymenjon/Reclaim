using System;
using Reclaim.Calendar;
using UnityEngine;

namespace Reclaim.Climate
{
    /// <summary>
    /// Mantém a temperatura atual a partir do perfil do mapa e da data do relógio.
    /// </summary>
    public class ClimateSystem : MonoBehaviour
    {
        [SerializeField] private ClimateProfile profile;

        public float CurrentTemperatureCelsius { get; private set; }

        public event Action<float> OnTemperatureChanged;

        public void Initialize(CalendarDate startDate)
        {
            if (profile == null)
            {
                Debug.LogError("[ClimateSystem] ClimateProfile não atribuído. Temperatura fixa em 0 °C.", this);
            }

            Refresh(startDate);
        }

        public float Refresh(CalendarDate date)
        {
            float temperature = profile != null ? profile.EvaluateCelsius(date) : 0f;
            if (!Mathf.Approximately(temperature, CurrentTemperatureCelsius))
            {
                CurrentTemperatureCelsius = temperature;
                OnTemperatureChanged?.Invoke(CurrentTemperatureCelsius);
            }

            return CurrentTemperatureCelsius;
        }
    }
}
