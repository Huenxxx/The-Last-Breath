using System.Collections.Generic;
using UnityEngine;
using TheLastBreath.Characters;
using CharacterController = TheLastBreath.Characters.CharacterController;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// Manages unit selection for up to 3 characters with visual feedback
    /// </summary>
    public class UnitSelectionSystem : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private int maxSelectedUnits = 3;
        [SerializeField] private LayerMask selectableLayerMask = -1;
        [SerializeField] private LayerMask groundLayerMask = 1;
        
        [Header("Selection Box")]
        [SerializeField] private bool enableSelectionBox = true;
        [SerializeField] private Material selectionBoxMaterial;
        
        [Header("Input Settings")]
        [SerializeField] private KeyCode addToSelectionKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode deselectAllKey = KeyCode.Escape;
        
        // Selection state
        private List<CharacterController> selectedCharacters = new List<CharacterController>();
        private Camera playerCamera;
        
        // Selection box
        private bool isSelecting = false;
        private Vector3 selectionStartPosition;
        private Vector3 selectionEndPosition;
        private Rect selectionRect;
        
        // Events
        public System.Action<List<CharacterController>> OnSelectionChanged;
        public System.Action<CharacterController> OnCharacterSelected;
        public System.Action<CharacterController> OnCharacterDeselected;
        
        // Properties
        public List<CharacterController> SelectedCharacters => new List<CharacterController>(selectedCharacters);
        public int SelectedCount => selectedCharacters.Count;
        public bool HasSelection => selectedCharacters.Count > 0;
        
        /// <summary>
        /// Initialize the selection system
        /// </summary>
        void Start()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        /// <summary>
        /// Handle input and selection updates
        /// </summary>
        void Update()
        {
            HandleInput();
            UpdateSelectionBox();
        }
        
        /// <summary>
        /// Handle all selection input
        /// </summary>
        private void HandleInput()
        {
            // Deselect all with escape key
            if (Input.GetKeyDown(deselectAllKey))
            {
                DeselectAll();
                return;
            }
            
            // Handle mouse input
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                HandleMouseDown();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleMouseUp();
            }
            
            // Handle right-click for movement commands
            if (Input.GetMouseButtonDown(1) && HasSelection) // Right mouse button
            {
                HandleMovementCommand();
            }
        }
        
        /// <summary>
        /// Handle mouse button down
        /// </summary>
        private void HandleMouseDown()
        {
            Vector3 mousePosition = Input.mousePosition;
            
            // Check if clicking on a character
            Ray ray = playerCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayerMask))
            {
                CharacterController character = hit.collider.GetComponent<CharacterController>();
                if (character != null && character.IsPlayerControlled)
                {
                    HandleCharacterClick(character);
                    return;
                }
            }
            
            // Start selection box if not clicking on character
            if (enableSelectionBox)
            {
                StartSelectionBox(mousePosition);
            }
            else if (!Input.GetKey(addToSelectionKey))
            {
                // Deselect all if not holding shift and not starting selection box
                DeselectAll();
            }
        }
        
        /// <summary>
        /// Handle mouse button up
        /// </summary>
        private void HandleMouseUp()
        {
            if (isSelecting)
            {
                EndSelectionBox();
            }
        }
        
        /// <summary>
        /// Handle clicking on a character
        /// </summary>
        /// <param name="character">Character that was clicked</param>
        private void HandleCharacterClick(CharacterController character)
        {
            bool addToSelection = Input.GetKey(addToSelectionKey);
            
            if (addToSelection)
            {
                // Add to selection or remove if already selected
                if (character.IsSelected)
                {
                    DeselectCharacter(character);
                }
                else
                {
                    SelectCharacter(character);
                }
            }
            else
            {
                // Replace selection with this character
                DeselectAll();
                SelectCharacter(character);
            }
        }
        
        /// <summary>
        /// Start selection box
        /// </summary>
        /// <param name="mousePosition">Starting mouse position</param>
        private void StartSelectionBox(Vector3 mousePosition)
        {
            isSelecting = true;
            selectionStartPosition = mousePosition;
            
            // Deselect all if not adding to selection
            if (!Input.GetKey(addToSelectionKey))
            {
                DeselectAll();
            }
        }
        
        /// <summary>
        /// Update selection box
        /// </summary>
        private void UpdateSelectionBox()
        {
            if (!isSelecting) return;
            
            selectionEndPosition = Input.mousePosition;
            
            // Calculate selection rectangle
            float minX = Mathf.Min(selectionStartPosition.x, selectionEndPosition.x);
            float maxX = Mathf.Max(selectionStartPosition.x, selectionEndPosition.x);
            float minY = Mathf.Min(selectionStartPosition.y, selectionEndPosition.y);
            float maxY = Mathf.Max(selectionStartPosition.y, selectionEndPosition.y);
            
            selectionRect = new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        /// <summary>
        /// End selection box and select characters within it
        /// </summary>
        private void EndSelectionBox()
        {
            isSelecting = false;
            
            // Find all characters within selection box
            CharacterController[] allCharacters = FindObjectsOfType<CharacterController>();
            
            foreach (CharacterController character in allCharacters)
            {
                if (!character.IsPlayerControlled) continue;
                
                Vector3 screenPosition = playerCamera.WorldToScreenPoint(character.Position);
                
                // Check if character is within selection rectangle
                if (selectionRect.Contains(new Vector2(screenPosition.x, screenPosition.y)))
                {
                    if (!character.IsSelected)
                    {
                        SelectCharacter(character);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle movement command for selected units
        /// </summary>
        private void HandleMovementCommand()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 targetPosition = hit.point;
                
                // Move all selected characters
                for (int i = 0; i < selectedCharacters.Count; i++)
                {
                    CharacterController character = selectedCharacters[i];
                    
                    // Calculate formation position
                    Vector3 formationOffset = CalculateFormationOffset(i, selectedCharacters.Count);
                    Vector3 finalPosition = targetPosition + formationOffset;
                    
                    // Check if running (shift key)
                    bool isRunning = Input.GetKey(KeyCode.LeftShift);
                    character.MoveTo(finalPosition, isRunning);
                }
            }
        }
        
        /// <summary>
        /// Calculate formation offset for character in group
        /// </summary>
        /// <param name="index">Character index in selection</param>
        /// <param name="totalCount">Total selected characters</param>
        /// <returns>Formation offset</returns>
        private Vector3 CalculateFormationOffset(int index, int totalCount)
        {
            if (totalCount == 1) return Vector3.zero;
            
            float spacing = 2f;
            
            switch (totalCount)
            {
                case 2:
                    return new Vector3((index - 0.5f) * spacing, 0, 0);
                case 3:
                    switch (index)
                    {
                        case 0: return Vector3.zero;
                        case 1: return new Vector3(-spacing, 0, -spacing);
                        case 2: return new Vector3(spacing, 0, -spacing);
                    }
                    break;
            }
            
            return Vector3.zero;
        }
        
        /// <summary>
        /// Select a character
        /// </summary>
        /// <param name="character">Character to select</param>
        public void SelectCharacter(CharacterController character)
        {
            if (character == null || character.IsSelected) return;
            
            // Check if we've reached the selection limit
            if (selectedCharacters.Count >= maxSelectedUnits)
            {
                // Remove the first selected character
                DeselectCharacter(selectedCharacters[0]);
            }
            
            selectedCharacters.Add(character);
            character.SetSelected(true);
            
            OnCharacterSelected?.Invoke(character);
            OnSelectionChanged?.Invoke(selectedCharacters);
        }
        
        /// <summary>
        /// Deselect a character
        /// </summary>
        /// <param name="character">Character to deselect</param>
        public void DeselectCharacter(CharacterController character)
        {
            if (character == null || !character.IsSelected) return;
            
            selectedCharacters.Remove(character);
            character.SetSelected(false);
            
            OnCharacterDeselected?.Invoke(character);
            OnSelectionChanged?.Invoke(selectedCharacters);
        }
        
        /// <summary>
        /// Deselect all characters
        /// </summary>
        public void DeselectAll()
        {
            List<CharacterController> charactersToDeselect = new List<CharacterController>(selectedCharacters);
            
            foreach (CharacterController character in charactersToDeselect)
            {
                DeselectCharacter(character);
            }
        }
        
        /// <summary>
        /// Get the primary selected character (first in list)
        /// </summary>
        /// <returns>Primary selected character or null</returns>
        public CharacterController GetPrimarySelection()
        {
            return selectedCharacters.Count > 0 ? selectedCharacters[0] : null;
        }
        
        /// <summary>
        /// Check if character is selected
        /// </summary>
        /// <param name="character">Character to check</param>
        /// <returns>True if character is selected</returns>
        public bool IsCharacterSelected(CharacterController character)
        {
            return selectedCharacters.Contains(character);
        }
        
        /// <summary>
        /// Draw selection box GUI
        /// </summary>
        void OnGUI()
        {
            if (isSelecting && enableSelectionBox)
            {
                // Draw selection rectangle
                GUI.color = new Color(1, 1, 1, 0.1f);
                GUI.DrawTexture(selectionRect, Texture2D.whiteTexture);
                
                // Draw selection border
                GUI.color = Color.white;
                DrawRectangleBorder(selectionRect, 2);
            }
        }
        
        /// <summary>
        /// Draw rectangle border
        /// </summary>
        /// <param name="rect">Rectangle to draw</param>
        /// <param name="thickness">Border thickness</param>
        private void DrawRectangleBorder(Rect rect, int thickness)
        {
            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), Texture2D.whiteTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        }
    }
}