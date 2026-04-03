using System.Collections.Generic;
using Reclaim.Grid;
using UnityEngine;

namespace Reclaim.Building
{
    public class Building : MonoBehaviour
    {
        public BuildingData Data { get; private set; }
        public GridCoordinate OriginCell { get; private set; }
        public int RotationSteps { get; private set; }
        public IReadOnlyList<GridCoordinate> OccupiedCells => _occupiedCells;

        private readonly List<GridCoordinate> _occupiedCells = new List<GridCoordinate>();

        public void Initialize(BuildingData data, GridCoordinate originCell, int rotationSteps, IReadOnlyList<GridCoordinate> occupiedCells)
        {
            Data = data;
            OriginCell = originCell;
            RotationSteps = rotationSteps;

            _occupiedCells.Clear();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                _occupiedCells.Add(occupiedCells[i]);
            }
        }
    }
}
