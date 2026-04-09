using UnityEngine;
using UnityEngine.Serialization;

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
        [TextArea(2, 5)]
        [SerializeField] private string description = "Sem descricao.";
        [FormerlySerializedAs("cost")]
        [SerializeField] private int woodCost = 10;
        [SerializeField] private int scrapCost = 0;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject Prefab => prefab;
        public Vector2Int Size => size;
        public bool CanRotate => canRotate;
        public int WoodCost => woodCost;
        public int ScrapCost => scrapCost;
        public int Cost => woodCost + scrapCost;
    }
}
