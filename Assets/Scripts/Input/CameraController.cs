using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Input
{
    /// <summary>
    /// RTS/city-builder camera controller (Manor Lords / Frostpunk style).
    /// Hierarchy recommendation:
    /// CameraRig (this script, yaw + planar movement)
    ///   └─ CameraPivot (pitch)
    ///       └─ Main Camera (local z = -zoom distance)
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Rig References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera targetCamera;

        [Header("Planar Movement")]
        [SerializeField] private float moveSensitivity = 28f;
        [SerializeField] private float edgeScrollSensitivity = 28f;
        [SerializeField] private int edgeSizePixels = 18;
        [SerializeField] private float moveAcceleration = 90f;
        [SerializeField] private float moveDeceleration = 110f;
        [SerializeField] private bool speedScalesWithZoom = true;
        [SerializeField] private float minZoomMoveMultiplier = 0.75f;
        [SerializeField] private float maxZoomMoveMultiplier = 1.35f;

        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 0.08f;
        [SerializeField] private float zoomAcceleration = 26f;
        [SerializeField] private float zoomDeceleration = 40f;
        [SerializeField] private float zoomSmoothTime = 0.08f;
        [SerializeField] private float minZoomDistance = 10f;
        [SerializeField] private float maxZoomDistance = 55f;
        [SerializeField] private bool zoomTowardMouse = true;
        [SerializeField] private float zoomToMouseInfluence = 0.12f;
        [SerializeField] private LayerMask groundRaycastMask = ~0;
        [SerializeField] private float fallbackGroundHeight = 0f;

        [Header("Rotation")]
        [SerializeField] private float yawSensitivity = 220f;
        [SerializeField] private float pitchSensitivity = 160f;
        [SerializeField] private float keyboardYawSensitivity = 120f;
        [SerializeField] private float minPitch = 30f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float rotationSmoothTime = 0.06f;
        [SerializeField] private bool enableMiddleMouseOrbit = true;
        [SerializeField] private bool enableKeyboardRotation = true;

        [Header("Optional Panning")]
        [SerializeField] private bool enableRightMousePan = true;
        [SerializeField] private float rightMousePanSensitivity = 0.28f;

        [Header("Map Bounds")]
        [SerializeField] private bool useMapBounds = true;
        [SerializeField] private Terrain mapTerrain;
        [SerializeField] private bool autoBoundsFromTerrain = true;
        [SerializeField] private bool centerOnTerrainAtStart = true;
        [SerializeField] private Renderer mapPlaneRenderer;
        [SerializeField] private bool autoBoundsFromPlaneRenderer = true;
        [SerializeField] private bool disablePlaneWhenTerrainExists = true;
        [SerializeField] private float boundsPadding = 0.5f;
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
            if (cameraPivot == null && transform.childCount > 0)
            {
                cameraPivot = transform.GetChild(0);
            }

            if (targetCamera == null)
            {
                targetCamera = GetComponentInChildren<Camera>();
            }

            if (cameraPivot == null || targetCamera == null)
            {
                enabled = false;
                return;
            }

            // Orbit mode is used when the scene has no rig/pivot hierarchy.
            // In this mode, this transform is the camera itself orbiting around a ground focus point.
            _useOrbitCameraMode = cameraPivot == null || cameraPivot == transform || targetCamera.transform == transform;

            _targetRigPosition = transform.position;
            _currentRigPosition = _targetRigPosition;
            _targetYaw = transform.eulerAngles.y;
            _currentYaw = _targetYaw;

            _targetPitch = _useOrbitCameraMode
                ? NormalizeAngle(transform.eulerAngles.x)
                : NormalizeAngle(cameraPivot.localEulerAngles.x);
            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
            _currentPitch = _targetPitch;

            float initialZoomDistance;
            if (_useOrbitCameraMode)
            {
                initialZoomDistance = Mathf.Max(minZoomDistance, Mathf.Abs(transform.position.y));
            }
            else
            {
                initialZoomDistance = Mathf.Abs(targetCamera.transform.localPosition.z);
            }

            _targetZoomDistance = Mathf.Clamp(initialZoomDistance, minZoomDistance, maxZoomDistance);
            _currentZoomDistance = _targetZoomDistance;

            ResolveTerrainReference();
            DisableFallbackPlaneWhenNeeded();
            RefreshBoundsFromSurface();
            CenterCameraOnTerrainIfNeeded();
        }

        private void OnValidate()
        {
            minZoomDistance = Mathf.Max(1f, minZoomDistance);
            maxZoomDistance = Mathf.Max(minZoomDistance + 0.01f, maxZoomDistance);
            minPitch = Mathf.Clamp(minPitch, 5f, 89f);
            maxPitch = Mathf.Clamp(maxPitch, minPitch + 0.1f, 89f);

            ResolveTerrainReference();
            RefreshBoundsFromSurface();
        }

        private void Update()
        {
            if (cameraPivot == null || targetCamera == null)
            {
                return;
            }

            float dt = Time.deltaTime;

            ProcessMovement(dt);
            ProcessRotation(dt);
            ProcessZoom(dt);
            ClampTargets();
            ApplySmoothedState(dt);
        }

        private void ProcessMovement(float deltaTime)
        {
            Vector2 keyboardInput = GetMoveInput();
            Vector2 edgeInput = GetEdgeScrollInput();
            Vector2 panInput = enableRightMousePan ? GetRightMousePanInput() : Vector2.zero;

            float edgeWeight = moveSensitivity <= 0.0001f ? 0f : edgeScrollSensitivity / moveSensitivity;
            Vector2 desiredPlanarInput = keyboardInput + edgeInput * edgeWeight + panInput;
            desiredPlanarInput = Vector2.ClampMagnitude(desiredPlanarInput, 1f);

            Vector3 forwardSource = _useOrbitCameraMode
                ? Quaternion.Euler(0f, _targetYaw, 0f) * Vector3.forward
                : transform.forward;

            Vector3 flatForward = forwardSource;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 rightSource = _useOrbitCameraMode
                ? Quaternion.Euler(0f, _targetYaw, 0f) * Vector3.right
                : transform.right;

            Vector3 flatRight = rightSource;
            flatRight.y = 0f;
            flatRight.Normalize();

            float zoomMoveMultiplier = 1f;
            if (speedScalesWithZoom)
            {
                float zoomT = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
                zoomMoveMultiplier = Mathf.Lerp(minZoomMoveMultiplier, maxZoomMoveMultiplier, zoomT);
            }

            Vector3 desiredVelocity = (flatRight * desiredPlanarInput.x + flatForward * desiredPlanarInput.y)
                                      * moveSensitivity
                                      * zoomMoveMultiplier;

            float acceleration = desiredVelocity.sqrMagnitude > 0.0001f ? moveAcceleration : moveDeceleration;
            _currentPlanarVelocity = Vector3.MoveTowards(_currentPlanarVelocity, desiredVelocity, acceleration * deltaTime);

            _targetRigPosition += _currentPlanarVelocity * deltaTime;
        }

        private void ProcessRotation(float deltaTime)
        {
            float yawDelta = 0f;
            float pitchDelta = 0f;

            if (enableMiddleMouseOrbit && IsMiddleMouseHeld())
            {
                Vector2 mouseDelta = GetMouseDelta();
                yawDelta += mouseDelta.x * yawSensitivity * deltaTime;
                pitchDelta -= mouseDelta.y * pitchSensitivity * deltaTime;
            }

            if (enableKeyboardRotation)
            {
                yawDelta += GetKeyboardYawInput() * keyboardYawSensitivity * deltaTime;
            }

            _targetYaw += yawDelta;
            _targetPitch = Mathf.Clamp(_targetPitch + pitchDelta, minPitch, maxPitch);
        }

        private void ProcessZoom(float deltaTime)
        {
            float scrollDelta = GetScrollDeltaNormalized();
            if (Mathf.Approximately(scrollDelta, 0f))
            {
                _zoomSpeed = Mathf.MoveTowards(_zoomSpeed, 0f, zoomDeceleration * deltaTime);
            }
            else
            {
                _zoomSpeed += scrollDelta * zoomAcceleration;
            }

            _targetZoomDistance -= _zoomSpeed * zoomSensitivity;
            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, minZoomDistance, maxZoomDistance);

            if (zoomTowardMouse && !Mathf.Approximately(scrollDelta, 0f) && TryGetMouseGroundPoint(out Vector3 mouseGroundPoint))
            {
                Vector3 towardMouse = mouseGroundPoint - _targetRigPosition;
                towardMouse.y = 0f;

                float zoomT = 1f - Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
                _targetRigPosition += towardMouse * (zoomToMouseInfluence * zoomT * Mathf.Abs(scrollDelta));
            }
        }

        private void ClampTargets()
        {
            _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, minZoomDistance, maxZoomDistance);
            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);

            if (useMapBounds)
            {
                _targetRigPosition.x = Mathf.Clamp(_targetRigPosition.x, minBounds.x, maxBounds.x);
                _targetRigPosition.z = Mathf.Clamp(_targetRigPosition.z, minBounds.y, maxBounds.y);
            }
        }

        private void ApplySmoothedState(float deltaTime)
        {
            Vector3 smoothedRigPosition = Vector3.Lerp(
                _currentRigPosition,
                _targetRigPosition,
                1f - Mathf.Exp(-12f * deltaTime)
            );
            _currentRigPosition = smoothedRigPosition;

            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, rotationSmoothTime);
            _currentPitch = Mathf.SmoothDampAngle(_currentPitch, _targetPitch, ref _pitchVelocity, rotationSmoothTime);
            _currentZoomDistance = Mathf.SmoothDamp(_currentZoomDistance, _targetZoomDistance, ref _zoomDistanceVelocity, zoomSmoothTime);

            if (_useOrbitCameraMode)
            {
                Quaternion orbitRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
                Vector3 cameraOffset = orbitRotation * Vector3.back * _currentZoomDistance;
                Vector3 cameraPosition = smoothedRigPosition + cameraOffset;
                transform.SetPositionAndRotation(cameraPosition, orbitRotation);
            }
            else
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    _currentRigPosition,
                    1f - Mathf.Exp(-12f * deltaTime)
                );
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

                Vector3 localPos = targetCamera.transform.localPosition;
                localPos.x = 0f;
                localPos.y = 0f;
                localPos.z = -_currentZoomDistance;
                targetCamera.transform.localPosition = localPos;
            }
        }

        private Vector2 GetMoveInput()
        {
            Vector2 result = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) result.y += 1f;
                if (Keyboard.current.sKey.isPressed) result.y -= 1f;
                if (Keyboard.current.dKey.isPressed) result.x += 1f;
                if (Keyboard.current.aKey.isPressed) result.x -= 1f;
            }
