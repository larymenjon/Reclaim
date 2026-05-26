using System.Collections.Generic;
using Reclaim.Grid;
using Reclaim.Survival.Families;
using Reclaim.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Reclaim.Resources.Forestry
{
    public class TreeForestrySystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TopHeaderHudController topHud;
        [SerializeField] private FamilyManager familyManager;

        [Header("Workforce")]
        [SerializeField, Min(1)] private int populationPerWorker = 4;
        [SerializeField] private int maxWorkersOverride;

        [Header("Automation")]
        [SerializeField] private bool autoHarvest = true;
        [SerializeField, Min(0.2f)] private float scanIntervalSeconds = 1f;
        [SerializeField] private List<TreeResourceNode> nodes = new List<TreeResourceNode>();

        [Header("Spawning")]
        [SerializeField] private GameObject treeNodePrefab;
        [SerializeField, Min(1)] private int spawnCount = 30;
        [SerializeField, Range(0f, 1f)] private float spawnDensity = 0.35f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float minTreeSpacing = 2f;

        [Header("Wood Popup Feedback")]
        [SerializeField] private bool showWoodGainPopup = true;
        [SerializeField] private Vector3 popupWorldOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField, Min(0.1f)] private float popupDurationSeconds = 1.1f;
        [SerializeField] private Color popupColor = new Color(0.39f, 0.88f, 0.45f, 1f);

        private int _activeWorkers;
        private float _nextScanTime;
        private GridManager _gridManager;

        public int ActiveWorkers => _activeWorkers;
        public int MaxWorkers => ResolveMaxWorkers();

        private void Awake()
        {
            if (topHud == null) topHud = FindFirstObjectByType<TopHeaderHudController>();
            if (familyManager == null) familyManager = FindFirstObjectByType<FamilyManager>();
            _gridManager ??= FindFirstObjectByType<GridManager>();

            if (nodes.Count == 0)
            {
                nodes.AddRange(FindObjectsByType<TreeResourceNode>(FindObjectsSortMode.None));
            }
        }

        private void Update()
        {
            if (!autoHarvest || Time.time < _nextScanTime)
            {
                return;
            }

            _nextScanTime = Time.time + scanIntervalSeconds;
            TryHarvestNextAvailableTree();
        }

        public void Initialize(GridManager gridManager)
        {
            _gridManager = gridManager != null ? gridManager : FindFirstObjectByType<GridManager>();
        }

        public bool TryReserveWorker()
        {
            if (_activeWorkers >= ResolveMaxWorkers())
            {
                return false;
            }

            _activeWorkers++;
            return true;
        }

        public void ReleaseWorker()
        {
            _activeWorkers = Mathf.Max(0, _activeWorkers - 1);
        }

        public void AddWood(int amount)
        {
            if (topHud == null || amount <= 0)
            {
                return;
            }

            topHud.AddWood(amount);
        }

        public void AddWood(int amount, Vector3 worldPosition)
        {
            AddWood(amount);
            if (showWoodGainPopup && amount > 0)
            {
                SpawnWoodPopup(worldPosition, amount);
            }
        }

        public void RegisterNode(TreeResourceNode node)
        {
            if (node == null || nodes.Contains(node))
            {
                return;
            }

            nodes.Add(node);
        }

        public void SpawnTrees(GridManager gridManager)
        {
            if (treeNodePrefab == null)
            {
                Debug.LogWarning("[TreeForestrySystem] treeNodePrefab is not assigned.", this);
                return;
            }

            _gridManager = gridManager != null ? gridManager : _gridManager;
            if (_gridManager == null)
            {
                _gridManager = FindFirstObjectByType<GridManager>();
            }

            if (_gridManager == null)
            {
                Debug.LogWarning("[TreeForestrySystem] GridManager not found for tree spawning.", this);
                return;
            }

            List<GridCoordinate> freeCells = _gridManager.GetAllBuildableCells();
            if (freeCells.Count == 0)
            {
                return;
            }

            ShuffleList(freeCells);
            int densityCap = Mathf.Max(1, Mathf.FloorToInt(freeCells.Count * spawnDensity));
            int toSpawn = Mathf.Min(spawnCount, densityCap);

            List<Vector3> spawnedPositions = new List<Vector3>(toSpawn);

            for (int i = 0; i < freeCells.Count && spawnedPositions.Count < toSpawn; i++)
            {
                Vector3 worldPos = _gridManager.GridToWorld(freeCells[i], true);
                worldPos = ResolveGroundPosition(worldPos);

                bool tooClose = false;
                for (int p = 0; p < spawnedPositions.Count; p++)
                {
                    if (Vector3.Distance(worldPos, spawnedPositions[p]) < minTreeSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                {
                    continue;
                }

                GameObject go = Instantiate(treeNodePrefab, worldPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                TreeResourceNode node = go.GetComponent<TreeResourceNode>();
                if (node == null)
                {
                    node = go.GetComponentInChildren<TreeResourceNode>();
                }

                if (node != null)
                {
                    RegisterNode(node);
                    spawnedPositions.Add(worldPos);
                }
                else
                {
                    Debug.LogWarning("[TreeForestrySystem] Spawned prefab has no TreeResourceNode.", go);
                    Destroy(go);
                }
            }
        }

        public void ClearAllSpawnedTrees()
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i] != null)
                {
                    Destroy(nodes[i].gameObject);
                }
            }

            nodes.Clear();
        }

        public void TryHarvestNextAvailableTree()
        {
            if (_activeWorkers >= ResolveMaxWorkers())
            {
                return;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                TreeResourceNode node = nodes[i];
                if (node != null && node.IsHarvestable && node.TryHarvest(this))
                {
                    return;
                }
            }
        }

        private int ResolveMaxWorkers()
        {
            if (maxWorkersOverride > 0)
            {
                return maxWorkersOverride;
            }

            int population = 0;
            if (familyManager != null)
            {
                population = familyManager.TotalPopulation;
            }
            else if (topHud != null)
            {
                population = topHud.Families;
            }

            int byPopulation = Mathf.Max(1, Mathf.FloorToInt(population / Mathf.Max(1f, populationPerWorker)));
            return byPopulation;
        }

        private void SpawnWoodPopup(Vector3 worldPosition, int amount)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main != null ? UnityEngine.Camera.main : FindFirstObjectByType<UnityEngine.Camera>();
            if (camera == null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("WoodGainPopup", typeof(Canvas));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(220f, 56f);
            canvasObject.transform.position = worldPosition + popupWorldOffset;
            canvasObject.transform.localScale = Vector3.one * 0.01f;

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvasObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.text = $"+{amount} madeira";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.fontSize = 28;
            text.color = popupColor;
            text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ??
                        UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");

            WoodGainPopup popup = canvasObject.AddComponent<WoodGainPopup>();
            popup.Initialize(camera, popupDurationSeconds, popupColor);
        }

        private Vector3 ResolveGroundPosition(Vector3 worldPos)
        {
            if (groundLayer.value == 0)
            {
                return worldPos;
            }

            Vector3 rayStart = worldPos + Vector3.up * 1000f;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 3000f, groundLayer))
            {
                worldPos.y = hit.point.y;
            }

            return worldPos;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
