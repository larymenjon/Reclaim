using Reclaim.Calendar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// "Estação" do Figma: ícones na ordem inverno, primavera, verão, outono; a estação atual fica maior
    /// e a barra mostra o avanço no ano.
    /// </summary>
    public class SeasonView : MonoBehaviour
    {
        [SerializeField] private SimulationClock clock;
        [SerializeField] private HudTheme theme;
        [SerializeField] private TMP_Text seasonNameText;
        [SerializeField] private TMP_Text dayOfSeasonText;
        [SerializeField] private Image yearFill;
        [Tooltip("Um ícone por estação, na ordem do enum Season.")]
        [SerializeField] private Image[] seasonIcons = new Image[CalendarDate.SeasonsPerYear];
        [SerializeField, Min(1f)] private float activeIconSize = 36f;
        [SerializeField, Min(1f)] private float inactiveIconSize = 24f;

        private int _lastDayIndex = -1;

        private void Awake()
        {
            if (clock == null || theme == null || seasonIcons.Length != CalendarDate.SeasonsPerYear)
            {
                Debug.LogError("[SeasonView] Configure SimulationClock, HudTheme e os 4 ícones de estação.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            clock.OnTick += HandleTick;
            _lastDayIndex = -1;
            Apply(clock.CurrentDate);
        }

        private void OnDisable()
        {
            clock.OnTick -= HandleTick;
        }

        private void HandleTick(int tick, CalendarDate date, float deltaDays) => Apply(date);

        private void Apply(CalendarDate date)
        {
            yearFill.fillAmount = (date.DayOfYear - 1 + date.DayProgress01) / date.DaysPerYear;

            // Texto e ícones só mudam na virada do dia.
            if (date.DayIndex == _lastDayIndex)
            {
                return;
            }

            _lastDayIndex = date.DayIndex;
            seasonNameText.text = HudFormat.SeasonName(date.Season);
            dayOfSeasonText.text = $"Dia {date.DayOfSeason}/{date.DaysPerSeason}";

            for (int i = 0; i < seasonIcons.Length; i++)
            {
                bool active = i == (int)date.Season;
                Image icon = seasonIcons[i];
                float size = active ? activeIconSize : inactiveIconSize;
                icon.rectTransform.sizeDelta = new Vector2(size, size);
                Color color = active ? theme.TextPrimary : theme.TextTertiary;
                color.a = active ? 1f : theme.InactiveAlpha;
                icon.color = color;
            }
        }
    }
}
