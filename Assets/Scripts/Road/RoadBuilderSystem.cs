using System;
using System.Collections.Generic;
using Reclaim.Grid;
using Reclaim.Input;
using UnityEngine;

namespace Reclaim.Road
{
    /// <summary>
    /// Ferramenta de estrada: o jogador clica pontos no terreno, vê a curva e confirma no martelo.
    /// A estrada pronta ocupa as células do grid, impedindo construções por cima.
    /// </summary>
    public class RoadBuilderSystem : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private RoadSettings settings;

        [Header("Referências de cena")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private RoadPreviewController previewController;
        [SerializeField] private ConfirmBuildUI confirmUI;
        [SerializeField] private Transform roadsParent;

        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly List<Vector3> _previewPoints = new List<Vector3>();
        private readonly List<GridCoordinate> _roadCells = new List<GridCoordinate>();
        private Camera _camera;
        private Vector3 _cursor;
        private bool _cursorOnGround;

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
            _points.Clear();
            SetPreviewVisible(true);
            HideConfirm();
        }

        public void Disable()
        {
            IsBuilding = false;
            _points.Clear();
            SetPreviewVisible(false);
            HideConfirm();
        }

        private void HandleInput()
        {
            if (PointerInput.SecondaryPressedThisFrame)
            {
                RemoveLastPoint();
                return;
            }

            if (PointerInput.PrimaryPressedThisFrame && !PointerInput.IsOverUI)
            {
                TryAddPoint();
            }
        }

        private void TryAddPoint()
        {
            if (!_cursorOnGround)
            {
                return;
            }

            if (_points.Count > 0 && Vector3.Distance(_points[_points.Count - 1], _cursor) < settings.MinPointDistance)
            {
                return;
            }

            _points.Add(_cursor);
            if (_points.Count >= 2)
            {
                ShowConfirm();
            }
        }

        private void RemoveLastPoint()
        {
            if (_points.Count == 0)
            {
                OnFinished?.Invoke();
                return;
            }

            _points.RemoveAt(_points.Count - 1);
            if (_points.Count < 2)
            {
                HideConfirm();
            }
        }

        private void Confirm()
        {
            if (_points.Count < 2)
            {
                return;
            }

            List<Vector3> spline = RoadSpline.BuildSpline(_points, settings.ResolutionPerMeter);
            if (!CollectRoadCells(spline))
            {
                Debug.Log("[RoadBuilderSystem] A estrada atravessa uma construção. Ajuste o traçado.");
                ShowConfirm();
                return;
            }

            BuildRoad(_points);
            if (gridManager != null)
            {
                gridManager.SetOccupancy(_roadCells, OccupancyType.Road, this);
            }

            OnFinished?.Invoke();
        }

        /// <summary>Preenche as células cobertas pela estrada. Falso se alguma for de construção.</summary>
        private bool CollectRoadCells(IReadOnlyList<Vector3> spline)
        {
            _roadCells.Clear();
            if (gridManager == null)
            {
                return true;
            }

            for (int i = 0; i < spline.Count; i++)
            {
                if (!gridManager.TryWorldToGrid(spline[i], out GridCoordinate cell))
                {
                    continue;
                }

                if (!gridManager.CanPlaceRoad(cell))
                {
                    return false;
                }

                if (!_roadCells.Contains(cell))
                {
                    _roadCells.Add(cell);
                }
            }

            return true;
        }

        private void BuildRoad(IReadOnlyList<Vector3> controlPoints)
        {
            GameObject road = new GameObject("DirtRoad");
            road.transform.SetParent(roadsParent, true);
            road.AddComponent<RoadMeshBuilder>().Build(controlPoints, settings);
        }

        private void RefreshPreview()
        {
            _previewPoints.Clear();
            _previewPoints.AddRange(_points);
            if (_cursorOnGround)
            {
                _previewPoints.Add(_cursor);
            }

            if (previewController != null)
            {
                previewController.UpdatePreview(_previewPoints, _points.Count, _cursorOnGround);
            }

            if (confirmUI != null && confirmUI.IsVisible && _points.Count >= 2)
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

        private Vector3 LastPoint() => _points.Count > 0 ? _points[_points.Count - 1] : Vector3.zero;
    }
}
