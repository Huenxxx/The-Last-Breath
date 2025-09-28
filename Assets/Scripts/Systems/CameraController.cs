using UnityEngine;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// RTS-style camera controller with free movement and follow modes
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera cameraComponent;
        [SerializeField] private Transform followTarget;
        
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float fastMoveSpeed = 20f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float smoothTime = 0.3f;
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;
        [SerializeField] private float currentZoom = 15f;
        
        [Header("Boundaries")]
        [SerializeField] private Vector2 mapBounds = new Vector2(100f, 100f);
        [SerializeField] private bool useBoundaries = true;
        
        [Header("Camera Modes")]
        [SerializeField] private CameraMode currentMode = CameraMode.Free;
        [SerializeField] private float followDistance = 10f;
        [SerializeField] private float followHeight = 8f;
        
        public enum CameraMode
        {
            Free,
            Follow
        }
        
        // Private variables
        private Vector3 targetPosition;
        private Vector3 velocity = Vector3.zero;
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        
        // Input tracking
        private bool isShiftPressed = false;
        
        /// <summary>
        /// Initialize camera settings
        /// </summary>
        void Start()
        {
            if (cameraComponent == null)
                cameraComponent = GetComponent<Camera>();
            
            targetPosition = transform.position;
            
            // Set initial camera angle for RTS view
            transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        }
        
        /// <summary>
        /// Handle camera updates
        /// </summary>
        void Update()
        {
            HandleInput();
            UpdateCameraPosition();
            UpdateZoom();
        }
        
        /// <summary>
        /// Process all input for camera control
        /// </summary>
        private void HandleInput()
        {
            // Track shift key for fast movement
            isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            // Camera mode switching
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleCameraMode();
            }
            
            // Only handle free camera input in free mode
            if (currentMode == CameraMode.Free)
            {
                HandleFreeCameraInput();
            }
            
            HandleZoomInput();
            HandleRotationInput();
        }
        
        /// <summary>
        /// Handle input for free camera movement
        /// </summary>
        private void HandleFreeCameraInput()
        {
            Vector3 inputVector = Vector3.zero;
            
            // WASD movement
            if (Input.GetKey(KeyCode.W)) inputVector += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) inputVector += Vector3.back;
            if (Input.GetKey(KeyCode.A)) inputVector += Vector3.left;
            if (Input.GetKey(KeyCode.D)) inputVector += Vector3.right;
            
            // Arrow key movement
            if (Input.GetKey(KeyCode.UpArrow)) inputVector += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow)) inputVector += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow)) inputVector += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow)) inputVector += Vector3.right;
            
            // Apply movement
            if (inputVector != Vector3.zero)
            {
                float currentMoveSpeed = isShiftPressed ? fastMoveSpeed : moveSpeed;
                Vector3 moveDirection = transform.TransformDirection(inputVector.normalized);
                moveDirection.y = 0; // Keep movement horizontal
                
                targetPosition += moveDirection * currentMoveSpeed * Time.deltaTime;
            }
            
            // Mouse edge scrolling
            HandleMouseEdgeScrolling();
            
            // Middle mouse button dragging
            HandleMouseDragging();
            
            // Apply boundaries
            if (useBoundaries)
            {
                ApplyBoundaries();
            }
        }
        
        /// <summary>
        /// Handle mouse edge scrolling
        /// </summary>
        private void HandleMouseEdgeScrolling()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 moveDirection = Vector3.zero;
            
            float edgeSize = 10f;
            
            if (mousePosition.x < edgeSize)
                moveDirection += Vector3.left;
            else if (mousePosition.x > Screen.width - edgeSize)
                moveDirection += Vector3.right;
                
            if (mousePosition.y < edgeSize)
                moveDirection += Vector3.back;
            else if (mousePosition.y > Screen.height - edgeSize)
                moveDirection += Vector3.forward;
            
            if (moveDirection != Vector3.zero)
            {
                float currentMoveSpeed = isShiftPressed ? fastMoveSpeed : moveSpeed;
                Vector3 worldMoveDirection = transform.TransformDirection(moveDirection.normalized);
                worldMoveDirection.y = 0;
                
                targetPosition += worldMoveDirection * currentMoveSpeed * Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Handle middle mouse button dragging
        /// </summary>
        private void HandleMouseDragging()
        {
            if (Input.GetMouseButtonDown(2)) // Middle mouse button
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }
            
            if (isDragging)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                Vector3 moveDirection = new Vector3(-mouseDelta.x, 0, -mouseDelta.y);
                
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection.y = 0;
                
                targetPosition += moveDirection * moveSpeed * Time.deltaTime * 0.01f;
                lastMousePosition = Input.mousePosition;
            }
        }
        
        /// <summary>
        /// Handle zoom input
        /// </summary>
        private void HandleZoomInput()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentZoom -= scroll * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            }
        }
        
        /// <summary>
        /// Handle camera rotation input
        /// </summary>
        private void HandleRotationInput()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0, Space.World);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
            }
        }
        
        /// <summary>
        /// Update camera position based on current mode
        /// </summary>
        private void UpdateCameraPosition()
        {
            Vector3 desiredPosition = targetPosition;
            
            if (currentMode == CameraMode.Follow && followTarget != null)
            {
                // Calculate follow position
                Vector3 followPos = followTarget.position;
                followPos -= transform.forward * followDistance;
                followPos.y += followHeight;
                desiredPosition = followPos;
            }
            
            // Smooth movement to target position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
        
        /// <summary>
        /// Update camera zoom (field of view)
        /// </summary>
        private void UpdateZoom()
        {
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, currentZoom, Time.deltaTime * 5f);
            }
        }
        
        /// <summary>
        /// Apply movement boundaries to keep camera within map bounds
        /// </summary>
        private void ApplyBoundaries()
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, -mapBounds.x, mapBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, -mapBounds.y, mapBounds.y);
        }
        
        /// <summary>
        /// Toggle between free and follow camera modes
        /// </summary>
        public void ToggleCameraMode()
        {
            currentMode = currentMode == CameraMode.Free ? CameraMode.Follow : CameraMode.Free;
            
            if (currentMode == CameraMode.Free)
            {
                targetPosition = transform.position;
            }
        }
        
        /// <summary>
        /// Set the target to follow
        /// </summary>
        /// <param name="target">Transform to follow</param>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }
        
        /// <summary>
        /// Set camera mode
        /// </summary>
        /// <param name="mode">Camera mode to set</param>
        public void SetCameraMode(CameraMode mode)
        {
            currentMode = mode;
            if (currentMode == CameraMode.Free)
            {
                targetPosition = transform.position;
            }
        }
        
        /// <summary>
        /// Get current camera mode
        /// </summary>
        /// <returns>Current camera mode</returns>
        public CameraMode GetCameraMode()
        {
            return currentMode;
        }
        
        /// <summary>
        /// Focus camera on a specific position
        /// </summary>
        /// <param name="position">Position to focus on</param>
        public void FocusOnPosition(Vector3 position)
        {
            targetPosition = position;
            targetPosition.y = transform.position.y; // Maintain current height
        }
    }
}