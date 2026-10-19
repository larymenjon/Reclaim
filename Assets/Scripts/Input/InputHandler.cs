using System;
using Reclaim.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reclaim.Input
{
    /// <summary>
    /// Converte o ponteiro do mouse em coordenadas do grid e dispara eventos de alto nível
    /// (mover, clicar, cancelar, girar, desfazer) para os sistemas de construção.
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

        [Header("Keys")]
        [SerializeField] private Key rotateKey = Key.R;
        [SerializeField] private Key undoKey = Key.Z;

        private bool _hasLastCell;
        private GridCoordinate _lastCell;

        public event Action<GridCoordinate, Vector3> OnPointerMoved;
        public event Action<GridCoordinate, Vector3> OnPrimaryPressed;
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

            bool hasCell = TryGetPointerCell(out GridCoordinate cell, out Vector3 worldPoint);
            bool pointerOverUI = PointerInput.IsOverUI;

            if (hasCell && (!_hasLastCell || !cell.Equals(_lastCell)))
            {
                OnPointerMoved?.Invoke(cell, worldPoint);
            }

            _hasLastCell = hasCell;
            _lastCell = cell;

            if (!pointerOverUI && hasCell && PointerInput.PrimaryPressedThisFrame)
            {
                OnPrimaryPressed?.Invoke(cell, worldPoint);
            }

            if (!pointerOverUI && PointerInput.SecondaryPressedThisFrame)
            {
                OnSecondaryPressed?.Invoke();
            }

            if (PointerInput.KeyPressedThisFrame(rotateKey))
            {
                OnRotatePressed?.Invoke();
            }

            if (PointerInput.KeyPressedThisFrame(undoKey))
            {
                OnUndoPressed?.Invoke();
            }
        }

        public void SetGridManager(GridManager manager)
        {
            gridManager = manager;
            _hasLastCell = false;
        }

        private bool TryGetPointerCell(out GridCoordinate cell, out Vector3 worldPoint)
        {
            cell = default;
            Ray ray = inputCamera.ScreenPointToRay(PointerInput.Position);

            if (!TryGetWorldPoint(ray, out worldPoint))
            {
                return false;
            }

            return gridManager.TryWorldToGrid(worldPoint, out cell);
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
            if (terrain == null || !terrain.TryGetComponent(out TerrainCollider terrainCollider) || !terrainCollider.enabled)
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
    }
}
