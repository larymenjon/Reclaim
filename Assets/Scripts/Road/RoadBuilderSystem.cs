using System;
using System.Collections.Generic;
using Reclaim.Grid;
using Reclaim.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reclaim.Road
{
    /// <summary>
    /// Ferramenta de estrada no estilo Manor Lords: o jogador clica para colocar pontos e
    /// a estrada é uma curva suave entre eles. Clicar e arrastar ao colocar um ponto define a
    /// direção em que a estrada passa por ele; sem arrastar, a curva se ajusta sozinha.
    /// Confirma no martelo ou com Enter. A estrada pronta ocupa as células do grid.
    /// </summary>
    public class RoadBuilderSystem : MonoBehaviour
    {
        private const int NoDrag = -1;

        [Header("Configurações")]
        [SerializeField] private RoadSettings settings;

        [Header("Referências de cena")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private RoadPreviewController previewController;
        [SerializeField] private ConfirmBuildUI confirmUI;
        [SerializeField] private Transform roadsParent;

        private readonly List<RoadNode> _nodes = new List<RoadNode>();
        private readonly List<RoadNode> _previewNodes = new List<RoadNode>();
        private readonly List<GridCoordinate> _roadCells = new List<GridCoordinate>();
        private Camera _camera;
        private Vector3 _cursor;
        private bool _cursorOnGround;
        private int _dragIndex = NoDrag;

        public bool IsBuilding { get; private set; }

        public event Action OnFinished;

        private void Awake()
        {
            _camera = Camera.main;

            if (settings == null)
            {
                Debug.LogError("[RoadBuilderSystem] RoadSettings não atribuído.", this);
                enabled = false;
                return;
            }

            if (previewController != null)
            {
                previewController.Initialize(settings);
            }

            if (gridManager == null)
            {
                Debug.LogWarning("[RoadBuilderSystem] GridManager não configurado; estradas não vão ocupar o grid.", this);
            }
        }

        private void Update()
        {
            if (!IsBuilding)
            {
                return;
            }

            UpdateCursor();
            HandleInput();
            RefreshPreview();
        }

        public void Enable()
        {
            if (!enabled)
            {
                return;
            }

            IsBuilding = true;
            _nodes.Clear();
            _dragIndex = NoDrag;
            SetPreviewVisible(true);
            HideConfirm();
        }

        public void Disable()
        {
            IsBuilding = false;
            _nodes.Clear();
            _dragIndex = NoDrag;
            SetPreviewVisible(false);
            HideConfirm();
        }

        // ---------- Entrada ----------

        private void HandleInput()
        {
            if (PointerInput.SecondaryPressedThisFrame)
            {
                RemoveLastNode();
                return;
            }

            if (_dragIndex != NoDrag)
            {
                ContinueDrag();
                return;
            }

            if (PointerInput.KeyPressedThisFrame(Key.Enter) || PointerInput.KeyPressedThisFrame(Key.NumpadEnter))
            {
                Confirm();
                return;
            }

            if (PointerInput.PrimaryPressedThisFrame && !PointerInput.IsOverUI)
            {
                TryAddNode();
            }
        }

        private void TryAddNode()
        {
            if (!_cursorOnGround)
            {
                return;
            }

            if (_nodes.Count > 0 && Vector3.Distance(_nodes[_nodes.Count - 1].Position, _cursor) < settings.MinPointDistance)
            {
                return;
            }

            _nodes.Add(new RoadNode(_cursor));
            _dragIndex = _nodes.Count - 1;
            HideConfirm();
        }

        /// <summary>Enquanto o botão está pressionado, o arrasto define a direção da estrada no ponto.</summary>
        private void ContinueDrag()
        {
            RoadNode node = _nodes[_dragIndex];
            Vector3 offset = _cursor - node.Position;
            offset.y = 0f;
            bool dragged = _cursorOnGround && offset.magnitude >= settings.DragThreshold;

            node.ManualDirection = dragged ? offset : Vector3.zero;
            _nodes[_dragIndex] = node;

            if (previewController != null)
            {
                if (dragged)
                {
                    previewController.ShowHandle(node.Position, node.Position + offset);
                }
                else
                {
                    previewController.HideHandle();
                }
            }

            if (PointerInput.PrimaryHeld)
            {
                return;
            }

            _dragIndex = NoDrag;
            if (previewController != null)
            {
                previewController.HideHandle();
            }

            if (_nodes.Count >= 2)
            {
                ShowConfirm();
            }
        }

        private void RemoveLastNode()
        {
            _dragIndex = NoDrag;
            if (previewController != null)
            {
                previewController.HideHandle();
            }

            if (_nodes.Count == 0)
            {
                OnFinished?.Invoke();
                return;
            }

            _nodes.RemoveAt(_nodes.Count - 1);
            if (_nodes.Count < 2)
            {
                HideConfirm();
            }
        }

        // ---------- Construção ----------

        private void Confirm()
        {
            if (_nodes.Count < 2)
            {
                return;
            }

            if (!CollectRoadCells(_nodes, _roadCells))
            {
                Debug.Log("[RoadBuilderSystem] A estrada atravessa uma construção. Ajuste o traçado.");
                ShowConfirm();
                return;
            }

            BuildRoad(_nodes);
            if (gridManager != null)
            {
                gridManager.SetOccupancy(_roadCells, OccupancyType.Road, this);
            }

            OnFinished?.Invoke();
        }

        /// <summary>
        /// Células cobertas pela estrada, considerando a largura. Falso se alguma for de construção.
        /// Com output nulo só valida.
        /// </summary>
        private bool CollectRoadCells(IReadOnlyList<RoadNode> nodes, List<GridCoordinate> output)
        {
            output?.Clear();
            if (gridManager == null)
            {
                return true;
            }

            List<Vector3> spline = RoadSpline.BuildSpline(nodes, settings.ResolutionPerMeter);
            float halfWidth = settings.RoadWidth * 0.5f;
            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 tangent = RoadSpline.GetTangent(spline, i);
                tangent.y = 0f;
                Vector3 right = tangent.sqrMagnitude > 0.0001f ? Vector3.Cross(Vector3.up, tangent.normalized) : Vector3.right;

                for (int lane = -1; lane <= 1; lane++)
                {
                    if (!TryClaimCell(spline[i] + right * (lane * halfWidth), output))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool TryClaimCell(Vector3 worldPoint, List<GridCoordinate> output)
        {
            if (!gridManager.TryWorldToGrid(worldPoint, out GridCoordinate cell))
            {
                return true;
            }

            if (!gridManager.CanPlaceRoad(cell))
            {
                return false;
            }

            if (output != null && !output.Contains(cell))
            {
                output.Add(cell);
            }

            return true;
        }

        private void BuildRoad(IReadOnlyList<RoadNode> nodes)
        {
            GameObject road = new GameObject("DirtRoad");
            road.transform.SetParent(roadsParent, true);
            road.AddComponent<RoadMeshBuilder>().Build(nodes, settings);
        }

        // ---------- Preview ----------

        private void RefreshPreview()
        {
            _previewNodes.Clear();
            _previewNodes.AddRange(_nodes);
            if (_cursorOnGround && _dragIndex == NoDrag)
            {
                _previewNodes.Add(new RoadNode(_cursor));
            }

            bool valid = _previewNodes.Count < 2 || CollectRoadCells(_previewNodes, null);
            if (previewController != null)
            {
                previewController.UpdatePreview(_previewNodes, _nodes.Count, valid);
            }

            if (confirmUI != null && confirmUI.IsVisible && _nodes.Count >= 2)
            {
                confirmUI.UpdatePosition(LastPoint());
            }
        }

        private void UpdateCursor()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            Ray ray = _camera.ScreenPointToRay(PointerInput.Position);
            _cursorOnGround = RoadGroundProbe.TryRaycast(ray, 2000f, settings.TerrainLayer, out RaycastHit hit);
            if (_cursorOnGround)
            {
                _cursor = hit.point;
            }
        }

        private void SetPreviewVisible(bool visible)
        {
            if (previewController == null)
            {
                return;
            }

            if (visible)
            {
                previewController.Show();
            }
            else
            {
                previewController.Hide();
            }
        }

        private void ShowConfirm()
        {
            if (confirmUI != null)
            {
                confirmUI.Show(LastPoint(), Confirm);
            }
        }

        private void HideConfirm()
        {
            if (confirmUI != null)
            {
                confirmUI.Hide();
            }
        }

        private Vector3 LastPoint() => _nodes.Count > 0 ? _nodes[_nodes.Count - 1].Position : Vector3.zero;
    }
}
