using UnityEngine;
using UnityEngine.AI;
using TheLastBreath.Characters;
using System.Collections;
using CharacterController = TheLastBreath.Characters.CharacterController;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Enum for different types of NPC jobs
    /// </summary>
    public enum NPCJob
    {
        None,
        Guard,
        Farmer,
        Crafter,
        Patrol,
        Scavenger,
        Cook,
        Build,
        Farm,
        Craft,
        Scavenge
    }

    /// <summary>
    /// Controls NPC behavior including needs-based AI, utility decisions, and basic behaviors
    /// </summary>
    [RequireComponent(typeof(NPCNeeds))]
    [RequireComponent(typeof(UtilityAI))]
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Configuration")]
        public string npcName = "NPC";
        public NPCJob currentJob = NPCJob.None;
        
        [Header("Wandering Settings")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float minWanderDelay = 2f;
        [SerializeField] private float maxWanderDelay = 8f;
        [SerializeField] private float idleTime = 3f;
        [SerializeField] private float restDuration = 5f;
        [SerializeField] private float actionCooldown = 2f;
        
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 15f;
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private LayerMask threatLayer = -1;
        [SerializeField] private LayerMask foodLayer = -1;
        
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 1.5f;
        [SerializeField] private float runSpeed = 4f;
        
        // Components
        private CharacterController characterController;
        private NavMeshAgent navMeshAgent;
        private NPCNeeds needs;
        private UtilityAI utilityAI;
        private NPCJobSystem jobSystem;
        
        // AI State
        private NPCState currentState = NPCState.Idle;
        private Vector3 homePosition;
        private float stateTimer;
        private Vector3 currentDestination;
        private float lastActionTime;
        private Coroutine currentAction;
        
        // Detection
        private Transform detectedPlayer;
        private Transform currentTarget;
        private Vector3 wanderTarget;
        
        public enum NPCState
        {
            Idle,
            Wandering,
            MovingToDestination,
            Investigating,
            Fleeing,
            Resting,
            Seeking,
            Working
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
            
            // Subscribe to needs events
            if (needs != null)
            {
                needs.OnNeedCritical += HandleCriticalNeed;
                needs.OnNeedChanged += HandleNeedChanged;
            }
            
            StartCoroutine(AIUpdateLoop());
        }
        
        /// <summary>
        /// Initialize required components
        /// </summary>
        private void InitializeComponents()
        {
            characterController = GetComponent<CharacterController>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            needs = GetComponent<NPCNeeds>();
            utilityAI = GetComponent<UtilityAI>();
            jobSystem = GetComponent<NPCJobSystem>();
            
            if (characterController == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires a CharacterController component!");
            }
            
            if (navMeshAgent == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires a NavMeshAgent component!");
            }
            
            if (needs == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires an NPCNeeds component!");
            }
            
            if (utilityAI == null)
            {
                Debug.LogError($"NPCController on {gameObject.name} requires a UtilityAI component!");
            }
            
            if (jobSystem == null)
            {
                Debug.LogWarning($"NPCJobSystem not found on {gameObject.name} - jobs will not be available");
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
        /// AI Update Loop - replaces traditional Update for needs-based AI
        /// </summary>
        IEnumerator AIUpdateLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // Update AI every second
                
                if (Time.time - lastActionTime >= actionCooldown)
                {
                    UpdateAI();
                }
                
                // Still update detection and movement every frame
                UpdateDetection();
                UpdateMovement();
            }
        }
        
        /// <summary>
        /// Update AI decision making using Utility AI
        /// </summary>
        void UpdateAI()
        {
            // Don't interrupt critical actions
            if (currentState == NPCState.Fleeing || currentState == NPCState.Resting)
                return;
                
            // Get best action from Utility AI
            var bestAction = utilityAI.GetBestAction(this);
            
            if (bestAction != null)
            {
                ExecuteAction(bestAction);
                lastActionTime = Time.time;
            }
        }
        
        /// <summary>
        /// Execute AI action based on Utility AI decision
        /// </summary>
        void ExecuteAction(AIAction action)
        {
            // Stop current action
            if (currentAction != null)
            {
                StopCoroutine(currentAction);
            }
            
            // Execute new action
            switch (action.GetType().Name)
            {
                case "SeekFoodAction":
                    currentAction = StartCoroutine(SeekFood());
                    break;
                case "RestAction":
                    currentAction = StartCoroutine(Rest());
                    break;
                case "FleeAction":
                    currentAction = StartCoroutine(Flee());
                    break;
                case "WanderAction":
                    currentAction = StartCoroutine(WanderAI());
                    break;
                case "SeekShelterAction":
                    currentAction = StartCoroutine(SeekShelter());
                    break;
                case "SocializeAction":
                    currentAction = StartCoroutine(Socialize());
                    break;
            }
        }
        
        /// <summary>
        /// Update NPC behavior (legacy - now simplified)
        /// </summary>
        void Update()
        {
            // Legacy update for compatibility - most logic moved to AIUpdateLoop
            UpdateDetection();
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
            
            // Draw current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
            
            #if UNITY_EDITOR
            // Draw state info
            if (Application.isPlaying)
            {
                Vector3 labelPosition = transform.position + Vector3.up * 2f;
                string stateInfo = $"State: {currentState}";
                if (needs != null)
                {
                    stateInfo += $"\nHunger: {needs.GetNeed(NPCNeeds.NeedType.Hunger):F0}";
                    stateInfo += $"\nFatigue: {needs.GetNeed(NPCNeeds.NeedType.Fatigue):F0}";
                    stateInfo += $"\nMorale: {needs.GetNeed(NPCNeeds.NeedType.Morale):F0}";
                }
                UnityEditor.Handles.Label(labelPosition, stateInfo);
            }
            #endif
        }
        
        #region AI Action Implementations
        
        /// <summary>
        /// Seek food behavior
        /// </summary>
        IEnumerator SeekFood()
        {
            ChangeState(NPCState.Seeking);
            
            // Look for food in detection radius
            Collider[] foodSources = Physics.OverlapSphere(transform.position, detectionRadius, foodLayer);
            
            if (foodSources.Length > 0)
            {
                // Move to closest food source
                Transform closestFood = GetClosestTarget(foodSources);
                yield return StartCoroutine(MoveToTarget(closestFood.position));
                
                // Consume food
                if (Vector3.Distance(transform.position, closestFood.position) <= 2f)
                {
                    needs.ModifyNeed(NPCNeeds.NeedType.Hunger, 30f);
                    Debug.Log($"{npcName} found and consumed food!");
                    
                    // Destroy food object or mark as consumed
                    Destroy(closestFood.gameObject);
                }
            }
            else
            {
                // No food found, wander to search
                yield return StartCoroutine(WanderAI());
            }
            
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Rest behavior
        /// </summary>
        IEnumerator Rest()
        {
            ChangeState(NPCState.Resting);
            
            // Find safe spot near home
            Vector3 restSpot = homePosition + Random.insideUnitSphere * 3f;
            restSpot.y = transform.position.y;
            
            yield return StartCoroutine(MoveToTarget(restSpot));
            
            // Rest animation/behavior
            if (navMeshAgent != null)
                navMeshAgent.isStopped = true;
            
            float restTime = 0f;
            while (restTime < restDuration && needs.GetNeed(NPCNeeds.NeedType.Fatigue) < 80f)
            {
                needs.ModifyNeed(NPCNeeds.NeedType.Fatigue, 10f * Time.deltaTime);
                restTime += Time.deltaTime;
                yield return null;
            }
            
            if (navMeshAgent != null)
                navMeshAgent.isStopped = false;
            Debug.Log($"{npcName} finished resting.");
            
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Flee behavior
        /// </summary>
        IEnumerator Flee()
        {
            ChangeState(NPCState.Fleeing);
            
            // Find threats
            Collider[] threats = Physics.OverlapSphere(transform.position, detectionRadius, threatLayer);
            
            if (threats.Length > 0)
            {
                // Calculate flee direction (away from threats)
                Vector3 fleeDirection = Vector3.zero;
                foreach (var threat in threats)
                {
                    Vector3 directionFromThreat = transform.position - threat.transform.position;
                    fleeDirection += directionFromThreat.normalized;
                }
                
                fleeDirection = fleeDirection.normalized;
                Vector3 fleeTarget = transform.position + fleeDirection * wanderRadius;
                
                yield return StartCoroutine(MoveToTarget(fleeTarget));
                
                // Increase fear
                needs.ModifyNeed(NPCNeeds.NeedType.Fear, 20f);
                Debug.Log($"{npcName} fled from threats!");
            }
            
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Wander behavior for AI system
        /// </summary>
        IEnumerator WanderAI()
        {
            ChangeState(NPCState.Wandering);
            
            // Generate random wander target
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += homePosition;
            randomDirection.y = transform.position.y;
            
            wanderTarget = randomDirection;
            
            yield return StartCoroutine(MoveToTarget(wanderTarget));
            
            // Small morale boost from exploration
            needs.ModifyNeed(NPCNeeds.NeedType.Morale, 2f);
            
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Seek shelter behavior
        /// </summary>
        IEnumerator SeekShelter()
        {
            ChangeState(NPCState.Seeking);
            
            // Move towards home position for now
            yield return StartCoroutine(MoveToTarget(homePosition));
            
            // Reduce fear when in shelter
            needs.ModifyNeed(NPCNeeds.NeedType.Fear, -15f);
            Debug.Log($"{npcName} found shelter.");
            
            ChangeState(NPCState.Idle);
        }
        
        /// <summary>
        /// Socialize behavior
        /// </summary>
        IEnumerator Socialize()
        {
            ChangeState(NPCState.MovingToDestination);
            
            // Look for other NPCs
            NPCController[] nearbyNPCs = FindObjectsOfType<NPCController>();
            NPCController closestNPC = null;
            float closestDistance = float.MaxValue;
            
            foreach (var npc in nearbyNPCs)
            {
                if (npc != this)
                {
                    float distance = Vector3.Distance(transform.position, npc.transform.position);
                    if (distance < detectionRadius && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }
            
            if (closestNPC != null)
            {
                yield return StartCoroutine(MoveToTarget(closestNPC.transform.position));
                
                // Boost morale from socializing
                needs.ModifyNeed(NPCNeeds.NeedType.Morale, 10f);
                Debug.Log($"{npcName} socialized with {closestNPC.npcName}");
            }
            
            ChangeState(NPCState.Idle);
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Move to target position using coroutine
        /// </summary>
        IEnumerator MoveToTarget(Vector3 target)
        {
            if (navMeshAgent == null) yield break;
            
            // Find valid position on NavMesh
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
                
                while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
                {
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// Get closest target from array of colliders
        /// </summary>
        Transform GetClosestTarget(Collider[] targets)
        {
            Transform closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (var target in targets)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = target.transform;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Handle critical need events
        /// </summary>
        void HandleCriticalNeed(NeedType needType)
        {
            Debug.Log($"{npcName} has critical {needType}");
            
            // Force immediate action for critical needs
            switch (needType)
            {
                case NeedType.Health:
                    // Emergency behavior
                    if (currentAction != null) StopCoroutine(currentAction);
                    currentAction = StartCoroutine(SeekShelter());
                    break;
                    
                case NeedType.Fear:
                    if (currentAction != null) StopCoroutine(currentAction);
                    currentAction = StartCoroutine(Flee());
                    break;
            }
        }
        
        /// <summary>
        /// Handle need change events
        /// </summary>
        void HandleNeedChanged(NeedType needType, float newValue)
        {
            // Optional: React to need changes
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Assign job to NPC
        /// </summary>
        public void AssignJob(NPCJob job)
        {
            currentJob = job;
            Debug.Log($"{npcName} assigned to job: {job}");
        }
        
        /// <summary>
        /// Get current NPC state
        /// </summary>
        public NPCState GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Get home position
        /// </summary>
        public Vector3 GetHomePosition()
        {
            return homePosition;
        }
        
        #endregion
        
        void OnDestroy()
        {
            if (needs != null)
            {
                needs.OnNeedCritical -= HandleCriticalNeed;
                needs.OnNeedChanged -= HandleNeedChanged;
            }
            
            if (jobSystem != null)
            {
                jobSystem.OnJobStarted -= HandleJobStarted;
                jobSystem.OnJobCompleted -= HandleJobCompleted;
            }
        }
        
        #region Job System Integration
        
        /// <summary>
        /// Handle job started event
        /// </summary>
        void HandleJobStarted(NPCJob job)
        {
            currentJob = job;
            Debug.Log($"{npcName} started job: {job}");
        }
        
        /// <summary>
        /// Handle job completed event
        /// </summary>
        void HandleJobCompleted(NPCJob job)
        {
            Debug.Log($"{npcName} completed job: {job}");
        }
        
        #endregion
    }
}