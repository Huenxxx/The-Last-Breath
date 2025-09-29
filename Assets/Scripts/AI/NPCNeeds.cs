using UnityEngine;

namespace TheLastBreath.AI
{
    /// <summary>
    /// Enum for different types of needs
    /// </summary>
    public enum NeedType
    {
        Hunger,
        Fatigue,
        Morale,
        Fear,
        Health,
        Infection
    }

    /// <summary>
    /// Sistema de necesidades para NPCs que controla valores como hambre, fatiga, moral, etc.
    /// Los valores decaen con el tiempo y afectan el comportamiento del NPC.
    /// </summary>
    [System.Serializable]
    public class NPCNeeds : MonoBehaviour
    {
        [Header("Current Need Values (0-100)")]
        [Range(0f, 100f)] public float hunger = 50f;
        [Range(0f, 100f)] public float fatigue = 50f;
        [Range(0f, 100f)] public float morale = 50f;
        [Range(0f, 100f)] public float fear = 0f;
        [Range(0f, 100f)] public float health = 100f;
        [Range(0f, 100f)] public float infection = 0f;

        [Header("Decay Rates (per second)")]
        [SerializeField] private float hungerDecayRate = 0.5f;
        [SerializeField] private float fatigueDecayRate = 0.3f;
        [SerializeField] private float moraleDecayRate = 0.1f;
        [SerializeField] private float fearDecayRate = -0.2f; // Fear decreases over time
        [SerializeField] private float healthDecayRate = 0f; // Health doesn't decay naturally
        [SerializeField] private float infectionDecayRate = -0.1f; // Infection can heal slowly

        [Header("Critical Thresholds")]
        [SerializeField] private float criticalHungerThreshold = 20f;
        [SerializeField] private float criticalFatigueThreshold = 20f;
        [SerializeField] private float criticalMoraleThreshold = 20f;
        [SerializeField] private float criticalFearThreshold = 80f;
        [SerializeField] private float criticalHealthThreshold = 20f;
        [SerializeField] private float criticalInfectionThreshold = 80f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        // Events for need changes
        public System.Action<float> OnHungerChanged;
        public System.Action<float> OnFatigueChanged;
        public System.Action<float> OnMoraleChanged;
        public System.Action<float> OnFearChanged;
        public System.Action<float> OnHealthChanged;
        public System.Action<float> OnInfectionChanged;

        // Critical state events
        public System.Action OnCriticalHunger;
        public System.Action OnCriticalFatigue;
        public System.Action OnCriticalMorale;
        public System.Action OnCriticalFear;
        public System.Action OnCriticalHealth;
        public System.Action OnCriticalInfection;

        // Generic events for external systems
        public System.Action<NeedType> OnNeedCritical;
        public System.Action<NeedType, float> OnNeedChanged;

        private void Update()
        {
            UpdateNeeds();
            CheckCriticalStates();
        }

        private void UpdateNeeds()
        {
            float deltaTime = Time.deltaTime;

            // Update hunger
            float newHunger = Mathf.Clamp(hunger - (hungerDecayRate * deltaTime), 0f, 100f);
            if (newHunger != hunger)
            {
                hunger = newHunger;
                OnHungerChanged?.Invoke(hunger);
            }

            // Update fatigue
            float newFatigue = Mathf.Clamp(fatigue - (fatigueDecayRate * deltaTime), 0f, 100f);
            if (newFatigue != fatigue)
            {
                fatigue = newFatigue;
                OnFatigueChanged?.Invoke(fatigue);
            }

            // Update morale
            float newMorale = Mathf.Clamp(morale - (moraleDecayRate * deltaTime), 0f, 100f);
            if (newMorale != morale)
            {
                morale = newMorale;
                OnMoraleChanged?.Invoke(morale);
            }

            // Update fear (decreases over time)
            float newFear = Mathf.Clamp(fear - (fearDecayRate * deltaTime), 0f, 100f);
            if (newFear != fear)
            {
                fear = newFear;
                OnFearChanged?.Invoke(fear);
            }

            // Update health
            float newHealth = Mathf.Clamp(health - (healthDecayRate * deltaTime), 0f, 100f);
            if (newHealth != health)
            {
                health = newHealth;
                OnHealthChanged?.Invoke(health);
            }

            // Update infection
            float newInfection = Mathf.Clamp(infection - (infectionDecayRate * deltaTime), 0f, 100f);
            if (newInfection != infection)
            {
                infection = newInfection;
                OnInfectionChanged?.Invoke(infection);
            }
        }

        private void CheckCriticalStates()
        {
            if (hunger <= criticalHungerThreshold)
            {
                OnCriticalHunger?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Hunger);
            }

            if (fatigue <= criticalFatigueThreshold)
            {
                OnCriticalFatigue?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Fatigue);
            }

