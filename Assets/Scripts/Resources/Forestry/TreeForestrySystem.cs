using System.Collections.Generic;
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

        [Header("Wood Popup Feedback")]
        [SerializeField] private bool showWoodGainPopup = true;
        [SerializeField] private Vector3 popupWorldOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField, Min(0.1f)] private float popupDurationSeconds = 1.1f;
        [SerializeField] private Color popupColor = new Color(0.39f, 0.88f, 0.45f, 1f);

        private int _activeWorkers;
        private float _nextScanTime;

        public int ActiveWorkers => _activeWorkers;
        public int MaxWorkers => ResolveMaxWorkers();

        private void Awake()
        {
            if (topHud == null) topHud = FindFirstObjectByType<TopHeaderHudController>();
            if (familyManager == null) familyManager = FindFirstObjectByType<FamilyManager>();
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
    }
}

