// ============================================================
//  ItemPickup.cs
//  Place in: Assets/Scripts/Systems/
//  Attach to any world item that the player can walk over
//  to auto-collect, or press [E] to pick up manually.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    public class ItemPickup : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Item")]
        [SerializeField] private InventoryItem item;
        [SerializeField] private int           quantity     = 1;

        [Header("Collection Mode")]
        [SerializeField] private bool autoCollect          = false; // if true, collect on trigger enter
        [SerializeField] private float interactRange       = 2f;

        [Header("Visual / Animation")]
        [SerializeField] private float bobAmplitude        = 0.15f;
        [SerializeField] private float bobFrequency        = 1.5f;
        [SerializeField] private float rotateSpeed         = 60f;

        [Header("Unique World ID (for save system)")]
        [SerializeField] private string worldItemId;       // Set unique per instance

        // ── Runtime State ────────────────────────────────────────
        private Vector3   _startPos;
        private bool      _collected = false;
        private Transform _player;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Start()
        {
            _startPos = transform.position;
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;

            // Check if this item was already collected in a previous session
            if (!string.IsNullOrEmpty(worldItemId) && PlayerPrefs.GetInt(worldItemId, 0) == 1)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            if (_collected) return;

            // Bob and rotate
            float newY = _startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

            // Manual pickup
            if (!autoCollect && _player != null)
            {
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist <= interactRange)
                {
                    GameEvents.TriggerInteractionPromptChanged($"[E] Pick up {item?.displayName}");

                    if (Input.GetKeyDown(KeyCode.E))
                        Collect();
                }
                else
                {
                    GameEvents.TriggerInteractionPromptChanged("");
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!autoCollect || _collected) return;
            if (!other.CompareTag("Player")) return;
            Collect();
        }

        private void Collect()
        {
            if (_collected || item == null) return;

            bool added = InventorySystem.Instance?.AddItem(item, quantity) ?? false;
            if (!added) return; // Inventory full

            _collected = true;

            // Mark as collected in save data
            if (!string.IsNullOrEmpty(worldItemId))
                PlayerPrefs.SetInt(worldItemId, 1);

            GameEvents.TriggerInteractionPromptChanged("");
            GameEvents.TriggerPlaySFX(item.pickupSFX);

            // Visual: quick pop effect could go here
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
