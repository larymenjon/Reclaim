using Reclaim.Building;
using TMPro;
using UnityEngine;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Tooltip for construction entries in the bottom HUD.
    /// </summary>
    public class BuildingTooltipUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text woodCostText;
        [SerializeField] private TMP_Text scrapCostText;
        [SerializeField] private Canvas canvas;

        [Header("Positioning")]
        [SerializeField] private Vector2 screenOffset = new Vector2(20f, 24f);
        [SerializeField] private bool clampInsideCanvas = true;

        private Camera _uiCamera;
        private bool _isVisible;

        private void Awake()
        {
            if (tooltipRoot == null)
            {
                tooltipRoot = transform as RectTransform;
            }

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _uiCamera = canvas.worldCamera;
            }

            Hide();
        }

        private void LateUpdate()
        {
            if (_isVisible)
            {
                SetPosition(UnityEngine.Input.mousePosition);
            }
        }

        public void Show(BuildingData data)
        {
            if (data == null)
            {
                Hide();
                return;
            }

            if (titleText != null) titleText.text = data.DisplayName;
            if (descriptionText != null) descriptionText.text = data.Description;
            if (woodCostText != null) woodCostText.text = data.WoodCost.ToString();
            if (scrapCostText != null) scrapCostText.text = data.ScrapCost.ToString();

            _isVisible = true;
            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(true);
                tooltipRoot.SetAsLastSibling();
            }
        }

        public void Hide()
        {
            _isVisible = false;
            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(false);
            }
        }

        private void SetPosition(Vector2 screenPosition)
        {
            if (tooltipRoot == null)
            {
                return;
            }

            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            if (canvasRect == null)
            {
                tooltipRoot.position = screenPosition + screenOffset;
                return;
            }

            Vector2 anchored;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition + screenOffset, _uiCamera, out anchored);
            tooltipRoot.anchoredPosition = anchored;

            if (!clampInsideCanvas)
            {
                return;
            }

            Vector2 size = tooltipRoot.rect.size;
            Vector2 min = canvasRect.rect.min + size * 0.5f;
            Vector2 max = canvasRect.rect.max - size * 0.5f;
            Vector2 clamped = tooltipRoot.anchoredPosition;
            clamped.x = Mathf.Clamp(clamped.x, min.x, max.x);
            clamped.y = Mathf.Clamp(clamped.y, min.y, max.y);
            tooltipRoot.anchoredPosition = clamped;
        }
    }
}
