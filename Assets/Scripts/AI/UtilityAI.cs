using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Sistema de Utility AI para toma de decisiones de NPCs.
    /// Evalúa acciones basándose en utilidad calculada a partir de necesidades y contexto.
    /// </summary>
    public class UtilityAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField] private float decisionInterval = 1f; // Seconds between decisions
        [SerializeField] private float minUtilityThreshold = 0.1f; // Minimum utility to consider action
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool logDecisions = false;

        private NPCNeeds npcNeeds;
        private List<AIAction> availableActions;
        private AIAction currentAction;
        private float lastDecisionTime;
        private string lastDecisionReason;

        private void Awake()
        {
            npcNeeds = GetComponent<NPCNeeds>();
            if (npcNeeds == null)
            {
                Debug.LogError($"UtilityAI on {gameObject.name} requires NPCNeeds component!");
            }

            InitializeActions();
        }

        private void Update()
        {
            if (Time.time - lastDecisionTime >= decisionInterval)
            {
                MakeDecision();
                lastDecisionTime = Time.time;
            }

            // Execute current action
            if (currentAction != null && currentAction.IsValid())
            {
                currentAction.Execute();
            }
        }

        private void InitializeActions()
        {
            availableActions = new List<AIAction>
            {
                new SeekFoodAction(this),
                new RestAction(this),
                new FleeAction(this),
                new WanderAction(this),
                new SeekShelterAction(this),
                new SocializeAction(this)
            };
        }

        private void MakeDecision()
        {
            if (npcNeeds == null) return;

            float bestUtility = 0f;
            AIAction bestAction = null;
            string decisionLog = "";

            foreach (var action in availableActions)
            {
                if (!action.CanExecute()) continue;

                float utility = action.CalculateUtility();
                
                if (logDecisions)
                {
                    decisionLog += $"{action.GetType().Name}: {utility:F2} | ";
                }

                if (utility > bestUtility && utility >= minUtilityThreshold)
                {
                    bestUtility = utility;
                    bestAction = action;
                }
            }

            // Only change action if new one is significantly better or current is invalid
            if (bestAction != currentAction)
            {
                if (currentAction != null)
                {
                    currentAction.OnExit();
                }

                currentAction = bestAction;
                lastDecisionReason = $"Chose {bestAction?.GetType().Name ?? "None"} (Utility: {bestUtility:F2})";

                if (currentAction != null)
                {
                    currentAction.OnEnter();
                }

                if (logDecisions)
                {
                    Debug.Log($"{gameObject.name}: {lastDecisionReason} | {decisionLog}");
                }
            }
        }

        public NPCNeeds GetNeeds() => npcNeeds;
        public AIAction GetCurrentAction() => currentAction;
        public string GetLastDecisionReason() => lastDecisionReason;

        /// <summary>
        /// Get the best action for the given NPC controller
        /// </summary>
        public AIAction GetBestAction(NPCController npcController)
        {
            if (npcNeeds == null) return null;

            float bestUtility = 0f;
            AIAction bestAction = null;

            foreach (var action in availableActions)
            {
                if (!action.CanExecute()) continue;

                float utility = action.CalculateUtility();

                if (utility > bestUtility && utility >= minUtilityThreshold)
                {
                    bestUtility = utility;
                    bestAction = action;
                }
            }

            return bestAction;
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(220, 10, 300, 150));
            GUILayout.Label($"AI: {gameObject.name}");
            GUILayout.Label($"Current Action: {currentAction?.GetType().Name ?? "None"}");
            GUILayout.Label($"Last Decision: {lastDecisionReason}");
            GUILayout.Label($"Decision Cooldown: {decisionInterval - (Time.time - lastDecisionTime):F1}s");
            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Clase base para todas las acciones de IA
    /// </summary>
    public abstract class AIAction
    {
        protected UtilityAI ai;
        protected NPCNeeds needs;
        protected Transform transform;

        public AIAction(UtilityAI utilityAI)
        {
            ai = utilityAI;
            needs = utilityAI.GetNeeds();
            transform = utilityAI.transform;
        }

        public abstract float CalculateUtility();
        public abstract bool CanExecute();
        public abstract void Execute();
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual bool IsValid() => true;
    }

    /// <summary>
    /// Acción para buscar comida cuando el NPC tiene hambre
    /// </summary>
    public class SeekFoodAction : AIAction
    {
        public SeekFoodAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            // Higher utility when hunger is low
            float hungerUtility = (100f - needs.hunger) / 100f;
            
            // Bonus if hunger is critical
            if (needs.IsHungerCritical)
                hungerUtility *= 2f;

            return Mathf.Clamp01(hungerUtility);
        }

        public override bool CanExecute()
        {
            return needs.hunger < 80f; // Only seek food if not full
        }

        public override void Execute()
        {
            // Simple implementation: slowly restore hunger
            needs.ModifyHunger(10f * Time.deltaTime);
            
            // TODO: Implement actual food seeking behavior (pathfinding to food sources)
        }
    }

    /// <summary>
    /// Acción para descansar cuando el NPC está fatigado
    /// </summary>
    public class RestAction : AIAction
    {
        public RestAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            float fatigueUtility = (100f - needs.fatigue) / 100f;
            
            if (needs.IsFatigueCritical)
                fatigueUtility *= 2f;

            return Mathf.Clamp01(fatigueUtility);
        }

        public override bool CanExecute()
        {
            return needs.fatigue < 80f;
        }

        public override void Execute()
        {
            // Restore fatigue while resting
            needs.ModifyFatigue(15f * Time.deltaTime);
            
            // TODO: Implement rest animation/behavior
        }
    }

    /// <summary>
    /// Acción para huir cuando el NPC tiene miedo
    /// </summary>
    public class FleeAction : AIAction
    {
        public FleeAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            float fearUtility = needs.fear / 100f;
            
            // Also consider low morale
            if (needs.IsMoraleCritical)
                fearUtility += 0.3f;

            return Mathf.Clamp01(fearUtility);
        }

        public override bool CanExecute()
        {
            return needs.fear > 30f || needs.IsMoraleCritical;
        }

        public override void Execute()
        {
            // Simple flee behavior - move away from threats
            Vector3 fleeDirection = Random.insideUnitSphere;
            fleeDirection.y = 0;
            transform.Translate(fleeDirection.normalized * 2f * Time.deltaTime);
            
            // Gradually reduce fear while fleeing
            needs.ModifyFear(-5f * Time.deltaTime);
            
            // TODO: Implement proper threat detection and avoidance
        }
    }

    /// <summary>
    /// Acción por defecto para vagar cuando no hay necesidades urgentes
    /// </summary>
    public class WanderAction : AIAction
    {
        private Vector3 wanderTarget;
        private float wanderRadius = 10f;

        public WanderAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            // Base utility for wandering (always available but low priority)
            float baseUtility = 0.2f;
            
            // Higher utility if no urgent needs
            if (!needs.IsHungerCritical && !needs.IsFatigueCritical && 
                !needs.IsFearCritical && !needs.IsMoraleCritical)
            {
                baseUtility = 0.4f;
            }

            return baseUtility;
        }

        public override bool CanExecute()
        {
            return true; // Always can wander
        }

        public override void OnEnter()
        {
            // Set new wander target
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        public override void Execute()
        {
            // Move towards wander target
            Vector3 direction = (wanderTarget - transform.position).normalized;
            transform.Translate(direction * 1f * Time.deltaTime);

            // If close to target, pick new one
            if (Vector3.Distance(transform.position, wanderTarget) < 1f)
            {
                OnEnter(); // Pick new target
            }
        }
    }

    /// <summary>
    /// Acción para buscar refugio cuando la salud es baja o hay infección
    /// </summary>
    public class SeekShelterAction : AIAction
    {
        public SeekShelterAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            float healthUtility = (100f - needs.health) / 100f;
            float infectionUtility = needs.infection / 100f;
            
            return Mathf.Clamp01(Mathf.Max(healthUtility, infectionUtility));
        }

        public override bool CanExecute()
        {
            return needs.IsHealthCritical || needs.IsInfectionCritical;
        }

        public override void Execute()
        {
            // Simple shelter seeking - reduce infection/improve health slowly
            if (needs.IsInfectionCritical)
                needs.ModifyInfection(-2f * Time.deltaTime);
            
            if (needs.IsHealthCritical)
                needs.ModifyHealth(1f * Time.deltaTime);
            
            // TODO: Implement pathfinding to shelter locations
        }
    }

    /// <summary>
    /// Acción para socializar y mejorar la moral
    /// </summary>
    public class SocializeAction : AIAction
    {
        public SocializeAction(UtilityAI ai) : base(ai) { }

        public override float CalculateUtility()
        {
            float moraleUtility = (100f - needs.morale) / 100f;
            
            if (needs.IsMoraleCritical)
                moraleUtility *= 1.5f;

            return Mathf.Clamp01(moraleUtility * 0.6f); // Lower priority than survival needs
        }

        public override bool CanExecute()
        {
            return needs.morale < 70f;
        }

        public override void Execute()
        {
            // Improve morale through socialization
            needs.ModifyMorale(5f * Time.deltaTime);
            
            // TODO: Implement finding other NPCs to socialize with
        }
    }
}