            if (morale <= criticalMoraleThreshold)
            {
                OnCriticalMorale?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Morale);
            }

            if (fear >= criticalFearThreshold)
            {
                OnCriticalFear?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Fear);
            }

            if (health <= criticalHealthThreshold)
            {
                OnCriticalHealth?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Health);
            }

            if (infection >= criticalInfectionThreshold)
            {
                OnCriticalInfection?.Invoke();
                OnNeedCritical?.Invoke(NeedType.Infection);
            }
        }

        // Generic method to modify needs by type
        public void ModifyNeed(NeedType needType, float amount)
        {
            switch (needType)
            {
                case NeedType.Hunger:
                    ModifyHunger(amount);
                    break;
                case NeedType.Fatigue:
                    ModifyFatigue(amount);
                    break;
                case NeedType.Morale:
                    ModifyMorale(amount);
                    break;
                case NeedType.Fear:
                    ModifyFear(amount);
                    break;
                case NeedType.Health:
                    ModifyHealth(amount);
                    break;
                case NeedType.Infection:
                    ModifyInfection(amount);
                    break;
            }
        }

        // Public methods to modify needs
        public void ModifyHunger(float amount)
        {
            hunger = Mathf.Clamp(hunger + amount, 0f, 100f);
            OnHungerChanged?.Invoke(hunger);
            OnNeedChanged?.Invoke(NeedType.Hunger, hunger);
        }

        public void ModifyFatigue(float amount)
        {
            fatigue = Mathf.Clamp(fatigue + amount, 0f, 100f);
            OnFatigueChanged?.Invoke(fatigue);
            OnNeedChanged?.Invoke(NeedType.Fatigue, fatigue);
        }

        public void ModifyMorale(float amount)
        {
            morale = Mathf.Clamp(morale + amount, 0f, 100f);
            OnMoraleChanged?.Invoke(morale);
            OnNeedChanged?.Invoke(NeedType.Morale, morale);
        }

        public void ModifyFear(float amount)
        {
            fear = Mathf.Clamp(fear + amount, 0f, 100f);
            OnFearChanged?.Invoke(fear);
            OnNeedChanged?.Invoke(NeedType.Fear, fear);
        }

        public void ModifyHealth(float amount)
        {
            health = Mathf.Clamp(health + amount, 0f, 100f);
            OnHealthChanged?.Invoke(health);
            OnNeedChanged?.Invoke(NeedType.Health, health);
        }

        public void ModifyInfection(float amount)
        {
            infection = Mathf.Clamp(infection + amount, 0f, 100f);
            OnInfectionChanged?.Invoke(infection);
            OnNeedChanged?.Invoke(NeedType.Infection, infection);
        }

        // Getters for critical states
        public bool IsHungerCritical => hunger <= criticalHungerThreshold;
        public bool IsFatigueCritical => fatigue <= criticalFatigueThreshold;
        public bool IsMoraleCritical => morale <= criticalMoraleThreshold;
        public bool IsFearCritical => fear >= criticalFearThreshold;
        public bool IsHealthCritical => health <= criticalHealthThreshold;
        public bool IsInfectionCritical => infection >= criticalInfectionThreshold;

        // Get the most urgent need (lowest value for positive needs, highest for negative)
        public string GetMostUrgentNeed()
        {
            float minPositiveNeed = Mathf.Min(hunger, fatigue, morale, health);
            float maxNegativeNeed = Mathf.Max(fear, infection);

            if (minPositiveNeed < 30f)
            {
                if (hunger == minPositiveNeed) return "Hunger";
                if (fatigue == minPositiveNeed) return "Fatigue";
                if (morale == minPositiveNeed) return "Morale";
                if (health == minPositiveNeed) return "Health";
            }

            if (maxNegativeNeed > 70f)
            {
                if (fear == maxNegativeNeed) return "Fear";
                if (infection == maxNegativeNeed) return "Infection";
            }

            return "None";
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 200));
            GUILayout.Label($"NPC: {gameObject.name}");
            GUILayout.Label($"Hunger: {hunger:F1}");
            GUILayout.Label($"Fatigue: {fatigue:F1}");
            GUILayout.Label($"Morale: {morale:F1}");
            GUILayout.Label($"Fear: {fear:F1}");
            GUILayout.Label($"Health: {health:F1}");
            GUILayout.Label($"Infection: {infection:F1}");
            GUILayout.Label($"Urgent Need: {GetMostUrgentNeed()}");
            GUILayout.EndArea();
        }

        // Public properties for accessing need values
        public float Hunger => hunger;
        public float Fatigue => fatigue;
        public float Morale => morale;
        public float Fear => fear;
        public float Health => health;
        public float Infection => infection;
        
        // Critical threshold property
        public float CriticalThreshold => criticalHungerThreshold;
        
        // Method to get need value by type
        public float GetNeed(NeedType needType)
        {
            switch (needType)
            {
                case NeedType.Hunger:
                    return hunger;
                case NeedType.Fatigue:
                    return fatigue;
                case NeedType.Morale:
                    return morale;
                case NeedType.Fear:
                    return fear;
                case NeedType.Health:
                    return health;
                case NeedType.Infection:
                    return infection;
                default:
                    return 0f;
            }
        }
    }
}