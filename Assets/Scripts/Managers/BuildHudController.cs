using Reclaim.Building;
using Reclaim.Road;
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
        [SerializeField] private RoadSystem roadSystem;
        [SerializeField] private BuildingData defaultBuilding;
        [SerializeField] private GameObject defaultRoadPrefab;
        [SerializeField, Min(0)] private int defaultRoadCoinCostPerCell = 0;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (roadSystem == null)
            {
                roadSystem = FindFirstObjectByType<RoadSystem>();
            }
        }

        public void SelectRoadTool()
        {
            if (gameManager == null)
            {
                return;
            }

            if (roadSystem != null && defaultRoadPrefab != null)
            {
                roadSystem.SetRoadPlacementOptions(defaultRoadPrefab, defaultRoadCoinCostPerCell);
            }

            gameManager.EnterRoadMode();
        }

        public void SelectRoadToolWithPrefab(GameObject roadPrefab)
        {
            if (gameManager == null || roadSystem == null || roadPrefab == null)
            {
                return;
            }

            roadSystem.SetRoadPlacementOptions(roadPrefab, 0);
            gameManager.EnterRoadMode();
        }

        public void SelectRoadToolWithPrefabAndCost(GameObject roadPrefab, int coinsPerCell)
        {
            if (gameManager == null || roadSystem == null || roadPrefab == null)
            {
                return;
            }

            roadSystem.SetRoadPlacementOptions(roadPrefab, Mathf.Max(0, coinsPerCell));
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
