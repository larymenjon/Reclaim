using System.Collections.Generic;
using Reclaim.Core;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Road
{
    public class RoadBuilderSystem : MonoBehaviour
    {
        [Header("Configuracoes")]
        [SerializeField] private RoadSettings settings;

        [Header("Referencias de Cena")]
        [SerializeField] private RoadPreviewController previewController;
        [SerializeField] private ConfirmBuildUI confirmUI;
        [SerializeField] private Transform roadsParent;

        private GameManager _gameManager;
        private readonly List<Vector3> _points = new();
        private Vector3 _cursor;
        private bool _onTerrain;
        private bool _isActive;
        private Camera _cam;

        public bool IsBuilding => _isActive;

        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;
            _cam = Camera.main;

            if (settings == null) Debug.LogError("[RoadBuilder] RoadSettings nao atribuido!", this);
            if (confirmUI == null) Debug.LogWarning("[RoadBuilder] ConfirmBuildUI nao atribuido!", this);
            if (previewController == null) Debug.LogWarning("[RoadBuilder] RoadPreviewController nao atribuido!", this);

            previewController?.Initialize(settings);
            _gameManager.OnModeChanged += OnModeChanged;
            Disable();
        }

        private void Update()
        {
            if (!_isActive) return;
            UpdateCursor();
            HandleInput();
            RefreshPreview();
        }

        private void OnModeChanged(GameMode mode)
        {
            if (mode == GameMode.Road) Enable();
            else Disable();
        }

        public void EnterBuildMode()
        {
            if (_gameManager != null && _gameManager.CurrentMode != GameMode.Road)
            {
                _gameManager.SetMode(GameMode.Road);
                return;
            }

            Enable();
        }

        public void ExitBuildMode()
        {
            Disable();
        }

        private void Enable()
        {
            _isActive = true;
            _points.Clear();
            previewController?.Show();
            confirmUI?.Hide();
            Debug.Log("[RoadBuilder] Ativo - clique no terreno para colocar pontos.");
        }

        private void Disable()
        {
            _isActive = false;
            _points.Clear();
            previewController?.Hide();
            confirmUI?.Hide();
        }

        private void HandleInput()
        {
            if (GetRightClickDown())
            {
                Undo();
                return;
            }

            if (GetLeftClickDown())
            {
                if (!IsPointerOverUI()) TryPlace();
            }
        }

        private void TryPlace()
        {
            if (!_onTerrain)
            {
                Debug.Log("[RoadBuilder] Cursor fora do terreno - verifique Terrain Layer no RoadSettings.");
                return;
            }

            if (_points.Count > 0 && Vector3.Distance(_points[^1], _cursor) < settings.minPointDistance)
                return;

            _points.Add(_cursor);
            Debug.Log($"[RoadBuilder] Ponto {_points.Count} em {_cursor}");

            if (_points.Count >= 2)
                confirmUI?.Show(ConfirmButtonPos(), OnConfirm);
        }

        private void Undo()
        {
            if (_points.Count == 0)
            {
                _gameManager.EnterIdleMode();
                return;
            }

            _points.RemoveAt(_points.Count - 1);
            Debug.Log($"[RoadBuilder] Undo - {_points.Count} ponto(s) restante(s).");

            if (_points.Count < 2) confirmUI?.Hide();
            else confirmUI?.UpdatePosition(ConfirmButtonPos());
        }

        private void OnConfirm()
        {
            Debug.Log($"[RoadBuilder] Confirmado com {_points.Count} pontos.");
            if (_points.Count < 2) return;

            BuildRoad(new List<Vector3>(_points));
            _gameManager.EnterIdleMode();
        }

        private void BuildRoad(List<Vector3> pts)
        {
            if (roadsParent == null)
            {
                var go = GameObject.Find("Roads") ?? new GameObject("Roads");
                roadsParent = go.transform;
            }

            var road = new GameObject("DirtRoad");
            road.transform.SetParent(roadsParent, true);
            road.AddComponent<RoadMeshBuilder>().Build(pts, settings);
            Debug.Log($"[RoadBuilder] Estrada '{road.name}' criada em cena.");
        }

        private void RefreshPreview()
        {
            var pts = new List<Vector3>(_points);
            if (_onTerrain) pts.Add(_cursor);

            previewController?.UpdatePreview(pts, _points.Count, _onTerrain && pts.Count >= 1);

            if (confirmUI != null && confirmUI.IsVisible && _points.Count >= 2)
                confirmUI.UpdatePosition(ConfirmButtonPos());
        }

        private void UpdateCursor()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) return;
            }

            var ray = _cam.ScreenPointToRay(GetPointerPosition());
            _onTerrain = TryRaycastGround(ray, 2000f, settings.terrainLayer, out RaycastHit hit);
            if (_onTerrain) _cursor = hit.point;
        }

        private Vector3 ConfirmButtonPos() => _points.Count > 0 ? _points[^1] : Vector3.zero;

        private static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        private static bool GetLeftClickDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(0);
#endif
        }

        private static bool GetRightClickDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(1);
#endif
        }

        private static Vector2 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static bool TryRaycastGround(Ray ray, float maxDistance, LayerMask layerMask, out RaycastHit bestHit)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask);
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
