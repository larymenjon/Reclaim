using UnityEngine;

namespace Reclaim.UI.Hud
{
    /// <summary>
    /// Feed lateral de notificações. Regras do Figma: até 4 na tela, a mais nova em cima,
    /// somem em 8 s, exceto Crítica, que fica até o clique.
    /// </summary>
    public class NotificationFeedView : MonoBehaviour
    {
        [SerializeField] private HudTheme theme;
        [Tooltip("Linhas pré-criadas; o tamanho do array é o limite de notificações na tela.")]
        [SerializeField] private NotificationItemView[] slots = new NotificationItemView[4];
        [SerializeField, Min(0.5f)] private float lifetimeSeconds = 8f;
        [SerializeField, Min(0f)] private float fadeSeconds = 0.4f;

        [Header("Ícones por gravidade")]
        [SerializeField] private Sprite criticalIcon;
        [SerializeField] private Sprite alertIcon;
        [SerializeField] private Sprite successIcon;
        [SerializeField] private Sprite radioIcon;

        private void Awake()
        {
            if (theme == null || slots.Length == 0)
            {
                Debug.LogError("[NotificationFeedView] Configure HudTheme e as linhas do feed.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        public void Post(NotificationSeverity severity, string title, string message, string detail = null)
        {
            if (!enabled)
            {
                return;
            }

            string key = title + "|" + message;
            NotificationItemView same = FindShowing(key);
            if (same != null)
            {
                same.Restart();
                same.transform.SetAsFirstSibling();
                return;
            }

            NotificationItemView slot = TakeSlot();
            float lifetime = severity == NotificationSeverity.Critical ? 0f : lifetimeSeconds;
            slot.transform.SetAsFirstSibling();
            slot.Show(key, title, message, detail, IconFor(severity), AccentFor(severity),
                theme.TextPrimary, theme.TextTertiary, lifetime, fadeSeconds);
        }

        public void ClearAll()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        private NotificationItemView FindShowing(string key)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsShowing && slots[i].Key == key)
                {
                    return slots[i];
                }
            }

            return null;
        }

        /// <summary>Linha livre; se o feed estiver cheio, troca a mais antiga que não é crítica.</summary>
        private NotificationItemView TakeSlot()
        {
            NotificationItemView oldestTemporary = null;
            NotificationItemView oldest = null;
            for (int i = 0; i < slots.Length; i++)
            {
                NotificationItemView slot = slots[i];
                if (!slot.IsShowing)
                {
                    return slot;
                }

                if (oldest == null || slot.ShownAt < oldest.ShownAt)
                {
                    oldest = slot;
                }

                if (!slot.IsPersistent && (oldestTemporary == null || slot.ShownAt < oldestTemporary.ShownAt))
                {
                    oldestTemporary = slot;
                }
            }

            return oldestTemporary != null ? oldestTemporary : oldest;
        }

        private Color AccentFor(NotificationSeverity severity)
        {
            switch (severity)
            {
                case NotificationSeverity.Critical: return theme.Critical;
                case NotificationSeverity.Alert: return theme.Warning;
                case NotificationSeverity.Success: return theme.Good;
                default: return theme.Info;
            }
        }

        private Sprite IconFor(NotificationSeverity severity)
        {
            switch (severity)
            {
                case NotificationSeverity.Critical: return criticalIcon;
                case NotificationSeverity.Alert: return alertIcon;
                case NotificationSeverity.Success: return successIcon;
                default: return radioIcon;
            }
        }
    }
}
