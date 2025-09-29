using UnityEngine;
using UnityEngine.AI;

namespace TheLastBreath.Characters
{
    /// <summary>
    /// Character controller for both player and NPC characters
    /// Handles movement, animation, and basic character state
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Character Settings")]
        [SerializeField] private string characterName = "Character";
        [SerializeField] private bool isPlayerControlled = false;
        [SerializeField] private float interactionRange = 2f;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float rotationSpeed = 360f;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private bool hasAnimator = false;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private Material defaultMaterial;
        
        // Components
        private NavMeshAgent navAgent;
        private Renderer characterRenderer;
        
        // State
        private bool isSelected = false;
        private bool isMoving = false;
        private Vector3 targetDestination;
        private CharacterState currentState = CharacterState.Idle;
        
        // Animation hashes for performance
        private int speedHash;
        private int isMovingHash;
        
        public enum CharacterState
        {
            Idle,
            Moving,
            Interacting,
            Dead
        }
        
        // Events
        public System.Action<CharacterController> OnCharacterSelected;
        public System.Action<CharacterController> OnCharacterDeselected;
        public System.Action<CharacterController, Vector3> OnDestinationReached;
        
        // Properties
        public bool IsSelected => isSelected;
        public bool IsPlayerControlled => isPlayerControlled;
        public string CharacterName => characterName;
        public CharacterState CurrentState => currentState;
        public Vector3 Position => transform.position;
        
        /// <summary>
        /// Initialize character components and settings
        /// </summary>
        void Start()
        {
            InitializeComponents();
            SetupNavMeshAgent();
            SetupAnimation();
            SetupVisuals();
        }
        
