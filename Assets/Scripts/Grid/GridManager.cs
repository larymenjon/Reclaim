using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Grid
{
    /// <summary>
    /// Owns grid dimensions, conversions and occupancy data.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int gridWidth = 32;
        [SerializeField] private int gridHeight = 32;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private bool autoSyncFromTerrain = true;
        [SerializeField] private Terrain sourceTerrain;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gridLineColor = new Color(1f, 1f, 1f, 0.3f);

        private GridCellData[,] _cells;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;
        public float PlaneHeight => gridOrigin.y;

        private static readonly GridCoordinate[] CardinalDirections =
        {
            new GridCoordinate(0, 1),
            new GridCoordinate(1, 0),
            new GridCoordinate(0, -1),
            new GridCoordinate(-1, 0)
        };

        private void Awake()
        {
            SyncFromTerrainIfNeeded();
            InitializeGrid();
        }

        private void OnValidate()
        {
            SyncFromTerrainIfNeeded();
        }

        public void InitializeGrid()
        {
            _cells = new GridCellData[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    _cells[x, y] = new GridCellData(new GridCoordinate(x, y));
                }
            }
        }

        public bool IsInsideGrid(GridCoordinate coordinate)
        {
            return coordinate.X >= 0 && coordinate.X < gridWidth &&
                   coordinate.Y >= 0 && coordinate.Y < gridHeight;
        }

        public bool TryGetCell(GridCoordinate coordinate, out GridCellData cell)
        {
            cell = null;
            if (!IsInsideGrid(coordinate))
            {
                return false;
            }

            cell = _cells[coordinate.X, coordinate.Y];
            return true;
        }

        public bool TryWorldToGrid(Vector3 worldPosition, out GridCoordinate coordinate)
        {
            Vector3 relative = worldPosition - gridOrigin;

            int x = Mathf.FloorToInt(relative.x / cellSize);
            int y = Mathf.FloorToInt(relative.z / cellSize);

            coordinate = new GridCoordinate(x, y);
            return IsInsideGrid(coordinate);
        }

        public Vector3 GridToWorld(GridCoordinate coordinate, bool center = true)
        {
            float offset = center ? cellSize * 0.5f : 0f;
            return gridOrigin + new Vector3(
                coordinate.X * cellSize + offset,
                0f,
                coordinate.Y * cellSize + offset
            );
        }

        public Vector3 GetFootprintCenterWorld(GridCoordinate originCell, Vector2Int size, int rotationSteps)
        {
            Vector2Int effectiveSize = GetEffectiveSize(size, rotationSteps);

            return gridOrigin + new Vector3(
                (originCell.X + effectiveSize.x * 0.5f) * cellSize,
                0f,
                (originCell.Y + effectiveSize.y * 0.5f) * cellSize
            );
        }

        public List<GridCoordinate> GetFootprintCells(GridCoordinate originCell, Vector2Int size, int rotationSteps)
        {
            Vector2Int effectiveSize = GetEffectiveSize(size, rotationSteps);
            List<GridCoordinate> result = new List<GridCoordinate>(effectiveSize.x * effectiveSize.y);

            for (int x = 0; x < effectiveSize.x; x++)
            {
                for (int y = 0; y < effectiveSize.y; y++)
                {
                    result.Add(new GridCoordinate(originCell.X + x, originCell.Y + y));
                }
            }

            return result;
        }

        public bool AreCellsBuildable(IReadOnlyList<GridCoordinate> coordinates)
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (!TryGetCell(coordinates[i], out GridCellData cell))
                {
                    return false;
                }

                if (cell.IsOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanPlaceRoad(GridCoordinate coordinate)
        {
            if (!TryGetCell(coordinate, out GridCellData cell))
            {
                return false;
            }

            return cell.OccupancyType != OccupancyType.Building;
        }

        public bool IsRoadCell(GridCoordinate coordinate)
        {
            return TryGetCell(coordinate, out GridCellData cell) && cell.OccupancyType == OccupancyType.Road;
        }

        public bool IsCellOccupied(GridCoordinate coordinate)
        {
            return TryGetCell(coordinate, out GridCellData cell) && cell.IsOccupied;
        }

        public void SetOccupancy(IReadOnlyList<GridCoordinate> coordinates, OccupancyType occupancyType, Object occupant)
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (TryGetCell(coordinates[i], out GridCellData cell))
                {
                    cell.SetOccupancy(occupancyType, occupant);
                }
            }
        }

        public void SetOccupancy(GridCoordinate coordinate, OccupancyType occupancyType, Object occupant)
        {
            if (TryGetCell(coordinate, out GridCellData cell))
            {
                cell.SetOccupancy(occupancyType, occupant);
            }
        }

        public void ClearOccupancy(IReadOnlyList<GridCoordinate> coordinates, Object expectedOccupant = null)
        {
            for (int i = 0; i < coordinates.Count; i++)
            {
                if (TryGetCell(coordinates[i], out GridCellData cell))
                {
                    cell.ClearOccupancy(expectedOccupant);
                }
            }
        }

        public void ClearOccupancy(GridCoordinate coordinate, Object expectedOccupant = null)
        {
            if (TryGetCell(coordinate, out GridCellData cell))
            {
                cell.ClearOccupancy(expectedOccupant);
            }
        }

        public IEnumerable<GridCoordinate> GetCardinalNeighbors(GridCoordinate coordinate)
        {
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                GridCoordinate neighbor = coordinate + CardinalDirections[i];
                if (IsInsideGrid(neighbor))
                {
                    yield return neighbor;
                }
            }
        }

        private static Vector2Int GetEffectiveSize(Vector2Int original, int rotationSteps)
        {
            int normalized = NormalizeRotationSteps(rotationSteps);
            bool swap = normalized % 2 != 0;
            return swap ? new Vector2Int(original.y, original.x) : original;
        }

        private static int NormalizeRotationSteps(int rotationSteps)
        {
            int normalized = rotationSteps % 4;
            if (normalized < 0)
            {
                normalized += 4;
            }

            return normalized;
        }

        private void SyncFromTerrainIfNeeded()
        {
            if (!autoSyncFromTerrain)
            {
                return;
            }

            if (sourceTerrain == null)
            {
                sourceTerrain = FindFirstObjectByType<Terrain>();
            }

            if (sourceTerrain == null || sourceTerrain.terrainData == null)
            {
                return;
            }

            Vector3 terrainOrigin = sourceTerrain.transform.position;
            Vector3 terrainSize = sourceTerrain.terrainData.size;

            gridOrigin = new Vector3(terrainOrigin.x, terrainOrigin.y, terrainOrigin.z);
            gridWidth = Mathf.Max(1, Mathf.FloorToInt(terrainSize.x / Mathf.Max(0.001f, cellSize)));
            gridHeight = Mathf.Max(1, Mathf.FloorToInt(terrainSize.z / Mathf.Max(0.001f, cellSize)));
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || cellSize <= 0f || gridWidth <= 0 || gridHeight <= 0)
            {
                return;
            }

            Gizmos.color = gridLineColor;
            Vector3 size = new Vector3(gridWidth * cellSize, 0f, gridHeight * cellSize);
            Vector3 center = gridOrigin + new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);
            Gizmos.DrawWireCube(center, size);

            for (int x = 0; x <= gridWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0f, 0f);
                Vector3 end = start + new Vector3(0f, 0f, gridHeight * cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0f, 0f, y * cellSize);
                Vector3 end = start + new Vector3(gridWidth * cellSize, 0f, 0f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
