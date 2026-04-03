using System.Collections.Generic;
using Reclaim;
using Reclaim.Core;
using Reclaim.Grid;
using Reclaim.Input;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Handles drag placement and neighbor connectivity of roads.
    /// </summary>
    public class RoadSystem : MonoBehaviour
    {
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private float roadHeightOffset = 0.05f;
        [SerializeField] private Transform roadParent;

        private GridManager _gridManager;
        private PreviewSystem _previewSystem;
        private GameManager _gameManager;
        private PlacementHistory _history;

        private readonly Dictionary<GridCoordinate, Road> _roads = new Dictionary<GridCoordinate, Road>();
        private bool _isDragging;
        private bool _isDeletingDrag;
        private GridCoordinate _lastDragCoordinate;
        private bool _isRoadPreviewActive;

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
            inputHandler.OnPrimaryHeld += HandlePrimaryHeld;
            inputHandler.OnPrimaryReleased += HandlePrimaryReleased;
            inputHandler.OnDeletePressed += HandleDeletePressed;
            inputHandler.OnDeleteHeld += HandleDeleteHeld;
            inputHandler.OnSecondaryPressed += HandleSecondaryPressed;
            _gameManager.OnModeChanged += HandleModeChanged;
        }

        public void SetRoadPrefab(GameObject prefab)
        {
            roadPrefab = prefab;
        }

        private void HandlePointerMoved(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Road || roadPrefab == null)
            {
                return;
            }

            EnsureRoadPreview();
            _previewSystem.SetVisible(true);
            _previewSystem.SetTransform(GetRoadWorldPosition(coordinate), Quaternion.identity);
            _previewSystem.SetValidity(_gridManager.CanPlaceRoad(coordinate));
        }

        private void HandlePrimaryPressed(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Road || roadPrefab == null)
            {
                return;
            }

            _isDragging = true;
            _isDeletingDrag = false;
            _lastDragCoordinate = coordinate;
            TryPlaceRoad(coordinate);
        }

        private void HandlePrimaryHeld(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Road || !_isDragging)
            {
                return;
            }

            if (coordinate.Equals(_lastDragCoordinate))
            {
                return;
            }

            PlaceRoadLine(_lastDragCoordinate, coordinate);
            _lastDragCoordinate = coordinate;
        }

        private void HandlePrimaryReleased(GridCoordinate _, Vector3 __)
        {
            _isDragging = false;
            _isDeletingDrag = false;
        }

        private void HandleDeletePressed(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Road)
            {
                return;
            }

            _isDragging = true;
            _isDeletingDrag = true;
            _lastDragCoordinate = coordinate;
            TryRemoveRoad(coordinate);
        }

        private void HandleDeleteHeld(GridCoordinate coordinate, Vector3 _)
        {
            if (_gameManager.CurrentMode != GameMode.Road || !_isDragging || !_isDeletingDrag)
            {
                return;
            }

            if (coordinate.Equals(_lastDragCoordinate))
            {
                return;
            }

            RemoveRoadLine(_lastDragCoordinate, coordinate);
            _lastDragCoordinate = coordinate;
        }

        private void HandleSecondaryPressed()
        {
            if (_gameManager.CurrentMode == GameMode.Road)
            {
                _gameManager.SetMode(GameMode.None);
            }
        }

        private void HandleModeChanged(GameMode mode)
        {
            if (mode == GameMode.Road)
            {
                EnsureRoadPreview();
                _previewSystem.SetVisible(true);
                return;
            }

            _isDragging = false;
            _isDeletingDrag = false;
            _isRoadPreviewActive = false;
            _previewSystem.SetVisible(false);
        }

        private void EnsureRoadPreview()
        {
            if (_isRoadPreviewActive || roadPrefab == null)
            {
                return;
            }

            _previewSystem.SetPreviewPrefab(roadPrefab);
            _isRoadPreviewActive = true;
        }

        private void PlaceRoadLine(GridCoordinate from, GridCoordinate to)
        {
            GridCoordinate current = from;
            while (!current.Equals(to))
            {
                int stepX = Mathf.Clamp(to.X - current.X, -1, 1);
                int stepY = Mathf.Clamp(to.Y - current.Y, -1, 1);

                if (stepX != 0)
                {
                    current = new GridCoordinate(current.X + stepX, current.Y);
                }
                else if (stepY != 0)
                {
                    current = new GridCoordinate(current.X, current.Y + stepY);
                }

                TryPlaceRoad(current);
            }
        }

        private void TryPlaceRoad(GridCoordinate coordinate)
        {
            if (!_gridManager.CanPlaceRoad(coordinate))
            {
                return;
            }

            if (_roads.ContainsKey(coordinate))
            {
                return;
            }

            GameObject instance = Instantiate(roadPrefab, GetRoadWorldPosition(coordinate), Quaternion.identity, roadParent);
            Road road = instance.GetComponent<Road>();
            if (road == null)
            {
                road = instance.AddComponent<Road>();
            }

            _roads[coordinate] = road;
            _gridManager.SetOccupancy(coordinate, OccupancyType.Road, instance);
            RefreshConnectionsAround(coordinate);

            _history.Record(() =>
            {
                _roads.Remove(coordinate);
                _gridManager.ClearOccupancy(coordinate, instance);

                if (instance != null)
                {
                    Destroy(instance);
                }

                RefreshConnectionsAround(coordinate);
            });
        }

        private void RemoveRoadLine(GridCoordinate from, GridCoordinate to)
        {
            GridCoordinate current = from;
            while (!current.Equals(to))
            {
                int stepX = Mathf.Clamp(to.X - current.X, -1, 1);
                int stepY = Mathf.Clamp(to.Y - current.Y, -1, 1);

                if (stepX != 0)
                {
                    current = new GridCoordinate(current.X + stepX, current.Y);
                }
                else if (stepY != 0)
                {
                    current = new GridCoordinate(current.X, current.Y + stepY);
                }

                TryRemoveRoad(current);
            }
        }

        private void TryRemoveRoad(GridCoordinate coordinate)
        {
            if (!_roads.TryGetValue(coordinate, out Road road))
            {
                return;
            }

            GameObject instance = road.gameObject;
            _roads.Remove(coordinate);
            _gridManager.ClearOccupancy(coordinate, instance);

            if (instance != null)
            {
                Destroy(instance);
            }

            RefreshConnectionsAround(coordinate);

            _history.Record(() =>
            {
                if (_roads.ContainsKey(coordinate) || !_gridManager.CanPlaceRoad(coordinate))
                {
                    return;
                }

                GameObject restored = Instantiate(roadPrefab, GetRoadWorldPosition(coordinate), Quaternion.identity, roadParent);
                Road restoredRoad = restored.GetComponent<Road>();
                if (restoredRoad == null)
                {
                    restoredRoad = restored.AddComponent<Road>();
                }

                _roads[coordinate] = restoredRoad;
                _gridManager.SetOccupancy(coordinate, OccupancyType.Road, restored);
                RefreshConnectionsAround(coordinate);
            });
        }

        private void RefreshConnectionsAround(GridCoordinate coordinate)
        {
            RefreshRoadAt(coordinate);

            foreach (GridCoordinate neighbor in _gridManager.GetCardinalNeighbors(coordinate))
            {
                RefreshRoadAt(neighbor);
            }
        }

        private void RefreshRoadAt(GridCoordinate coordinate)
        {
            if (!_roads.TryGetValue(coordinate, out Road road))
            {
                return;
            }

            bool north = _roads.ContainsKey(new GridCoordinate(coordinate.X, coordinate.Y + 1));
            bool east = _roads.ContainsKey(new GridCoordinate(coordinate.X + 1, coordinate.Y));
            bool south = _roads.ContainsKey(new GridCoordinate(coordinate.X, coordinate.Y - 1));
            bool west = _roads.ContainsKey(new GridCoordinate(coordinate.X - 1, coordinate.Y));

            road.SetConnections(north, east, south, west);
        }

        private Vector3 GetRoadWorldPosition(GridCoordinate coordinate)
        {
            Vector3 world = _gridManager.GridToWorld(coordinate);
            world.y += roadHeightOffset;
            return world;
        }
    }
}
