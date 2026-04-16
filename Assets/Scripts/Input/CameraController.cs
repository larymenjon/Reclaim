using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Input
{
    /// <summary>
    /// RTS/city-builder camera controller (Manor Lords / Frostpunk style).
    /// Hierarchy:
    /// CameraRig (this script) -> Fica no chão (Y=0)
    ///   └─ CameraPivot -> Controla a inclinação (Pitch)
    ///       └─ Main Camera -> Controla o Zoom (Z local negativo)
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Rig References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera targetCamera;

        [Header("Planar Movement")]
        [SerializeField] private float moveSensitivity = 50f;
        [SerializeField] private float edgeScrollSensitivity = 50f;
        [SerializeField] private int edgeSizePixels = 18;
        [SerializeField] private float moveAcceleration = 150f;
        [SerializeField] private float moveDeceleration = 180f;
        [SerializeField] private bool speedScalesWithZoom = true;
        [SerializeField] private float minZoomMoveMultiplier = 0.5f; // Mais lento quando perto
        [SerializeField] private float maxZoomMoveMultiplier = 1.5f; // Mais rápido quando longe

        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 1.5f; // Aumentado para melhor resposta
        [SerializeField] private float zoomAcceleration = 30f;
        [SerializeField] private float zoomDeceleration = 40f;
        [SerializeField] private float zoomSmoothTime = 0.12f;
        [SerializeField] private float minZoomDistance = 8f;   // Perto para ver detalhes
        [SerializeField] private float maxZoomDistance = 65f;  // Longe para estratégia
        [SerializeField] private bool zoomTowardMouse = true;
        [SerializeField] private float zoomToMouseInfluence = 0.15f;
        [SerializeField] private LayerMask groundRaycastMask = ~0;
        [SerializeField] private float fallbackGroundHeight = 0f;

        [Header("Rotation & Auto-Pitch")]
        [SerializeField] private float yawSensitivity = 220f;
        [SerializeField] private float keyboardYawSensitivity = 120f;
        [SerializeField] private float minPitch = 35f;  // Inclinação de horizonte (Zoom perto)
        [SerializeField] private float maxPitch = 82f;  // Quase top-down (Zoom longe)
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private bool enableMiddleMouseOrbit = true;
        [SerializeField] private bool enableKeyboardRotation = true;

        [Header("Map Bounds")]
        [SerializeField] private bool useMapBounds = true;
        [SerializeField] private Terrain mapTerrain;
        [SerializeField] private bool autoBoundsFromTerrain = true;
        [SerializeField] private float boundsPadding = 2f;
        [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

        private Vector3 _targetRigPosition;
        private Vector3 _currentRigPosition;
        private Vector3 _currentPlanarVelocity;

        private float _targetYaw;
        private float _targetPitch;
        private float _currentYaw;
        private float _currentPitch;
        private float _yawVelocity;
        private float _pitchVelocity;

        private float _targetZoomDistance;
        private float _currentZoomDistance;
        private float _zoomDistanceVelocity;
        private float _zoomSpeed;
        private bool _useOrbitCameraMode;

        private void Awake()
        {
            if (cameraPivot == null && transform.childCount > 0) cameraPivot = transform.GetChild(0);
            if (targetCamera == null) targetCamera = GetComponentInChildren<Camera>();

            if (targetCamera == null) { enabled = false; return; }

            _useOrbitCameraMode = cameraPivot == null || targetCamera.transform == transform;

            _targetRigPosition = transform.position;
            _currentRigPosition = _targetRigPosition;
            _targetYaw = transform.eulerAngles.y;
            _currentYaw = _targetYaw;

            // Inicializa zoom
            float initialZoom = _useOrbitCameraMode ? Mathf.Abs(transform.position.y) : Mathf.Abs(targetCamera.transform.localPosition.z);
            _targetZoomDistance = Mathf.Clamp(initialZoom, minZoomDistance, maxZoomDistance);
            _currentZoomDistance = _targetZoomDistance;

            ResolveTerrainReference();
            RefreshBoundsFromSurface();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            ProcessMovement(dt);
            ProcessRotation(dt); // Agora inclui o cálculo de Auto-Pitch
            ProcessZoom(dt);
            ClampTargets();
            ApplySmoothedState(dt);
        }

        private void ProcessMovement(float deltaTime)
        {
            Vector2 keyboardInput = GetMoveInput();
            Vector2 edgeInput = GetEdgeScrollInput();
            
            Vector2 desiredPlanarInput = Vector2.ClampMagnitude(keyboardInput + edgeInput, 1f);

            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 flatRight = transform.right;
            flatRight.y = 0f;
            flatRight.Normalize();

            float zoomT = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
            float speedMultiplier = speedScalesWithZoom ? Mathf.Lerp(minZoomMoveMultiplier, maxZoomMoveMultiplier, zoomT) : 1f;

            Vector3 desiredVelocity = (flatRight * desiredPlanarInput.x + flatForward * desiredPlanarInput.y) * moveSensitivity * speedMultiplier;

            float acceleration = desiredVelocity.sqrMagnitude > 0.0001f ? moveAcceleration : moveDeceleration;
            _currentPlanarVelocity = Vector3.MoveTowards(_currentPlanarVelocity, desiredVelocity, acceleration * deltaTime);

            _targetRigPosition += _currentPlanarVelocity * deltaTime;
        }

        private void ProcessRotation(float deltaTime)
        {
            // Rotação Y (Yaw)
            float yawDelta = 0f;
            if (enableMiddleMouseOrbit && IsMiddleMouseHeld())
                yawDelta += GetMouseDelta().x * yawSensitivity * deltaTime;
            if (enableKeyboardRotation)
                yawDelta += GetKeyboardYawInput() * keyboardYawSensitivity * deltaTime;

            _targetYaw += yawDelta;

            // LÓGICA DE AUTO-PITCH: A inclinação depende do Zoom
            float zoomT = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
            _targetPitch = Mathf.Lerp(minPitch, maxPitch, zoomT);
        }

        private void ProcessZoom(float deltaTime)
        {
            float scrollDelta = GetScrollDeltaNormalized();
            if (Mathf.Approximately(scrollDelta, 0f))
                _zoomSpeed = Mathf.MoveTowards(_zoomSpeed, 0f, zoomDeceleration * deltaTime);
            else
                _zoomSpeed += scrollDelta * zoomAcceleration;

            _targetZoomDistance -= _zoomSpeed * zoomSensitivity;
            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, minZoomDistance, maxZoomDistance);

            // Zoom na direção do mouse (Efeito Manor Lords)
            if (zoomTowardMouse && !Mathf.Approximately(scrollDelta, 0f) && TryGetMouseGroundPoint(out Vector3 mouseGroundPoint))
            {
                Vector3 towardMouse = mouseGroundPoint - _targetRigPosition;
                towardMouse.y = 0f;
                float zoomT = 1f - Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
                _targetRigPosition += towardMouse * (zoomToMouseInfluence * zoomT * Mathf.Abs(scrollDelta));
            }
        }

        private void ApplySmoothedState(float deltaTime)
        {
            // Suavização Exponencial para posição (mais fluida que Lerp simples)
            _currentRigPosition = Vector3.Lerp(_currentRigPosition, _targetRigPosition, 1f - Mathf.Exp(-12f * deltaTime));

            // Suavização das rotações
            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, rotationSmoothTime);
            _currentPitch = Mathf.SmoothDampAngle(_currentPitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);
            _currentZoomDistance = Mathf.SmoothDamp(_currentZoomDistance, _targetZoomDistance, ref _zoomDistanceVelocity, zoomSmoothTime);

            if (_useOrbitCameraMode)
            {
                Quaternion orbitRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
                transform.position = _currentRigPosition + (orbitRotation * Vector3.back * _currentZoomDistance);
                transform.rotation = orbitRotation;
            }
            else
            {
                // Aplica no Rig (Movimento e Yaw)
                transform.position = _currentRigPosition;
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                
                // Aplica no Pivot (Pitch dinâmico)
                cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

                // Aplica na Câmera (Zoom)
                Vector3 localCamPos = targetCamera.transform.localPosition;
                localCamPos.z = -_currentZoomDistance;
                targetCamera.transform.localPosition = localCamPos;
            }
        }

        private void ClampTargets()
        {
            if (useMapBounds)
            {
                _targetRigPosition.x = Mathf.Clamp(_targetRigPosition.x, minBounds.x, maxBounds.x);
                _targetRigPosition.z = Mathf.Clamp(_targetRigPosition.z, minBounds.y, maxBounds.y);
            }
        }

        // --- HELPERS DE INPUT ---
        private Vector2 GetMoveInput()
        {
            Vector2 res = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null) {
                if (k.wKey.isPressed) res.y += 1; if (k.sKey.isPressed) res.y -= 1;
                if (k.dKey.isPressed) res.x += 1; if (k.aKey.isPressed) res.x -= 1;
            }
