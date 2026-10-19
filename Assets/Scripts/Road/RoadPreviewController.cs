using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Preview visual da estrada em construção usando LineRenderer:
    /// a curva, os pontos já colocados e a alça de direção enquanto o jogador arrasta.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RoadPreviewController : MonoBehaviour
    {
        private const float HandleWidth = 0.15f;
        private static readonly Color ValidColor = new Color(0.95f, 0.75f, 0.4f, 0.8f);
        private static readonly Color InvalidColor = new Color(1f, 0.2f, 0.2f, 0.75f);
        private static readonly Color HandleColor = new Color(1f, 1f, 1f, 0.9f);

        [SerializeField] private RoadSettings      settings;
        [SerializeField] private GameObject        waypointMarkerPrefab;

        private LineRenderer            _line;
        private LineRenderer            _handleLine;
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
        /// Atualiza o preview. nodes = pontos colocados (+ cursor, se estiver sobre o terreno).
        /// confirmedCount = quantos são pontos de verdade e ganham marcador.
        /// </summary>
        public void UpdatePreview(IReadOnlyList<RoadNode> nodes, int confirmedCount, bool valid)
        {
            List<Vector3> displayPts = nodes.Count >= 2
                ? RoadSpline.BuildSpline(nodes, settings != null ? settings.ResolutionPerMeter : 3f)
                : PositionsOf(nodes);

            _line.positionCount = displayPts.Count;
            for (int i = 0; i < displayPts.Count; i++)
                _line.SetPosition(i, displayPts[i] + Vector3.up * 0.05f);

            _line.startColor = _line.endColor = valid ? ValidColor : InvalidColor;

            SyncMarkers(nodes, confirmedCount);
        }

        /// <summary>Alça de direção: linha do ponto até o mouse enquanto o jogador arrasta.</summary>
        public void ShowHandle(Vector3 from, Vector3 to)
        {
            EnsureHandleLine();
            _handleLine.enabled = true;
            _handleLine.positionCount = 2;
            _handleLine.SetPosition(0, from + Vector3.up * 0.1f);
            _handleLine.SetPosition(1, to + Vector3.up * 0.1f);
        }

        public void HideHandle()
        {
            if (_handleLine != null)
                _handleLine.enabled = false;
        }

        public void Show() => SetVisible(true);

        public void Hide()
        {
            SetVisible(false);
            _line.positionCount = 0;
            HideHandle();
            ClearMarkers();
        }

        // ── Internals ─────────────────────────────────────────────────

        private void ApplySettings()
        {
            _line.useWorldSpace     = true;
            _line.startWidth        = settings.PreviewWidth;
            _line.endWidth          = settings.PreviewWidth;
            _line.numCornerVertices = 5;
            _line.numCapVertices    = 5;
            _line.alignment         = LineAlignment.TransformZ;
            transform.rotation      = Quaternion.Euler(90f, 0f, 0f);
            _line.sharedMaterial    = settings.PreviewMaterial != null ? settings.PreviewMaterial : DefaultLineMaterial();
        }

        private static Material _defaultLineMaterial;

        /// <summary>Material simples que respeita a cor da linha, para quando o asset não define um.</summary>
        private static Material DefaultLineMaterial()
        {
            if (_defaultLineMaterial == null)
                _defaultLineMaterial = new Material(Shader.Find("Sprites/Default")) { name = "RoadPreviewDefault" };
            return _defaultLineMaterial;
        }

        private void SetVisible(bool v) => _line.enabled = v;

        private void EnsureHandleLine()
        {
            if (_handleLine != null)
                return;

            GameObject handle = new GameObject("RoadHandle", typeof(LineRenderer));
            handle.transform.SetParent(transform, false);
            _handleLine = handle.GetComponent<LineRenderer>();
            _handleLine.useWorldSpace = true;
            _handleLine.startWidth = _handleLine.endWidth = HandleWidth;
            _handleLine.numCapVertices = 4;
            _handleLine.alignment = LineAlignment.TransformZ;
            handle.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _handleLine.sharedMaterial = DefaultLineMaterial();
            _handleLine.startColor = _handleLine.endColor = HandleColor;
        }

        private static List<Vector3> PositionsOf(IReadOnlyList<RoadNode> nodes)
        {
            List<Vector3> positions = new List<Vector3>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
                positions.Add(nodes[i].Position);
            return positions;
        }

        private void SyncMarkers(IReadOnlyList<RoadNode> nodes, int confirmedCount)
        {
            int needed = Mathf.Max(0, confirmedCount);

            while (_markers.Count < needed)  _markers.Add(CreateMarker());
            while (_markers.Count > needed)
            {
                Destroy(_markers[^1]);
                _markers.RemoveAt(_markers.Count - 1);
            }

            for (int i = 0; i < needed && i < nodes.Count; i++)
                _markers[i].transform.position = nodes[i].Position + Vector3.up * 0.15f;
        }

        private GameObject CreateMarker()
        {
            if (waypointMarkerPrefab != null) return Instantiate(waypointMarkerPrefab);

            // Sem prefab, usa uma esfera com o material padrão do pipeline (URP Lit).
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "RoadWaypoint";
            marker.transform.localScale = Vector3.one * 0.35f;
            Destroy(marker.GetComponent<Collider>());
            return marker;
        }

        private void ClearMarkers()
        {
            foreach (var m in _markers) if (m) Destroy(m);
            _markers.Clear();
        }
    }
}
