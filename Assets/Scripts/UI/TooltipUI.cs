using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Reclaim.UI
{
    /// <summary>
    /// Manages tooltip display and positioning.
    /// Shows tooltip near mouse/button on hover.
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private Image background;
        [SerializeField] private LayoutElement layoutElement;
        
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private int maxWidth = 300;
        [SerializeField] private Vector2 offset = new Vector2(10, 10);

        private RectTransform _rectTransform;
        private bool _isShowing = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            // Start invisible
            canvasGroup.alpha = 0;
            _isShowing = false;
        }

        private void Update()
        {
            // Smooth fade in/out
            float targetAlpha = _isShowing ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        }

        public void Show(string title, string description, string info, Color titleColor, RectTransform targetButton)
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _isShowing = true;

            // Update text
            if (titleText != null)
            {
                titleText.text = title;
                titleText.color = titleColor;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            if (infoText != null)
            {
                infoText.text = info;
            }

            // Adjust size based on content
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = maxWidth;
                LayoutRebuilder.ForceRebuildLayoutHierarchy(_rectTransform);
            }

            // Position near button
            UpdatePosition(targetButton);
        }

        public void Hide()
        {
            _isShowing = false;
        }

        private void UpdatePosition(RectTransform targetButton)
        {
            if (targetButton == null)
                return;

            // Get button position
            Vector2 buttonPos = targetButton.rect.max;
            
            // Set tooltip position relative to button
            _rectTransform.anchoredPosition = buttonPos + offset;

            // Adjust if goes off-screen
            Vector2 tooltipSize = _rectTransform.rect.size;
            Canvas parentCanvas = GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                
                // Check right edge
                if (_rectTransform.anchoredPosition.x + tooltipSize.x > canvasRect.rect.width)
                {
                    _rectTransform.anchoredPosition -= new Vector2(tooltipSize.x + offset.x, 0);
                }

                // Check top edge
                if (_rectTransform.anchoredPosition.y + tooltipSize.y > canvasRect.rect.height)
                {
                    _rectTransform.anchoredPosition -= new Vector2(0, tooltipSize.y + offset.y);
                }
            }
        }
    }
}
