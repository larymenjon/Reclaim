using Reclaim.Building;
using Reclaim.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Binds one construction option button to BuildingData selection + tooltip.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BuildingMenuItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private BuildingData buildingData;
        [SerializeField] private BuildHudController buildHudController;
        [SerializeField] private BuildingTooltipUI tooltipUI;
        [SerializeField] private bool hideTooltipAfterClick = true;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);

            if (buildHudController == null)
            {
                buildHudController = FindFirstObjectByType<BuildHudController>();
            }
        }

        public void SetBuildingData(BuildingData data)
        {
            buildingData = data;
        }

        public void SetTooltip(BuildingTooltipUI tooltip)
        {
            tooltipUI = tooltip;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipUI != null && buildingData != null)
            {
                tooltipUI.Show(buildingData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipUI != null)
            {
                tooltipUI.Hide();
            }
        }

        private void HandleClick()
        {
            if (buildHudController == null || buildingData == null)
            {
                return;
            }

            buildHudController.SelectBuildingTool(buildingData);

            if (hideTooltipAfterClick && tooltipUI != null)
            {
                tooltipUI.Hide();
            }
        }
    }
}
