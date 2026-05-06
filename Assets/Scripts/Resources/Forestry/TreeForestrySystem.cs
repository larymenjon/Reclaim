using System.Collections.Generic;
using Reclaim.Survival.Families;
using Reclaim.UI;
using UnityEngine;

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
    }
}
