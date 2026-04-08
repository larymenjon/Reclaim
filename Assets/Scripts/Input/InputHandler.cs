using System;
using Reclaim.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Input
{
    /// <summary>
    /// Converts raw input into grid-based events.
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera inputCamera;
        [SerializeField] private GridManager gridManager;

        [Header("Raycast")]
        [SerializeField] private LayerMask raycastMask = ~0;
        [SerializeField] private float maxRayDistance = 1000f;
        [SerializeField] private bool preferTerrainHit = true;

        public event Action<GridCoordinate, Vector3> OnPointerMoved;
        public event Action<GridCoordinate, Vector3> OnPrimaryPressed;
        public event Action<GridCoordinate, Vector3> OnPrimaryHeld;
        public event Action<GridCoordinate, Vector3> OnPrimaryReleased;
        public event Action<GridCoordinate, Vector3> OnDeletePressed;
        public event Action<GridCoordinate, Vector3> OnDeleteHeld;
        public event Action OnSecondaryPressed;
        public event Action OnRotatePressed;
        public event Action OnUndoPressed;

        private void Awake()
        {
            if (inputCamera == null)
            {
                inputCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (gridManager == null || inputCamera == null)
            {
                return;
            }

            bool hasGridPosition = TryGetMouseGridPosition(out GridCoordinate coordinate, out Vector3 worldPoint);
            bool isPointerOverUI = IsPointerOverUI();

            if (hasGridPosition)
            {
                OnPointerMoved?.Invoke(coordinate, worldPoint);
            }

            if (!isPointerOverUI && hasGridPosition && GetPrimaryMouseDown())
            {
                if (IsDeleteModifierHeld())
                {
                    OnDeletePressed?.Invoke(coordinate, worldPoint);
                }
                else
                {
                    OnPrimaryPressed?.Invoke(coordinate, worldPoint);
                }
            }

            if (!isPointerOverUI && hasGridPosition && GetPrimaryMouseHeld())
            {
                if (IsDeleteModifierHeld())
                {
                    OnDeleteHeld?.Invoke(coordinate, worldPoint);
                }
                else
                {
                    OnPrimaryHeld?.Invoke(coordinate, worldPoint);
                }
            }

            if (!isPointerOverUI && hasGridPosition && GetPrimaryMouseUp())
            {
                OnPrimaryReleased?.Invoke(coordinate, worldPoint);
            }

            if (!isPointerOverUI && GetSecondaryMouseDown())
            {
                OnSecondaryPressed?.Invoke();
            }

            if (GetRotatePressed())
            {
                OnRotatePressed?.Invoke();
            }

            if (GetUndoPressed())
            {
                OnUndoPressed?.Invoke();
            }
        }

        public void SetGridManager(GridManager manager)
        {
            gridManager = manager;
        }

        private bool TryGetMouseGridPosition(out GridCoordinate coordinate, out Vector3 worldPoint)
        {
            coordinate = default;
            worldPoint = default;

            Vector2 mousePosition = GetMousePosition();
            Ray ray = inputCamera.ScreenPointToRay(mousePosition);
            if (!TryGetWorldPoint(ray, out worldPoint))
            {
                return false;
            }

            return gridManager.TryWorldToGrid(worldPoint, out coordinate);
        }

        private bool TryGetWorldPoint(Ray ray, out Vector3 worldPoint)
        {
            if (preferTerrainHit && TryGetTerrainHitPoint(ray, out worldPoint))
            {
                return true;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, raycastMask, QueryTriggerInteraction.Ignore))
            {
                worldPoint = hit.point;
                return true;
            }

            Plane plane = new Plane(Vector3.up, new Vector3(0f, gridManager.PlaneHeight, 0f));
            if (plane.Raycast(ray, out float enter))
            {
                worldPoint = ray.GetPoint(enter);
                return true;
            }

            worldPoint = default;
            return false;
        }

        private bool TryGetTerrainHitPoint(Ray ray, out Vector3 worldPoint)
        {
            worldPoint = default;

            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                return false;
            }

            TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
            if (terrainCollider == null || !terrainCollider.enabled)
            {
                return false;
            }

            if (!terrainCollider.Raycast(ray, out RaycastHit hit, maxRayDistance))
            {
                return false;
            }

            worldPoint = hit.point;
            return true;
        }

        private static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static bool GetPrimaryMouseDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(0);
#endif
        }

        private static bool GetPrimaryMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
            return UnityEngine.Input.GetMouseButton(0);
#endif
        }

        private static bool GetPrimaryMouseUp()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonUp(0);
#endif
        }

        private static bool GetSecondaryMouseDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(1);
#endif
        }

        private static bool GetRotatePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetKeyDown(KeyCode.R);
#endif
        }

        private static bool GetUndoPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetKeyDown(KeyCode.Z);
#endif
        }

        private static bool IsDeleteModifierHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
#else
            return UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl);
#endif
        }

        private static bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            return EventSystem.current.IsPointerOverGameObject();
#else
            return EventSystem.current.IsPointerOverGameObject();
#endif
        }
    }
}
