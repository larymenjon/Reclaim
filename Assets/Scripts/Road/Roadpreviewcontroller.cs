using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Preview visual da estrada em construção usando LineRenderer.
    /// Usa RoadSettings em vez de RoadPrefabLibrary.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RoadPreviewController : MonoBehaviour
    {
        [SerializeField] private RoadSettings      settings;
        [SerializeField] private GameObject        waypointMarkerPrefab;

        private LineRenderer            _line;
        private readonly List<GameObject> _markers = new();

        // ── Unity ────────────────────────────────────────────────────

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            if (settings != null) ApplySettings();
            SetVisible(false);
        }

        // ── API Pública ───────────────────────────────────────────────

        public void Initialize(RoadSettings s)
        {
            settings = s;
            ApplySettings();
        }

        /// <summary>
        /// Atualiza o preview. allPoints = pontos confirmados + cursor.
        /// </summary>
        public void UpdatePreview(IReadOnlyList<Vector3> allPoints, int confirmedCount, bool valid)
        {
            // Suaviza o preview com spline se houver pontos suficientes
            IReadOnlyList<Vector3> displayPts = allPoints.Count >= 2
                ? RoadSpline.BuildSpline(allPoints, settings != null ? settings.resolutionPerMeter : 3f)
                : allPoints;

            _line.positionCount = displayPts.Count;
            for (int i = 0; i < displayPts.Count; i++)
                _line.SetPosition(i, displayPts[i] + Vector3.up * 0.05f);

            Color c = valid
                ? new Color(0.95f, 0.75f, 0.4f, 0.8f)   // areia/terra quente
                : new Color(1f, 0.2f, 0.2f, 0.75f);      // vermelho = inválido
            _line.startColor = _line.endColor = c;

            SyncMarkers(allPoints, confirmedCount);
        }

        public void Show() => SetVisible(true);

        public void Hide()
        {
            SetVisible(false);
            _line.positionCount = 0;
            ClearMarkers();
        }

        // ── Internals ─────────────────────────────────────────────────

        private void ApplySettings()
        {
            _line.useWorldSpace     = true;
            _line.startWidth        = settings.previewWidth;
            _line.endWidth          = settings.previewWidth;
            _line.numCornerVertices = 5;
            _line.numCapVertices    = 5;
            if (settings.previewMaterial != null)
                _line.material = settings.previewMaterial;
        }

        private void SetVisible(bool v) => _line.enabled = v;

        private void SyncMarkers(IReadOnlyList<Vector3> allPoints, int confirmedCount)
        {
            int needed = Mathf.Max(0, confirmedCount);

            while (_markers.Count < needed)  _markers.Add(CreateMarker());
            while (_markers.Count > needed)
            {
                Destroy(_markers[^1]);
                _markers.RemoveAt(_markers.Count - 1);
            }

            for (int i = 0; i < needed && i < allPoints.Count; i++)
                _markers[i].transform.position = allPoints[i] + Vector3.up * 0.15f;
        }

        private GameObject CreateMarker()
        {
            if (waypointMarkerPrefab != null) return Instantiate(waypointMarkerPrefab);

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.35f;
            Destroy(go.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Standard"))
                { color = new Color(0.95f, 0.75f, 0.3f) };
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        private void ClearMarkers()
        {
            foreach (var m in _markers) if (m) Destroy(m);
            _markers.Clear();
        }
    }
}