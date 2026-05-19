using System.Collections.Generic;
using Reclaim.Grid;
using UnityEngine;

namespace Reclaim.Building
{
    public class Building : MonoBehaviour
    {
        public enum ConstructionStage
        {
            Foundation = 0,
            Mid = 1,
            Completed = 2
        }

        public BuildingData Data { get; private set; }
        public GridCoordinate OriginCell { get; private set; }
        public int RotationSteps { get; private set; }
        public IReadOnlyList<GridCoordinate> OccupiedCells => _occupiedCells;
        public ConstructionStage CurrentStage { get; private set; } = ConstructionStage.Completed;
        public bool IsConstructionComplete => CurrentStage == ConstructionStage.Completed;
        public float ConstructionProgress01 { get; private set; } = 1f;

        private readonly List<GridCoordinate> _occupiedCells = new List<GridCoordinate>();
        private GameObject _activeStageInstance;

        public event System.Action<Building> OnConstructionCompleted;
        public event System.Action<Building, float> OnConstructionProgressChanged;

        public void Initialize(BuildingData data, GridCoordinate originCell, int rotationSteps, IReadOnlyList<GridCoordinate> occupiedCells)
        {
            if (data == null)
            {
                Debug.LogError("Building.Initialize called with null BuildingData.", this);
                return;
            }

            if (occupiedCells == null)
            {
                Debug.LogError("Building.Initialize called with null occupiedCells list.", this);
                return;
            }

            Data = data;
            OriginCell = originCell;
            RotationSteps = rotationSteps;

            _occupiedCells.Clear();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                _occupiedCells.Add(occupiedCells[i]);
            }

            EnsureProgressUi();
            BeginConstruction();
        }

        private void BeginConstruction()
        {
            if (Data == null)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ConstructionRoutine());
        }

        private System.Collections.IEnumerator ConstructionRoutine()
        {
            float duration = Mathf.Max(0f, Data.ConstructionDurationSeconds);
            if (duration <= 0.01f)
            {
                ConstructionProgress01 = 1f;
                OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
                SetStage(ConstructionStage.Completed);
                yield break;
            }

            ConstructionProgress01 = 0f;
            OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
            SetStage(ConstructionStage.Foundation);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ConstructionProgress01 = Mathf.Clamp01(elapsed / duration);
                OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);

                if (ConstructionProgress01 >= (1f / 3f) && CurrentStage != ConstructionStage.Mid)
                {
                    SetStage(ConstructionStage.Mid);
                }

                yield return null;
            }

            ConstructionProgress01 = 1f;
            OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
            SetStage(ConstructionStage.Completed);
        }

        private void SetStage(ConstructionStage stage)
        {
            CurrentStage = stage;
            GameObject stagePrefab = ResolveStagePrefab(stage);
            if (stagePrefab != null)
            {
                ReplaceStageVisual(stagePrefab);
            }

            if (stage == ConstructionStage.Completed)
            {
                OnConstructionCompleted?.Invoke(this);
            }
        }

        private GameObject ResolveStagePrefab(ConstructionStage stage)
        {
            if (Data == null)
            {
                return null;
            }

            switch (stage)
            {
                case ConstructionStage.Foundation:
                    return Data.FoundationStagePrefab != null ? Data.FoundationStagePrefab : Data.Prefab;
                case ConstructionStage.Mid:
                    if (Data.MidStagePrefab != null) return Data.MidStagePrefab;
                    if (Data.FoundationStagePrefab != null) return Data.FoundationStagePrefab;
                    return Data.Prefab;
                case ConstructionStage.Completed:
                    if (Data.CompletedStagePrefab != null) return Data.CompletedStagePrefab;
                    return Data.Prefab;
                default:
                    return Data.Prefab;
            }
        }

        private void ReplaceStageVisual(GameObject prefab)
        {
            if (_activeStageInstance != null)
            {
                Destroy(_activeStageInstance);
            }

            _activeStageInstance = Instantiate(prefab, transform);
            _activeStageInstance.transform.localPosition = Vector3.zero;
            _activeStageInstance.transform.localRotation = Quaternion.identity;
        }

        private void EnsureProgressUi()
        {
            if (GetComponent<BuildingConstructionProgressUI>() == null)
            {
                gameObject.AddComponent<BuildingConstructionProgressUI>();
            }
        }

        private void OnDestroy()
        {
            if (_activeStageInstance != null)
            {
                Destroy(_activeStageInstance);
            }
        }
    }
}