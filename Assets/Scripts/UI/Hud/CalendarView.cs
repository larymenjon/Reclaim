using Reclaim.Calendar;
using Reclaim.Climate;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    /// <summary>"Calendário" do Figma: controle de velocidade em cima; ano e dia, período, hora e temperatura embaixo.</summary>
    public class CalendarView : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private ClimateSystem climate;
        [SerializeField] private HudTheme theme;

        [Header("Velocidade")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button fastButton;
        [SerializeField] private Button fastestButton;

        [Header("Textos")]
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text temperatureText;
        [SerializeField] private Image temperatureIcon;

        private bool _isDirty = true;

        private void Awake()
        {
            if (clock == null || theme == null)
            {
                Debug.LogError("[CalendarView] SimulationClock ou HudTheme não configurados.", this);
                enabled = false;
                return;
            }

            pauseButton.onClick.AddListener(clock.TogglePause);
            normalButton.onClick.AddListener(clock.SetSpeedNormal);
            fastButton.onClick.AddListener(clock.SetSpeedFast);
            fastestButton.onClick.AddListener(clock.SetSpeedFastest);
        }

        private void OnEnable()
        {
            clock.OnTick += HandleTick;
            clock.OnSpeedChanged += HandleSpeedChanged;
            if (climate != null)
            {
                climate.OnTemperatureChanged += HandleTemperatureChanged;
            }

            HandleSpeedChanged(clock.Speed);
            _isDirty = true;
        }

        private void OnDisable()
        {
            clock.OnTick -= HandleTick;
            clock.OnSpeedChanged -= HandleSpeedChanged;
            if (climate != null)
            {
                climate.OnTemperatureChanged -= HandleTemperatureChanged;
            }
        }

        private void LateUpdate()
        {
            if (!_isDirty)
            {
                return;
            }

            _isDirty = false;
            CalendarDate date = clock.CurrentDate;
            dateText.text = HudFormat.Date(date);
            timeText.text = HudFormat.TimeOfDay(date);

            bool hasClimate = climate != null;
            temperatureText.gameObject.SetActive(hasClimate);
            temperatureIcon.gameObject.SetActive(hasClimate);
            if (hasClimate)
            {
                temperatureText.text = HudFormat.Temperature(climate.CurrentTemperatureCelsius);
            }
        }

        private void HandleTick(int tick, CalendarDate date, float deltaDays) => _isDirty = true;

        private void HandleTemperatureChanged(float celsius) => _isDirty = true;

        private void HandleSpeedChanged(SimulationSpeed speed)
        {
            Highlight(pauseButton, speed == SimulationSpeed.Paused);
            Highlight(normalButton, speed == SimulationSpeed.Normal);
            Highlight(fastButton, speed == SimulationSpeed.Fast);
            Highlight(fastestButton, speed == SimulationSpeed.Fastest);
        }

        private void Highlight(Button button, bool active)
        {
            // O ícone é o targetGraphic do botão; a cor de estado fica no próprio ícone.
            button.targetGraphic.color = active ? theme.TextPrimary : theme.TextTertiary;
        }
    }
}
