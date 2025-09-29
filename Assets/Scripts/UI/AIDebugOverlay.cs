using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheLastBreath.AI;
using TheLastBreath.Factions;

namespace TheLastBreath.UI
{
    /// <summary>
    /// Debug overlay for AI system showing needs, behaviors, and jobs
    /// </summary>
    public class AIDebugOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Transform npcListParent;
        [SerializeField] private GameObject npcDebugItemPrefab;
        [SerializeField] private TextMeshProUGUI factionInfoText;
        [SerializeField] private Button toggleButton;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        
        [Header("Settings")]
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private int maxNPCsToShow = 10;
        [SerializeField] private float maxDistanceToShow = 50f;
        
        // Runtime data
        private bool isVisible = false;
        private float lastUpdateTime = 0f;
        private List<NPCController> trackedNPCs = new List<NPCController>();
        private List<GameObject> npcDebugItems = new List<GameObject>();
        private Camera playerCamera;
        
        void Start()
        {
            InitializeDebugOverlay();
            
            if (showOnStart)
                ShowOverlay();
            else
                HideOverlay();
        }
        
        void InitializeDebugOverlay()
        {
            // Get player camera
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindObjectOfType<Camera>();
            
            // Setup toggle button
            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleOverlay);
            
            // Create debug panel if not assigned
            if (debugPanel == null)
                CreateDebugPanel();
            
