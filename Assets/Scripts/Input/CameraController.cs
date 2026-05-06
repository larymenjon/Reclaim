using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Reclaim.Input
{
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
        [SerializeField] private float keyboardAccelerationMultiplier = 1.5f;
        [SerializeField] private float directionChangeBoost = 1.2f;
        [SerializeField] private bool speedScalesWithZoom = true;
        [SerializeField] private float minZoomMoveMultiplier = 0.5f;
        [SerializeField] private float maxZoomMoveMultiplier = 1.5f;
        [SerializeField] private float edgeSpeedMultiplier = 0.35f;
        [SerializeField] private float edgeRampExponent = 2f;

        [Header("Zoom")]
        [SerializeField] private float zoomSensitivity = 0.75f;
        [SerializeField] private float zoomAcceleration = 12f;
        [SerializeField] private float zoomDeceleration = 16f;
        [SerializeField] private float zoomSmoothTime = 0.2f;
        [SerializeField] private float maxZoomSpeed = 4f;
        [SerializeField] private float minZoomDistance = 8f;
        [SerializeField] private float maxZoomDistance = 65f;
        [SerializeField] private bool zoomTowardMouse = true;
        [SerializeField] private float zoomToMouseInfluence = 0.15f;
        [SerializeField] private LayerMask groundRaycastMask = ~0;
        [SerializeField] private float fallbackGroundHeight = 0f;

        [Header("Rotation & Auto-Pitch")]
        [SerializeField] private float yawSensitivity = 220f;
        [SerializeField] private float keyboardYawSensitivity = 120f;
        [SerializeField] private float minPitch = 35f;
        [SerializeField] private float maxPitch = 82f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private bool enableMiddleMouseOrbit = true;
        [SerializeField] private bool enableKeyboardRotation = true;

        [Header("Map Bounds")]
        [SerializeField] private bool useMapBounds = true;
        [SerializeField] private Terrain mapTerrain;
        [SerializeField] private bool autoBoundsFromTerrain = true;
        [SerializeField] private float boundsPadding = 2f;
        [SerializeField] private bool keepCameraViewInsideBounds = true;
        [SerializeField] private float cameraViewEdgePadding = 1.5f;
        [SerializeField] private bool useElasticEdgeResistance = true;
        [SerializeField] private float edgeResistanceDistance = 8f;
        [SerializeField] private float minEdgeSpeedFactor = 0.2f;
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
            ProcessRotation(dt);
            ProcessZoom(dt);
            ClampTargets();
            ApplySmoothedState(dt);
        }

        private void ProcessMovement(float deltaTime)
        {
            Vector2 keyboardInput = GetMoveInput();
            Vector2 edgeInput = GetEdgeScrollInput();
            bool hasKeyboardInput = keyboardInput.sqrMagnitude > 0.0001f;

            Vector2 weightedInput = keyboardInput + (edgeInput * edgeSpeedMultiplier);
            Vector2 desiredPlanarInput = Vector2.ClampMagnitude(weightedInput, 1f);

            Vector3 flatForward = transform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            Vector3 flatRight = transform.right;
            flatRight.y = 0f;
            flatRight.Normalize();

            float zoomT = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
            float speedMultiplier = speedScalesWithZoom ? Mathf.Lerp(minZoomMoveMultiplier, maxZoomMoveMultiplier, zoomT) : 1f;

            Vector3 desiredVelocity = (flatRight * desiredPlanarInput.x + flatForward * desiredPlanarInput.y) * moveSensitivity * speedMultiplier;
            desiredVelocity = ApplyElasticEdgeResistance(desiredVelocity);

            float acceleration = desiredVelocity.sqrMagnitude > 0.0001f ? moveAcceleration : moveDeceleration;
            if (hasKeyboardInput && desiredVelocity.sqrMagnitude > 0.0001f)
            {
                acceleration *= keyboardAccelerationMultiplier;
            }

            if (_currentPlanarVelocity.sqrMagnitude > 0.0001f && desiredVelocity.sqrMagnitude > 0.0001f)
            {
                float alignment = Vector3.Dot(_currentPlanarVelocity.normalized, desiredVelocity.normalized);
                if (alignment < 0.35f)
                {
                    acceleration *= directionChangeBoost;
                }
            }

            _currentPlanarVelocity = Vector3.MoveTowards(_currentPlanarVelocity, desiredVelocity, acceleration * deltaTime);
            _targetRigPosition += _currentPlanarVelocity * deltaTime;
        }

        private void ProcessRotation(float deltaTime)
        {
            float yawDelta = 0f;
            if (enableMiddleMouseOrbit && IsMiddleMouseHeld()) yawDelta += GetMouseDelta().x * yawSensitivity * deltaTime;
            if (enableKeyboardRotation) yawDelta += GetKeyboardYawInput() * keyboardYawSensitivity * deltaTime;

            _targetYaw += yawDelta;

            float zoomT = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _targetZoomDistance);
            _targetPitch = Mathf.Lerp(minPitch, maxPitch, zoomT);
        }

        private void ProcessZoom(float deltaTime)
        {
            float scrollDelta = GetScrollDeltaNormalized();
            if (Mathf.Approximately(scrollDelta, 0f)) _zoomSpeed = Mathf.MoveTowards(_zoomSpeed, 0f, zoomDeceleration * deltaTime);
            else _zoomSpeed += scrollDelta * zoomAcceleration;

            _zoomSpeed = Mathf.Clamp(_zoomSpeed, -maxZoomSpeed, maxZoomSpeed);

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

        private void ApplySmoothedState(float deltaTime)
        {
            _currentRigPosition = Vector3.Lerp(_currentRigPosition, _targetRigPosition, 1f - Mathf.Exp(-12f * deltaTime));
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
                transform.position = _currentRigPosition;
                transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
                cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

                Vector3 localCamPos = targetCamera.transform.localPosition;
                localCamPos.z = -_currentZoomDistance;
                targetCamera.transform.localPosition = localCamPos;
            }
        }

        private void ClampTargets()
        {
            if (!useMapBounds)
            {
                return;
            }

            GetEffectiveBounds(out float minX, out float maxX, out float minZ, out float maxZ);
            _targetRigPosition.x = Mathf.Clamp(_targetRigPosition.x, minX, maxX);
            _targetRigPosition.z = Mathf.Clamp(_targetRigPosition.z, minZ, maxZ);
        }

        private Vector3 ApplyElasticEdgeResistance(Vector3 desiredVelocity)
        {
            if (!useMapBounds || !useElasticEdgeResistance)
            {
                return desiredVelocity;
            }

            GetEffectiveBounds(out float minX, out float maxX, out float minZ, out float maxZ);

            float xFactor = GetAxisEdgeResistanceFactor(_targetRigPosition.x, minX, maxX);
            float zFactor = GetAxisEdgeResistanceFactor(_targetRigPosition.z, minZ, maxZ);
            float factor = Mathf.Min(xFactor, zFactor);

            return desiredVelocity * factor;
        }

        private float GetAxisEdgeResistanceFactor(float value, float min, float max)
        {
            if (max <= min)
            {
                return minEdgeSpeedFactor;
            }

            float distanceToMin = value - min;
            float distanceToMax = max - value;
            float nearest = Mathf.Min(distanceToMin, distanceToMax);

            if (nearest <= 0f)
            {
                return minEdgeSpeedFactor;
            }

            if (nearest >= edgeResistanceDistance || edgeResistanceDistance <= 0f)
            {
                return 1f;
            }

            float t = Mathf.Clamp01(nearest / edgeResistanceDistance);
            return Mathf.Lerp(minEdgeSpeedFactor, 1f, t);
        }

        private void GetEffectiveBounds(out float minX, out float maxX, out float minZ, out float maxZ)
        {
            minX = minBounds.x;
            maxX = maxBounds.x;
            minZ = minBounds.y;
            maxZ = maxBounds.y;

            if (keepCameraViewInsideBounds && TryGetGroundViewExtents(out float halfWidth, out float halfHeight))
            {
                minX += halfWidth + cameraViewEdgePadding;
                maxX -= halfWidth + cameraViewEdgePadding;
                minZ += halfHeight + cameraViewEdgePadding;
                maxZ -= halfHeight + cameraViewEdgePadding;
            }

            if (minX > maxX)
            {
                float centerX = (minBounds.x + maxBounds.x) * 0.5f;
                minX = centerX;
                maxX = centerX;
            }

            if (minZ > maxZ)
            {
                float centerZ = (minBounds.y + maxBounds.y) * 0.5f;
                minZ = centerZ;
                maxZ = centerZ;
            }
        }

        private Vector2 GetMoveInput()
        {
            Vector2 res = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.wKey.isPressed) res.y += 1;
                if (k.sKey.isPressed) res.y -= 1;
                if (k.dKey.isPressed) res.x += 1;
                if (k.aKey.isPressed) res.x -= 1;
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

            float left = GetEdgeFactor(mouse.x, edgeSizePixels);
            float right = GetEdgeFactor(Screen.width - mouse.x, edgeSizePixels);
            float bottom = GetEdgeFactor(mouse.y, edgeSizePixels);
            float top = GetEdgeFactor(Screen.height - mouse.y, edgeSizePixels);

            res.x = right - left;
            res.y = top - bottom;
            return res * (edgeScrollSensitivity / moveSensitivity);
        }

        private float GetEdgeFactor(float distanceToEdge, float threshold)
        {
            if (threshold <= 0f || distanceToEdge > threshold)
            {
                return 0f;
            }

            float t = 1f - Mathf.Clamp01(distanceToEdge / threshold);
            return Mathf.Pow(t, edgeRampExponent);
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

        private float GetKeyboardYawInput()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            return k != null ? (k.qKey.isPressed ? -1 : (k.eKey.isPressed ? 1 : 0)) : 0;
#else
            return UnityEngine.Input.GetKey(KeyCode.Q) ? -1 : (UnityEngine.Input.GetKey(KeyCode.E) ? 1 : 0);
#endif
        }

        private bool TryGetMouseGroundPoint(out Vector3 point)
        {
            point = Vector3.zero;
            Ray ray = targetCamera.ScreenPointToRay(GetMousePosition());
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundRaycastMask))
            {
                point = hit.point;
                return true;
            }
            return false;
        }

        private void ResolveTerrainReference()
        {
            if (mapTerrain == null) mapTerrain = FindFirstObjectByType<Terrain>();
        }

        private void RefreshBoundsFromSurface()
        {
            if (autoBoundsFromTerrain && mapTerrain != null)
            {
                Vector3 pos = mapTerrain.transform.position;
                Vector3 size = mapTerrain.terrainData.size;
                minBounds = new Vector2(pos.x + boundsPadding, pos.z + boundsPadding);
                maxBounds = new Vector2(pos.x + size.x - boundsPadding, pos.z + size.z - boundsPadding);
            }
        }

        private bool TryGetGroundViewExtents(out float halfWidth, out float halfHeight)
        {
            halfWidth = 0f;
            halfHeight = 0f;
            if (targetCamera == null)
            {
                return false;
            }

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, fallbackGroundHeight, 0f));
            Vector3[] hits = new Vector3[4];
            Vector2[] corners =
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                Ray ray = targetCamera.ViewportPointToRay(new Vector3(corners[i].x, corners[i].y, 0f));
                if (!groundPlane.Raycast(ray, out float enter))
                {
                    return false;
                }

                hits[i] = ray.GetPoint(enter);
            }

            float minX = hits[0].x;
            float maxX = hits[0].x;
            float minZ = hits[0].z;
            float maxZ = hits[0].z;

            for (int i = 1; i < hits.Length; i++)
            {
                Vector3 p = hits[i];
                minX = Mathf.Min(minX, p.x);
                maxX = Mathf.Max(maxX, p.x);
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
            }

            halfWidth = (maxX - minX) * 0.5f;
            halfHeight = (maxZ - minZ) * 0.5f;
            return true;
        }
    }
}
