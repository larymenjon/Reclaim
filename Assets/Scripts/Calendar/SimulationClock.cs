using System;
using UnityEngine;

namespace Reclaim.Calendar
{
    /// <summary>
    /// Relógio da simulação. Avança em ticks discretos, controla a velocidade (pausa, 1x, 2x, 4x)
    /// e publica a data do jogo. Não conhece clima nem regras da colônia.
    /// A velocidade é aplicada em Time.timeScale para que obras, corte de árvores e animações
    /// também parem na pausa. Câmera e UI devem usar Time.unscaledDeltaTime.
    /// </summary>
    public class SimulationClock : MonoBehaviour
    {
        [Header("Tick")]
        [SerializeField, Min(1)] private int ticksPerDay = 96;
        [SerializeField, Min(0.01f)] private float secondsPerTick = 0.5f;

        [Header("Calendário")]
        [Tooltip("O GDD define um ano de 80 dias: quatro estações de 20 dias.")]
        [SerializeField, Min(1)] private int daysPerSeason = 20;
        [SerializeField, Range(0f, 23.99f)] private float startHour = 7f;

        [Header("Início")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private SimulationSpeed startSpeed = SimulationSpeed.Normal;

        private float _accumulator;
        private SimulationSpeed _resumeSpeed = SimulationSpeed.Normal;

        public int CurrentTick { get; private set; }
        public SimulationSpeed Speed { get; private set; } = SimulationSpeed.Paused;
        public bool IsPaused => Speed == SimulationSpeed.Paused;
        public int TicksPerDay => ticksPerDay;
        public int DaysPerSeason => daysPerSeason;
        public CalendarDate CurrentDate => BuildDate(CurrentTick);

        /// <summary>Disparado a cada tick com o índice, a data e a fração de dia que passou.</summary>
        public event Action<int, CalendarDate, float> OnTick;
        public event Action<CalendarDate> OnDayStarted;
        public event Action<Season> OnSeasonChanged;
        public event Action<SimulationSpeed> OnSpeedChanged;

        private void Awake()
        {
            CurrentTick = Mathf.RoundToInt(startHour / 24f * ticksPerDay);
        }

        private void OnDestroy()
        {
            // Evita que o Menu abra congelado se a cena de jogo for descarregada em pausa.
            UnityEngine.Time.timeScale = 1f;
        }

        private void Start()
        {
            SetSpeed(autoStart ? startSpeed : SimulationSpeed.Paused);
        }

        private void Update()
        {
            if (IsPaused)
            {
                return;
            }

            _accumulator += UnityEngine.Time.deltaTime;
            while (_accumulator >= secondsPerTick && !IsPaused)
            {
                _accumulator -= secondsPerTick;
                AdvanceTick();
            }
        }

        public void SetSpeed(SimulationSpeed speed)
        {
            if (speed != SimulationSpeed.Paused)
            {
                _resumeSpeed = speed;
            }

            UnityEngine.Time.timeScale = (int)speed;

            if (Speed == speed)
            {
                return;
            }

            Speed = speed;
            OnSpeedChanged?.Invoke(Speed);
        }

        public void Pause() => SetSpeed(SimulationSpeed.Paused);

        public void Resume() => SetSpeed(_resumeSpeed);

        public void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        // Atalhos para OnClick de botões da HUD, que não aceitam parâmetros enum.
        public void SetSpeedNormal() => SetSpeed(SimulationSpeed.Normal);
        public void SetSpeedFast() => SetSpeed(SimulationSpeed.Fast);
        public void SetSpeedFastest() => SetSpeed(SimulationSpeed.Fastest);

        private void AdvanceTick()
        {
            CalendarDate previous = CurrentDate;
            CurrentTick++;
            CalendarDate current = CurrentDate;

            OnTick?.Invoke(CurrentTick, current, 1f / ticksPerDay);

            if (current.DayIndex == previous.DayIndex)
            {
                return;
            }

            OnDayStarted?.Invoke(current);

            if (current.Season != previous.Season)
            {
                OnSeasonChanged?.Invoke(current.Season);
            }
        }

        private CalendarDate BuildDate(int tick)
        {
            int dayIndex = tick / ticksPerDay;
            float progress = (float)(tick % ticksPerDay) / ticksPerDay;
            return new CalendarDate(dayIndex, progress, daysPerSeason);
        }
    }
}
