// ============================================================
//  TreasureChest.cs
//  Place in: Assets/Scripts/World/
//  Interactable treasure chest with loot table and open animation.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Systems;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.World
{
    public class TreasureChest : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Identity")]
        [SerializeField] private string      chestId;       // Unique per chest for save state
        [SerializeField] private float       interactRange  = 2f;

        [Header("Loot Table")]
        [SerializeField] private InventoryItem[] possibleItems;
        [SerializeField] private int[]           quantities;        // parallel to possibleItems
        [SerializeField] private int             minItems = 1;
        [SerializeField] private int             maxItems = 3;

        [Header("Animation")]
        [SerializeField] private Animator    chestAnimator;
        [SerializeField] private string      openTrigger    = "Open";

        [Header("Visual")]
        [SerializeField] private ParticleSystem openEffect;
        [SerializeField] private Renderer[]      glowRenderers;
        [SerializeField] private Color           openColor = Color.gray;

        // ── Runtime ───────────────────────────────────────────────
        private bool      _isOpen      = false;
        private Transform _player;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Start()
        {
            GameObject pg = GameObject.FindGameObjectWithTag("Player");
            if (pg != null) _player = pg.transform;

            // Restore opened state from save
            if (!string.IsNullOrEmpty(chestId) && PlayerPrefs.GetInt("Chest_" + chestId, 0) == 1)
                SetOpenedVisuals();
        }

        private void Update()
        {
            if (_isOpen || _player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= interactRange)
            {
                GameEvents.TriggerInteractionPromptChanged("[E] Open Chest");

                if (Input.GetKeyDown(KeyCode.E))
                    Open();
            }
            else
            {
                GameEvents.TriggerInteractionPromptChanged("");
            }
        }

        // ── Private ───────────────────────────────────────────────

        private void Open()
        {
            _isOpen = true;
            GameEvents.TriggerInteractionPromptChanged("");

            // Animation
            chestAnimator?.SetTrigger(openTrigger);
            openEffect?.Play();

            // Give loot
            int itemCount = Random.Range(minItems, maxItems + 1);
            itemCount = Mathf.Min(itemCount, possibleItems.Length);

            // Shuffle loot table
            ShuffleItems();

            for (int i = 0; i < itemCount; i++)
            {
                if (possibleItems[i] == null) continue;
                int qty = (quantities != null && i < quantities.Length) ? quantities[i] : 1;
                InventorySystem.Instance?.AddItem(possibleItems[i], qty);
            }

            // Save that this chest was opened
            if (!string.IsNullOrEmpty(chestId))
                PlayerPrefs.SetInt("Chest_" + chestId, 1);

            SetOpenedVisuals();
            GameEvents.TriggerPlaySFX("ChestOpen");
        }

        private void SetOpenedVisuals()
        {
            _isOpen = true;
            if (glowRenderers != null)
                foreach (var r in glowRenderers)
                    r.material.color = openColor;
        }

        private void ShuffleItems()
        {
            if (possibleItems == null) return;
            for (int i = possibleItems.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (possibleItems[i], possibleItems[j]) = (possibleItems[j], possibleItems[i]);
                if (quantities != null && i < quantities.Length && j < quantities.Length)
                    (quantities[i], quantities[j]) = (quantities[j], quantities[i]);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
