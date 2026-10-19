using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reclaim.Calendar
{
    public enum DayPhase
    {
        Night = 0,
        Dawn = 1,
        Day = 2,
        Dusk = 3
    }

    /// <summary>
    /// Traduz a hora do relógio em fases do dia (madrugada, amanhecer, dia, entardecer).
    /// Consumido pela música, pela iluminação e pelo indicador de dia e noite da HUD.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [FormerlySerializedAs("timeSystem")]
        [SerializeField] private SimulationClock clock;

        [Header("Horários de início de cada fase (0 a 24)")]
        [SerializeField, Range(0f, 24f)] private float dawnStartHour = 5f;
        [SerializeField, Range(0f, 24f)] private float dayStartHour = 7f;
        [SerializeField, Range(0f, 24f)] private float duskStartHour = 18f;
        [SerializeField, Range(0f, 24f)] private float nightStartHour = 20f;

        public DayPhase CurrentPhase { get; private set; } = DayPhase.Day;
        public bool IsNight => CurrentPhase == DayPhase.Night;

        public event Action<DayPhase> OnPhaseChanged;

        private void Awake()
        {
            if (clock == null)
            {
                Debug.LogError("[DayNightCycle] Referência ao SimulationClock não configurada.", this);
                enabled = false;
                return;
            }

            CurrentPhase = GetPhase(clock.CurrentDate.DayProgress01 * 24f);
        }

        private void OnEnable()
        {
            if (clock != null)
            {
                clock.OnTick += HandleTick;
            }
        }

        private void OnDisable()
        {
            if (clock != null)
            {
                clock.OnTick -= HandleTick;
            }
        }

        public DayPhase GetPhase(float hour)
        {
            if (hour >= nightStartHour || hour < dawnStartHour)
            {
                return DayPhase.Night;
            }

            if (hour < dayStartHour)
            {
                return DayPhase.Dawn;
            }

            return hour < duskStartHour ? DayPhase.Day : DayPhase.Dusk;
        }

        private void HandleTick(int tickIndex, CalendarDate date, float deltaDays)
        {
            DayPhase phase = GetPhase(date.DayProgress01 * 24f);
            if (phase == CurrentPhase)
            {
                return;
            }

            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }
}
