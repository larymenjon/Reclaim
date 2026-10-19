using System.Collections.Generic;
using Reclaim.Grid;
using Reclaim.Population;
using TMPro;
using UnityEngine;

namespace Reclaim.Economy.Forestry
{
    /// <summary>
    /// Coordena o corte automático de árvores: quantos lenhadores a população permite,
    /// qual árvore cortar e onde depositar a madeira.
    /// </summary>
    public class TreeForestrySystem : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private ResourceStock resourceStock;
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private GridManager gridManager;

        [Header("Mão de obra")]
        [Tooltip("Quantos moradores sustentam um lenhador.")]
        [SerializeField, Min(1)] private int populationPerWorker = 4;
        [Tooltip("Se maior que zero, ignora a população e usa este limite fixo.")]
        [SerializeField, Min(0)] private int maxWorkersOverride;

        [Header("Automação")]
        [SerializeField] private bool autoHarvest = true;
        [SerializeField, Min(0.2f)] private float scanIntervalSeconds = 1f;
        [SerializeField] private List<TreeResourceNode> nodes = new List<TreeResourceNode>();

        [Header("Geração de árvores")]
        [SerializeField] private GameObject treeNodePrefab;
        [SerializeField, Min(1)] private int spawnCount = 30;
        [SerializeField, Range(0f, 1f)] private float spawnDensity = 0.35f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField, Min(0f)] private float minTreeSpacing = 2f;

        [Header("Feedback de madeira")]
        [SerializeField] private bool showWoodGainPopup = true;
        [SerializeField] private Vector3 popupWorldOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField, Min(0.1f)] private float popupDurationSeconds = 1.1f;
        [SerializeField] private Color popupColor = new Color(0.40f, 0.66f, 0.32f, 1f);
        [SerializeField, Min(0.1f)] private float popupFontSize = 3f;

        private float _nextScanTime;

        public int ActiveWorkers { get; private set; }
        public int MaxWorkers => ResolveMaxWorkers();

        private void Awake()
        {
            if (resourceStock == null)
            {
                Debug.LogError("[TreeForestrySystem] ResourceStock não configurado; a madeira cortada será perdida.", this);
            }

            if (nodes.Count == 0)
            {
                nodes.AddRange(FindObjectsByType<TreeResourceNode>());
            }
        }

        private void Update()
        {
            if (!autoHarvest || UnityEngine.Time.time < _nextScanTime)
            {
                return;
            }

            _nextScanTime = UnityEngine.Time.time + scanIntervalSeconds;
            TryHarvestNextAvailableTree();
        }

        public bool TryReserveWorker()
        {
            if (ActiveWorkers >= ResolveMaxWorkers())
            {
                return false;
            }

            ActiveWorkers++;
            return true;
        }

        public void ReleaseWorker()
        {
            ActiveWorkers = Mathf.Max(0, ActiveWorkers - 1);
        }

        public void DepositWood(int amount, Vector3 worldPosition)
        {
            if (amount <= 0 || resourceStock == null)
            {
                return;
            }

            int stored = Mathf.FloorToInt(resourceStock.Add(ResourceType.Wood, amount));
            if (showWoodGainPopup && stored > 0)
            {
                SpawnWoodPopup(worldPosition, stored);
            }
        }

        public void RegisterNode(TreeResourceNode node)
        {
            if (node != null && !nodes.Contains(node))
            {
                nodes.Add(node);
            }
        }

        public void SpawnTrees()
        {
            if (treeNodePrefab == null || gridManager == null)
            {
                Debug.LogWarning("[TreeForestrySystem] Para gerar árvores, configure Tree Node Prefab e Grid Manager.", this);
                return;
            }

            List<GridCoordinate> freeCells = gridManager.GetAllBuildableCells();
            Shuffle(freeCells);

            int densityCap = Mathf.Max(1, Mathf.FloorToInt(freeCells.Count * spawnDensity));
            int toSpawn = Mathf.Min(spawnCount, densityCap);
            List<Vector3> spawned = new List<Vector3>(toSpawn);

            for (int i = 0; i < freeCells.Count && spawned.Count < toSpawn; i++)
            {
                Vector3 position = ResolveGroundPosition(gridManager.GridToWorld(freeCells[i]));
                if (IsTooClose(position, spawned))
                {
                    continue;
                }

                if (TrySpawnNode(position))
                {
                    spawned.Add(position);
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
            if (ActiveWorkers >= ResolveMaxWorkers())
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

            int population = familyManager != null ? familyManager.TotalPopulation : 0;
            return Mathf.Max(1, population / populationPerWorker);
        }

        private bool TrySpawnNode(Vector3 position)
        {
            GameObject instance = Instantiate(treeNodePrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            TreeResourceNode node = instance.GetComponentInChildren<TreeResourceNode>();
            if (node == null)
            {
                Debug.LogWarning("[TreeForestrySystem] O prefab de árvore não tem TreeResourceNode.", instance);
                Destroy(instance);
                return false;
            }

            RegisterNode(node);
            return true;
        }

        private bool IsTooClose(Vector3 position, List<Vector3> others)
        {
            float minSqr = minTreeSpacing * minTreeSpacing;
            for (int i = 0; i < others.Count; i++)
            {
                if ((others[i] - position).sqrMagnitude < minSqr)
                {
                    return true;
                }
            }

            return false;
        }

        private void SpawnWoodPopup(Vector3 worldPosition, int amount)
        {
            GameObject popupObject = new GameObject("WoodGainPopup");
            popupObject.transform.position = worldPosition + popupWorldOffset;

            TextMeshPro text = popupObject.AddComponent<TextMeshPro>();
            text.text = $"+{amount} madeira";
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = popupFontSize;
            text.color = popupColor;

            popupObject.AddComponent<WoodGainPopup>().Initialize(Camera.main, popupDurationSeconds, popupColor);
        }

        private Vector3 ResolveGroundPosition(Vector3 worldPosition)
        {
            if (groundLayer.value == 0)
            {
                return worldPosition;
            }

            Vector3 rayStart = worldPosition + Vector3.up * 1000f;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 3000f, groundLayer))
            {
                worldPosition.y = hit.point.y;
            }

            return worldPosition;
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
