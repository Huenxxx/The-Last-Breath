using UnityEngine;
using UnityEngine.SceneManagement;
using TheLastBreath.Systems;
using TheLastBreath.Characters;
using TheLastBreath.AI;
using CharacterController = TheLastBreath.Characters.CharacterController;

namespace TheLastBreath.Core
{
    /// <summary>
    /// Main game manager that coordinates all systems and handles game flow
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool enableDebugMode = true;
        
        [Header("Prototype 0 Settings")]
        [SerializeField] private int npcCount = 2;
        [SerializeField] private int lootItemCount = 5;
        [SerializeField] private float spawnRadius = 20f;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject playerCharacterPrefab;
        [SerializeField] private GameObject npcCharacterPrefab;
        [SerializeField] private GameObject lootItemPrefab;
        
        // System references
        private TerrainGenerator terrainGenerator;
        private CameraController cameraController;
        private UnitSelectionSystem selectionSystem;
        private InteractionSystem interactionSystem;
        private SaveLoadSystem saveLoadSystem;
        
        // Game state
        private bool isInitialized = false;
        private CharacterController playerCharacter;
        
        // Singleton
        public static GameManager Instance { get; private set; }
        
        // Events
        public System.Action OnGameInitialized;
        public System.Action OnGameStarted;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        
        // Properties
        public bool IsInitialized => isInitialized;
        public CharacterController PlayerCharacter => playerCharacter;
        public bool IsDebugMode => enableDebugMode;
        
        /// <summary>
        /// Initialize singleton
        /// </summary>
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        /// <summary>
        /// Start game initialization
        /// </summary>
        void Start()
        {
            if (autoInitialize)
            {
                InitializeGame();
            }
        }
        
        /// <summary>
        /// Handle input and game updates
        /// </summary>
        void Update()
        {
            if (!isInitialized) return;
            
            HandleInput();
        }
        