#else
            if (UnityEngine.Input.GetKey(KeyCode.W)) result.y += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S)) result.y -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D)) result.x += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A)) result.x -= 1f;
#endif

            return result;
        }

        private Vector2 GetEdgeScrollInput()
        {
            Vector2 mousePosition = GetMousePosition();
            Vector2 result = Vector2.zero;

            if (mousePosition.x >= Screen.width - edgeSizePixels) result.x += 1f;
            if (mousePosition.x <= edgeSizePixels) result.x -= 1f;
            if (mousePosition.y >= Screen.height - edgeSizePixels) result.y += 1f;
            if (mousePosition.y <= edgeSizePixels) result.y -= 1f;

            return result;
        }

        private Vector2 GetRightMousePanInput()
        {
            if (!IsRightMouseHeld())
            {
                return Vector2.zero;
            }

            Vector2 delta = GetMouseDelta();
            Vector2 pan = new Vector2(-delta.x, -delta.y) * rightMousePanSensitivity;
            return Vector2.ClampMagnitude(pan, 1f);
        }

        private static float GetKeyboardYawInput()
        {
            float value = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed) value -= 1f;
                if (Keyboard.current.eKey.isPressed) value += 1f;
            }
#else
            if (UnityEngine.Input.GetKey(KeyCode.Q)) value -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.E)) value += 1f;
