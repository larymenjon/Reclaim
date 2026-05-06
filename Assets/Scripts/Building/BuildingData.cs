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
        [SerializeField] private bool countsAsHouse;

        [Header("Economy")]
        [TextArea(2, 5)]
        [SerializeField] private string description = "Sem descricao.";
        [FormerlySerializedAs("cost")]
        [SerializeField] private int woodCost = 10;
        [SerializeField] private int scrapCost = 0;
        [SerializeField, Min(0f)] private float constructionDurationSeconds = 10f;

        [Header("Construction Visual Stages (Optional)")]
        [SerializeField] private GameObject foundationStagePrefab;
        [SerializeField] private GameObject midStagePrefab;
        [SerializeField] private GameObject completedStagePrefab;

        [Header("House Garden (Optional)")]
        [SerializeField] private bool allowGardenPlotSelection;
        [SerializeField] private Vector2Int defaultGardenPlotSize = new Vector2Int(2, 2);

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public GameObject Prefab => prefab;
        public Vector2Int Size => size;
        public bool CanRotate => canRotate;
        public bool CountsAsHouse => countsAsHouse;
        public int WoodCost => woodCost;
        public int ScrapCost => scrapCost;
        public int Cost => woodCost + scrapCost;
        public float ConstructionDurationSeconds => constructionDurationSeconds;
        public GameObject FoundationStagePrefab => foundationStagePrefab;
        public GameObject MidStagePrefab => midStagePrefab;
        public GameObject CompletedStagePrefab => completedStagePrefab;
        public bool AllowGardenPlotSelection => allowGardenPlotSelection;
        public Vector2Int DefaultGardenPlotSize => defaultGardenPlotSize;
    }
}
