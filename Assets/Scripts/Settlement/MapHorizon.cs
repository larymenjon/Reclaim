using System.Collections.Generic;
using Reclaim.Calendar;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reclaim.Settlement
{
    /// <summary>
    /// O mapa tem limite, mas o campo de visão não mostra o fim do mundo: em volta do terreno cresce um
    /// anel de montanhas procedurais que sai da borda do terreno e se perde na névoa.
    /// A câmera continua presa aos limites do mapa; o horizonte é só cenário.
    /// </summary>
    public class MapHorizon : MonoBehaviour
    {
        private const float TargetCellSize = 50f;
        private const float RampDistance = 600f;
        private const float NoiseScale = 0.0011f;
        private const float TextureTileMeters = 40f;

        private SettlementSettings _settings;
        private SimulationClock _clock;

        public void Initialize(SettlementSettings settings, SimulationClock clock)
        {
            _settings = settings;
            _clock = clock;

            if (settings.CreateHorizon && WorldGround.TryGetTerrainRect(out Rect rect))
            {
                BuildRing(rect);
            }

            if (settings.UseFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = settings.FogStartDistance;
                RenderSettings.fogEndDistance = settings.FogEndDistance;
                UpdateFogColor();
            }
        }

        private void Update()
        {
            if (_settings != null && _settings.UseFog)
            {
                UpdateFogColor();
            }
        }

        private void UpdateFogColor()
        {
            float hour = _clock != null ? _clock.CurrentDate.DayProgress01 * 24f : 12f;
            float daylight = Mathf.Clamp01(Mathf.Sin((hour - 6f) / 12f * Mathf.PI) * 1.6f);
            RenderSettings.fogColor = Color.Lerp(_settings.FogNightColor, _settings.FogDayColor, daylight);
        }

        private void BuildRing(Rect rect)
        {
            int cellsX = Mathf.Max(1, Mathf.RoundToInt(rect.width / TargetCellSize));
            int cellsZ = Mathf.Max(1, Mathf.RoundToInt(rect.height / TargetCellSize));
            float cellX = rect.width / cellsX;
            float cellZ = rect.height / cellsZ;
            int marginX = Mathf.CeilToInt(_settings.HorizonWidth / cellX);
            int marginZ = Mathf.CeilToInt(_settings.HorizonWidth / cellZ);
            int countX = cellsX + 2 * marginX;
            int countZ = cellsZ + 2 * marginZ;

            Vector3[] vertices = new Vector3[(countX + 1) * (countZ + 1)];
            Vector2[] uvs = new Vector2[vertices.Length];
            float seed = Random.Range(0f, 1000f);

            for (int j = 0; j <= countZ; j++)
            {
                for (int i = 0; i <= countX; i++)
                {
                    float x = rect.xMin + (i - marginX) * cellX;
                    float z = rect.yMin + (j - marginZ) * cellZ;
                    int index = j * (countX + 1) + i;
                    vertices[index] = new Vector3(x, HeightFor(rect, x, z, seed), z);
                    uvs[index] = new Vector2(x, z) / TextureTileMeters;
                }
            }

            List<int> triangles = new List<int>(countX * countZ * 6);
            for (int j = 0; j < countZ; j++)
            {
                for (int i = 0; i < countX; i++)
                {
                    float centerX = rect.xMin + (i + 0.5f - marginX) * cellX;
                    float centerZ = rect.yMin + (j + 0.5f - marginZ) * cellZ;
                    if (rect.Contains(new Vector2(centerX, centerZ)))
                    {
                        continue;
                    }

                    int a = j * (countX + 1) + i;
                    int b = a + 1;
                    int c = a + countX + 1;
                    int d = c + 1;
                    triangles.Add(a); triangles.Add(c); triangles.Add(b);
                    triangles.Add(b); triangles.Add(c); triangles.Add(d);
                }
            }

            Mesh mesh = new Mesh { name = "MapHorizon", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = BuildMaterial();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>Na borda a altura é a do terreno; depois sobe em cordilheiras de ruído.</summary>
        private float HeightFor(Rect rect, float x, float z, float seed)
        {
            float edgeX = Mathf.Clamp(x, rect.xMin, rect.xMax);
            float edgeZ = Mathf.Clamp(z, rect.yMin, rect.yMax);
            float baseHeight = WorldGround.HeightAt(edgeX, edgeZ);

            float distance = Vector2.Distance(new Vector2(x, z), new Vector2(edgeX, edgeZ));
            if (distance <= 0f)
            {
                return baseHeight;
            }

            float ramp = Mathf.SmoothStep(0f, 1f, distance / RampDistance);
            float ridge = Mathf.PerlinNoise(seed + x * NoiseScale, seed + z * NoiseScale);
            float detail = Mathf.PerlinNoise(seed + x * NoiseScale * 4f, seed + z * NoiseScale * 4f);
            float mountains = (0.25f + 0.75f * ridge) * _settings.HorizonMountainHeight + detail * 25f;
            return baseHeight + ramp * mountains;
        }

        private static Material BuildMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader) { name = "MapHorizon" };
            Color tint = new Color(0.55f, 0.5f, 0.42f);

            Terrain terrain = Terrain.activeTerrain;
            TerrainLayer[] layers = terrain != null ? terrain.terrainData.terrainLayers : null;
            if (layers != null && layers.Length > 0 && layers[0] != null && layers[0].diffuseTexture != null)
            {
                Texture texture = layers[0].diffuseTexture;
                material.mainTexture = texture;
                material.mainTextureScale = Vector2.one;
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
            }

            material.color = tint;
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0f);
            }

            return material;
        }
    }
}
