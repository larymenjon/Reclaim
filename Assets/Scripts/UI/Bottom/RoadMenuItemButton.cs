using Reclaim.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.UI.Bottom
{
    /// <summary>
    /// Binds a road option button to a specific road prefab.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RoadMenuItemButton : MonoBehaviour
    {
        [SerializeField] private BuildHudController buildHudController;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField, Min(0)] private int coinCostPerCell = 0;

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

        private void HandleClick()
        {
            if (buildHudController == null || roadPrefab == null)
            {
                return;
            }

            buildHudController.SelectRoadToolWithPrefabAndCost(roadPrefab, coinCostPerCell);
        }
    }
}
