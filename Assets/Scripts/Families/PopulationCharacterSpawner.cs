using System.Collections.Generic;
using Reclaim.Survival.Families;
using Reclaim.UI;
using UnityEngine;

namespace Reclaim.Families
{
    public class PopulationCharacterSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FamilyManager familyManager;
        [SerializeField] private TopHeaderHudController topHud;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private Transform spawnRoot;

        [Header("Spawn Area")]
        [SerializeField] private Vector3 center = Vector3.zero;
        [SerializeField] private Vector2 size = new Vector2(12f, 12f);
        [SerializeField, Min(1)] private int maxVisualCharacters = 30;
        [SerializeField, Min(0.2f)] private float syncIntervalSeconds = 0.5f;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private float _nextSync;

        private void Awake()
        {
            if (familyManager == null) familyManager = FindFirstObjectByType<FamilyManager>();
            if (topHud == null) topHud = FindFirstObjectByType<TopHeaderHudController>();
            if (spawnRoot == null) spawnRoot = transform;
        }

        private void Update()
        {
            if (characterPrefab == null || Time.time < _nextSync)
            {
                return;
            }

            _nextSync = Time.time + syncIntervalSeconds;
            SyncCharacters();
        }

        private void SyncCharacters()
        {
            int population = familyManager != null ? familyManager.TotalPopulation : (topHud != null ? topHud.Families : 0);
            int targetCount = Mathf.Clamp(population, 0, maxVisualCharacters);

            while (_spawned.Count < targetCount)
            {
                SpawnOne();
            }

            while (_spawned.Count > targetCount)
            {
                RemoveOne();
            }
        }

        private void SpawnOne()
        {
            Vector3 localOffset = new Vector3(
                Random.Range(-size.x * 0.5f, size.x * 0.5f),
                0f,
                Random.Range(-size.y * 0.5f, size.y * 0.5f));

            GameObject instance = Instantiate(characterPrefab, spawnRoot);
            instance.transform.localPosition = center + localOffset;
            instance.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            _spawned.Add(instance);
        }

        private void RemoveOne()
        {
            int lastIndex = _spawned.Count - 1;
            GameObject toRemove = _spawned[lastIndex];
            _spawned.RemoveAt(lastIndex);
            if (toRemove != null)
            {
                Destroy(toRemove);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.35f);
            Vector3 worldCenter = transform.TransformPoint(center);
            Gizmos.DrawCube(worldCenter, new Vector3(size.x, 0.1f, size.y));
        }
    }
}
