using System;
using System.Collections.Generic;
using Reclaim;
using Reclaim.Core;
using Reclaim.Grid;
using Reclaim.Input;
using UnityEngine;

namespace Reclaim.Building
{
    /// <summary>
    /// Handles building selection, validation and placement.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        private GridManager _gridManager;
        private PreviewSystem _previewSystem;
        private GameManager _gameManager;
        private PlacementHistory _history;

        private BuildingData _selectedBuilding;
        private int _rotationSteps;
        private GridCoordinate _lastHoverCoordinate;
        private bool _hasHover;

        public event Action<BuildingData> OnSelectedBuildingChanged;

        public void Initialize(
            GridManager gridManager,
            PreviewSystem previewSystem,
            GameManager gameManager,
            PlacementHistory history,
            InputHandler inputHandler)
        {
            _gridManager = gridManager;
            _previewSystem = previewSystem;
            _gameManager = gameManager;
            _history = history;

            inputHandler.OnPointerMoved += HandlePointerMoved;
            inputHandler.OnPrimaryPressed += HandlePrimaryPressed;
            inputHandler.OnRotatePressed += HandleRotatePressed;
            inputHandler.OnSecondaryPressed += HandleSecondaryPressed;
            _gameManager.OnModeChanged += HandleModeChanged;
        }

        public void SelectBuilding(BuildingData buildingData)
        {
            _selectedBuilding = buildingData;
            _rotationSteps = 0;
            OnSelectedBuildingChanged?.Invoke(_selectedBuilding);

            if (_selectedBuilding == null)
            {
                _previewSystem.ClearPreview();
                return;
            }

            _previewSystem.SetPreviewPrefab(_selectedBuilding.Prefab);
            _previewSystem.SetVisible(true);
        }

        public void ClearSelection()
        {
            SelectBuilding(null);
        }

        private void HandlePointerMoved(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Build || _selectedBuilding == null)
            {
                _previewSystem.SetVisible(false);
                return;
            }

            List<GridCoordinate> cells = _gridManager.GetFootprintCells(coordinate, _selectedBuilding.Size, _rotationSteps);
            bool isValid = _gridManager.AreCellsBuildable(cells);

            Vector3 worldPosition = _gridManager.GetFootprintCenterWorld(coordinate, _selectedBuilding.Size, _rotationSteps);
            Quaternion rotation = Quaternion.Euler(0f, _rotationSteps * 90f, 0f);

            _previewSystem.SetVisible(true);
            _previewSystem.SetTransform(worldPosition, rotation);
            _previewSystem.SetValidity(isValid);
            _lastHoverCoordinate = coordinate;
            _hasHover = true;
        }

        private void HandlePrimaryPressed(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Build || _selectedBuilding == null)
            {
                return;
            }

            List<GridCoordinate> cells = _gridManager.GetFootprintCells(coordinate, _selectedBuilding.Size, _rotationSteps);
            if (!_gridManager.AreCellsBuildable(cells))
            {
                return;
            }

            Vector3 worldPosition = _gridManager.GetFootprintCenterWorld(coordinate, _selectedBuilding.Size, _rotationSteps);
            Quaternion rotation = Quaternion.Euler(0f, _rotationSteps * 90f, 0f);

            GameObject instance = Instantiate(_selectedBuilding.Prefab, worldPosition, rotation);
            Building building = instance.GetComponent<Building>();
            if (building == null)
            {
                building = instance.AddComponent<Building>();
            }

            building.Initialize(_selectedBuilding, coordinate, _rotationSteps, cells);
            _gridManager.SetOccupancy(cells, OccupancyType.Building, instance);

            _history.Record(() =>
            {
                _gridManager.ClearOccupancy(cells, instance);
                if (instance != null)
                {
                    Destroy(instance);
                }
            });
        }

        private void HandleRotatePressed()
        {
            if (_gameManager.CurrentMode != GameMode.Build || _selectedBuilding == null || !_selectedBuilding.CanRotate)
            {
                return;
            }

            _rotationSteps = (_rotationSteps + 1) % 4;

            if (_hasHover)
            {
                HandlePointerMoved(_lastHoverCoordinate, Vector3.zero);
            }
        }

        private void HandleSecondaryPressed()
        {
            if (_gameManager.CurrentMode == GameMode.Build)
            {
                _gameManager.SetMode(GameMode.None);
            }
        }

        private void HandleModeChanged(GameMode mode)
        {
            if (mode != GameMode.Build)
            {
                _previewSystem.SetVisible(false);
                _hasHover = false;
                return;
            }

            if (_selectedBuilding != null)
            {
                _previewSystem.SetPreviewPrefab(_selectedBuilding.Prefab);
                _previewSystem.SetVisible(true);
            }
        }
    }
}
