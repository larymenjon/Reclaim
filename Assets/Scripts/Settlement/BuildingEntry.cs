using Reclaim.Construction;
using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>Porta de uma construção: o ponto do lado de fora, na frente (-Z do modelo), onde moradores chegam e entram.</summary>
    public static class BuildingEntry
    {
        public static Vector3 PointFor(Building building, float cellSize, float doorDistance = 1.6f)
        {
            float halfDepth = building.Data.Size.y * cellSize * 0.5f;
            float centerX = 0f;
            float centerZ = 0f;

            if (BuildingVisualFit.TryGetLocalBounds(building.transform, building.gameObject, out Bounds bounds))
            {
                halfDepth = Mathf.Max(halfDepth, bounds.extents.z);
                centerX = bounds.center.x;
                centerZ = bounds.center.z;
            }

            Vector3 local = new Vector3(centerX, 0f, centerZ - halfDepth - doorDistance);
            return WorldGround.Snap(building.transform.TransformPoint(local));
        }
    }
}
