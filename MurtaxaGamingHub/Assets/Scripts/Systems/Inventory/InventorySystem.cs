// ============================================================
//  InventorySystem.cs
//  Place in: Assets/Scripts/Systems/Inventory/
//  Manages the player's inventory: add, remove, use, stack items.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    [System.Serializable]
    public class InventorySlot
    {
        public InventoryItem item;
        public int           quantity;

        public InventorySlot(InventoryItem item, int qty)
        {
            this.item     = item;
            this.quantity = qty;
        }
    }

    public class InventorySystem : Singleton<InventorySystem>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Capacity")]
        [SerializeField] private int maxSlots = 30;

        // ── Runtime State ────────────────────────────────────────
        private List<InventorySlot> _slots = new List<InventorySlot>();
        public  IReadOnlyList<InventorySlot> Slots => _slots.AsReadOnly();

        // Reference to player stats for applying consumable effects
        private Player.PlayerStats _playerStats;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Start()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _playerStats = playerGO.GetComponent<Player.PlayerStats>();
        }

        private void OnEnable()
        {
            GameEvents.OnInventoryToggled += HandleToggle;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryToggled -= HandleToggle;
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Add qty of item to inventory. Returns false if full.</summary>
        public bool AddItem(InventoryItem item, int qty = 1)
        {
            if (item == null) return false;

            // Try to stack into existing slot
            if (item.isStackable)
            {
                InventorySlot existing = _slots.Find(s => s.item.itemId == item.itemId && s.quantity < item.maxStackSize);
                if (existing != null)
                {
                    int space = item.maxStackSize - existing.quantity;
                    int added = Mathf.Min(qty, space);
                    existing.quantity += added;

                    int remaining = qty - added;
                    if (remaining > 0) AddItem(item, remaining); // overflow to new slot

                    NotifyChanged();
                    GameEvents.TriggerItemCollected(item.displayName);
                    return true;
                }
            }

            // New slot
            if (_slots.Count >= maxSlots)
            {
                Debug.LogWarning("[InventorySystem] Inventory full.");
                return false;
            }

            _slots.Add(new InventorySlot(item, qty));
            NotifyChanged();
            GameEvents.TriggerItemCollected(item.displayName);
            GameEvents.TriggerPlaySFX(item.pickupSFX);
            return true;
        }

        /// <summary>Remove qty of item by itemId. Returns false if not found.</summary>
        public bool RemoveItem(string itemId, int qty = 1)
        {
            InventorySlot slot = _slots.Find(s => s.item.itemId == itemId);
            if (slot == null) return false;

            slot.quantity -= qty;
            if (slot.quantity <= 0)
                _slots.Remove(slot);

            NotifyChanged();
            return true;
        }

        /// <summary>Use a consumable item from the slot at given index.</summary>
        public void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return;

            InventorySlot slot = _slots[slotIndex];
            InventoryItem item = slot.item;

            if (item.itemType != ItemType.Consumable)
            {
                Debug.Log($"[InventorySystem] {item.displayName} is not consumable.");
                return;
            }

            // Apply effects
            if (_playerStats != null)
            {
                if (item.healthRestore  > 0f) _playerStats.Heal(item.healthRestore);
                if (item.staminaRestore > 0f) _playerStats.UseStamina(-item.staminaRestore); // negative = restore
                if (item.xpGrant        > 0)  _playerStats.GainXP(item.xpGrant);
            }

            GameEvents.TriggerPlaySFX(item.useSFX);
            RemoveItem(item.itemId, 1);
        }

        /// <summary>Check if player has at least qty of itemId.</summary>
        public bool HasItem(string itemId, int qty = 1)
        {
            InventorySlot slot = _slots.Find(s => s.item.itemId == itemId);
            return slot != null && slot.quantity >= qty;
        }

        /// <summary>Get total count of a specific item.</summary>
        public int GetItemCount(string itemId)
        {
            int total = 0;
            foreach (var slot in _slots)
                if (slot.item.itemId == itemId) total += slot.quantity;
            return total;
        }

        /// <summary>Clear all items (used on new game).</summary>
        public void Clear() { _slots.Clear(); NotifyChanged(); }

        // ── Save / Load Helpers ───────────────────────────────────

        public List<SaveSystem.SlotSaveData> GetSaveData()
        {
            var data = new List<SaveSystem.SlotSaveData>();
            foreach (var slot in _slots)
                data.Add(new SaveSystem.SlotSaveData { itemId = slot.item.itemId, quantity = slot.quantity });
            return data;
        }

        public void LoadData(List<SaveSystem.SlotSaveData> data, InventoryItem[] allItems)
        {
            _slots.Clear();
            foreach (var slotData in data)
            {
                InventoryItem item = System.Array.Find(allItems, i => i.itemId == slotData.itemId);
                if (item != null) _slots.Add(new InventorySlot(item, slotData.quantity));
            }
            NotifyChanged();
        }

        // ── Private ───────────────────────────────────────────────
        private void NotifyChanged() => UI.InventoryUI.Instance?.Refresh();
        private void HandleToggle(bool open) { /* handled by InventoryUI */ }
    }
}
