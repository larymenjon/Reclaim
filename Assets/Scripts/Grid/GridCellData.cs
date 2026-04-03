using UnityEngine;

namespace Reclaim.Grid
{
    public class GridCellData
    {
        public GridCoordinate Coordinate { get; }
        public OccupancyType OccupancyType { get; private set; }
        public Object Occupant { get; private set; }

        public bool IsOccupied => OccupancyType != OccupancyType.None;

        public GridCellData(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
            OccupancyType = OccupancyType.None;
        }

        public void SetOccupancy(OccupancyType occupancyType, Object occupant)
        {
            OccupancyType = occupancyType;
            Occupant = occupant;
        }

        public void ClearOccupancy(Object expectedOccupant = null)
        {
            if (expectedOccupant != null && Occupant != expectedOccupant)
            {
                return;
            }

            OccupancyType = OccupancyType.None;
            Occupant = null;
        }
    }
}
