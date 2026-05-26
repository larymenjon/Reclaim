using System;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Road
{
    /// <summary>
    /// Botao de confirmacao (martelo) em Screen Space Overlay.
    /// Segue a posicao do ultimo ponto projetada na tela.
    /// </summary>
    public class ConfirmBuildUI : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Canvas confirmCanvas;
        [SerializeField] private Button confirmButton;
        [SerializeField] private RectTransform buttonRect;

        [Header("Posicionamento")]
        [Tooltip("Offset em pixels acima do ponto projetado na tela.")]
        [SerializeField] private Vector2 screenOffset = new(0f, 60f);

        private Action _onConfirm;
        private Vector3 _worldTarget;
        private Camera _cam;

        public bool IsVisible => confirmButton != null
            ? confirmButton.gameObject.activeSelf
            : confirmCanvas != null && confirmCanvas.gameObject.activeSelf;

        private void Awake()
        {
            _cam = Camera.main;

            if (confirmCanvas != null)
                confirmCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnButtonClicked);

            if (buttonRect == null && confirmButton != null)
                buttonRect = confirmButton.GetComponent<RectTransform>();

            Hide();
        }

        private void LateUpdate()
        {
            if (!IsVisible || _cam == null || buttonRect == null)
                return;

            Vector3 screenPos = _cam.WorldToScreenPoint(_worldTarget);

            if (screenPos.z < 0f)
            {
                SetConfirmVisible(false);
                return;
            }

            SetConfirmVisible(true);
            buttonRect.position = new Vector2(screenPos.x, screenPos.y) + screenOffset;
        }

        public void Show(Vector3 worldPos, Action onConfirm)
        {
            _worldTarget = worldPos;
            _onConfirm = onConfirm;

            SetConfirmVisible(true);
            Debug.Log($"[ConfirmUI] Botao exibido em mundo={worldPos}");
        }

        public void UpdatePosition(Vector3 worldPos)
        {
            _worldTarget = worldPos;
        }

        public void Hide()
        {
            SetConfirmVisible(false);
            _onConfirm = null;
        }

        private void OnButtonClicked()
        {
            Debug.Log("[ConfirmUI] Botao clicado - disparando OnConfirm.");
            var cb = _onConfirm;
            Hide();
            cb?.Invoke();
        }

        private void SetConfirmVisible(bool visible)
        {
            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(visible);
                return;
            }

            if (confirmCanvas != null)
                confirmCanvas.gameObject.SetActive(visible);
        }
    }
}
