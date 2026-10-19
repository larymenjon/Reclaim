using System;
using System.Collections.Generic;
using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Grid;
using Reclaim.Input;
using UnityEngine;

namespace Reclaim.Construction
{
    /// <summary>
    /// Ferramenta de construção: seleção, pré-visualização, validação, cobrança e posicionamento.
    /// Só reage à entrada enquanto estiver ativa; quem ativa é o ToolModeController.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        [Header("Dependências de cena")]
        [SerializeField] private ResourceStock resourceStock;
        [SerializeField] private TreeForestrySystem forestrySystem;
        [Tooltip("Pai dos objetos construídos. Se vazio, ficam na raiz da cena.")]
        [SerializeField] private Transform buildingsParent;

        private readonly List<GridCoordinate> _footprint = new List<GridCoordinate>();
        private readonly HashSet<TreeResourceNode> _treesInFootprint = new HashSet<TreeResourceNode>();
        private readonly Collider[] _overlapHits = new Collider[16];

        private GridManager _gridManager;
        private PreviewSystem _previewSystem;
        private PlacementHistory _history;
        private InputHandler _inputHandler;

        private BuildingData _selectedBuilding;
        private int _rotationSteps;
        private GridCoordinate _lastHoverCell;
        private bool _hasHover;
        private bool _isActive;

        public BuildingData SelectedBuilding => _selectedBuilding;
        public int CompletedHouses { get; private set; }

        public event Action<BuildingData> OnSelectedBuildingChanged;
        public event Action<Building> OnBuildingPlaced;
        public event Action<Building> OnBuildingCompleted;
        /// <summary>Jogador pediu para sair da ferramenta (botão direito).</summary>
        public event Action OnCancelRequested;
        /// <summary>Clique válido no grid, mas sem recursos. Usado pelo aviso rápido da HUD.</summary>
        public event Action<BuildingData> OnPlacementRejectedForCost;

        public void Initialize(GridManager gridManager, PreviewSystem previewSystem, PlacementHistory history, InputHandler inputHandler)
        {
            _gridManager = gridManager;
            _previewSystem = previewSystem;
            _history = history;
            _inputHandler = inputHandler;

            if (resourceStock == null)
            {
                Debug.LogError("[BuildingSystem] ResourceStock não configurado. Construções não serão cobradas.", this);
            }

            _inputHandler.OnPointerMoved += HandlePointerMoved;
            _inputHandler.OnPrimaryPressed += HandlePrimaryPressed;
            _inputHandler.OnRotatePressed += HandleRotatePressed;
            _inputHandler.OnSecondaryPressed += HandleSecondaryPressed;
        }

        private void OnDestroy()
        {
            if (_inputHandler == null)
            {
                return;
            }

            _inputHandler.OnPointerMoved -= HandlePointerMoved;
            _inputHandler.OnPrimaryPressed -= HandlePrimaryPressed;
            _inputHandler.OnRotatePressed -= HandleRotatePressed;
            _inputHandler.OnSecondaryPressed -= HandleSecondaryPressed;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            _hasHover = false;

            if (!active || _selectedBuilding == null)
            {
                _previewSystem.ClearPreview();
                return;
            }

            ShowPreviewFor(_selectedBuilding);
        }

        public void SelectBuilding(BuildingData buildingData)
        {
            _selectedBuilding = buildingData != null && buildingData.Prefab != null ? buildingData : null;
            _rotationSteps = 0;

            if (buildingData != null && _selectedBuilding == null)
            {
                Debug.LogError($"[BuildingSystem] '{buildingData.name}' não tem prefab atribuído.", buildingData);
            }

            OnSelectedBuildingChanged?.Invoke(_selectedBuilding);

            if (_isActive && _selectedBuilding != null)
            {
                ShowPreviewFor(_selectedBuilding);
            }
            else
            {
                _previewSystem.ClearPreview();
            }
        }

        public bool CanAfford(BuildingData data)
        {
            return data != null && (resourceStock == null || resourceStock.CanAfford(data.Costs));
        }

        private void ShowPreviewFor(BuildingData data)
        {
            _previewSystem.SetPreviewPrefab(data.Prefab);
            _previewSystem.SetVisible(false);
        }

