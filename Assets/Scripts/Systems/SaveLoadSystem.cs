using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TheLastBreath.Characters;
using CharacterController = TheLastBreath.Characters.CharacterController;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// Handles saving and loading game state
    /// </summary>
    public class SaveLoadSystem : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 60f; // seconds
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Save data
        private GameSaveData currentSaveData;
        private string savePath;
        private float autoSaveTimer;
        
        // Systems references
        private UnitSelectionSystem selectionSystem;
        private TerrainGenerator terrainGenerator;
        
        // Events
        public System.Action OnGameSaved;
        public System.Action OnGameLoaded;
        public System.Action<string> OnSaveError;
        public System.Action<string> OnLoadError;
        
        /// <summary>
        /// Initialize save/load system
        /// </summary>
        void Start()
        {
            InitializeSaveSystem();
            FindSystemReferences();
        }
        
        /// <summary>
        /// Initialize save system
        /// </summary>
        private void InitializeSaveSystem()
        {
            // Set up save path
            savePath = Path.Combine(Application.persistentDataPath, saveFileName);
            
            if (enableDebugLogs)
            {
                Debug.Log($"Save file path: {savePath}");
            }
            
            // Initialize save data
            currentSaveData = new GameSaveData();
        }
        
        /// <summary>
        /// Find references to other systems
        /// </summary>
        private void FindSystemReferences()
        {
            selectionSystem = FindObjectOfType<UnitSelectionSystem>();
            terrainGenerator = FindObjectOfType<TerrainGenerator>();
        }
        
        /// <summary>
        /// Update auto-save timer
        /// </summary>
        void Update()
        {
            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    SaveGame();
                }
            }
        }
        
        /// <summary>
        /// Save current game state
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // Collect save data
                CollectSaveData();
                
                // Convert to JSON
                string jsonData = JsonUtility.ToJson(currentSaveData, true);
                
                // Write to file
                File.WriteAllText(savePath, jsonData);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Game saved successfully to: {savePath}");
                }
                
                OnGameSaved?.Invoke();
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to save game: {e.Message}";
                Debug.LogError(errorMessage);
                OnSaveError?.Invoke(errorMessage);
            }
        }
        
        /// <summary>
        /// Load game state from file
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (!File.Exists(savePath))
                {
                    string errorMessage = "No save file found";
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning(errorMessage);
                    }
                    OnLoadError?.Invoke(errorMessage);
                    return;
                }
                
                // Read file
                string jsonData = File.ReadAllText(savePath);
                
                // Parse JSON
                currentSaveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                // Apply save data
                ApplySaveData();
                
                if (enableDebugLogs)
                {
                    Debug.Log($"Game loaded successfully from: {savePath}");
                }
                
                OnGameLoaded?.Invoke();
            }
            catch (Exception e)
            {
                string errorMessage = $"Failed to load game: {e.Message}";
                Debug.LogError(errorMessage);
                OnLoadError?.Invoke(errorMessage);
            }
        }
        
        /// <summary>
        /// Check if save file exists
        /// </summary>
        /// <returns>True if save file exists</returns>
        public bool SaveFileExists()
        {
            return File.Exists(savePath);
        }
        
        /// <summary>
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    
                    if (enableDebugLogs)
                    {
                        Debug.Log("Save file deleted");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
        
        /// <summary>
        /// Collect current game state for saving
        /// </summary>
        private void CollectSaveData()
        {
            currentSaveData.saveVersion = 1;
            currentSaveData.saveTime = DateTime.Now.ToBinary();
            currentSaveData.gameTime = Time.time;
            
            // Save camera data
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                currentSaveData.cameraPosition = mainCamera.transform.position;
                currentSaveData.cameraRotation = mainCamera.transform.rotation;
            }
            
            // Save terrain data
            if (terrainGenerator != null)
            {
                currentSaveData.terrainSeed = terrainGenerator.Seed;
                Vector2 terrainSizeVector = terrainGenerator.TerrainSize;
                currentSaveData.terrainSize = Mathf.RoundToInt(terrainSizeVector.x); // Using x component as terrain size
            }
            
            // Save character data
            SaveCharacterData();
            
            // Save lootable items
            SaveLootableItems();
        }
        
        /// <summary>
        /// Save character data
        /// </summary>
        private void SaveCharacterData()
        {
            currentSaveData.characters.Clear();
            
            CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
            
            foreach (CharacterController character in allCharacters)
            {
                CharacterSaveData charData = new CharacterSaveData
                {
                    name = character.name,
                    position = character.Position,
                    rotation = character.transform.rotation,
                    isPlayerControlled = character.IsPlayerControlled,
                    isSelected = character.IsSelected,
                    currentState = character.CurrentState.ToString()
                };
                
                currentSaveData.characters.Add(charData);
            }
        }
        
        /// <summary>
        /// Save lootable items data
        /// </summary>
        private void SaveLootableItems()
        {
            currentSaveData.lootableItems.Clear();
            
            LootableItem[] allItems = FindObjectsOfType<LootableItem>();
            
            foreach (LootableItem item in allItems)
            {
                if (!item.IsPickedUp) // Only save items that haven't been picked up
                {
                    LootableItemSaveData itemData = new LootableItemSaveData
                    {
                        name = item.ItemName,
                        position = item.Position,
                        rotation = item.transform.rotation,
                        quantity = item.Quantity
                    };
                    
                    currentSaveData.lootableItems.Add(itemData);
                }
            }
        }
        
        /// <summary>
        /// Apply loaded save data to game state
        /// </summary>
        private void ApplySaveData()
        {
            // Apply camera data
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = currentSaveData.cameraPosition;
                mainCamera.transform.rotation = currentSaveData.cameraRotation;
            }
            
            // Apply terrain data
            if (terrainGenerator != null)
            {
                terrainGenerator.SetSeed(currentSaveData.terrainSeed);
                terrainGenerator.SetTerrainSize(new Vector2(currentSaveData.terrainSize, currentSaveData.terrainSize));
                terrainGenerator.RegenerateTerrain();
            }
            
            // Apply character data
            LoadCharacterData();
            
            // Apply lootable items
            LoadLootableItems();
        }
        
        /// <summary>
        /// Load character data
        /// </summary>
        private void LoadCharacterData()
        {
            CharacterController[] existingCharacters = FindObjectsOfType<CharacterController>();
            
            // Match existing characters with save data
            for (int i = 0; i < currentSaveData.characters.Count && i < existingCharacters.Length; i++)
            {
                CharacterSaveData charData = currentSaveData.characters[i];
                CharacterController character = existingCharacters[i];
                
                // Apply position and rotation
                character.transform.position = charData.position;
                character.transform.rotation = charData.rotation;
                
                // Apply character properties
                character.SetPlayerControlled(charData.isPlayerControlled);
                
                // Apply selection state
                if (charData.isSelected && selectionSystem != null)
                {
                    selectionSystem.SelectCharacter(character);
                }
            }
        }
        
        /// <summary>
        /// Load lootable items
        /// </summary>
        private void LoadLootableItems()
        {
            // Clear existing items
            LootableItem[] existingItems = FindObjectsOfType<LootableItem>();
            foreach (LootableItem item in existingItems)
            {
                DestroyImmediate(item.gameObject);
            }
            
            // Create items from save data
            foreach (LootableItemSaveData itemData in currentSaveData.lootableItems)
            {
                CreateLootableItem(itemData);
            }
        }
        
        /// <summary>
        /// Create lootable item from save data
        /// </summary>
        /// <param name="itemData">Item save data</param>
        private void CreateLootableItem(LootableItemSaveData itemData)
        {
            // Create basic item GameObject
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.name = itemData.name;
            itemObject.transform.position = itemData.position;
            itemObject.transform.rotation = itemData.rotation;
            itemObject.transform.localScale = Vector3.one * 0.5f;
            
            // Add LootableItem component
            LootableItem lootableItem = itemObject.AddComponent<LootableItem>();
            lootableItem.SetItemProperties(itemData.name, $"A {itemData.name}", itemData.quantity);
            
            // Set up collider
            Collider col = itemObject.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }
        
        /// <summary>
        /// Get save file info
        /// </summary>
        /// <returns>Save file information</returns>
        public SaveFileInfo GetSaveFileInfo()
        {
            if (!SaveFileExists())
            {
                return null;
            }
            
            try
            {
                string jsonData = File.ReadAllText(savePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                return new SaveFileInfo
                {
                    saveTime = DateTime.FromBinary(saveData.saveTime),
                    gameTime = saveData.gameTime,
                    characterCount = saveData.characters.Count,
                    itemCount = saveData.lootableItems.Count
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read save file info: {e.Message}");
                return null;
            }
        }
    }
    
    /// <summary>
    /// Main game save data structure
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public long saveTime;
        public float gameTime;
        
        // Camera data
        public Vector3 cameraPosition;
        public Quaternion cameraRotation;
        
        // Terrain data
        public int terrainSeed;
        public int terrainSize;
        
        // Character data
        public List<CharacterSaveData> characters = new List<CharacterSaveData>();
        
        // Item data
        public List<LootableItemSaveData> lootableItems = new List<LootableItemSaveData>();
    }
    
    /// <summary>
    /// Character save data
    /// </summary>
    [System.Serializable]
    public class CharacterSaveData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public bool isPlayerControlled;
        public bool isSelected;
        public string currentState;
    }
    
    /// <summary>
    /// Lootable item save data
    /// </summary>
    [System.Serializable]
    public class LootableItemSaveData
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public int quantity;
    }
    
    /// <summary>
    /// Save file information
    /// </summary>
    public class SaveFileInfo
    {
        public DateTime saveTime;
        public float gameTime;
        public int characterCount;
        public int itemCount;
    }
}