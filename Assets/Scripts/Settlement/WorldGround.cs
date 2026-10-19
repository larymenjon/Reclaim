using UnityEngine;

namespace Reclaim.Settlement
{
    /// <summary>Consulta de altura e inclinação do terreno, usada por quem coloca coisas no mundo.</summary>
    public static class WorldGround
    {
        private const float RayStartHeight = 2000f;

        /// <summary>Altura do chão em (x, z). Sem terreno nem colisor, devolve o fallback.</summary>
        public static float HeightAt(float x, float z, float fallback = 0f)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                return terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
            }

            Vector3 origin = new Vector3(x, RayStartHeight, z);
            return Physics.Raycast(origin, Vector3.down, out RaycastHit hit, RayStartHeight * 2f) ? hit.point.y : fallback;
        }

        public static Vector3 Snap(Vector3 position)
        {
            position.y = HeightAt(position.x, position.z, position.y);
            return position;
        }

        /// <summary>Inclinação do terreno em graus em (x, z). Sem terreno, 0.</summary>
        public static float SlopeAt(float x, float z)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                return 0f;
            }

            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            float u = Mathf.Clamp01((x - origin.x) / size.x);
            float v = Mathf.Clamp01((z - origin.z) / size.z);
            return terrain.terrainData.GetSteepness(u, v);
        }

        /// <summary>Limites do terreno no plano XZ. Falso se a cena não tem terreno.</summary>
        public static bool TryGetTerrainRect(out Rect rect)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                rect = default;
                return false;
            }

            Vector3 origin = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            rect = new Rect(origin.x, origin.z, size.x, size.z);
            return true;
        }
    }
}
