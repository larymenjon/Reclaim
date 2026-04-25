using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI
{
    /// <summary>
    /// Central manager for map tooltip UI.
    /// </summary>
    public class TooltipUIManager : MonoBehaviour
    {
        public static TooltipUIManager Instance { get; private set; }

        [Header("Tooltip References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text descriptionText;

        [Header("Positioning")]
        [SerializeField] private bool clampInsideCanvas = true;
        [SerializeField] private Vector2 screenOffset = new Vector2(18f, 18f);

        private RectTransform _tooltipRect;
        private RectTransform _canvasRect;
        private Canvas _canvas;
        private Camera _uiCamera;
        private RectTransform _trackedTarget;
        private Vector3 _anchorScreenPosition;
        private bool _followMouse;
        private bool _isVisible;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (tooltipPanel == null)
            {
                tooltipPanel = gameObject;
            }

            _tooltipRect = tooltipPanel.transform as RectTransform;

            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null)
            {
                _canvasRect = _canvas.transform as RectTransform;
                if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    _uiCamera = _canvas.worldCamera;
                }
            }

            EnsureTooltipDoesNotBlockRaycasts();
            HideTooltip();
        }

        private void Update()
        {
            if (!_isVisible)
            {
                return;
            }

            Vector2 screenPosition = ResolveScreenPosition();
            SetTooltipPosition(screenPosition);
        }

        /// <summary>
        /// Configures if tooltip follows mouse or appears next to a target.
        /// </summary>
        public void ConfigureTracking(bool followMouse, RectTransform target)
        {
            _followMouse = followMouse;
            _trackedTarget = target;
        }

        public void ShowTooltip(Sprite image, string text, Vector3 position)
        {
            if (previewImage != null)
            {
                previewImage.sprite = image;
                previewImage.enabled = image != null;
                previewImage.preserveAspect = true;
            }

            if (descriptionText != null)
            {
                descriptionText.text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
            }

            _anchorScreenPosition = position;
            _isVisible = true;

            if (tooltipPanel != null && !tooltipPanel.activeSelf)
            {
                tooltipPanel.SetActive(true);
            }

            if (_tooltipRect != null)
            {
                _tooltipRect.SetAsLastSibling();
            }

            SetTooltipPosition(ResolveScreenPosition());
        }

        public void HideTooltip()
        {
            _isVisible = false;
            _trackedTarget = null;

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private Vector2 ResolveScreenPosition()
        {
            if (_followMouse)
            {
                return UnityEngine.Input.mousePosition;
            }

            if (_trackedTarget != null)
            {
                return RectTransformUtility.WorldToScreenPoint(_uiCamera, _trackedTarget.position);
            }

            return _anchorScreenPosition;
        }

        private void SetTooltipPosition(Vector2 screenPosition)
        {
            if (_tooltipRect == null)
            {
                return;
            }

            if (_canvasRect == null)
            {
                _tooltipRect.position = screenPosition + screenOffset;
                return;
            }

            Vector2 anchoredPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                screenPosition + screenOffset,
                _uiCamera,
                out anchoredPosition
            );
            _tooltipRect.anchoredPosition = anchoredPosition;

            if (!clampInsideCanvas)
            {
                return;
            }

            Vector2 tooltipSize = _tooltipRect.rect.size;
            Vector2 min = _canvasRect.rect.min + tooltipSize * 0.5f;
            Vector2 max = _canvasRect.rect.max - tooltipSize * 0.5f;
            Vector2 clamped = _tooltipRect.anchoredPosition;
            clamped.x = Mathf.Clamp(clamped.x, min.x, max.x);
            clamped.y = Mathf.Clamp(clamped.y, min.y, max.y);
            _tooltipRect.anchoredPosition = clamped;
        }

        private void EnsureTooltipDoesNotBlockRaycasts()
        {
            if (tooltipPanel == null)
            {
                return;
            }

            CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Graphic[] graphics = tooltipPanel.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }
        }
    }
}
