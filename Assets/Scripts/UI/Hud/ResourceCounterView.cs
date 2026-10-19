using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    public enum CounterState
    {
        Normal,
        Low,
        Positive
    }

    /// <summary>Componente "Contador de recurso" do Figma: ícone, valor e unidade, com três estados.</summary>
    public class ResourceCounterView : MonoBehaviour
    {
        [SerializeField] private HudTheme theme;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text unitText;
        [Tooltip("Losango que separa este contador do anterior. Opcional.")]
        [SerializeField] private GameObject separator;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetSeparatorVisible(bool visible)
        {
            if (separator != null)
            {
                separator.SetActive(visible);
            }
        }

        public void Show(string value, string unit, CounterState state)
        {
            valueText.text = value;
            bool hasUnit = !string.IsNullOrEmpty(unit);
            unitText.gameObject.SetActive(hasUnit);
            if (hasUnit)
            {
                unitText.text = unit;
            }

            Color main = state == CounterState.Low ? theme.Critical : theme.TextPrimary;
            icon.color = main;
            valueText.color = main;
            unitText.color = state == CounterState.Low ? theme.Critical
                : state == CounterState.Positive ? theme.Good
                : theme.TextSecondary;
        }
    }
}
