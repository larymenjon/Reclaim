using UnityEngine;

namespace TMPro.Examples
{
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Movement (WASD / Edge Scrolling)")]
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float sprintMultiplier = 2.5f;
        [SerializeField] private float movementSmoothing = 10f;
        [SerializeField] private bool useEdgeScrolling = true;
        [SerializeField] private float edgePaddingPixels = 15f;

        [Header("Rotation (Middle Mouse / Alt + Mouse)")]
        [SerializeField] private float rotationSensitivity = 120f;
        [SerializeField] private float rotationSmoothing = 15f;
        [SerializeField] private float minElevationAngle = 15f;
        [SerializeField] private float maxElevationAngle = 75f;

        [Header("Zoom (Mouse Wheel)")]
        [SerializeField] private float zoomSensitivity = 15f;
        [SerializeField] private float zoomSmoothing = 8f;
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float maxZoomDistance = 60f;
        
        [Tooltip("Garante que a inclinação mude dependendo do nível de zoom, igual a Manor Lords.")]
        [SerializeField] private bool dynamicPitchBasedOnZoom = true;

        // Estados internos (Targeting para interpolação suave)
        private Vector3 _targetPivotPosition;
        private float _targetOrbitAngle;
        private float _targetElevationAngle;
        private float _targetZoomDistance;

        private Vector3 _currentPivotPosition;
        private float _currentOrbitAngle;
        private float _currentElevationAngle;
        private float _currentZoomDistance;

        private Transform _cameraTransform;

        private void Awake()
        {
            _cameraTransform = transform;
            
            // Inicializa os alvos com os valores atuais da cena para evitar saltos visuais
            _currentPivotPosition = _targetPivotPosition = transform.position;
            _targetOrbitAngle = _currentOrbitAngle = transform.eulerAngles.y;
            _targetElevationAngle = _currentElevationAngle = transform.eulerAngles.x;
            _targetZoomDistance = _currentZoomDistance = maxZoomDistance * 0.5f;
        }

        private void Update()
        {
            HandleInputs();
            CalculateSmoothing();
            ApplyTransform();
        }

        private void HandleInputs()
        {
            // 1. VELOCIDADE DE MOVIMENTO (SHIFT PARA SPRINTAR)
            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed *= sprintMultiplier;
            }

            // 2. ENTRADA DE MOVIMENTO (WASD / SETAS)
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 moveInput = new Vector3(inputX, 0f, inputZ).normalized;

            // 3. EDGE SCROLLING (Manor Lords / Farthest Frontier)
            if (useEdgeScrolling && moveInput.sqrMagnitude < 0.01f)
            {
                Vector3 mousePos = Input.mousePosition;
                if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
                {
                    if (mousePos.x < edgePaddingPixels) moveInput.x = -1f;
                    else if (mousePos.x > Screen.width - edgePaddingPixels) moveInput.x = 1f;

                    if (mousePos.y < edgePaddingPixels) moveInput.z = -1f;
                    else if (mousePos.y > Screen.height - edgePaddingPixels) moveInput.z = 1f;
                }
            }

            // Move o pivô baseado na rotação horizontal atual da câmera (pressione W e vá para onde a câmera olha)
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 forward = _cameraTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 right = _cameraTransform.right;
                right.y = 0f;
                right.Normalize();

                Vector3 direction = (forward * moveInput.z + right * moveInput.x).normalized;
                _targetPivotPosition += direction * (currentSpeed * Time.deltaTime);
            }

            // 4. ROTAÇÃO DA CÂMERA (Botão do Meio do Mouse ou Alt+Clique Direito)
            if (Input.GetMouseButton(2) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1)))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                _targetOrbitAngle += Input.GetAxis("Mouse X") * rotationSensitivity * Time.deltaTime;
                
                if (!dynamicPitchBasedOnZoom)
                {
                    _targetElevationAngle -= Input.GetAxis("Mouse Y") * rotationSensitivity * Time.deltaTime;
                    _targetElevationAngle = Mathf.Clamp(_targetElevationAngle, minElevationAngle, maxElevationAngle);
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // 5. ZOOM (Roda do mouse)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetZoomDistance -= scroll * zoomSensitivity;
                _targetZoomDistance = Mathf.Clamp(_targetZoomDistance, minZoomDistance, maxZoomDistance);
            }
        }

        private void CalculateSmoothing()
        {
            // Interpolações usando Lerp amortecido por DeltaTime (Garante fluidez independente do framerate)
            _currentPivotPosition = Vector3.Lerp(_currentPivotPosition, _targetPivotPosition, Time.deltaTime * movementSmoothing);
            _currentOrbitAngle = Mathf.LerpAngle(_currentOrbitAngle, _targetOrbitAngle, Time.deltaTime * rotationSmoothing);
            _currentZoomDistance = Mathf.Lerp(_currentZoomDistance, _targetZoomDistance, Time.deltaTime * zoomSmoothing);

            if (dynamicPitchBasedOnZoom)
            {
                // Quanto mais perto do chão (Zoom menor), mais deitada/horizontal a câmera fica.
                float zoomt = Mathf.InverseLerp(minZoomDistance, maxZoomDistance, _currentZoomDistance);
                _targetElevationAngle = Mathf.Lerp(minElevationAngle, maxElevationAngle, zoomt);
                _currentElevationAngle = Mathf.LerpAngle(_currentElevationAngle, _targetElevationAngle, Time.deltaTime * rotationSmoothing);
            }
            else
            {
                _currentElevationAngle = Mathf.LerpAngle(_currentElevationAngle, _targetElevationAngle, Time.deltaTime * rotationSmoothing);
            }
        }

        private void ApplyTransform()
        {
            // Calcula a rotação final combinada
            Quaternion rotation = Quaternion.Euler(_currentElevationAngle, _currentOrbitAngle, 0f);
            
            // Calcula a posição recuada baseada no zoom a partir do ponto central focal (pivô)
            Vector3 positionOffset = rotation * new Vector3(0f, 0f, -_currentZoomDistance);
            
            // Aplica os dados transformados diretamente na câmera
            transform.position = _currentPivotPosition + positionOffset;
            transform.rotation = rotation;
        }

        /// <summary>
        /// Permite que outros sistemas (como foco em eventos do jogo) teleportem ou centralizem a câmera instantaneamente.
        /// </summary>
        public void FocusOnPosition(Vector3 worldPosition)
        {
            _targetPivotPosition = worldPosition;
        }
    }
}
