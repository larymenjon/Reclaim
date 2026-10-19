using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    public enum NotificationSeverity
    {
        Critical,
        Alert,
        Success,
        Radio
    }

    /// <summary>Uma linha do feed de notificações (componente "Notificação" do Figma).</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationItemView : MonoBehaviour
    {
        [SerializeField] private Image stripe;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private Button button;

        private CanvasGroup _group;
        private float _lifetime;
        private float _fadeDuration;
        private float _elapsed;

        public bool IsShowing => gameObject.activeSelf;
        public bool IsPersistent { get; private set; }
        public float ShownAt { get; private set; }
        public string Key { get; private set; }

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            button.onClick.AddListener(Dismiss);
        }

        public void Show(string key, string title, string message, string detail, Sprite iconSprite, Color accent,
            Color primary, Color tertiary, float lifetime, float fadeDuration)
        {
            Key = key;
            stripe.color = accent;
            icon.sprite = iconSprite;
            icon.color = accent;
            titleText.text = title;
            titleText.color = accent;
            messageText.text = message;
            messageText.color = primary;
            detailText.text = detail;
            detailText.color = tertiary;
            detailText.gameObject.SetActive(!string.IsNullOrEmpty(detail));

            IsPersistent = lifetime <= 0f;
            _lifetime = lifetime;
            _fadeDuration = Mathf.Max(0.01f, fadeDuration);
            Restart();
            gameObject.SetActive(true);
        }

        /// <summary>Recomeça a contagem, usado quando a mesma notificação chega de novo.</summary>
        public void Restart()
        {
            _elapsed = 0f;
            ShownAt = Time.unscaledTime;
            if (_group != null)
            {
                _group.alpha = 1f;
            }
        }

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }

        // Tempo de tela não depende da velocidade do jogo nem da pausa.
        private void Update()
        {
            if (IsPersistent)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float fadeStart = _lifetime - _fadeDuration;
            _group.alpha = _elapsed <= fadeStart ? 1f : Mathf.Clamp01(1f - (_elapsed - fadeStart) / _fadeDuration);
            if (_elapsed >= _lifetime)
            {
                Dismiss();
            }
        }
    }
}
