using Reclaim.Building;
using UnityEngine;

namespace Reclaim.Managers
{
    /// <summary>
    /// Simple HUD hook for tool selection buttons.
    /// Link these public methods in Unity UI Button OnClick events.
    /// </summary>
    public class BuildHudController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BuildingData defaultBuilding;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
        }

        public void SelectRoadTool()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.EnterRoadMode();
        }

        public void SelectBuildTool()
        {
            if (gameManager == null || defaultBuilding == null)
            {
                return;
            }

            gameManager.EnterBuildMode(defaultBuilding);
        }

        public void SelectBuildingTool(BuildingData buildingData)
        {
            if (gameManager == null || buildingData == null)
            {
                return;
            }

            gameManager.EnterBuildMode(buildingData);
        }

        public void SelectIdleTool()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.EnterIdleMode();
        }
    }
}
