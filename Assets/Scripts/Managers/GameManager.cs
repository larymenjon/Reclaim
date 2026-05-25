using System;
using Reclaim.Building;
using Reclaim.Core;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Road;
using UnityEngine;

namespace Reclaim
{
    /// <summary>
    /// Composition root for runtime systems and game mode state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private PreviewSystem _previewSystem;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private BuildingSystem _buildingSystem;
        [SerializeField] private PlacementHistory _placementHistory;

        [Header("Road")]
        [SerializeField] private RoadBuilderManager _roadBuilderManager;

        [Header("Startup")]
        [SerializeField] private GameMode _startMode = GameMode.None;
        [SerializeField] private BuildingData _defaultBuilding;
        [SerializeField] private bool _selectDefaultBuildingOnStart;

        public event Action<GameMode> OnModeChanged;

        public GameMode CurrentMode { get; private set; } = GameMode.None;

        private void Awake()
        {
            ResolveReferences();

            if (_inputHandler != null)
            {
                _inputHandler.SetGridManager(_gridManager);
                _inputHandler.OnUndoPressed += HandleUndoPressed;
            }

            if (_buildingSystem != null)
            {
                _buildingSystem.Initialize(_gridManager, _previewSystem, this, _placementHistory, _inputHandler);
            }
        }

        private void Start()
        {
            SetMode(_startMode);

            if (_selectDefaultBuildingOnStart && _defaultBuilding != null && _buildingSystem != null)
            {
                _buildingSystem.SelectBuilding(_defaultBuilding);
            }
        }

        public void SetMode(GameMode mode)
        {
            CurrentMode = mode;
            OnModeChanged?.Invoke(CurrentMode);

            if (mode != GameMode.Build && _previewSystem != null)
            {
                _previewSystem.ClearPreview();
            }
        }

        public void EnterBuildMode(BuildingData buildingData)
        {
            if (buildingData != null && _buildingSystem != null)
            {
                _buildingSystem.SelectBuilding(buildingData);
            }

            SetMode(GameMode.Build);
        }

        public void EnterBuildMode(UnityEngine.Object buildingAsset)
        {
            EnterBuildMode(buildingAsset as BuildingData);
        }

        public void SelectBuildingForUI(BuildingData buildingData)
        {
            if (_buildingSystem != null)
            {
                _buildingSystem.SelectBuilding(buildingData);
            }
        }

        public void SelectBuildingForUI(UnityEngine.Object buildingAsset)
        {
            SelectBuildingForUI(buildingAsset as BuildingData);
        }

        public void EnterRoadMode()
        {
            SetMode(GameMode.Road);
            EnsureRoadBuilderManager();

            if (_roadBuilderManager != null && !_roadBuilderManager.IsBuilding)
            {
                _roadBuilderManager.StartBuildMode();
            }
        }

        public void EnterIdleMode()
        {
            if (_roadBuilderManager != null && _roadBuilderManager.IsBuilding)
            {
                _roadBuilderManager.ExitBuildMode();
            }

            SetMode(GameMode.None);
        }

        private void HandleUndoPressed()
        {
            if (CurrentMode != GameMode.Road && _placementHistory != null)
            {
                _placementHistory.UndoLast();
            }
        }

        private void ResolveReferences()
        {
            _gridManager ??= FindFirstObjectByType<GridManager>();
            _previewSystem ??= FindFirstObjectByType<PreviewSystem>();
            _inputHandler ??= FindFirstObjectByType<InputHandler>();
            _buildingSystem ??= FindFirstObjectByType<BuildingSystem>();
            _placementHistory ??= FindFirstObjectByType<PlacementHistory>();
            _roadBuilderManager ??= FindFirstObjectByType<RoadBuilderManager>();
        }

        private void EnsureRoadBuilderManager()
        {
            _roadBuilderManager ??= FindFirstObjectByType<RoadBuilderManager>();
        }
    }
}
