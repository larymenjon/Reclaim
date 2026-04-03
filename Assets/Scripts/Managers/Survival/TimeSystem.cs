using System;
using Reclaim.Survival.Core;
using UnityEngine;

namespace Reclaim.Survival.Managers
{
    public class TimeSystem : MonoBehaviour
    {
        [Header("Tick Settings")]
        [SerializeField] private int ticksPerDay = 96;
        [SerializeField] private float secondsPerTick = 0.5f;
        [SerializeField] private float timeScale = 1f;
        [SerializeField] private bool autoStart = true;

        [Header("Temperature Model")]
        [SerializeField] private float baseTemperatureCelsius = -2f;
        [SerializeField] private float dailyTemperatureAmplitude = 8f;
        [SerializeField] private float seasonalTemperatureAmplitude = 12f;
        [SerializeField] private float seasonLengthDays = 20f;
        [SerializeField] private AnimationCurve dailyTemperatureCurve = AnimationCurve.EaseInOut(0f, -0.35f, 1f, 0.35f);

        private float _accumulator;
        private bool _isRunning;

        public event Action<TickContext> OnTick;
        public event Action<int> OnDayAdvanced;

        public int CurrentTick { get; private set; }
        public int CurrentDay { get; private set; }
        public int TicksPerDay => Mathf.Max(1, ticksPerDay);
        public float TimeScale => timeScale;
        public bool IsRunning => _isRunning;

        private void Start()
        {
            _isRunning = autoStart;
        }

        private void Update()
        {
            if (!_isRunning || secondsPerTick <= 0f)
            {
                return;
            }

            _accumulator += Time.deltaTime * Mathf.Max(0f, timeScale);
            while (_accumulator >= secondsPerTick)
            {
                _accumulator -= secondsPerTick;
                AdvanceTick();
            }
        }

        public void SetRunning(bool running) => _isRunning = running;
        public void SetTimeScale(float newScale) => timeScale = Mathf.Max(0f, newScale);

        private void AdvanceTick()
        {
            CurrentTick++;

            int ticksIntoDay = CurrentTick % TicksPerDay;
            int newDay = CurrentTick / TicksPerDay;
            if (newDay != CurrentDay)
            {
                CurrentDay = newDay;
                OnDayAdvanced?.Invoke(CurrentDay);
            }

            float dayProgress = (float)ticksIntoDay / TicksPerDay;
            float deltaDays = 1f / TicksPerDay;
            float temperature = ComputeTemperature(CurrentDay, dayProgress);

            TickContext context = new TickContext(
                CurrentTick,
                CurrentDay,
                dayProgress,
                secondsPerTick,
                deltaDays,
                temperature
            );

            OnTick?.Invoke(context);
        }

        private float ComputeTemperature(int day, float dayProgress)
        {
            float daily = dailyTemperatureCurve.Evaluate(dayProgress) * dailyTemperatureAmplitude;
            float seasonalPhase = seasonLengthDays <= 0f ? 0f : (day / seasonLengthDays) * Mathf.PI * 2f;
            float seasonal = Mathf.Sin(seasonalPhase) * seasonalTemperatureAmplitude;
            return baseTemperatureCelsius + daily + seasonal;
        }
    }
}