        private void HandlePointerMoved(GridCoordinate cell, Vector3 worldPoint)
        {
            if (!_isActive || _selectedBuilding == null)
            {
                return;
            }

            _lastHoverCell = cell;
            _hasHover = true;
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            FillFootprint(_lastHoverCell);
            bool valid = _gridManager.AreCellsBuildable(_footprint) && CanAfford(_selectedBuilding);

            _previewSystem.SetTransform(GetPlacementPosition(_lastHoverCell), GetPlacementRotation());
            _previewSystem.SetValidity(valid);
            _previewSystem.SetVisible(true);
        }

        private void HandlePrimaryPressed(GridCoordinate cell, Vector3 worldPoint)
        {
            if (!_isActive || _selectedBuilding == null)
            {
                return;
            }

            FillFootprint(cell);
            if (!_gridManager.AreCellsBuildable(_footprint))
            {
                return;
            }

            if (resourceStock != null && !resourceStock.TrySpend(_selectedBuilding.Costs))
            {
                OnPlacementRejectedForCost?.Invoke(_selectedBuilding);
                return;
            }

            Place(_selectedBuilding, cell);
        }

        private void Place(BuildingData data, GridCoordinate originCell)
        {
            List<GridCoordinate> cells = new List<GridCoordinate>(_footprint);
            ClearTreesInside(cells);

            GameObject root = new GameObject($"{data.DisplayName}_Building");
            root.transform.SetParent(buildingsParent, false);
            root.transform.SetPositionAndRotation(GetPlacementPosition(originCell), GetPlacementRotation());

            Building building = root.AddComponent<Building>();
            building.OnConstructionCompleted += HandleBuildingCompleted;
            building.Initialize(data, originCell, _rotationSteps, cells);
            _gridManager.SetOccupancy(cells, OccupancyType.Building, root);

            _history.Record(() => UndoPlacement(building, data, cells));
            OnBuildingPlaced?.Invoke(building);
        }

        private void UndoPlacement(Building building, BuildingData data, List<GridCoordinate> cells)
        {
            if (building == null)
            {
                return;
            }

            if (building.IsConstructionComplete && data.CountsAsHouse)
            {
                CompletedHouses = Mathf.Max(0, CompletedHouses - 1);
            }

            _gridManager.ClearOccupancy(cells, building.gameObject);
            Destroy(building.gameObject);

            if (resourceStock != null)
            {
                resourceStock.AddAll(data.Costs);
            }
        }

        private void HandleRotatePressed()
        {
            if (!_isActive || _selectedBuilding == null || !_selectedBuilding.CanRotate)
            {
                return;
            }

            _rotationSteps = (_rotationSteps + 1) % 4;
            if (_hasHover)
            {
                RefreshPreview();
            }
        }

        private void HandleSecondaryPressed()
        {
            if (_isActive)
            {
                OnCancelRequested?.Invoke();
            }
        }

        private void HandleBuildingCompleted(Building building)
        {
            building.OnConstructionCompleted -= HandleBuildingCompleted;
            if (building.Data.CountsAsHouse)
            {
                CompletedHouses++;
            }

            OnBuildingCompleted?.Invoke(building);
        }

        private void FillFootprint(GridCoordinate originCell)
        {
            _footprint.Clear();
            _footprint.AddRange(_gridManager.GetFootprintCells(originCell, _selectedBuilding.Size, _rotationSteps));
        }

        private Vector3 GetPlacementPosition(GridCoordinate originCell)
        {
            return _gridManager.GetFootprintCenterWorld(originCell, _selectedBuilding.Size, _rotationSteps);
        }

        private Quaternion GetPlacementRotation()
        {
            return Quaternion.Euler(0f, _rotationSteps * 90f, 0f);
        }

        private void ClearTreesInside(IReadOnlyList<GridCoordinate> cells)
        {
            if (forestrySystem == null)
            {
                return;
            }

            _treesInFootprint.Clear();
            float radius = Mathf.Max(0.2f, _gridManager.CellSize * 0.45f);

            for (int i = 0; i < cells.Count; i++)
            {
                Vector3 center = _gridManager.GridToWorld(cells[i]) + Vector3.up * 0.5f;
                int hitCount = Physics.OverlapSphereNonAlloc(center, radius, _overlapHits);
                for (int h = 0; h < hitCount; h++)
                {
                    TreeResourceNode node = _overlapHits[h].GetComponentInParent<TreeResourceNode>();
                    if (node != null)
                    {
                        _treesInFootprint.Add(node);
                    }
                }
            }

            foreach (TreeResourceNode node in _treesInFootprint)
            {
                node.TryClearForConstruction(forestrySystem);
            }
        }
    }
}
