using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Reclaim.UI
{
    /// <summary>
    /// Simple tooltip trigger that shows/hides a tooltip on hover.
    /// Attach this to any button and configure with tooltip info.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string tooltipTitle = "Item";
        [SerializeField] private string tooltipDescription = "";
        [SerializeField] private string tooltipInfo = "";
        [SerializeField] private Color tooltipTitleColor = Color.white;
        
        private TooltipUI _tooltipUI;

        private void Start()
        {
            // Find tooltip UI in scene
            _tooltipUI = FindFirstObjectByType<TooltipUI>();
            
            if (_tooltipUI == null)
            {
                Debug.LogWarning($"TooltipUI not found in scene! Tooltip for '{tooltipTitle}' won't work.");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipUI != null)
            {
                _tooltipUI.Show(tooltipTitle, tooltipDescription, tooltipInfo, tooltipTitleColor, transform as RectTransform);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltipUI != null)
            {
                _tooltipUI.Hide();
            }
        }

        /// <summary>
        /// Update tooltip content dynamically
        /// </summary>
        public void SetTooltipContent(string title, string description, string info)
        {
            tooltipTitle = title;
            tooltipDescription = description;
            tooltipInfo = info;
        }
    }
}
