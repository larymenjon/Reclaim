using System;
using System.Collections.Generic;
using Reclaim;
using Reclaim.Core;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Resources.Forestry;
using Reclaim.UI;
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
        private TopHeaderHudController _topHud;
        private InputHandler _inputHandler;
        private TreeForestrySystem _forestrySystem;

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
            _inputHandler = inputHandler;
            _topHud = FindFirstObjectByType<TopHeaderHudController>();
            _forestrySystem = FindFirstObjectByType<TreeForestrySystem>();

            if (_inputHandler == null || _gameManager == null || _previewSystem == null || _gridManager == null)
            {
                Debug.LogError("BuildingSystem.Initialize missing required dependencies.", this);
                return;
            }

            _inputHandler.OnPointerMoved += HandlePointerMoved;
            _inputHandler.OnPrimaryPressed += HandlePrimaryPressed;
            _inputHandler.OnRotatePressed += HandleRotatePressed;
            _inputHandler.OnSecondaryPressed += HandleSecondaryPressed;
            _gameManager.OnModeChanged += HandleModeChanged;
        }

        private void OnDestroy()
        {
            if (_inputHandler != null)
            {
                _inputHandler.OnPointerMoved -= HandlePointerMoved;
                _inputHandler.OnPrimaryPressed -= HandlePrimaryPressed;
                _inputHandler.OnRotatePressed -= HandleRotatePressed;
                _inputHandler.OnSecondaryPressed -= HandleSecondaryPressed;
            }

            if (_gameManager != null)
            {
                _gameManager.OnModeChanged -= HandleModeChanged;
            }
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

            if (!TryGetSelectedPrefab(out GameObject prefab))
            {
                _selectedBuilding = null;
                _previewSystem.ClearPreview();
                return;
            }

            _previewSystem.SetPreviewPrefab(prefab);
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

        private void HandlePrimaryPressed(GridCoordinate coordinate, Vector3 worldPoint)
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

            if (!TryGetSelectedPrefab(out GameObject ignoredPrefab))
            {
                _previewSystem.ClearPreview();
                return;
            }

            BuildingData placedData = _selectedBuilding;

            if (_topHud != null && !_topHud.TrySpendBuildingCost(placedData.WoodCost, placedData.ScrapCost))
            {
                Debug.Log($"Not enough resources to build '{placedData.DisplayName}'. Need Wood {placedData.WoodCost}, Scrap {placedData.ScrapCost}.");
                return;
            }

            ClearTreesInsideFootprint(cells);

            GameObject root = new GameObject($"{placedData.DisplayName}_Building");
            root.transform.SetPositionAndRotation(worldPosition, rotation);
            Building building = root.GetComponent<Building>();
            if (building == null)
            {
                building = root.AddComponent<Building>();
            }

            building.Initialize(placedData, coordinate, _rotationSteps, cells);
            building.OnConstructionCompleted += HandleBuildingCompleted;
            _gridManager.SetOccupancy(cells, OccupancyType.Building, root);

            _history.Record(() =>
            {
                bool wasCompleted = root != null && building != null && building.IsConstructionComplete;
                _gridManager.ClearOccupancy(cells, root);
                if (root != null)
                {
                    Destroy(root);
                }

                if (_topHud != null)
                {
                    _topHud.AddWood(placedData != null ? placedData.WoodCost : 0);
                    _topHud.AddScrap(placedData != null ? placedData.ScrapCost : 0);
                    if (wasCompleted && placedData != null && placedData.CountsAsHouse)
                    {
                        _topHud.AddHouses(-1);
                    }
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
                if (!TryGetSelectedPrefab(out GameObject prefab))
                {
                    _selectedBuilding = null;
                    _previewSystem.ClearPreview();
                    return;
                }

                _previewSystem.SetPreviewPrefab(prefab);
                _previewSystem.SetVisible(true);
            }
        }

        private bool TryGetSelectedPrefab(out GameObject prefab)
        {
            prefab = null;
            if (_selectedBuilding == null)
            {
                return false;
            }

            try
            {
                prefab = _selectedBuilding.Prefab;
            }
            catch (MissingReferenceException)
            {
                Debug.LogError(
                    $"BuildingData '{_selectedBuilding.name}' has a missing prefab reference. Reassign the prefab in the inspector.",
                    _selectedBuilding);
                return false;
            }

            if (prefab == null)
            {
                Debug.LogError(
                    $"BuildingData '{_selectedBuilding.name}' has no prefab assigned. Assign a prefab in the inspector.",
                    _selectedBuilding);
                return false;
            }

            return true;
        }

        private void HandleBuildingCompleted(Building building)
        {
            if (building == null || building.Data == null)
            {
                return;
            }

            if (_topHud != null && building.Data.CountsAsHouse)
            {
                _topHud.AddHouses(1);
            }

            if (building.Data.AllowGardenPlotSelection)
            {
                HouseGardenPlotSelector selector = building.GetComponent<HouseGardenPlotSelector>();
                if (selector == null)
                {
                    selector = building.gameObject.AddComponent<HouseGardenPlotSelector>();
                }

                selector.BeginSelection(building.Data.DefaultGardenPlotSize);
            }
        }

        private void ClearTreesInsideFootprint(IReadOnlyList<GridCoordinate> cells)
        {
            if (_forestrySystem == null || _gridManager == null || cells == null || cells.Count == 0)
            {
                return;
            }

            float overlapRadius = Mathf.Max(0.2f, _gridManager.CellSize * 0.45f);
            HashSet<TreeResourceNode> affectedNodes = new HashSet<TreeResourceNode>();

            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 cellCenter = _gridManager.GridToWorld(cells[i], true);
                Collider[] hits = Physics.OverlapSphere(cellCenter + Vector3.up * 0.5f, overlapRadius);
                for (int h = 0; h < hits.Length; h++)
                {
                    if (hits[h] == null)
                    {
                        continue;
                    }

                    TreeResourceNode node = hits[h].GetComponentInParent<TreeResourceNode>();
                    if (node != null)
                    {
                        affectedNodes.Add(node);
                    }
                }
            }

            foreach (TreeResourceNode node in affectedNodes)
            {
                node.TryClearForConstruction(_forestrySystem);
            }
        }
    }
}