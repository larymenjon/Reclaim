using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Exibe o preview da estrada enquanto o jogador desenha.
    /// Requer um LineRenderer no mesmo GameObject.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RoadPreviewController : MonoBehaviour
    {
        [SerializeField] private RoadPrefabLibrary prefabLibrary;
        [SerializeField] private GameObject        waypointMarkerPrefab;

        private LineRenderer        _line;
        private readonly List<GameObject> _markers = new();

        // ── Unity ────────────────────────────────────────────────────

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            ConfigureLine();
            SetVisible(false);
        }

        private void ConfigureLine()
        {
            _line.useWorldSpace     = true;
            _line.startWidth        = prefabLibrary != null ? prefabLibrary.roadWidth : 2f;
            _line.endWidth          = _line.startWidth;
            _line.positionCount     = 0;
            _line.numCornerVertices = 4;
            _line.numCapVertices    = 4;

            if (prefabLibrary?.previewValidMaterial != null)
                _line.material = prefabLibrary.previewValidMaterial;
        }

        // ── Public API ────────────────────────────────────────────────

        public void Initialize(RoadPrefabLibrary library)
        {
            prefabLibrary = library;
            ConfigureLine();
        }

        /// <summary>
        /// Atualiza o preview.
        /// </summary>
        /// <param name="allPoints">Pontos confirmados + posição atual do cursor.</param>
        /// <param name="confirmedCount">Quantos pontos foram confirmados (recebem marcador).</param>
        /// <param name="valid">Se a posição do cursor é válida para colocação.</param>
        public void UpdatePreview(IReadOnlyList<Vector3> allPoints, int confirmedCount, bool valid)
        {
            // LineRenderer
            _line.positionCount = allPoints.Count;
            for (int i = 0; i < allPoints.Count; i++)
                _line.SetPosition(i, allPoints[i] + Vector3.up * 0.05f);

            Color c = valid
                ? new Color(0.2f, 1f, 0.2f, 0.75f)
                : new Color(1f, 0.2f, 0.2f, 0.75f);
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

        private void SetVisible(bool visible) => _line.enabled = visible;

        private void SyncMarkers(IReadOnlyList<Vector3> allPoints, int confirmedCount)
        {
            int needed = Mathf.Max(0, confirmedCount);

            while (_markers.Count < needed)
                _markers.Add(CreateMarker());

            while (_markers.Count > needed)
            {
                Destroy(_markers[^1]);
                _markers.RemoveAt(_markers.Count - 1);
            }

            for (int i = 0; i < needed && i < allPoints.Count; i++)
                _markers[i].transform.position = allPoints[i] + Vector3.up * 0.1f;
        }

        private GameObject CreateMarker()
        {
            if (waypointMarkerPrefab != null)
                return Instantiate(waypointMarkerPrefab);

            // Fallback: esfera amarela
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.4f;
            Destroy(go.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Standard")) { color = new Color(1f, 0.85f, 0f) };
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
