using UnityEngine;
using TheLastBreath.Factions;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Component that handles faction-based behavior for individual NPCs
    /// </summary>
    public class NPCFactionBehavior : MonoBehaviour
    {
        [Header("Faction Assignment")]
        [SerializeField] private FactionData assignedFaction;
        [SerializeField] private bool overrideFactionBehavior = false;
        
        [Header("Individual Behavior Overrides")]
        [SerializeField] private FactionBehavior personalBehaviorTowardsPlayer = FactionBehavior.Neutral;
        [SerializeField] private int personalReputationModifier = 0;
        
        [Header("Combat Settings")]
        [SerializeField] private bool canEngageInCombat = true;
        [SerializeField] private float aggressionMultiplier = 1f;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private LayerMask enemyLayers = -1;
        
        // Events
        public System.Action<NPCFactionBehavior, FactionData> OnFactionChanged;
        public System.Action<NPCFactionBehavior, GameObject> OnEnemyDetected;
        public System.Action<NPCFactionBehavior, GameObject> OnAllyDetected;
        
        // Runtime data
        private NPCController npcController;
        private NPCNeeds npcNeeds;
        private Collider[] detectionBuffer = new Collider[10];
        private float lastDetectionTime = 0f;
        private float detectionInterval = 1f;
        
        // Properties
        public FactionData AssignedFaction => assignedFaction;
        public bool HasFaction => assignedFaction != null;
        public FactionBehavior CurrentBehaviorTowardsPlayer => GetBehaviorTowardsPlayer();
        public int EffectivePlayerReputation => GetEffectivePlayerReputation();
        
        void Start()
        {
            InitializeComponents();
            
            if (HasFaction)
            {
                Debug.Log($"{name} assigned to faction: {assignedFaction.factionName}");
            }
        }
        
        void InitializeComponents()
        {
            npcController = GetComponent<NPCController>();
            npcNeeds = GetComponent<NPCNeeds>();
            
            if (npcController == null)
                Debug.LogWarning($"NPCFactionBehavior on {name} requires NPCController component");
        }
        
        void Update()
        {
            if (canEngageInCombat && Time.time - lastDetectionTime >= detectionInterval)
            {
                DetectNearbyEntities();
                lastDetectionTime = Time.time;
            }
        }
        
        #region Faction Management
        
        /// <summary>
        /// Assign this NPC to a faction
        /// </summary>
        public void AssignToFaction(FactionData faction)
        {
            var oldFaction = assignedFaction;
            assignedFaction = faction;
            
            OnFactionChanged?.Invoke(this, oldFaction);
            
            Debug.Log($"{name} assigned to faction: {faction?.factionName ?? "None"}");
        }
        
        /// <summary>
        /// Get behavior towards player considering faction and personal modifiers
        /// </summary>
        public FactionBehavior GetBehaviorTowardsPlayer()
        {
            if (overrideFactionBehavior)
                return personalBehaviorTowardsPlayer;
            
            if (!HasFaction)
                return FactionBehavior.Neutral;
            
            if (FactionManager.Instance != null)
                return FactionManager.Instance.GetFactionBehaviorTowardsPlayer(assignedFaction);
            
            return assignedFaction.defaultBehaviorToStrangers;
        }
        
        /// <summary>
        /// Get effective player reputation considering personal modifiers
        /// </summary>
        public int GetEffectivePlayerReputation()
        {
            if (!HasFaction || FactionManager.Instance == null)
                return personalReputationModifier;
            
            int baseReputation = FactionManager.Instance.GetPlayerReputation(assignedFaction);
            return baseReputation + personalReputationModifier;
        }
        
        /// <summary>
        /// Check if this NPC should attack player on sight
        /// </summary>
        public bool ShouldAttackPlayerOnSight()
        {
            if (!canEngageInCombat) return false;
            
            var behavior = GetBehaviorTowardsPlayer();
            var reputation = GetEffectivePlayerReputation();
            
            return behavior == FactionBehavior.Aggressive && reputation < -50;
        }
        
        /// <summary>
        /// Check if this NPC will trade with player
        /// </summary>
        public bool WillTradeWithPlayer()
        {
            var reputation = GetEffectivePlayerReputation();
            return reputation > -30; // Won't trade if very hostile
        }
        
        #endregion
        
        #region Entity Detection & Relations
        
        void DetectNearbyEntities()
        {
            Vector3 position = transform.position;
            int hitCount = Physics.OverlapSphereNonAlloc(position, detectionRange, detectionBuffer, enemyLayers);
            
            for (int i = 0; i < hitCount; i++)
            {
                Collider other = detectionBuffer[i];
                if (other.gameObject == gameObject) continue;
                
                // Check for other NPCs
                NPCFactionBehavior otherNPC = other.GetComponent<NPCFactionBehavior>();
                if (otherNPC != null)
                {
                    ProcessNPCDetection(otherNPC);
                    continue;
                }
                
                // Check for player
                if (other.CompareTag("Player"))
                {
                    ProcessPlayerDetection(other.gameObject);
                    continue;
                }
            }
        }
        
        void ProcessNPCDetection(NPCFactionBehavior otherNPC)
        {
            if (!HasFaction || !otherNPC.HasFaction) return;
            
            FactionRelationship relationship = assignedFaction.GetRelationshipWith(otherNPC.AssignedFaction);
            
            if (relationship != null)
            {
                switch (relationship.relationshipType)
                {
                    case FactionRelationType.Enemy:
                        if (canEngageInCombat && ShouldEngageEnemy(otherNPC))
                        {
                            OnEnemyDetected?.Invoke(this, otherNPC.gameObject);
                            InitiateCombat(otherNPC.gameObject);
                        }
                        break;
                        
                    case FactionRelationType.Allied:
                        OnAllyDetected?.Invoke(this, otherNPC.gameObject);
                        // Could add ally assistance logic here
                        break;
                        
                    case FactionRelationType.Neutral:
                        // Neutral behavior - no immediate action
                        break;
                }
            }
        }
        
        void ProcessPlayerDetection(GameObject player)
        {
            if (ShouldAttackPlayerOnSight())
            {
                OnEnemyDetected?.Invoke(this, player);
                InitiateCombat(player);
            }
            else
            {
                // Could add neutral player interaction logic here
                var behavior = GetBehaviorTowardsPlayer();
                if (behavior == FactionBehavior.Friendly)
                {
                    OnAllyDetected?.Invoke(this, player);
                }
            }
        }
        
        bool ShouldEngageEnemy(NPCFactionBehavior enemy)
        {
            if (!canEngageInCombat) return false;
            
            // Check morale and fear
            if (npcNeeds != null)
            {
                if (npcNeeds.Fear > 70f) return false; // Too scared to fight
                if (npcNeeds.Morale < 30f) return false; // Too demoralized
                if (npcNeeds.Health < 50f) return false; // Too injured
            }
            
            // Check faction combat settings
            if (HasFaction)
            {
                float effectiveMorale = assignedFaction.combatMorale * aggressionMultiplier;
                return effectiveMorale > 50f;
            }
            
            return true;
        }
        
        void InitiateCombat(GameObject target)
        {
            if (npcController != null)
            {
                // Set NPC to aggressive state and target
                npcController.FleeFrom(target.transform.position); // Temporary - should be attack logic
                
                Debug.Log($"{name} ({assignedFaction?.factionName}) engaging {target.name} in combat!");
            }
        }
        
        #endregion
        
        #region Reputation Effects
        
        /// <summary>
        /// Handle reputation change effects on this NPC
        /// </summary>
        public void OnReputationChanged(FactionData faction, int oldRep, int newRep)
        {
            if (faction != assignedFaction) return;
            
            Debug.Log($"{name} reacting to reputation change: {oldRep} -> {newRep}");
            
            // Update behavior based on new reputation
            var newBehavior = GetBehaviorTowardsPlayer();
            
            // Could trigger specific reactions here
            if (newRep < -50 && oldRep >= -50)
            {
                Debug.Log($"{name} is now hostile to player!");
            }
            else if (newRep > 50 && oldRep <= 50)
            {
                Debug.Log($"{name} is now friendly to player!");
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Set personal behavior override
        /// </summary>
        public void SetPersonalBehavior(FactionBehavior behavior, bool override = true)
        {
            personalBehaviorTowardsPlayer = behavior;
            overrideFactionBehavior = override;
        }
        
        /// <summary>
        /// Add personal reputation modifier
        /// </summary>
        public void ModifyPersonalReputation(int change)
        {
            personalReputationModifier += change;
            personalReputationModifier = Mathf.Clamp(personalReputationModifier, -50, 50);
        }
        
        /// <summary>
        /// Get faction trade price modifier
        /// </summary>
        public float GetTradePriceModifier()
        {
            if (!HasFaction || FactionManager.Instance == null)
                return 1f;
            
            return FactionManager.Instance.GetTradePriceModifier(assignedFaction);
        }
        
        #endregion
        
        void OnEnable()
        {
            if (FactionManager.Instance != null)
            {
                FactionManager.OnReputationChanged += OnReputationChanged;
            }
        }
        
        void OnDisable()
        {
            if (FactionManager.Instance != null)
            {
                FactionManager.OnReputationChanged -= OnReputationChanged;
            }
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw faction color indicator
            if (HasFaction)
            {
                Gizmos.color = GetFactionColor();
                Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
            }
        }
        
        Color GetFactionColor()
        {
            if (!HasFaction) return Color.white;
            
            switch (assignedFaction.defaultAlignment)
            {
                case FactionAlignment.Friendly: return Color.green;
                case FactionAlignment.Hostile: return Color.red;
                case FactionAlignment.Neutral: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}