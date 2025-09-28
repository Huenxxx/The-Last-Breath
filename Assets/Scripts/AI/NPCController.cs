using UnityEngine;
using UnityEngine.AI;
using TheLastBreath.Characters;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Controls NPC behavior including wandering, idle states, and basic AI
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        [Header("Wandering Settings")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float minWanderDelay = 2f;
        [SerializeField] private float maxWanderDelay = 8f;
        [SerializeField] private float idleTime = 3f;
        
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private LayerMask playerLayerMask = 1;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 1.5f;
        [SerializeField] private float runSpeed = 4f;
        
        // Components
        private CharacterController characterController;
        private NavMeshAgent navMeshAgent;
        
        // AI State
        private NPCState currentState = NPCState.Idle;
        private Vector3 homePosition;
        private float stateTimer;
        private Vector3 currentDestination;
        
        // Detection
        private Transform detectedPlayer;
        
        public enum NPCState
        {
            Idle,
            Wandering,
            MovingToDestination,
            Investigating,
            Fleeing
        }
        
        // Properties
        public NPCState CurrentState => currentState;
        public Vector3 HomePosition => homePosition;
        public bool IsMoving => navMeshAgent.hasPath && navMeshAgent.remainingDistance > 0.1f;
        
        /// <summary>
        /// Initialize NPC
        /// </summary>
        void Start()
        {
            InitializeComponents();
            SetupNPC();
        }
        
        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            characterController = GetComponent<CharacterController>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            if (characterController == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires a CharacterController component!");
            }
            
            if (navMeshAgent == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires a NavMeshAgent component!");
            }
        }
        
        /// <summary>
        /// Set up NPC initial state
        /// </summary>
        private void SetupNPC()
        {
            homePosition = transform.position;
            
            // Configure NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = walkSpeed;
                navMeshAgent.stoppingDistance = 0.5f;
                navMeshAgent.autoBraking = true;
            }
            
            // Set initial state
            ChangeState(NPCState.Idle);
            
            // Mark as NPC (not player controlled)
            if (characterController != null)
            {
                characterController.SetPlayerControlled(false);
            }
        }
        
        /// <summary>
        /// Update NPC behavior
        /// </summary>
        void Update()
        {
            UpdateDetection();
            UpdateStateMachine();
            UpdateMovement();
        }
        
        /// <summary>
        /// Update player detection
        /// </summary>
        private void UpdateDetection()
        {
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRadius, playerLayerMask);
            
            detectedPlayer = null;
            foreach (Collider playerCollider in playersInRange)
            {
                CharacterController playerCharacter = playerCollider.GetComponent<CharacterController>();
                if (playerCharacter != null && playerCharacter.IsPlayerControlled)
                {
                    detectedPlayer = playerCollider.transform;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Update state machine
        /// </summary>
        private void UpdateStateMachine()
        {
            stateTimer += Time.deltaTime;
            
            switch (currentState)
            {
                case NPCState.Idle:
                    UpdateIdleState();
                    break;
                    
                case NPCState.Wandering:
                    UpdateWanderingState();
                    break;
                    
                case NPCState.MovingToDestination:
                    UpdateMovingState();
                    break;
                    
                case NPCState.Investigating:
                    UpdateInvestigatingState();
                    break;
                    
                case NPCState.Fleeing:
                    UpdateFleeingState();
                    break;
            }
        }
        
        /// <summary>
        /// Update idle state
        /// </summary>
        private void UpdateIdleState()
        {
            // Randomly start wandering after idle time
            if (stateTimer >= idleTime)
            {
                if (Random.Range(0f, 1f) < 0.7f) // 70% chance to wander
                {
                    StartWandering();
                }
                else
                {
                    // Stay idle longer
                    stateTimer = 0f;
                }
            }
            
            // React to nearby player
            if (detectedPlayer != null)
            {
                ChangeState(NPCState.Investigating);
            }
        }
        
        /// <summary>
        /// Update wandering state
        /// </summary>
        private void UpdateWanderingState()
        {
            // Check if reached destination
            if (!IsMoving)
            {
                ChangeState(NPCState.Idle);
            }
            
            // React to nearby player
            if (detectedPlayer != null)
            {
                navMeshAgent.ResetPath();
                ChangeState(NPCState.Investigating);
            }
        }
        
        /// <summary>
        /// Update moving to destination state
        /// </summary>
        private void UpdateMovingState()
        {
            // Check if reached destination
            if (!IsMoving)
            {
                ChangeState(NPCState.Idle);
            }
        }
        
        /// <summary>
        /// Update investigating state
        /// </summary>
        private void UpdateInvestigatingState()
        {
            if (detectedPlayer == null)
            {
                // Lost sight of player, return to wandering
                ChangeState(NPCState.Idle);
                return;
            }
            
            // Look at player
            Vector3 lookDirection = (detectedPlayer.position - transform.position).normalized;
            lookDirection.y = 0;
            
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
            }
            
            // After investigating for a while, return to normal behavior
            if (stateTimer >= 3f)
            {
                ChangeState(NPCState.Idle);
            }
        }
        
        /// <summary>
        /// Update fleeing state
        /// </summary>
        private void UpdateFleeingState()
        {
            // Check if reached safe distance or destination
            if (!IsMoving || (detectedPlayer != null && 
                Vector3.Distance(transform.position, detectedPlayer.position) > detectionRadius * 2f))
            {
                ChangeState(NPCState.Idle);
            }
        }
        
        /// <summary>
        /// Update movement and animation
        /// </summary>
        private void UpdateMovement()
        {
            if (characterController == null || navMeshAgent == null) return;
            
            // Update character controller with movement state
            bool isMoving = IsMoving;
            bool isRunning = navMeshAgent.speed > walkSpeed;
            
            characterController.SetMoving(isMoving);
            characterController.SetRunning(isRunning);
            
            // Update animation speed based on actual movement
            if (isMoving)
            {
                float speedRatio = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
                characterController.SetAnimationSpeed(speedRatio);
            }
        }
        
        /// <summary>
        /// Change NPC state
        /// </summary>
        /// <param name="newState">New state to change to</param>
        private void ChangeState(NPCState newState)
        {
            if (currentState == newState) return;
            
            // Exit current state
            OnExitState(currentState);
            
            // Change state
            NPCState previousState = currentState;
            currentState = newState;
            stateTimer = 0f;
            
            // Enter new state
            OnEnterState(newState, previousState);
        }
        
        /// <summary>
        /// Handle entering a new state
        /// </summary>
        /// <param name="state">State being entered</param>
        /// <param name="previousState">Previous state</param>
        private void OnEnterState(NPCState state, NPCState previousState)
        {
            switch (state)
            {
                case NPCState.Idle:
                    navMeshAgent.ResetPath();
                    break;
                    
                case NPCState.Wandering:
                    // Already handled in StartWandering()
                    break;
                    
                case NPCState.Investigating:
                    navMeshAgent.ResetPath();
                    break;
                    
                case NPCState.Fleeing:
                    navMeshAgent.speed = runSpeed;
                    break;
            }
        }
        
        /// <summary>
        /// Handle exiting a state
        /// </summary>
        /// <param name="state">State being exited</param>
        private void OnExitState(NPCState state)
        {
            switch (state)
            {
                case NPCState.Fleeing:
                    navMeshAgent.speed = walkSpeed;
                    break;
            }
        }
        
        /// <summary>
        /// Start wandering behavior
        /// </summary>
        private void StartWandering()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += homePosition;
            randomDirection.y = homePosition.y; // Keep same height
            
            // Find valid position on NavMesh
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                currentDestination = hit.position;
                navMeshAgent.SetDestination(currentDestination);
                ChangeState(NPCState.Wandering);
            }
            else
            {
                // Couldn't find valid position, stay idle
                ChangeState(NPCState.Idle);
            }
        }
        
        /// <summary>
        /// Move to specific position
        /// </summary>
        /// <param name="destination">Target position</param>
        /// <param name="run">Whether to run to destination</param>
        public void MoveTo(Vector3 destination, bool run = false)
        {
            if (navMeshAgent == null) return;
            
            currentDestination = destination;
            navMeshAgent.speed = run ? runSpeed : walkSpeed;
            navMeshAgent.SetDestination(destination);
            ChangeState(NPCState.MovingToDestination);
        }
        
        /// <summary>
        /// Stop current movement
        /// </summary>
        public void Stop()
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.ResetPath();
            }
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Make NPC flee from a position
        /// </summary>
        /// <param name="fleeFromPosition">Position to flee from</param>
        public void FleeFrom(Vector3 fleeFromPosition)
        {
            Vector3 fleeDirection = (transform.position - fleeFromPosition).normalized;
            Vector3 fleeDestination = transform.position + fleeDirection * wanderRadius;
            
            // Find valid position on NavMesh
            if (NavMesh.SamplePosition(fleeDestination, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                currentDestination = hit.position;
                navMeshAgent.SetDestination(currentDestination);
                ChangeState(NPCState.Fleeing);
            }
        }
        
        /// <summary>
        /// Set home position for wandering
        /// </summary>
        /// <param name="position">New home position</param>
        public void SetHomePosition(Vector3 position)
        {
            homePosition = position;
        }
        
        /// <summary>
        /// Draw debug information
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Draw wander radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(homePosition, wanderRadius);
            
            // Draw detection radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            // Draw current destination
            if (currentDestination != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(currentDestination, 0.5f);
                Gizmos.DrawLine(transform.position, currentDestination);
            }
            
            // Draw state info
            if (Application.isPlaying)
            {
                Vector3 labelPosition = transform.position + Vector3.up * 2f;
                UnityEditor.Handles.Label(labelPosition, $"State: {currentState}");
            }
        }
    }
}