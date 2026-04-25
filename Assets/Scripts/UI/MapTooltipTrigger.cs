using UnityEngine;
using UnityEngine.EventSystems;

namespace Reclaim.UI
{
    public class MapTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum TooltipPlacement
        {
            NextToElement = 0,
            FollowMouse = 1
        }

        [Header("Tooltip Data")]
        [SerializeField] private MapTooltipData tooltipData;

        [Header("Behavior")]
        [SerializeField] private TooltipPlacement placement = TooltipPlacement.NextToElement;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipUIManager.Instance == null)
            {
                return;
            }

            bool followMouse = placement == TooltipPlacement.FollowMouse;
            TooltipUIManager.Instance.ConfigureTracking(followMouse, _rectTransform);

            Vector3 position = followMouse ? (Vector3)eventData.position : transform.position;
            string text = BuildTooltipText();
            Sprite image = tooltipData != null ? tooltipData.previewImage : null;
            TooltipUIManager.Instance.ShowTooltip(image, text, position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipUIManager.Instance != null)
            {
                TooltipUIManager.Instance.HideTooltip();
            }
        }

        private string BuildTooltipText()
        {
            if (tooltipData == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(tooltipData.description))
            {
                return tooltipData.description;
            }

            return tooltipData.mapName ?? string.Empty;
        }
    }
}
