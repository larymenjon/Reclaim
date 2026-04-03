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
        [SerializeField] private GridManager gridManager;
        [SerializeField] private PreviewSystem previewSystem;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private BuildingSystem buildingSystem;
        [SerializeField] private RoadSystem roadSystem;
        [SerializeField] private PlacementHistory placementHistory;

        [Header("Startup")]
        [SerializeField] private GameMode startMode = GameMode.None;
        [SerializeField] private BuildingData defaultBuilding;

        public event Action<GameMode> OnModeChanged;

        public GameMode CurrentMode { get; private set; } = GameMode.None;

        private void Awake()
        {
            AutoResolveReferences();

            if (inputHandler != null)
            {
                inputHandler.SetGridManager(gridManager);
                inputHandler.OnUndoPressed += HandleUndoPressed;
            }

            buildingSystem.Initialize(gridManager, previewSystem, this, placementHistory, inputHandler);
            roadSystem.Initialize(gridManager, previewSystem, this, placementHistory, inputHandler);
        }

        private void Start()
        {
            SetMode(startMode);

            if (defaultBuilding != null)
            {
                buildingSystem.SelectBuilding(defaultBuilding);
            }
        }

        public void SetMode(GameMode mode)
        {
            CurrentMode = mode;
            OnModeChanged?.Invoke(CurrentMode);

            if (mode == GameMode.None)
            {
                previewSystem.ClearPreview();
            }
        }

        public void EnterBuildMode(BuildingData buildingData)
        {
            if (buildingData != null)
            {
                buildingSystem.SelectBuilding(buildingData);
            }

            SetMode(GameMode.Build);
        }

        public void EnterRoadMode()
        {
            SetMode(GameMode.Road);
        }

        public void EnterIdleMode()
        {
            SetMode(GameMode.None);
        }

        public void SelectBuildingForUI(BuildingData buildingData)
        {
            buildingSystem.SelectBuilding(buildingData);
        }

        private void HandleUndoPressed()
        {
            placementHistory.UndoLast();
        }

        private void AutoResolveReferences()
        {
            if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
            if (previewSystem == null) previewSystem = FindFirstObjectByType<PreviewSystem>();
            if (inputHandler == null) inputHandler = FindFirstObjectByType<InputHandler>();
            if (buildingSystem == null) buildingSystem = FindFirstObjectByType<BuildingSystem>();
            if (roadSystem == null) roadSystem = FindFirstObjectByType<RoadSystem>();
            if (placementHistory == null) placementHistory = FindFirstObjectByType<PlacementHistory>();
        }
    }
}
