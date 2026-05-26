using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class RoadMeshBuilder : MonoBehaviour
    {
        public List<Vector3> ControlPoints { get; private set; } = new();

        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MeshCollider _collider;

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<MeshCollider>();
        }

        public void Build(IReadOnlyList<Vector3> controlPoints, RoadSettings settings)
        {
            if (controlPoints == null || controlPoints.Count < 2)
            {
                Debug.LogError("[RoadMeshBuilder] Precisa de ao menos 2 pontos de controle.");
                return;
            }

            if (settings == null)
            {
                Debug.LogError("[RoadMeshBuilder] RoadSettings e null.");
                return;
            }

            ControlPoints = new List<Vector3>(controlPoints);

            if (settings.roadMaterial != null)
                _renderer.sharedMaterial = settings.roadMaterial;
            else
                Debug.LogWarning("[RoadMeshBuilder] roadMaterial nao atribuido no RoadSettings.");

            var spline = RoadSpline.BuildSpline(controlPoints, settings.resolutionPerMeter);
            if (spline.Count < 2)
            {
                Debug.LogError("[RoadMeshBuilder] Spline gerada com menos de 2 pontos.");
                return;
            }

            SnapToTerrain(spline, settings);

            var mesh = BuildMesh(spline, settings);
            if (mesh == null) return;

            _filter.mesh = mesh;
            _collider.sharedMesh = mesh;

            Debug.Log($"[RoadMeshBuilder] Mesh criada: {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangulos.");
        }

        private static void SnapToTerrain(List<Vector3> spline, RoadSettings s)
        {
            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 pt = spline[i];
                Vector3 origin = pt + Vector3.up * s.terrainSampleHeight;

                if (TryRaycastGroundDown(origin, s.terrainSampleHeight * 2f, s.terrainLayer, out RaycastHit hit))
                    pt.y = hit.point.y + s.verticalOffset;
                else
                    pt.y += s.verticalOffset;

                spline[i] = pt;
            }
        }

        private static Mesh BuildMesh(IReadOnlyList<Vector3> spline, RoadSettings s)
        {
            int n = spline.Count;
            int cross = Mathf.Max(3, s.crossSectionVerts);
            float halfW = s.roadWidth * 0.5f;

            var verts = new List<Vector3>(n * cross);
            var norms = new List<Vector3>(n * cross);
            var uvs = new List<Vector2>(n * cross);
            var tris = new List<int>((n - 1) * (cross - 1) * 6);

            float vCoord = 0f;

            for (int i = 0; i < n; i++)
            {
                Vector3 center = spline[i];
                Vector3 tangent = RoadSpline.GetTangent(spline, i);
                tangent.y = 0f;
                if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.forward;
                tangent.Normalize();

                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

                if (i > 0)
                    vCoord += Vector3.Distance(spline[i - 1], spline[i]) / Mathf.Max(0.01f, s.roadWidth);

                for (int v = 0; v < cross; v++)
                {
                    float t = (float)v / (cross - 1);
                    float xOff = Mathf.Lerp(-halfW, halfW, t);

                    float cosVal = Mathf.Cos((t - 0.5f) * Mathf.PI);
                    float depression = s.centerDepression * cosVal * cosVal;

                    Vector3 pos = center + right * xOff;
                    pos = ProjectPointToTerrain(pos, s);
                    pos.y -= depression;

                    verts.Add(pos);
                    norms.Add(Vector3.up);
                    uvs.Add(new Vector2(t, vCoord));
                }

                if (i > 0)
                {
                    int baseA = (i - 1) * cross;
                    int baseB = i * cross;

                    for (int v = 0; v < cross - 1; v++)
                    {
                        int a0 = baseA + v;
                        int a1 = baseA + v + 1;
                        int b0 = baseB + v;
                        int b1 = baseB + v + 1;
                        tris.Add(a0); tris.Add(b0); tris.Add(a1);
                        tris.Add(a1); tris.Add(b0); tris.Add(b1);
                    }
                }
            }

            if (verts.Count == 0)
            {
                Debug.LogError("[RoadMeshBuilder] Nenhum vertice gerado.");
                return null;
            }

            var mesh = new Mesh { name = "DirtRoad" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 ProjectPointToTerrain(Vector3 point, RoadSettings s)
        {
            Vector3 origin = point + Vector3.up * s.terrainSampleHeight;
            if (TryRaycastGroundDown(origin, s.terrainSampleHeight * 2f, s.terrainLayer, out RaycastHit hit))
            {
                point.y = hit.point.y + s.verticalOffset;
            }

            return point;
        }

        private static bool TryRaycastGroundDown(Vector3 origin, float maxDistance, LayerMask mask, out RaycastHit bestHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, maxDistance, mask);
            if (hits == null || hits.Length == 0)
            {
                bestHit = default;
                return false;
            }

            int bestIndex = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                float upDot = Vector3.Dot(hits[i].normal.normalized, Vector3.up);
                if (upDot < 0.55f)
                {
                    continue;
                }

                if (hits[i].distance < bestDistance)
                {
                    bestDistance = hits[i].distance;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                bestHit = default;
                return false;
            }

            bestHit = hits[bestIndex];
            return true;
        }
    }
}