        /// <summary>
        /// Handle global input
        /// </summary>
        private void HandleInput()
        {
            // Quick save/load
            if (Input.GetKeyDown(KeyCode.F5))
            {
                QuickSave();
            }
            
            if (Input.GetKeyDown(KeyCode.F9))
            {
                QuickLoad();
            }
            
            // Debug commands
            if (enableDebugMode)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    ToggleDebugInfo();
                }
                
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    RegenerateWorld();
                }
            }
        }
        
        /// <summary>
        /// Initialize all game systems
        /// </summary>
        public void InitializeGame()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Game is already initialized!");
                return;
            }
            
            Debug.Log("Initializing The Last Breath - Prototype 0...");
            
            // Find or create systems
            SetupSystems();
            
            // Initialize world
            InitializeWorld();
            
            // Create game objects
            CreateGameObjects();
            
            // Setup camera
            SetupCamera();
            
            // Mark as initialized
            isInitialized = true;
            
            Debug.Log("Game initialization complete!");
            OnGameInitialized?.Invoke();
            OnGameStarted?.Invoke();
        }
        
        /// <summary>
        /// Set up all game systems
        /// </summary>
        private void SetupSystems()
        {
            // Find existing systems or create them
            terrainGenerator = FindOrCreateSystem<TerrainGenerator>("TerrainGenerator");
            cameraController = FindOrCreateSystem<CameraController>("CameraController");
            selectionSystem = FindOrCreateSystem<UnitSelectionSystem>("UnitSelectionSystem");
            interactionSystem = FindOrCreateSystem<InteractionSystem>("InteractionSystem");
            saveLoadSystem = FindOrCreateSystem<SaveLoadSystem>("SaveLoadSystem");
            
            Debug.Log("All systems initialized");
        }
        
        /// <summary>
        /// Find existing system or create new one
        /// </summary>
        /// <typeparam name="T">System type</typeparam>
        /// <param name="objectName">GameObject name for the system</param>
        /// <returns>System component</returns>
        private T FindOrCreateSystem<T>(string objectName) where T : MonoBehaviour
        {
            T system = FindObjectOfType<T>();
            
            if (system == null)
            {
                GameObject systemObject = new GameObject(objectName);
                system = systemObject.AddComponent<T>();
                Debug.Log($"Created {typeof(T).Name} system");
            }
            else
            {
                Debug.Log($"Found existing {typeof(T).Name} system");
            }
            
            return system;
        }
        
        /// <summary>
        /// Initialize the game world
        /// </summary>
        private void InitializeWorld()
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.GenerateTerrain();
                Debug.Log("Terrain generated");
            }
        }
        
        /// <summary>
        /// Create game objects (characters, items, etc.)
        /// </summary>
        private void CreateGameObjects()
        {
            // Create player character
            CreatePlayerCharacter();
            
            // Create NPCs
            CreateNPCs();
            
            // Create loot items
            CreateLootItems();
        }
        
        /// <summary>
        /// Create player character
        /// </summary>
        private void CreatePlayerCharacter()
        {
            Vector3 spawnPosition = GetSafeSpawnPosition();
            
            GameObject playerObject;
            
            if (playerCharacterPrefab != null)
            {
                playerObject = Instantiate(playerCharacterPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Create basic player character
                playerObject = CreateBasicCharacter("Player", spawnPosition, true);
            }
            
            playerCharacter = playerObject.GetComponent<CharacterController>();
            if (playerCharacter == null)
            {
                playerCharacter = playerObject.AddComponent<CharacterController>();
            }
            
            playerCharacter.SetPlayerControlled(true);
            playerCharacter.name = "Player";
            
            Debug.Log($"Player character created at {spawnPosition}");
        }
        
        /// <summary>
        /// Create NPC characters
        /// </summary>
        private void CreateNPCs()
        {
            for (int i = 0; i < npcCount; i++)
            {
                Vector3 spawnPosition = GetSafeSpawnPosition();
                
                GameObject npcObject;
                
                if (npcCharacterPrefab != null)
                {
                    npcObject = Instantiate(npcCharacterPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    // Create basic NPC
                    npcObject = CreateBasicCharacter($"NPC_{i + 1}", spawnPosition, false);
                }
                
                // Ensure NPC has required components
                CharacterController npcCharacter = npcObject.GetComponent<CharacterController>();
                if (npcCharacter == null)
                {
                    npcCharacter = npcObject.AddComponent<CharacterController>();
                }
                
                NPCController npcController = npcObject.GetComponent<NPCController>();
                if (npcController == null)
                {
                    npcController = npcObject.AddComponent<NPCController>();
                }
                
                npcCharacter.SetPlayerControlled(false);
                
                Debug.Log($"NPC {i + 1} created at {spawnPosition}");
            }
        }
        
        /// <summary>
        /// Create loot items
        /// </summary>
        private void CreateLootItems()
        {
            string[] itemNames = { "Scrap Metal", "Food Ration", "Water Bottle", "Bandage", "Tool Kit" };
            
            for (int i = 0; i < lootItemCount; i++)
            {
                Vector3 spawnPosition = GetSafeSpawnPosition();
                
                GameObject itemObject;
                
                if (lootItemPrefab != null)
                {
                    itemObject = Instantiate(lootItemPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    // Create basic loot item
                    itemObject = CreateBasicLootItem(spawnPosition);
                }
                
                // Set up lootable item
                LootableItem lootableItem = itemObject.GetComponent<LootableItem>();
                if (lootableItem == null)
                {
                    lootableItem = itemObject.AddComponent<LootableItem>();
                }
                
                string itemName = itemNames[i % itemNames.Length];
                lootableItem.SetItemProperties(itemName, $"A useful {itemName.ToLower()}", Random.Range(1, 4));
                
                Debug.Log($"Loot item '{itemName}' created at {spawnPosition}");
            }
        }
        
        /// <summary>
        /// Create basic character GameObject
        /// </summary>
        /// <param name="characterName">Character name</param>
        /// <param name="position">Spawn position</param>
        /// <param name="isPlayer">Is this a player character</param>
        /// <returns>Character GameObject</returns>
        private GameObject CreateBasicCharacter(string characterName, Vector3 position, bool isPlayer)
        {
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = characterName;
            character.transform.position = position;
            
            // Set up visual appearance
            Renderer renderer = character.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = isPlayer ? Color.blue : Color.red;
            }
            
            // Add NavMeshAgent for pathfinding
            UnityEngine.AI.NavMeshAgent navAgent = character.AddComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = 3.5f;
            navAgent.stoppingDistance = 0.5f;
            
            return character;
        }
        
        /// <summary>
        /// Create basic loot item GameObject
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <returns>Loot item GameObject</returns>
        private GameObject CreateBasicLootItem(Vector3 position)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = "LootItem";
            item.transform.position = position;
            item.transform.localScale = Vector3.one * 0.5f;
            
            // Set up visual appearance
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
            
            // Set up collider
            Collider collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            return item;
        }
        
        /// <summary>
        /// Get safe spawn position on terrain
        /// </summary>
        /// <returns>Safe spawn position</returns>
        private Vector3 GetSafeSpawnPosition()
        {
            Vector3 randomPosition = Vector3.zero;
            
            // Try to find a position on the terrain
            for (int attempts = 0; attempts < 10; attempts++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                randomPosition = new Vector3(randomCircle.x, 0, randomCircle.y);
                
                // Raycast down to find ground
                if (Physics.Raycast(randomPosition + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f))
                {
                    randomPosition = hit.point + Vector3.up * 0.1f;
                    break;
                }
            }
            
            return randomPosition;
        }
        
        /// <summary>
        /// Set up camera system
        /// </summary>
        private void SetupCamera()
        {
            if (cameraController != null && playerCharacter != null)
            {
                cameraController.SetFollowTarget(playerCharacter.transform);
                Debug.Log("Camera setup complete");
            }
        }
        
        /// <summary>
        /// Quick save game
        /// </summary>
        public void QuickSave()
        {
            if (saveLoadSystem != null)
            {
                saveLoadSystem.SaveGame();
                Debug.Log("Quick save completed");
            }
        }
        
        /// <summary>
        /// Quick load game
        /// </summary>
        public void QuickLoad()
        {
            if (saveLoadSystem != null)
            {
                saveLoadSystem.LoadGame();
                Debug.Log("Quick load completed");
            }
        }
        
        /// <summary>
        /// Toggle debug information display
        /// </summary>
        private void ToggleDebugInfo()
        {
            // TODO: Implement debug overlay
            Debug.Log("Debug info toggled (not implemented yet)");
        }
        
        /// <summary>
        /// Regenerate the world
        /// </summary>
        private void RegenerateWorld()
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.RegenerateTerrain();
                Debug.Log("World regenerated");
            }
        }
        
        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        /// <summary>
        /// Draw debug information
        /// </summary>
        void OnGUI()
        {
            if (!enableDebugMode || !isInitialized) return;
            
            // Debug info panel
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("The Last Breath - Prototype 0", GUI.skin.label);
            GUILayout.Label($"FPS: {Mathf.Round(1f / Time.deltaTime)}");
            GUILayout.Label($"Game Time: {Time.time:F1}s");
            
            if (selectionSystem != null)
            {
                GUILayout.Label($"Selected Units: {selectionSystem.SelectedCount}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("F1 - Toggle Debug");
            GUILayout.Label("F2 - Regenerate World");
            GUILayout.Label("F5 - Quick Save");
            GUILayout.Label("F9 - Quick Load");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}