using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Road
{
    /// <summary>
    /// Handles user input for road drawing and confirms road construction.
    /// </summary>
    public class RoadBuilderManager : MonoBehaviour
    {
        private enum BuildState
        {
            Idle,
            Drawing
        }

        private const float MaxRayDistance = 2000f;

        public static RoadBuilderManager Instance { get; private set; }

        [Header("Required References")]
        [SerializeField] private RoadNetwork _roadNetwork;
        [SerializeField] private RoadPreviewController _previewController;
        [SerializeField] private ConfirmBuildUI _confirmBuildUI;
        [SerializeField] private RoadPrefabLibrary _prefabLibrary;

        [Header("Input")]
        [SerializeField] private LayerMask _terrainLayer;
        [SerializeField] private bool _enableDebugLogs;

        private readonly List<Vector3> _confirmedPoints = new List<Vector3>();

        private BuildState _state = BuildState.Idle;
        private Vector3 _currentMouseWorldPosition;
        private bool _isMouseOnTerrain;
        private RoadNode _snapTarget;
        private Camera _mainCamera;
        private bool _loggedMissingReferences;
        private bool _loggedMissingTerrainLayer;

        public bool IsBuilding => _state == BuildState.Drawing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_state == BuildState.Idle)
            {
                return;
            }

            UpdateMouseWorldPosition();
            HandleInput();
            UpdatePreview();
        }

        public void StartBuildMode()
        {
            if (_state == BuildState.Drawing)
            {
                return;
            }

            EnsureRuntimeReferences();
            if (!HasRequiredReferences())
            {
                if (_enableDebugLogs && !_loggedMissingReferences)
                {
                    Debug.LogWarning("[RoadBuilder] Missing references. Configure RoadNetwork, RoadPreviewController and RoadPrefabLibrary.");
                    _loggedMissingReferences = true;
                }

                return;
            }

            _state = BuildState.Drawing;
            _confirmedPoints.Clear();
            _snapTarget = null;

            _previewController?.Show();
            _confirmBuildUI?.Hide();
        }

        public void ExitBuildMode()
        {
            _state = BuildState.Idle;
            _confirmedPoints.Clear();
            _snapTarget = null;

            _previewController?.Hide();
            _confirmBuildUI?.Hide();
        }

        private void HandleInput()
        {
            if (IsPointerOverUI())
            {
                return;
            }

            if (GetRightMouseDown())
            {
                UndoLastPoint();
                return;
            }

            if (GetLeftMouseDown())
            {
                TryPlacePoint();
            }
        }

        private void TryPlacePoint()
        {
            if (!_isMouseOnTerrain || _prefabLibrary == null)
            {
                return;
            }

            Vector3 pointToPlace = _snapTarget != null ? _snapTarget.Position : _currentMouseWorldPosition;
            if (!CanPlacePoint(pointToPlace))
            {
                return;
            }

            _confirmedPoints.Add(pointToPlace);

            if (_confirmedPoints.Count >= 2)
            {
                ShowConfirmButton();
            }
        }

        private bool CanPlacePoint(Vector3 pointToPlace)
        {
            if (_confirmedPoints.Count == 0)
            {
                return true;
            }

            float distance = Vector3.Distance(_confirmedPoints[_confirmedPoints.Count - 1], pointToPlace);
            return distance >= _prefabLibrary.minPointDistance;
        }

        private void UndoLastPoint()
        {
            if (_confirmedPoints.Count == 0)
            {
                ExitBuildMode();
                return;
            }

            _confirmedPoints.RemoveAt(_confirmedPoints.Count - 1);

            if (_confirmedPoints.Count < 2)
            {
                _confirmBuildUI?.Hide();
            }
            else
            {
                ShowConfirmButton();
            }
        }

        private void ShowConfirmButton()
        {
            if (_confirmBuildUI == null)
            {
                return;
            }

            _confirmBuildUI.Show(GetConfirmButtonWorldPosition(), OnConfirmBuild);
        }

        private void OnConfirmBuild()
        {
            if (_confirmedPoints.Count < 2 || _roadNetwork == null)
            {
                return;
            }

            _roadNetwork.BuildRoad(new List<Vector3>(_confirmedPoints));
            ExitBuildMode();
        }

        private void UpdatePreview()
        {
            if (_previewController == null)
            {
                return;
            }

            List<Vector3> previewPoints = new List<Vector3>(_confirmedPoints);
            if (_isMouseOnTerrain)
            {
                Vector3 cursorPoint = _snapTarget != null ? _snapTarget.Position : _currentMouseWorldPosition;
                previewPoints.Add(cursorPoint);
            }

            bool isValid = _isMouseOnTerrain && previewPoints.Count >= 1;
            _previewController.UpdatePreview(previewPoints, _confirmedPoints.Count, isValid);

            if (_confirmBuildUI != null && _confirmBuildUI.IsVisible && _confirmedPoints.Count >= 2)
            {
                _confirmBuildUI.UpdatePosition(GetConfirmButtonWorldPosition());
            }
        }

        private void UpdateMouseWorldPosition()
        {
            EnsureRuntimeReferences();
            if (_mainCamera == null || _roadNetwork == null || _prefabLibrary == null)
            {
                _isMouseOnTerrain = false;
                _snapTarget = null;
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(GetMousePosition());
            int layerMask = _terrainLayer.value;
            if (layerMask == 0)
            {
                layerMask = ~0;
                if (_enableDebugLogs && !_loggedMissingTerrainLayer)
                {
                    Debug.LogWarning("[RoadBuilder] Terrain Layer is empty. Using fallback raycast on all layers.");
                    _loggedMissingTerrainLayer = true;
                }
            }

            if (Physics.Raycast(ray, out RaycastHit hit, MaxRayDistance, layerMask))
            {
                _currentMouseWorldPosition = hit.point;
                _isMouseOnTerrain = true;
                _snapTarget = _roadNetwork.FindNearestNode(hit.point, _prefabLibrary.snapRadius);
                return;
            }

            _isMouseOnTerrain = false;
            _snapTarget = null;
        }

        private void EnsureRuntimeReferences()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    _mainCamera = FindFirstObjectByType<Camera>();
                }
            }

            _roadNetwork ??= FindFirstObjectByType<RoadNetwork>();
            _previewController ??= FindFirstObjectByType<RoadPreviewController>();
        }

        private bool HasRequiredReferences()
        {
            return _mainCamera != null
                && _roadNetwork != null
                && _previewController != null
                && _prefabLibrary != null;
        }

        private Vector3 GetConfirmButtonWorldPosition()
        {
            if (_confirmedPoints.Count < 2)
            {
                return Vector3.zero;
            }

            Vector3 from = _confirmedPoints[_confirmedPoints.Count - 2];
            Vector3 to = _confirmedPoints[_confirmedPoints.Count - 1];
            return (from + to) * 0.5f;
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static bool GetLeftMouseDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(0);
#endif
        }

        private static bool GetRightMouseDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(1);
#endif
        }
    }
}