        /// <summary>
        /// Update character state and movement
        /// </summary>
        void Update()
        {
            UpdateMovement();
            UpdateAnimation();
            CheckDestinationReached();
        }
        
        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            navAgent = GetComponent<NavMeshAgent>();
            characterRenderer = GetComponentInChildren<Renderer>();
            
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                hasAnimator = animator != null;
            }
            
            // Create selection indicator if not assigned
            if (selectionIndicator == null)
            {
                CreateSelectionIndicator();
            }
        }
        
        /// <summary>
        /// Setup NavMesh agent properties
        /// </summary>
        private void SetupNavMeshAgent()
        {
            if (navAgent != null)
            {
                navAgent.speed = walkSpeed;
                navAgent.angularSpeed = rotationSpeed;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 0.5f;
            }
        }
        
        /// <summary>
        /// Setup animation parameters
        /// </summary>
        private void SetupAnimation()
        {
            if (hasAnimator)
            {
                speedHash = Animator.StringToHash("Speed");
                isMovingHash = Animator.StringToHash("IsMoving");
            }
        }
        
        /// <summary>
        /// Setup visual components
        /// </summary>
        private void SetupVisuals()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }
        
        /// <summary>
        /// Create a simple selection indicator
        /// </summary>
        private void CreateSelectionIndicator()
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            // Remove collider from indicator
            Collider indicatorCollider = indicator.GetComponent<Collider>();
            if (indicatorCollider != null)
            {
                DestroyImmediate(indicatorCollider);
            }
            
            // Set material to green
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                indicatorRenderer.material.color = Color.green;
            }
            
            selectionIndicator = indicator;
            selectionIndicator.SetActive(false);
        }
        
        /// <summary>
        /// Update movement state and navigation
        /// </summary>
        private void UpdateMovement()
        {
            if (navAgent == null) return;
            
            // Update movement state
            bool wasMoving = isMoving;
            isMoving = navAgent.velocity.magnitude > 0.1f && !navAgent.isStopped;
            
            // Update character state
            if (isMoving && currentState != CharacterState.Moving)
            {
                SetState(CharacterState.Moving);
            }
            else if (!isMoving && currentState == CharacterState.Moving)
            {
                SetState(CharacterState.Idle);
            }
        }
        
        /// <summary>
        /// Update animation parameters
        /// </summary>
        private void UpdateAnimation()
        {
            if (!hasAnimator) return;
            
            float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
            animator.SetFloat(speedHash, speed);
            animator.SetBool(isMovingHash, isMoving);
        }
        
        /// <summary>
        /// Check if character has reached destination
        /// </summary>
        private void CheckDestinationReached()
        {
            if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                if (isMoving)
                {
                    OnDestinationReached?.Invoke(this, targetDestination);
                }
            }
        }
        
        /// <summary>
        /// Move character to specified position
        /// </summary>
        /// <param name="destination">Target position</param>
        /// <param name="isRunning">Whether to run or walk</param>
        public void MoveTo(Vector3 destination, bool isRunning = false)
        {
            if (navAgent == null) return;
            
            targetDestination = destination;
            navAgent.speed = isRunning ? runSpeed : walkSpeed;
            navAgent.SetDestination(destination);
            
            SetState(CharacterState.Moving);
        }
        
        /// <summary>
        /// Stop character movement
        /// </summary>
        public void Stop()
        {
            if (navAgent != null)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
            }
            
            SetState(CharacterState.Idle);
        }
        
        /// <summary>
        /// Resume character movement
        /// </summary>
        public void Resume()
        {
            if (navAgent != null)
            {
                navAgent.isStopped = false;
            }
        }
        
        /// <summary>
        /// Set character selection state
        /// </summary>
        /// <param name="selected">Selection state</param>
        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            
            isSelected = selected;
            
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(selected);
            }
            
            // Change material if available
            if (characterRenderer != null)
            {
                if (selected && selectedMaterial != null)
                {
                    characterRenderer.material = selectedMaterial;
                }
                else if (!selected && defaultMaterial != null)
                {
                    characterRenderer.material = defaultMaterial;
                }
            }
            
            // Invoke events
            if (selected)
            {
                OnCharacterSelected?.Invoke(this);
            }
            else
            {
                OnCharacterDeselected?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Set character state
        /// </summary>
        /// <param name="newState">New character state</param>
        private void SetState(CharacterState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            
            // Handle state-specific logic
            switch (newState)
            {
                case CharacterState.Idle:
                    break;
                case CharacterState.Moving:
                    break;
                case CharacterState.Interacting:
                    Stop();
                    break;
                case CharacterState.Dead:
                    Stop();
                    break;
            }
        }
        
        /// <summary>
        /// Check if character can interact with target
        /// </summary>
        /// <param name="target">Target position</param>
        /// <returns>True if within interaction range</returns>
        public bool CanInteractWith(Vector3 target)
        {
            float distance = Vector3.Distance(transform.position, target);
            return distance <= interactionRange;
        }
        
        /// <summary>
        /// Get distance to target position
        /// </summary>
        /// <param name="target">Target position</param>
        /// <returns>Distance to target</returns>
        public float GetDistanceTo(Vector3 target)
        {
            return Vector3.Distance(transform.position, target);
        }
        
        /// <summary>
        /// Set player controlled state
        /// </summary>
        /// <param name="playerControlled">Player control state</param>
        public void SetPlayerControlled(bool playerControlled)
        {
            isPlayerControlled = playerControlled;
        }
        
        /// <summary>
        /// Set moving state for NPCController
        /// </summary>
        /// <param name="moving">Moving state</param>
        public void SetMoving(bool moving)
        {
            isMoving = moving;
            if (hasAnimator)
            {
                animator.SetBool(isMovingHash, moving);
            }
        }
        
        /// <summary>
        /// Set running state for NPCController
        /// </summary>
        /// <param name="running">Running state</param>
        public void SetRunning(bool running)
        {
            if (navAgent != null)
            {
                navAgent.speed = running ? runSpeed : walkSpeed;
            }
            
            if (hasAnimator)
            {
                animator.SetFloat(speedHash, running ? runSpeed : walkSpeed);
            }
        }
        
        /// <summary>
        /// Set animation speed for NPCController
        /// </summary>
        /// <param name="speed">Animation speed multiplier</param>
        public void SetAnimationSpeed(float speed)
        {
            if (hasAnimator)
            {
                animator.speed = speed;
            }
        }
        
        /// <summary>
        /// Handle mouse click on character
        /// </summary>
        void OnMouseDown()
        {
            if (isPlayerControlled)
            {
                // Handle character selection
                var selectionSystem = FindObjectOfType<TheLastBreath.Systems.UnitSelectionSystem>();
                if (selectionSystem != null)
                {
                    selectionSystem.SelectCharacter(this);
                }
            }
        }
        
        /// <summary>
        /// Draw gizmos for debugging
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Draw destination if moving
            if (navAgent != null && navAgent.hasPath)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(navAgent.destination, 0.5f);
                
                // Draw path
                Vector3[] path = navAgent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }
    }
}