using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Hud
{
    /// <summary>Botão de escolha do cartão de evento: borda e texto na cor de destaque, ícone opcional.</summary>
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public class EventChoiceButtonView : MonoBehaviour
    {
        [SerializeField] private Image border;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [Tooltip("Opacidade quando faltam recursos para a escolha.")]
        [SerializeField, Range(0f, 1f)] private float unaffordableAlpha = 0.4f;

        private Button _button;
        private CanvasGroup _group;
        private Action _onClick;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _group = GetComponent<CanvasGroup>();
            _button.onClick.AddListener(HandleClick);
        }

        public void Bind(string text, Color accent, Sprite iconSprite, Action onClick)
        {
            EnsureComponents();
            label.text = text;
            label.color = accent;
            border.color = accent;
            icon.sprite = iconSprite;
            icon.color = accent;
            icon.gameObject.SetActive(iconSprite != null);
            _onClick = onClick;
            gameObject.SetActive(true);
        }

        public void SetAffordable(bool affordable)
        {
            EnsureComponents();
            _button.interactable = affordable;
            _group.alpha = affordable ? 1f : unaffordableAlpha;
        }

        public void Hide()
        {
            _onClick = null;
            gameObject.SetActive(false);
        }

        private void HandleClick() => _onClick?.Invoke();

        // Bind pode chegar antes do primeiro Awake quando o botão começa desativado.
        private void EnsureComponents()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
                _group = GetComponent<CanvasGroup>();
            }
        }
    }
}
