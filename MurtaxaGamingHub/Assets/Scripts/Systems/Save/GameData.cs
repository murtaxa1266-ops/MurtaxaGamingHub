// ============================================================
//  GameData.cs
//  Place in: Assets/Scripts/Systems/Save/
//  Plain serializable class — the object that gets written to
//  disk by SaveSystem. No MonoBehaviour, no Unity types.
// ============================================================
using System.Collections.Generic;

namespace MurtaxaGaming.Systems
{
    [System.Serializable]
    public class GameData
    {
        // ── Player ────────────────────────────────────────────────
        public float currentHealth;
        public float currentStamina;
        public int   playerLevel;
        public int   playerXP;

        // ── Player Position (world) ───────────────────────────────
        public float posX, posY, posZ;
        public float rotY;

        // ── Inventory ─────────────────────────────────────────────
        public List<SaveSystem.SlotSaveData> inventorySlots = new List<SaveSystem.SlotSaveData>();

        // ── Quests ────────────────────────────────────────────────
        public List<string> activeQuestIds    = new List<string>();
        public List<string> completedQuestIds = new List<string>();

        // ── World State ───────────────────────────────────────────
        public List<string> openedChests      = new List<string>(); // chest IDs that were opened
        public List<string> collectedItems    = new List<string>(); // world item instance IDs

        // ── Settings ─────────────────────────────────────────────
        public float masterVolume   = 1f;
        public float musicVolume    = 0.8f;
        public float sfxVolume      = 1f;
        public int   qualityIndex   = 1;  // 0=Low, 1=Medium, 2=High
        public float touchSensitivity = 3f;

        // ── Meta ──────────────────────────────────────────────────
        public string saveVersion = "1.0";
        public string saveDateTime;
        public int    playtimeSeconds;
    }
}