#else
            res.x = UnityEngine.Input.GetAxisRaw("Horizontal");
            res.y = UnityEngine.Input.GetAxisRaw("Vertical");
#endif
            return res;
        }

        private float GetScrollDeltaNormalized()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.scroll.ReadValue().y * 0.01f : 0f;
#else
            return UnityEngine.Input.GetAxis("Mouse ScrollWheel");
#endif
        }

        private Vector2 GetEdgeScrollInput()
        {
            Vector2 mouse = GetMousePosition();
            Vector2 res = Vector2.zero;
            if (mouse.x >= Screen.width - edgeSizePixels) res.x = 1;
            else if (mouse.x <= edgeSizePixels) res.x = -1;
            if (mouse.y >= Screen.height - edgeSizePixels) res.y = 1;
            else if (mouse.y <= edgeSizePixels) res.y = -1;
            return res * (edgeScrollSensitivity / moveSensitivity);
        }

        private Vector2 GetMousePosition() => 
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            UnityEngine.Input.mousePosition;
#endif

        private Vector2 GetMouseDelta() =>
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
            new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
#endif

        private bool IsMiddleMouseHeld() =>
#if ENABLE_INPUT_SYSTEM
            Mouse.current != null && Mouse.current.middleButton.isPressed;
#else
            UnityEngine.Input.GetMouseButton(2);
#endif

        private float GetKeyboardYawInput() {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            return k != null ? (k.qKey.isPressed ? -1 : (k.eKey.isPressed ? 1 : 0)) : 0;
#else
            return (UnityEngine.Input.GetKey(KeyCode.Q) ? -1 : (UnityEngine.Input.GetKey(KeyCode.E) ? 1 : 0));
#endif
        }

        private bool TryGetMouseGroundPoint(out Vector3 point)
        {
            point = Vector3.zero;
            Ray ray = targetCamera.ScreenPointToRay(GetMousePosition());
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundRaycastMask)) {
                point = hit.point;
                return true;
            }
            return false;
        }

        private void ResolveTerrainReference() { if (mapTerrain == null) mapTerrain = FindFirstObjectByType<Terrain>(); }

        private void RefreshBoundsFromSurface()
        {
            if (autoBoundsFromTerrain && mapTerrain != null) {
                Vector3 pos = mapTerrain.transform.position;
                Vector3 size = mapTerrain.terrainData.size;
                minBounds = new Vector2(pos.x + boundsPadding, pos.z + boundsPadding);
                maxBounds = new Vector2(pos.x + size.x - boundsPadding, pos.z + size.z - boundsPadding);
            }
        }
    }
}