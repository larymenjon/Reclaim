using UnityEngine;

namespace Reclaim.Building
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Reclaim/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "building";
        [SerializeField] private string displayName = "New Building";

        [Header("Placement")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private bool canRotate = true;

        [Header("Economy")]
        [SerializeField] private int cost = 10;

        public string Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public Vector2Int Size => size;
        public bool CanRotate => canRotate;
        public int Cost => cost;
    }
}
