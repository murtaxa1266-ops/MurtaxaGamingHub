// ============================================================
//  InventoryUI.cs
//  Place in: Assets/Scripts/UI/
//  Renders the inventory grid, item tooltips, and use/drop actions.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Systems;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.UI
{
    public class InventoryUI : Singleton<InventoryUI>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Grid")]
        [SerializeField] private Transform       slotContainer;    // Parent for slot prefabs
        [SerializeField] private GameObject      slotPrefab;       // Prefab: Image+Text+Button

        [Header("Tooltip")]
        [SerializeField] private GameObject      tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipName;
        [SerializeField] private TextMeshProUGUI tooltipDesc;
        [SerializeField] private TextMeshProUGUI tooltipStats;

        [Header("Action Buttons")]
        [SerializeField] private Button          useButton;
        [SerializeField] private Button          dropButton;
        [SerializeField] private Button          closeButton;

        [Header("Input Binding")]
        [SerializeField] private KeyCode         toggleKey = KeyCode.I;

        // ── Runtime State ────────────────────────────────────────
        private List<GameObject>    _slotObjects = new List<GameObject>();
        private int                 _selectedSlot = -1;
        private bool                _isOpen = false;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            closeButton?.onClick.AddListener(() => Toggle(false));
            useButton  ?.onClick.AddListener(OnUseSelected);
            dropButton ?.onClick.AddListener(OnDropSelected);

            if (tooltipPanel != null) tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                Toggle(!_isOpen);
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Rebuild all inventory slots from current data.</summary>
        public void Refresh()
        {
            if (!_isOpen) return;

            // Clear old slots
            foreach (var go in _slotObjects) Destroy(go);
            _slotObjects.Clear();
            _selectedSlot = -1;
            HideTooltip();

            if (InventorySystem.Instance == null) return;

            var slots = InventorySystem.Instance.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                int index = i; // capture for closure
                GameObject slotGO = Instantiate(slotPrefab, slotContainer);
                _slotObjects.Add(slotGO);

                // Icon
                Image icon = slotGO.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && slots[i].item.icon != null)
                    icon.sprite = slots[i].item.icon;

                // Quantity text
                TextMeshProUGUI qty = slotGO.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
                if (qty != null)
                    qty.text = slots[i].quantity > 1 ? slots[i].quantity.ToString() : "";

                // Click to select
                Button btn = slotGO.GetComponent<Button>();
                btn?.onClick.AddListener(() => SelectSlot(index));
            }
        }

        public void Toggle(bool open)
        {
            _isOpen = open;
            gameObject.SetActive(open);
            GameEvents.TriggerInventoryToggled(open);

            if (open)
            {
                Refresh();
                GameEvents.TriggerPlaySFX("UIOpen");
                // Pause time-scale not needed; inventory is non-blocking in this design
            }
            else
            {
                HideTooltip();
                GameEvents.TriggerPlaySFX("UIClose");
            }
        }

        // ── Private ───────────────────────────────────────────────

        private void SelectSlot(int index)
        {
            _selectedSlot = index;
            GameEvents.TriggerPlaySFX("UIClick");

            var slots = InventorySystem.Instance?.Slots;
            if (slots == null || index >= slots.Count) return;

            var slot = slots[index];
            ShowTooltip(slot.item, slot.quantity);

            bool canUse = slot.item.itemType == ItemType.Consumable;
            useButton?.gameObject.SetActive(canUse);
        }

        private void ShowTooltip(InventoryItem item, int qty)
        {
            if (tooltipPanel == null) return;
            tooltipPanel.SetActive(true);

            if (tooltipName != null) tooltipName.text = item.displayName;
            if (tooltipDesc != null) tooltipDesc.text = item.description;

            string stats = "";
            if (item.healthRestore  > 0) stats += $"+{item.healthRestore} HP\n";
            if (item.staminaRestore > 0) stats += $"+{item.staminaRestore} Stamina\n";
            if (item.attackBonus    > 0) stats += $"+{item.attackBonus} Attack\n";
            if (item.defenseBonus   > 0) stats += $"+{item.defenseBonus} Defense\n";
            if (item.xpGrant        > 0) stats += $"+{item.xpGrant} XP\n";
            if (tooltipStats != null) tooltipStats.text = stats;
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null) tooltipPanel.SetActive(false);
            _selectedSlot = -1;
        }

        private void OnUseSelected()
        {
            if (_selectedSlot < 0) return;
            InventorySystem.Instance?.UseItem(_selectedSlot);
            Refresh();
        }

        private void OnDropSelected()
        {
            if (_selectedSlot < 0) return;
            var slots = InventorySystem.Instance?.Slots;
            if (slots == null || _selectedSlot >= slots.Count) return;

            InventorySystem.Instance?.RemoveItem(slots[_selectedSlot].item.itemId, 1);
            GameEvents.TriggerPlaySFX("ItemDrop");
            Refresh();
        }
    }
}
