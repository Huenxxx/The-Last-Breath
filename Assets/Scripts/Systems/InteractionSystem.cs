using UnityEngine;
using System.Collections.Generic;
using TheLastBreath.Characters;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// Handles basic interaction mechanics like looting and picking up items
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayerMask = -1;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Header("UI Settings")]
        [SerializeField] private bool showInteractionPrompts = true;
        [SerializeField] private float promptDisplayDistance = 3f;
        
        // Components
        private UnitSelectionSystem selectionSystem;
        private Camera playerCamera;
        
        // Interaction state
        private List<IInteractable> nearbyInteractables = new List<IInteractable>();
        private IInteractable highlightedInteractable;
        
        // Events
        public System.Action<IInteractable, CharacterController> OnInteractionStarted;
        public System.Action<IInteractable, CharacterController> OnInteractionCompleted;
        public System.Action<IInteractable, CharacterController> OnInteractionFailed;
        
        /// <summary>
        /// Initialize interaction system
        /// </summary>
        void Start()
        {
            selectionSystem = FindObjectOfType<UnitSelectionSystem>();
            playerCamera = Camera.main;
            
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        /// <summary>
        /// Update interaction system
        /// </summary>
        void Update()
        {
            UpdateNearbyInteractables();
            UpdateHighlightedInteractable();
            HandleInput();
        }
        
        /// <summary>
        /// Update list of nearby interactables
        /// </summary>
        private void UpdateNearbyInteractables()
        {
            nearbyInteractables.Clear();
            
            if (selectionSystem == null || !selectionSystem.HasSelection) return;
            
            CharacterController primaryCharacter = selectionSystem.GetPrimarySelection();
            if (primaryCharacter == null) return;
            
            // Find all interactables within range
            Collider[] colliders = Physics.OverlapSphere(primaryCharacter.Position, promptDisplayDistance, interactableLayerMask);
            
            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(primaryCharacter))
                {
                    nearbyInteractables.Add(interactable);
                }
            }
        }
        
        /// <summary>
        /// Update highlighted interactable based on mouse position
        /// </summary>
        private void UpdateHighlightedInteractable()
        {
            highlightedInteractable = null;
            
            if (playerCamera == null) return;
            
            // Raycast from mouse position
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayerMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && nearbyInteractables.Contains(interactable))
                {
                    highlightedInteractable = interactable;
                }
            }
        }
        
        /// <summary>
        /// Handle interaction input
        /// </summary>
        private void HandleInput()
        {
            // Handle E key interaction
            if (Input.GetKeyDown(interactKey))
            {
                TryInteractWithNearest();
            }
            
            // Handle mouse click interaction
            if (Input.GetMouseButtonDown(0) && highlightedInteractable != null)
            {
                TryInteract(highlightedInteractable);
            }
        }
        
        /// <summary>
        /// Try to interact with the nearest interactable
        /// </summary>
        private void TryInteractWithNearest()
        {
            if (selectionSystem == null || !selectionSystem.HasSelection) return;
            
            CharacterController primaryCharacter = selectionSystem.GetPrimarySelection();
            if (primaryCharacter == null) return;
            
            // Find closest interactable within interaction range
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;
            
            foreach (IInteractable interactable in nearbyInteractables)
            {
                float distance = Vector3.Distance(primaryCharacter.Position, interactable.Position);
                if (distance <= interactionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
            
            if (closestInteractable != null)
            {
                TryInteract(closestInteractable);
            }
        }
        
        /// <summary>
        /// Try to interact with specific interactable
        /// </summary>
        /// <param name="interactable">Interactable to interact with</param>
        private void TryInteract(IInteractable interactable)
        {
            if (selectionSystem == null || !selectionSystem.HasSelection) return;
            
            CharacterController primaryCharacter = selectionSystem.GetPrimarySelection();
            if (primaryCharacter == null) return;
            
            // Check if character can interact
            if (!interactable.CanInteract(primaryCharacter))
            {
                OnInteractionFailed?.Invoke(interactable, primaryCharacter);
                return;
            }
            
            // Check distance
            float distance = Vector3.Distance(primaryCharacter.Position, interactable.Position);
            if (distance > interactionRange)
            {
                // Move character to interactable first
                MoveToInteract(primaryCharacter, interactable);
                return;
            }
            
            // Perform interaction
            PerformInteraction(interactable, primaryCharacter);
        }
        
        /// <summary>
        /// Move character to interactable and then interact
        /// </summary>
        /// <param name="character">Character to move</param>
        /// <param name="interactable">Target interactable</param>
        private void MoveToInteract(CharacterController character, IInteractable interactable)
        {
            // Calculate position near the interactable
            Vector3 targetPosition = interactable.Position;
            Vector3 direction = (character.Position - targetPosition).normalized;
            targetPosition += direction * (interactionRange * 0.8f);
            
            // Move character
            character.MoveTo(targetPosition);
            
            // TODO: Add callback when character reaches destination to auto-interact
        }
        
        /// <summary>
        /// Perform the actual interaction
        /// </summary>
        /// <param name="interactable">Interactable to interact with</param>
        /// <param name="character">Character performing interaction</param>
        private void PerformInteraction(IInteractable interactable, CharacterController character)
        {
            OnInteractionStarted?.Invoke(interactable, character);
            
            // Face the interactable
            Vector3 lookDirection = (interactable.Position - character.Position).normalized;
            lookDirection.y = 0;
            
            if (lookDirection != Vector3.zero)
            {
                character.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            
            // Perform interaction
            bool success = interactable.Interact(character);
            
            if (success)
            {
                OnInteractionCompleted?.Invoke(interactable, character);
            }
            else
            {
                OnInteractionFailed?.Invoke(interactable, character);
            }
        }
        
        /// <summary>
        /// Get all nearby interactables
        /// </summary>
        /// <returns>List of nearby interactables</returns>
        public List<IInteractable> GetNearbyInteractables()
        {
            return new List<IInteractable>(nearbyInteractables);
        }
        
        /// <summary>
        /// Get currently highlighted interactable
        /// </summary>
        /// <returns>Highlighted interactable or null</returns>
        public IInteractable GetHighlightedInteractable()
        {
            return highlightedInteractable;
        }
        
        /// <summary>
        /// Draw interaction prompts
        /// </summary>
        void OnGUI()
        {
            if (!showInteractionPrompts || playerCamera == null) return;
            
            // Draw prompts for nearby interactables
            foreach (IInteractable interactable in nearbyInteractables)
            {
                DrawInteractionPrompt(interactable);
            }
        }
        
        /// <summary>
        /// Draw interaction prompt for specific interactable
        /// </summary>
        /// <param name="interactable">Interactable to draw prompt for</param>
        private void DrawInteractionPrompt(IInteractable interactable)
        {
            Vector3 worldPosition = interactable.Position + Vector3.up * 2f;
            Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);
            
            // Check if on screen
            if (screenPosition.z > 0 && screenPosition.x >= 0 && screenPosition.x <= Screen.width &&
                screenPosition.y >= 0 && screenPosition.y <= Screen.height)
            {
                // Convert to GUI coordinates (flip Y)
                screenPosition.y = Screen.height - screenPosition.y;
                
                // Draw prompt
                string promptText = interactable.GetInteractionPrompt();
                if (!string.IsNullOrEmpty(promptText))
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.alignment = TextAnchor.MiddleCenter;
                    style.normal.textColor = interactable == highlightedInteractable ? Color.yellow : Color.white;
                    
                    Vector2 textSize = style.CalcSize(new GUIContent(promptText));
                    Rect promptRect = new Rect(screenPosition.x - textSize.x / 2, screenPosition.y - textSize.y / 2, 
                                             textSize.x, textSize.y);
                    
                    // Draw background
                    GUI.color = new Color(0, 0, 0, 0.7f);
                    GUI.DrawTexture(promptRect, Texture2D.whiteTexture);
                    
                    // Draw text
                    GUI.color = style.normal.textColor;
                    GUI.Label(promptRect, promptText, style);
                    
                    GUI.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Draw debug gizmos
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (selectionSystem == null || !selectionSystem.HasSelection) return;
            
            CharacterController primaryCharacter = selectionSystem.GetPrimarySelection();
            if (primaryCharacter == null) return;
            
            // Draw interaction range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(primaryCharacter.Position, interactionRange);
            
            // Draw prompt display range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(primaryCharacter.Position, promptDisplayDistance);
        }
    }
    
    /// <summary>
    /// Interface for interactable objects
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Position of the interactable
        /// </summary>
        Vector3 Position { get; }
        
        /// <summary>
        /// Check if character can interact with this object
        /// </summary>
        /// <param name="character">Character attempting interaction</param>
        /// <returns>True if interaction is possible</returns>
        bool CanInteract(CharacterController character);
        
        /// <summary>
        /// Perform interaction with character
        /// </summary>
        /// <param name="character">Character performing interaction</param>
        /// <returns>True if interaction was successful</returns>
        bool Interact(CharacterController character);
        
        /// <summary>
        /// Get interaction prompt text
        /// </summary>
        /// <returns>Prompt text to display</returns>
        string GetInteractionPrompt();
    }
}