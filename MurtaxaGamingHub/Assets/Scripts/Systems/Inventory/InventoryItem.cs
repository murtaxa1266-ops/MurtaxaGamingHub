// ============================================================
//  InventoryItem.cs
//  Place in: Assets/Scripts/Systems/Inventory/
//  ScriptableObject definition for any collectible item.
//  Create instances via: Assets > Create > MurtaxaGaming > Item
// ============================================================
using UnityEngine;

namespace MurtaxaGaming.Systems
{
    public enum ItemType { Consumable, Weapon, Armor, Key, Treasure, Quest }

    [CreateAssetMenu(fileName = "NewItem", menuName = "MurtaxaGaming/Item", order = 0)]
    public class InventoryItem : ScriptableObject
    {
        [Header("Identity")]
        public string   itemId;           // Unique string ID (e.g. "health_potion_small")
        public string   displayName;
        [TextArea(2,4)]
        public string   description;
        public Sprite   icon;

        [Header("Type & Stack")]
        public ItemType itemType = ItemType.Consumable;
        public bool     isStackable = true;
        public int      maxStackSize = 99;

        [Header("Stats (for Consumables)")]
        public float    healthRestore   = 0f;
        public float    staminaRestore  = 0f;
        public int      xpGrant         = 0;

        [Header("Combat Stats (for Weapons/Armor)")]
        public float    attackBonus     = 0f;
        public float    defenseBonus    = 0f;

        [Header("Economy")]
        public int      sellValue       = 10;

        [Header("Audio")]
        public string   useSFX          = "ItemUse";
        public string   pickupSFX       = "ItemPickup";
    }
}
