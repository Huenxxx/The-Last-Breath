using UnityEngine;
using TheLastBreath.Characters;
using TheLastBreath.Systems;

namespace TheLastBreath.Systems
{
    /// <summary>
    /// Basic lootable item that can be picked up by characters
    /// </summary>
    public class LootableItem : MonoBehaviour, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private string itemName = "Item";
        [SerializeField] private string itemDescription = "A basic item";
        [SerializeField] private int quantity = 1;
        [SerializeField] private bool destroyOnPickup = true;
        
        [Header("Visual Settings")]
        [SerializeField] private GameObject visualModel;
        [SerializeField] private float bobHeight = 0.2f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private bool enableBobbing = true;
        
        [Header("Audio")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private float soundVolume = 0.5f;
        
        // State
        private Vector3 startPosition;
        private bool isPickedUp = false;
        private AudioSource audioSource;
        
        // IInteractable implementation
        public Vector3 Position => transform.position;
        
        // Properties
        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public int Quantity => quantity;
        public bool IsPickedUp => isPickedUp;
        
        /// <summary>
        /// Initialize lootable item
        /// </summary>
        void Start()
        {
            startPosition = transform.position;
            
            // Set up audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && pickupSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = soundVolume;
            }
            
            // Ensure we have a collider for interaction
            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                col = gameObject.AddComponent<SphereCollider>();
                col.isTrigger = true;
            }
        }
        
        /// <summary>
        /// Update item behavior
        /// </summary>
        void Update()
        {
            if (isPickedUp) return;
            
            // Handle bobbing animation
            if (enableBobbing)
            {
                UpdateBobbing();
            }
        }
        
        /// <summary>
        /// Update bobbing animation
        /// </summary>
        private void UpdateBobbing()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        
        /// <summary>
        /// Check if character can interact with this item
        /// </summary>
        /// <param name="character">Character attempting interaction</param>
        /// <returns>True if interaction is possible</returns>
        public bool CanInteract(CharacterController character)
        {
            if (isPickedUp) return false;
            if (character == null) return false;
            if (!character.IsPlayerControlled) return false;
            
            return true;
        }
        
        /// <summary>
        /// Perform interaction (pickup)
        /// </summary>
        /// <param name="character">Character performing interaction</param>
        /// <returns>True if interaction was successful</returns>
        public bool Interact(CharacterController character)
        {
            if (!CanInteract(character)) return false;
            
            // Perform pickup
            return PickupItem(character);
        }
        
        /// <summary>
        /// Get interaction prompt text
        /// </summary>
        /// <returns>Prompt text to display</returns>
        public string GetInteractionPrompt()
        {
            if (isPickedUp) return "";
            
            string quantityText = quantity > 1 ? $" ({quantity})" : "";
            return $"[E] Pick up {itemName}{quantityText}";
        }
        
        /// <summary>
        /// Pick up the item
        /// </summary>
        /// <param name="character">Character picking up the item</param>
        /// <returns>True if pickup was successful</returns>
        private bool PickupItem(CharacterController character)
        {
            if (isPickedUp) return false;
            
            // Mark as picked up
            isPickedUp = true;
            
            // Play pickup sound
            PlayPickupSound();
            
            // TODO: Add item to character's inventory when inventory system is implemented
            Debug.Log($"{character.name} picked up {itemName} (x{quantity})");
            
            // Handle visual feedback
            StartCoroutine(PickupAnimation());
            
            return true;
        }
        
        /// <summary>
        /// Play pickup sound effect
        /// </summary>
        private void PlayPickupSound()
        {
            if (audioSource != null && pickupSound != null)
            {
                audioSource.clip = pickupSound;
                audioSource.Play();
            }
        }
        
        /// <summary>
        /// Pickup animation coroutine
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        private System.Collections.IEnumerator PickupAnimation()
        {
            float animationTime = 0.5f;
            float elapsedTime = 0f;
            
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;
            
            // Disable collider to prevent further interactions
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            // Animate pickup
            while (elapsedTime < animationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / animationTime;
                
                // Ease out curve
                t = 1f - (1f - t) * (1f - t);
                
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                
                yield return null;
            }
            
            // Destroy or hide the item
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Set item properties
        /// </summary>
        /// <param name="name">Item name</param>
        /// <param name="description">Item description</param>
        /// <param name="qty">Item quantity</param>
        public void SetItemProperties(string name, string description, int qty = 1)
        {
            itemName = name;
            itemDescription = description;
            quantity = qty;
        }
        
        /// <summary>
        /// Reset item to initial state
        /// </summary>
        public void ResetItem()
        {
            isPickedUp = false;
            transform.position = startPosition;
            transform.localScale = Vector3.one;
            
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
            
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Draw debug information
        /// </summary>
        void OnDrawGizmosSelected()
        {
            // Draw interaction sphere
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Draw item info
            if (Application.isPlaying)
            {
                Vector3 labelPosition = transform.position + Vector3.up * 1.5f;
                UnityEditor.Handles.Label(labelPosition, $"{itemName} (x{quantity})");
            }
        }
    }
}