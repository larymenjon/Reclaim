using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// Tokens de cor e regras visuais do redesign (página HUD do Figma). As views leem daqui
    /// as cores que mudam com o estado; as cores fixas são aplicadas pela ferramenta que monta a HUD.
    /// </summary>
    [CreateAssetMenu(menuName = "Reclaim/UI/Tema da HUD", fileName = "HudTema")]
    public class HudTheme : ScriptableObject
    {
        [Header("Texto")]
        [SerializeField] private Color textPrimary = new Color32(0xEC, 0xE7, 0xDA, 0xFF);
        [SerializeField] private Color textSecondary = new Color32(0xB3, 0xAE, 0x9F, 0xFF);
        [SerializeField] private Color textTertiary = new Color32(0x8D, 0x8A, 0x7E, 0xFF);
        [SerializeField] private Color textInverse = new Color32(0x12, 0x13, 0x11, 0xFF);

        [Header("Superfícies")]
        [SerializeField] private Color panel = new Color32(0x19, 0x1A, 0x17, 0xFF);
        [SerializeField] private Color panelRaised = new Color32(0x22, 0x23, 0x1F, 0xFF);
        [SerializeField] private Color panelSunken = new Color32(0x12, 0x13, 0x11, 0xFF);
        [SerializeField] private Color border = new Color32(0x44, 0x45, 0x3E, 0xFF);

        [Header("Estados")]
        [SerializeField] private Color good = new Color32(0x67, 0xA8, 0x52, 0xFF);
        [SerializeField] private Color critical = new Color32(0xCF, 0x4A, 0x3C, 0xFF);
        [SerializeField] private Color criticalDark = new Color32(0x8E, 0x2A, 0x22, 0xFF);
        [SerializeField] private Color warning = new Color32(0xE3, 0xA6, 0x34, 0xFF);
        [SerializeField] private Color info = new Color32(0x6F, 0xA0, 0xB6, 0xFF);
        [SerializeField] private Color selected = new Color32(0xDC, 0x7D, 0x45, 0xFF);
        [SerializeField] private Color progress = new Color32(0xB9, 0xC4, 0x6A, 0xFF);

        [Header("Regras")]
        [Tooltip("Consumível com menos dias de reserva que isto fica em estado Baixo.")]
        [SerializeField, Min(0f)] private float lowReserveDays = 3f;
        [Tooltip("Opacidade da orbe e dos ícones de estação que não estão ativos.")]
        [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0.4f;

        public Color TextPrimary => textPrimary;
        public Color TextSecondary => textSecondary;
        public Color TextTertiary => textTertiary;
        public Color TextInverse => textInverse;
        public Color Panel => panel;
        public Color PanelRaised => panelRaised;
        public Color PanelSunken => panelSunken;
        public Color Border => border;
        public Color Good => good;
        public Color Critical => critical;
        public Color CriticalDark => criticalDark;
        public Color Warning => warning;
        public Color Info => info;
        public Color Selected => selected;
        public Color Progress => progress;
        public float LowReserveDays => lowReserveDays;
        public float InactiveAlpha => inactiveAlpha;
    }
}
