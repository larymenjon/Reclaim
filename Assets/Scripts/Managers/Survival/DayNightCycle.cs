using System;
using Reclaim.Survival.Core;
using UnityEngine;

namespace Reclaim.Survival.Managers
{
    public enum DayPhase
    {
        Dawn = 0,
        Day = 1,
        Dusk = 2,
        Night = 3
    }

    public class DayNightCycle : MonoBehaviour
    {
        [SerializeField] private TimeSystem timeSystem;
        [SerializeField, Range(0f, 1f)] private float dawnEnd = 0.1f;
        [SerializeField, Range(0f, 1f)] private float dayEnd = 0.65f;
        [SerializeField, Range(0f, 1f)] private float duskEnd = 0.8f;

        public event Action<DayPhase, TickContext> OnPhaseChanged;
        public event Action<float> OnSpeedChanged;

        public DayPhase CurrentPhase { get; private set; } = DayPhase.Dawn;

        private void Awake()
        {
            if (timeSystem == null)
            {
                timeSystem = FindFirstObjectByType<TimeSystem>();
            }
        }

        private void OnEnable()
        {
            if (timeSystem != null)
            {
                timeSystem.OnTick += HandleTick;
            }
        }

        private void OnDisable()
        {
            if (timeSystem != null)
            {
                timeSystem.OnTick -= HandleTick;
            }
        }

        public void SetPaused(bool paused)
        {
            if (timeSystem == null)
            {
                return;
            }

            timeSystem.SetRunning(!paused);
            OnSpeedChanged?.Invoke(paused ? 0f : timeSystem.TimeScale);
        }

        public void SetSpeed(float speedMultiplier)
        {
            if (timeSystem == null)
            {
                return;
            }

            timeSystem.SetTimeScale(speedMultiplier);
            OnSpeedChanged?.Invoke(timeSystem.TimeScale);
        }

        private void HandleTick(TickContext context)
        {
            DayPhase newPhase = GetPhaseForProgress(context.DayProgress01);
            if (newPhase == CurrentPhase)
            {
                return;
            }

            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(CurrentPhase, context);
        }

        private DayPhase GetPhaseForProgress(float dayProgress01)
        {
            if (dayProgress01 < dawnEnd)
            {
                return DayPhase.Dawn;
            }

            if (dayProgress01 < dayEnd)
            {
                return DayPhase.Day;
            }

            if (dayProgress01 < duskEnd)
            {
                return DayPhase.Dusk;
            }

            return DayPhase.Night;
        }
    }
}