#endif

            return value;
        }

        private static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static Vector2 GetMouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
            return new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));
#endif
        }

        private static float GetScrollDeltaNormalized()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null) return 0f;
            return Mouse.current.scroll.ReadValue().y * 0.01f;
#else
            return UnityEngine.Input.GetAxis("Mouse ScrollWheel");
#endif
        }

        private static bool IsMiddleMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.middleButton.isPressed;
#else
            return UnityEngine.Input.GetMouseButton(2);
#endif
        }

        private static bool IsRightMouseHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
            return UnityEngine.Input.GetMouseButton(1);
#endif
        }

        private bool TryGetMouseGroundPoint(out Vector3 point)
        {
            point = default;
            if (targetCamera == null)
            {
                return false;
            }

            Ray ray = targetCamera.ScreenPointToRay(GetMousePosition());
            if (TryGetTerrainRaycastHit(ray, out RaycastHit terrainHit))
            {
                point = terrainHit.point;
                return true;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, groundRaycastMask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
                return true;
            }

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, fallbackGroundHeight, 0f));
            if (groundPlane.Raycast(ray, out float enter))
            {
                point = ray.GetPoint(enter);
                return true;
            }

            return false;
        }

        private bool TryGetTerrainRaycastHit(Ray ray, out RaycastHit hit)
        {
            hit = default;
            if (mapTerrain == null)
            {
                return false;
            }

            TerrainCollider terrainCollider = mapTerrain.GetComponent<TerrainCollider>();
            if (terrainCollider == null || !terrainCollider.enabled)
            {
                return false;
            }

            return terrainCollider.Raycast(ray, out hit, 5000f);
        }

        private void ResolveTerrainReference()
        {
            if (mapTerrain == null)
            {
                mapTerrain = FindFirstObjectByType<Terrain>();
            }
        }

        private void DisableFallbackPlaneWhenNeeded()
        {
            if (!disablePlaneWhenTerrainExists || mapTerrain == null || mapPlaneRenderer == null)
            {
                return;
            }

            mapPlaneRenderer.enabled = false;

            Collider planeCollider = mapPlaneRenderer.GetComponent<Collider>();
            if (planeCollider != null)
            {
                planeCollider.enabled = false;
            }
        }

        private void RefreshBoundsFromSurface()
        {
            if (autoBoundsFromTerrain && mapTerrain != null)
            {
                Vector3 terrainPosition = mapTerrain.transform.position;
                Vector3 terrainSize = mapTerrain.terrainData != null ? mapTerrain.terrainData.size : Vector3.zero;

                minBounds = new Vector2(terrainPosition.x + boundsPadding, terrainPosition.z + boundsPadding);
                maxBounds = new Vector2(
                    terrainPosition.x + terrainSize.x - boundsPadding,
                    terrainPosition.z + terrainSize.z - boundsPadding
                );
                return;
            }

            if (!autoBoundsFromPlaneRenderer || mapPlaneRenderer == null)
            {
                return;
            }

            Bounds bounds = mapPlaneRenderer.bounds;
            minBounds = new Vector2(bounds.min.x + boundsPadding, bounds.min.z + boundsPadding);
            maxBounds = new Vector2(bounds.max.x - boundsPadding, bounds.max.z - boundsPadding);
        }

        private void CenterCameraOnTerrainIfNeeded()
        {
            if (!centerOnTerrainAtStart || mapTerrain == null || mapTerrain.terrainData == null)
            {
                return;
            }

            Vector3 terrainPosition = mapTerrain.transform.position;
            Vector3 terrainSize = mapTerrain.terrainData.size;
            Vector3 center = new Vector3(
                terrainPosition.x + terrainSize.x * 0.5f,
                Mathf.Max(terrainPosition.y + minZoomDistance, terrainPosition.y + 20f),
                terrainPosition.z + terrainSize.z * 0.5f
            );

            _targetRigPosition = center;
            _currentRigPosition = center;
            _targetYaw = 0f;
            _currentYaw = 0f;
            _targetPitch = Mathf.Clamp(70f, minPitch, maxPitch);
            _currentPitch = _targetPitch;
        }

        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }
    }
}
