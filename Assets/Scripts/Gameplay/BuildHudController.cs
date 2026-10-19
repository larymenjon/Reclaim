using Reclaim.Construction;
using UnityEngine;
using UnityEngine.Serialization;

namespace Reclaim.Gameplay
{
    /// <summary>
    /// Ponte entre os botões da doca de construção e o ToolModeController.
    /// Ligue estes métodos no OnClick dos botões.
    /// </summary>
    public class BuildHudController : MonoBehaviour
    {
        [FormerlySerializedAs("gameManager")]
        [SerializeField] private ToolModeController toolModeController;
        [SerializeField] private BuildingData defaultBuilding;

        private void Awake()
        {
            if (toolModeController == null)
            {
                Debug.LogError("[BuildHudController] ToolModeController não configurado.", this);
                enabled = false;
            }
        }

        public void SelectBuildTool()
        {
            SelectBuildingTool(defaultBuilding);
        }

        public void SelectBuildingTool(BuildingData buildingData)
        {
            if (enabled && buildingData != null)
            {
                toolModeController.EnterBuildMode(buildingData);
            }
        }

        /// <summary>Sobrecarga para OnClick configurado com argumento Object.</summary>
        public void SelectBuildingTool(Object buildingAsset)
        {
            SelectBuildingTool(buildingAsset as BuildingData);
        }

        public void SelectRoadTool()
        {
            if (enabled)
            {
                toolModeController.EnterRoadMode();
            }
        }

        public void SelectIdleTool()
        {
            if (enabled)
            {
                toolModeController.EnterIdleMode();
            }
        }
    }
}
