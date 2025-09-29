using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastBreath.Factions
{
    /// <summary>
    /// Central manager for faction system, handles reputation and relationships
    /// </summary>
    public class FactionManager : MonoBehaviour
    {
        [Header("Faction Configuration")]
        [SerializeField] private List<FactionData> allFactions = new List<FactionData>();
        [SerializeField] private bool debugMode = false;
        
        // Events
        public static event Action<FactionData, int, int> OnReputationChanged;
        public static event Action<FactionData, FactionData, FactionRelationType> OnRelationshipChanged;
        
        // Singleton
        public static FactionManager Instance { get; private set; }
        
        // Runtime data
        private Dictionary<FactionData, int> playerReputation = new Dictionary<FactionData, int>();
        private Dictionary<string, FactionData> factionLookup = new Dictionary<string, FactionData>();
        
        void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFactionSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void InitializeFactionSystem()
        {
            // Initialize player reputation for all factions
            foreach (var faction in allFactions)
            {
                if (faction != null)
                {
                    playerReputation[faction] = faction.initialPlayerReputation;
                    factionLookup[faction.factionName] = faction;
                    
                    if (debugMode)
                        Debug.Log($"Initialized faction: {faction.factionName} with reputation: {faction.initialPlayerReputation}");
                }
            }
            
            Debug.Log($"Faction system initialized with {allFactions.Count} factions");
        }
        
        #region Reputation Management
        
        /// <summary>
        /// Get current player reputation with faction
        /// </summary>
        public int GetPlayerReputation(FactionData faction)
        {
            return playerReputation.ContainsKey(faction) ? playerReputation[faction] : 0;
        }
        
        /// <summary>
        /// Get current player reputation with faction by name
        /// </summary>
        public int GetPlayerReputation(string factionName)
        {
            var faction = GetFactionByName(factionName);
            return faction != null ? GetPlayerReputation(faction) : 0;
        }
        
        /// <summary>
        /// Modify player reputation with faction
        /// </summary>
        public void ModifyReputation(FactionData faction, int change)
        {
            if (faction == null) return;
            
            int oldReputation = GetPlayerReputation(faction);
            int newReputation = Mathf.Clamp(oldReputation + change, -100, 100);
            
            playerReputation[faction] = newReputation;
            
            if (debugMode)
                Debug.Log($"Reputation with {faction.factionName}: {oldReputation} -> {newReputation} (change: {change})");
            
            OnReputationChanged?.Invoke(faction, oldReputation, newReputation);
            
            // Check for reputation level changes
            var oldLevel = faction.GetReputationLevel(oldReputation);
            var newLevel = faction.GetReputationLevel(newReputation);
            
            if (oldLevel != newLevel)
            {
                Debug.Log($"Reputation level with {faction.factionName} changed from {oldLevel} to {newLevel}");
            }
        }
        
        /// <summary>
        /// Modify player reputation with faction by name
        /// </summary>
        public void ModifyReputation(string factionName, int change)
        {
            var faction = GetFactionByName(factionName);
            if (faction != null)
                ModifyReputation(faction, change);
        }
        
        /// <summary>
        /// Get reputation level with faction
        /// </summary>
        public ReputationLevel GetReputationLevel(FactionData faction)
        {
            int reputation = GetPlayerReputation(faction);
            return faction.GetReputationLevel(reputation);
        }
        
        #endregion
        
        #region Faction Queries
        
        /// <summary>
        /// Get faction by name
        /// </summary>
        public FactionData GetFactionByName(string name)
        {
            return factionLookup.ContainsKey(name) ? factionLookup[name] : null;
        }
        
        /// <summary>
        /// Get all factions
        /// </summary>
        public List<FactionData> GetAllFactions()
        {
            return new List<FactionData>(allFactions);
        }
        
        /// <summary>
        /// Get factions with specific alignment
        /// </summary>
        public List<FactionData> GetFactionsByAlignment(FactionAlignment alignment)
        {
            List<FactionData> result = new List<FactionData>();
            
            foreach (var faction in allFactions)
            {
                if (faction.defaultAlignment == alignment)
                    result.Add(faction);
            }
            
            return result;
        }
        
        /// <summary>
        /// Get factions that control specific POI
        /// </summary>
        public FactionData GetFactionControllingPOI(string poiName)
        {
            foreach (var faction in allFactions)
            {
                if (faction.controlledPOIs.Contains(poiName))
                    return faction;
            }
            
            return null;
        }
        
        #endregion
        
        #region Behavior & Trading
        
        /// <summary>
        /// Get faction behavior towards player
        /// </summary>
        public FactionBehavior GetFactionBehaviorTowardsPlayer(FactionData faction)
        {
            int reputation = GetPlayerReputation(faction);
            return faction.GetBehaviorTowardsPlayer(reputation);
        }
        
        /// <summary>
        /// Get trade price modifier for faction
        /// </summary>
        public float GetTradePriceModifier(FactionData faction)
        {
            int reputation = GetPlayerReputation(faction);
            return faction.GetTradePriceModifier(reputation);
        }
        
        /// <summary>
        /// Check if faction will attack player on sight
        /// </summary>
        public bool WillAttackOnSight(FactionData faction)
        {
            var behavior = GetFactionBehaviorTowardsPlayer(faction);
            return behavior == FactionBehavior.Aggressive && GetReputationLevel(faction) == ReputationLevel.Hostile;
        }
        
        /// <summary>
        /// Check if faction will trade with player
        /// </summary>
        public bool WillTrade(FactionData faction)
        {
            var reputationLevel = GetReputationLevel(faction);
            return reputationLevel >= ReputationLevel.Unfriendly; // Won't trade if hostile
        }
        
        #endregion
        
        #region Faction Relations
        
        /// <summary>
        /// Get relationship between two factions
        /// </summary>
        public FactionRelationship GetFactionRelationship(FactionData faction1, FactionData faction2)
        {
            if (faction1 == null || faction2 == null) return null;
            
            return faction1.GetRelationshipWith(faction2);
        }
        
        /// <summary>
        /// Check if two factions are enemies
        /// </summary>
        public bool AreFactionsEnemies(FactionData faction1, FactionData faction2)
        {
            var relationship = GetFactionRelationship(faction1, faction2);
            return relationship != null && relationship.relationshipType == FactionRelationType.Enemy;
        }
        
        /// <summary>
        /// Check if two factions are allies
        /// </summary>
        public bool AreFactionsAllies(FactionData faction1, FactionData faction2)
        {
            var relationship = GetFactionRelationship(faction1, faction2);
            return relationship != null && relationship.relationshipType == FactionRelationType.Allied;
        }
        
        #endregion
        
        #region Player Actions & Reputation Changes
        
        /// <summary>
        /// Handle player killing faction member
        /// </summary>
        public void OnPlayerKilledFactionMember(FactionData faction, bool wasHostile = false)
        {
            if (faction == null) return;
            
            int reputationChange = wasHostile ? -5 : -15; // Less penalty if they were hostile first
            ModifyReputation(faction, reputationChange);
            
            // Affect allied factions
            foreach (var otherFaction in allFactions)
            {
                if (otherFaction != faction && AreFactionsAllies(otherFaction, faction))
                {
                    ModifyReputation(otherFaction, reputationChange / 2);
                }
            }
        }
        
        /// <summary>
        /// Handle player trading with faction
        /// </summary>
        public void OnPlayerTradedWithFaction(FactionData faction, float tradeValue)
        {
            if (faction == null) return;
            
            int reputationChange = Mathf.RoundToInt(tradeValue / 100f); // 1 rep per 100 value
            reputationChange = Mathf.Clamp(reputationChange, 1, 5); // Min 1, max 5 per trade
            
            ModifyReputation(faction, reputationChange);
        }
        
        /// <summary>
        /// Handle player helping faction
        /// </summary>
        public void OnPlayerHelpedFaction(FactionData faction, int helpAmount = 10)
        {
            if (faction == null) return;
            
            ModifyReputation(faction, helpAmount);
        }
        
        /// <summary>
        /// Handle player stealing from faction
        /// </summary>
        public void OnPlayerStoleFromFaction(FactionData faction, float stolenValue)
        {
            if (faction == null) return;
            
            int reputationChange = -Mathf.RoundToInt(stolenValue / 50f); // Harsher penalty for stealing
            reputationChange = Mathf.Clamp(reputationChange, -20, -5);
            
            ModifyReputation(faction, reputationChange);
        }
        
        #endregion
        
        #region Debug & Utilities
        
        /// <summary>
        /// Get debug info for all factions
        /// </summary>
        public string GetDebugInfo()
        {
            string info = "=== FACTION STATUS ===\n";
            
            foreach (var faction in allFactions)
            {
                int reputation = GetPlayerReputation(faction);
                var level = GetReputationLevel(faction);
                var behavior = GetFactionBehaviorTowardsPlayer(faction);
                
                info += $"{faction.factionName}: Rep {reputation} ({level}) - {behavior}\n";
            }
            
            return info;
        }
        
        /// <summary>
        /// Reset all reputation to initial values
        /// </summary>
        [ContextMenu("Reset All Reputation")]
        public void ResetAllReputation()
        {
            foreach (var faction in allFactions)
            {
                playerReputation[faction] = faction.initialPlayerReputation;
            }
            
            Debug.Log("All faction reputation reset to initial values");
        }
        
        #endregion
        
        void OnGUI()
        {
            if (debugMode && Application.isPlaying)
            {
                GUI.Box(new Rect(10, 10, 300, 200), "Faction Debug");
                
                GUILayout.BeginArea(new Rect(15, 35, 290, 170));
                
                foreach (var faction in allFactions)
                {
                    int reputation = GetPlayerReputation(faction);
                    var level = GetReputationLevel(faction);
                    
                    GUILayout.Label($"{faction.factionName}: {reputation} ({level})");
                }
                
                GUILayout.EndArea();
            }
        }
    }
}