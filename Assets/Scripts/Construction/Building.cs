using System.Collections.Generic;
using Reclaim.Grid;
using UnityEngine;

namespace Reclaim.Construction
{
    /// <summary>
    /// Instância de uma construção no mundo. Avança pelos estágios de obra
    /// (fundação, meio, concluída) e troca o visual de cada estágio.
    /// O meio da obra chega na metade do tempo. Construções sem modelos de estágio
    /// crescem em altura junto com o progresso.
    /// </summary>
    public class Building : MonoBehaviour
    {
        public enum ConstructionStage
        {
            Foundation = 0,
            Mid = 1,
            Completed = 2
        }

        private const float MidStageProgress = 0.5f;
        private const float GrowthStartHeight = 0.1f;

        public BuildingData Data { get; private set; }
        public GridCoordinate OriginCell { get; private set; }
        public int RotationSteps { get; private set; }
        public IReadOnlyList<GridCoordinate> OccupiedCells => _occupiedCells;
        public ConstructionStage CurrentStage { get; private set; } = ConstructionStage.Completed;
        public bool IsConstructionComplete => CurrentStage == ConstructionStage.Completed;
        public float ConstructionProgress01 { get; private set; } = 1f;

        /// <summary>Segundos de jogo que faltam para concluir a obra.</summary>
        public float ConstructionSecondsRemaining =>
            Data == null ? 0f : Mathf.Max(0f, Data.ConstructionDurationSeconds * (1f - ConstructionProgress01));

        /// <summary>Maior altura que o visual já teve, para posicionar a barra de progresso acima dele.</summary>
        public float VisualHeight { get; private set; }

        private readonly List<GridCoordinate> _occupiedCells = new List<GridCoordinate>();
        private Transform _visualRoot;
        private GameObject _activeStageInstance;
        private GameObject _activeStagePrefab;
        private float _cellSize = 1f;
        private bool _growsWithProgress;

        public event System.Action<Building> OnConstructionCompleted;
        public event System.Action<Building, float> OnConstructionProgressChanged;

        public void Initialize(BuildingData data, GridCoordinate originCell, int rotationSteps, IReadOnlyList<GridCoordinate> occupiedCells, float cellSize = 1f)
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
            _cellSize = cellSize;
            _growsWithProgress = data.FoundationStagePrefab == null && data.MidStagePrefab == null;

            _occupiedCells.Clear();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                _occupiedCells.Add(occupiedCells[i]);
            }

            EnsureVisualRoot();
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
                SetStage(ConstructionStage.Completed);
                OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
                yield break;
            }

            ConstructionProgress01 = 0f;
            SetStage(ConstructionStage.Foundation);
            OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                ConstructionProgress01 = Mathf.Clamp01(elapsed / duration);

                if (ConstructionProgress01 >= MidStageProgress && CurrentStage == ConstructionStage.Foundation)
                {
                    SetStage(ConstructionStage.Mid);
                }

                ApplyGrowth();
                OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
                yield return null;
            }

            ConstructionProgress01 = 1f;
            SetStage(ConstructionStage.Completed);
            OnConstructionProgressChanged?.Invoke(this, ConstructionProgress01);
        }

        private void SetStage(ConstructionStage stage)
        {
            CurrentStage = stage;
            ShowStageVisual(ResolveStagePrefab(stage));
            ApplyGrowth();

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

            GameObject completed = Data.CompletedStagePrefab != null ? Data.CompletedStagePrefab : Data.Prefab;
            if (_growsWithProgress || stage == ConstructionStage.Completed)
            {
                return completed;
            }

            GameObject foundation = Data.FoundationStagePrefab != null ? Data.FoundationStagePrefab : Data.Prefab;
            if (stage == ConstructionStage.Mid && Data.MidStagePrefab != null)
            {
                return Data.MidStagePrefab;
            }

            return foundation;
        }

        private void ShowStageVisual(GameObject prefab)
        {
            if (prefab == null || prefab == _activeStagePrefab)
            {
                return;
            }

            if (_activeStageInstance != null)
            {
                Destroy(_activeStageInstance);
            }

            _visualRoot.localScale = Vector3.one;
            _activeStagePrefab = prefab;
            _activeStageInstance = Instantiate(prefab, _visualRoot);
            _activeStageInstance.transform.localPosition = Vector3.zero;
            _activeStageInstance.transform.localRotation = Quaternion.identity;

            if (Data.FitToFootprint)
            {
                Vector2 target = new Vector2(Data.Size.x, Data.Size.y) * (_cellSize * Data.FootprintFill);
                VisualHeight = Mathf.Max(VisualHeight, BuildingVisualFit.Fit(_activeStageInstance.transform, _visualRoot, target));
                return;
            }

            Transform model = _activeStageInstance.transform;
            model.localScale *= Data.VisualScale;
            bool isCompletedModel = prefab == ResolveStagePrefab(ConstructionStage.Completed);
            float height = isCompletedModel || !TryGetCompletedSize(out Vector2 completedSize)
                ? BuildingVisualFit.Ground(model, _visualRoot)
                : BuildingVisualFit.Fit(model, _visualRoot, completedSize);
            VisualHeight = Mathf.Max(VisualHeight, height);
        }

        /// <summary>Largura e profundidade do modelo pronto, para a fundação e o meio da obra terem o mesmo tamanho.</summary>
        private bool TryGetCompletedSize(out Vector2 size)
        {
            size = default;
            GameObject completed = ResolveStagePrefab(ConstructionStage.Completed);
            if (completed == null)
            {
                return false;
            }

            GameObject probe = Instantiate(completed, _visualRoot);
            probe.transform.localPosition = Vector3.zero;
            probe.transform.localRotation = Quaternion.identity;
            probe.transform.localScale *= Data.VisualScale;
            bool measured = BuildingVisualFit.TryGetLocalBounds(_visualRoot, probe, out Bounds bounds);
            probe.SetActive(false);
            Destroy(probe);

            size = new Vector2(bounds.size.x, bounds.size.z);
            return measured && size.x > 0.0001f && size.y > 0.0001f;
        }

        /// <summary>Sem modelos de estágio, a obra sobe do chão até a altura final.</summary>
        private void ApplyGrowth()
        {
            if (_visualRoot == null)
            {
                return;
            }

            float height = _growsWithProgress && !IsConstructionComplete
                ? Mathf.Lerp(GrowthStartHeight, 1f, ConstructionProgress01)
                : 1f;
            _visualRoot.localScale = new Vector3(1f, height, 1f);
        }

        private void EnsureVisualRoot()
        {
            if (_visualRoot != null)
            {
                return;
            }

            _visualRoot = new GameObject("Visual").transform;
            _visualRoot.SetParent(transform, false);
        }

        private void EnsureProgressUi()
        {
            if (GetComponent<BuildingConstructionProgressUI>() == null)
            {
                gameObject.AddComponent<BuildingConstructionProgressUI>();
            }
        }
    }
}
