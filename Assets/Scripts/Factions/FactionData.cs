using System.Collections.Generic;
using UnityEngine;

namespace TheLastBreath.Factions
{
    /// <summary>
    /// ScriptableObject that defines faction data and behavior
    /// </summary>
    [CreateAssetMenu(fileName = "New Faction", menuName = "The Last Breath/Faction Data")]
    public class FactionData : ScriptableObject
    {
        [Header("Basic Information")]
        public string factionName;
        [TextArea(3, 5)]
        public string description;
        public Sprite factionIcon;
        public Color factionColor = Color.white;
        
        [Header("Alignment & Behavior")]
        public FactionAlignment defaultAlignment = FactionAlignment.Neutral;
        public FactionBehavior defaultBehaviorToStrangers = FactionBehavior.Cautious;
        
        [Header("Reputation System")]
        [Range(-100, 100)]
        public int initialPlayerReputation = 0;
        public ReputationThresholds reputationThresholds;
        
        [Header("Resources & Economy")]
        public List<ResourcePreference> preferredResources = new List<ResourcePreference>();
        public float baseTradePriceModifier = 1.0f;
        
        [Header("Territory & Presence")]
        public List<string> controlledPOIs = new List<string>();
        public bool hasPatrols = true;
        public bool hasCaravans = false;
        public float territoryInfluence = 1.0f;
        
        [Header("Combat & Relations")]
        public CombatStance defaultCombatStance = CombatStance.Defensive;
        public float combatMorale = 50f;
        public List<FactionRelationship> relationships = new List<FactionRelationship>();
        
        [Header("Special Traits")]
        public List<FactionTrait> traits = new List<FactionTrait>();
        
        /// <summary>
        /// Get reputation level based on current reputation value
        /// </summary>
        public ReputationLevel GetReputationLevel(int reputation)
        {
            if (reputation >= reputationThresholds.allied)
                return ReputationLevel.Allied;
            else if (reputation >= reputationThresholds.friendly)
                return ReputationLevel.Friendly;
            else if (reputation >= reputationThresholds.neutral)
                return ReputationLevel.Neutral;
            else if (reputation >= reputationThresholds.unfriendly)
                return ReputationLevel.Unfriendly;
            else
                return ReputationLevel.Hostile;
        }
        
        /// <summary>
        /// Get trade price modifier based on reputation
        /// </summary>
        public float GetTradePriceModifier(int reputation)
        {
            ReputationLevel level = GetReputationLevel(reputation);
            
            switch (level)
            {
                case ReputationLevel.Allied:
                    return baseTradePriceModifier * 0.8f; // 20% discount
                case ReputationLevel.Friendly:
                    return baseTradePriceModifier * 0.9f; // 10% discount
                case ReputationLevel.Neutral:
                    return baseTradePriceModifier;
                case ReputationLevel.Unfriendly:
                    return baseTradePriceModifier * 1.2f; // 20% surcharge
                case ReputationLevel.Hostile:
                    return baseTradePriceModifier * 1.5f; // 50% surcharge (if they trade at all)
                default:
                    return baseTradePriceModifier;
            }
        }
        
        /// <summary>
        /// Get behavior towards player based on reputation
        /// </summary>
        public FactionBehavior GetBehaviorTowardsPlayer(int reputation)
        {
            ReputationLevel level = GetReputationLevel(reputation);
            
            switch (level)
            {
                case ReputationLevel.Allied:
                    return FactionBehavior.Friendly;
                case ReputationLevel.Friendly:
                    return FactionBehavior.Welcoming;
                case ReputationLevel.Neutral:
                    return defaultBehaviorToStrangers;
                case ReputationLevel.Unfriendly:
                    return FactionBehavior.Suspicious;
                case ReputationLevel.Hostile:
                    return FactionBehavior.Aggressive;
                default:
                    return defaultBehaviorToStrangers;
            }
        }
        
        /// <summary>
        /// Check if faction has specific trait
        /// </summary>
        public bool HasTrait(FactionTrait trait)
        {
            return traits.Contains(trait);
        }
        
        /// <summary>
        /// Get relationship with another faction
        /// </summary>
        public FactionRelationship GetRelationshipWith(FactionData otherFaction)
        {
            foreach (var relationship in relationships)
            {
                if (relationship.targetFaction == otherFaction)
                    return relationship;
            }
            
            // Default neutral relationship
            return new FactionRelationship
            {
                targetFaction = otherFaction,
                relationshipType = FactionRelationType.Neutral,
                relationshipStrength = 0
            };
        }
    }
    
    #region Enums and Data Structures
    
    public enum FactionAlignment
    {
        Friendly,
        Neutral,
        Hostile
    }
    
    public enum FactionBehavior
    {
        Neutral,
        Friendly,
        Welcoming,
        Cautious,
        Suspicious,
        Aggressive
    }
    
    public enum ReputationLevel
    {
        Hostile = -2,
        Unfriendly = -1,
        Neutral = 0,
        Friendly = 1,
        Allied = 2
    }
    
    public enum CombatStance
    {
        Passive,
        Defensive,
        Aggressive
    }
    
    public enum FactionRelationType
    {
        Allied,
        Friendly,
        Neutral,
        Rival,
        Enemy
    }
    
    public enum FactionTrait
    {
        Nomadic,
        Technological,
        Militaristic,
        Peaceful,
        Traders,
        Raiders,
        Survivors,
        Religious,
        Isolationist,
        Expansionist
    }
    
    [System.Serializable]
    public class ReputationThresholds
    {
        [Range(-100, 100)]
        public int hostile = -50;
        [Range(-100, 100)]
        public int unfriendly = -20;
        [Range(-100, 100)]
        public int neutral = 20;
        [Range(-100, 100)]
        public int friendly = 50;
        [Range(-100, 100)]
        public int allied = 80;
    }
    
    [System.Serializable]
    public class ResourcePreference
    {
        public string resourceName;
        public float preferenceMultiplier = 1.0f;
        public bool isEssential = false;
    }
    
    [System.Serializable]
    public class FactionRelationship
    {
        public FactionData targetFaction;
        public FactionRelationType relationshipType = FactionRelationType.Neutral;
        [Range(-100, 100)]
        public int relationshipStrength = 0;
        public bool canTrade = true;
        public bool willAttackOnSight = false;
    }
    
    #endregion
}
 }