            // Find all NPCs in scene
            RefreshNPCList();
        }
        
        void CreateDebugPanel()
        {
            // Create main panel
            GameObject panel = new GameObject("AI Debug Panel");
            panel.transform.SetParent(transform);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.5f);
            panelRect.anchorMax = new Vector2(0.4f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            debugPanel = panel;
            
            // Create scroll view for NPC list
            CreateNPCScrollView();
            
            // Create faction info area
            CreateFactionInfoArea();
        }
        
        void CreateNPCScrollView()
        {
            GameObject scrollView = new GameObject("NPC Scroll View");
            scrollView.transform.SetParent(debugPanel.transform);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0.3f);
            scrollRect.anchorMax = new Vector2(1, 1f);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);
            
            // Create content area
            GameObject content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            npcListParent = content.transform;
        }
        
        void CreateFactionInfoArea()
        {
            GameObject factionArea = new GameObject("Faction Info");
            factionArea.transform.SetParent(debugPanel.transform);
            
            RectTransform factionRect = factionArea.AddComponent<RectTransform>();
            factionRect.anchorMin = new Vector2(0, 0);
            factionRect.anchorMax = new Vector2(1, 0.3f);
            factionRect.offsetMin = new Vector2(10, 10);
            factionRect.offsetMax = new Vector2(-10, -10);
            
            factionInfoText = factionArea.AddComponent<TextMeshProUGUI>();
            factionInfoText.text = "Faction Info";
            factionInfoText.fontSize = 12f;
            factionInfoText.color = Color.white;
        }
        
        void Update()
        {
            // Handle toggle input
            if (Input.GetKeyDown(toggleKey))
                ToggleOverlay();
            
            // Update debug info if visible
            if (isVisible && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDebugInfo();
                lastUpdateTime = Time.time;
            }
        }
        
        void UpdateDebugInfo()
        {
            RefreshNPCList();
            UpdateNPCDebugItems();
            UpdateFactionInfo();
        }
        
        void RefreshNPCList()
        {
            trackedNPCs.Clear();
            
            NPCController[] allNPCs = FindObjectsOfType<NPCController>();
            Vector3 playerPos = playerCamera != null ? playerCamera.transform.position : Vector3.zero;
            
            // Sort NPCs by distance and take closest ones
            List<NPCController> sortedNPCs = new List<NPCController>(allNPCs);
            sortedNPCs.Sort((a, b) => 
            {
                float distA = Vector3.Distance(a.transform.position, playerPos);
                float distB = Vector3.Distance(b.transform.position, playerPos);
                return distA.CompareTo(distB);
            });
            
            // Add NPCs within range
            for (int i = 0; i < Mathf.Min(sortedNPCs.Count, maxNPCsToShow); i++)
            {
                float distance = Vector3.Distance(sortedNPCs[i].transform.position, playerPos);
                if (distance <= maxDistanceToShow)
                    trackedNPCs.Add(sortedNPCs[i]);
            }
        }
        
        void UpdateNPCDebugItems()
        {
            // Clear existing items
            foreach (GameObject item in npcDebugItems)
            {
                if (item != null)
                    DestroyImmediate(item);
            }
            npcDebugItems.Clear();
            
            // Create new items for tracked NPCs
            foreach (NPCController npc in trackedNPCs)
            {
                if (npc != null)
                    CreateNPCDebugItem(npc);
            }
        }
        
        void CreateNPCDebugItem(NPCController npc)
        {
            GameObject item = new GameObject($"NPC Debug - {npc.name}");
            item.transform.SetParent(npcListParent);
            
            RectTransform itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 1);
            itemRect.anchorMax = new Vector2(1, 1);
            itemRect.pivot = new Vector2(0, 1);
            
            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Text component
            TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
            text.fontSize = 10f;
            text.color = Color.white;
            text.margin = new Vector4(5, 5, 5, 5);
            
            // Get NPC info
            string npcInfo = GetNPCDebugInfo(npc);
            text.text = npcInfo;
            
            // Layout element
            LayoutElement layoutElement = item.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            
            npcDebugItems.Add(item);
        }
        
        string GetNPCDebugInfo(NPCController npc)
        {
            string info = $"<b>{npc.name}</b>\n";
            
            // Distance
            if (playerCamera != null)
            {
                float distance = Vector3.Distance(npc.transform.position, playerCamera.transform.position);
                info += $"Distance: {distance:F1}m\n";
            }
            
            // Current state
            info += $"State: {npc.GetCurrentState()}\n";
            
            // Needs information
            NPCNeeds needs = npc.GetComponent<NPCNeeds>();
            if (needs != null)
            {
                info += $"<color=orange>Needs:</color>\n";
                info += $"  Hunger: {needs.Hunger:F0} ";
                if (needs.Hunger <= needs.CriticalThreshold) info += "<color=red>CRITICAL</color>";
                info += "\n";
                
                info += $"  Fatigue: {needs.Fatigue:F0} ";
                if (needs.Fatigue <= needs.CriticalThreshold) info += "<color=red>CRITICAL</color>";
                info += "\n";
                
                info += $"  Morale: {needs.Morale:F0} ";
                if (needs.Morale <= needs.CriticalThreshold) info += "<color=red>LOW</color>";
                info += "\n";
            }
            
            // Current action from Utility AI
            UtilityAI utilityAI = npc.GetComponent<UtilityAI>();
            if (utilityAI != null && utilityAI.CurrentAction != null)
            {
                info += $"<color=cyan>Action:</color> {utilityAI.CurrentAction.GetType().Name}\n";
            }
            
            // Job information
            NPCJobSystem jobSystem = npc.GetComponent<NPCJobSystem>();
            if (jobSystem != null)
            {
                if (jobSystem.HasActiveJob)
                {
                    info += $"<color=green>Job:</color> {jobSystem.CurrentJob}\n";
                    if (jobSystem.IsWorking)
                        info += $"Working: {jobSystem.WorkTimeRemaining:F0}s left\n";
                }
                else
                {
                    info += "<color=yellow>No Job</color>\n";
                }
            }
            
            return info;
        }
        
        void UpdateFactionInfo()
        {
            if (factionInfoText == null) return;
            
            string info = "<b>=== FACTION STATUS ===</b>\n";
            
            if (FactionManager.Instance != null)
            {
                var factions = FactionManager.Instance.GetAllFactions();
                
                foreach (var faction in factions)
                {
                    int reputation = FactionManager.Instance.GetPlayerReputation(faction);
                    var level = FactionManager.Instance.GetReputationLevel(faction);
                    var behavior = FactionManager.Instance.GetFactionBehaviorTowardsPlayer(faction);
                    
                    Color levelColor = GetReputationColor(level);
                    string colorHex = ColorUtility.ToHtmlStringRGB(levelColor);
                    
                    info += $"<b>{faction.factionName}</b>\n";
                    info += $"  Rep: {reputation} (<color=#{colorHex}>{level}</color>)\n";
                    info += $"  Behavior: {behavior}\n";
                    
                    if (FactionManager.Instance.WillAttackOnSight(faction))
                        info += $"  <color=red>HOSTILE - Attack on sight!</color>\n";
                    else if (FactionManager.Instance.WillTrade(faction))
                        info += $"  <color=green>Will trade</color>\n";
                    
                    info += "\n";
                }
            }
            else
            {
                info += "<color=red>FactionManager not found!</color>\n";
            }
            
            factionInfoText.text = info;
        }
        
        Color GetReputationColor(ReputationLevel level)
        {
            switch (level)
            {
                case ReputationLevel.Hostile: return Color.red;
                case ReputationLevel.Unfriendly: return new Color(1f, 0.5f, 0f); // Orange
                case ReputationLevel.Neutral: return Color.yellow;
                case ReputationLevel.Friendly: return Color.green;
                case ReputationLevel.Allied: return Color.cyan;
                default: return Color.white;
            }
        }
        
        public void ToggleOverlay()
        {
            if (isVisible)
                HideOverlay();
            else
                ShowOverlay();
        }
        
        public void ShowOverlay()
        {
            isVisible = true;
            if (debugPanel != null)
                debugPanel.SetActive(true);
            
            UpdateDebugInfo();
        }
        
        public void HideOverlay()
        {
            isVisible = false;
            if (debugPanel != null)
                debugPanel.SetActive(false);
        }
        
        // Public methods for external control
        public void SetMaxNPCsToShow(int count)
        {
            maxNPCsToShow = Mathf.Max(1, count);
        }
        
        public void SetMaxDistance(float distance)
        {
            maxDistanceToShow = Mathf.Max(1f, distance);
        }
        
        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }
        
        void OnDestroy()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(ToggleOverlay);
        }
        
        // Context menu for testing
        [ContextMenu("Toggle Debug Overlay")]
        void ContextToggle()
        {
            ToggleOverlay();
        }
        
        [ContextMenu("Refresh NPC List")]
        void ContextRefresh()
        {
            RefreshNPCList();
            UpdateNPCDebugItems();
        }
    }
}