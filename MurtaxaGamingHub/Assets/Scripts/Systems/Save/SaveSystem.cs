// ============================================================
//  SaveSystem.cs
//  Place in: Assets/Scripts/Systems/Save/
//  JSON-based save/load using Application.persistentDataPath.
//  Works on Android (internal storage, no permissions needed).
// ============================================================
using System;
using System.IO;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    [System.Serializable]
    public class SlotSaveData
    {
        public string itemId;
        public int    quantity;
    }

    public class SaveSystem : Singleton<SaveSystem>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Save Config")]
        [SerializeField] private string saveFileName = "murtaxa_save.json";

        [Header("Item Registry (for inventory restore)")]
        [SerializeField] private InventoryItem[] allItemDefinitions;

        // ── Runtime ───────────────────────────────────────────────
        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private GameData _cachedData;
        private float    _sessionStart;

        // ── Unity Lifecycle ───────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            _sessionStart = Time.realtimeSinceStartup;
        }

        // ── Public API ────────────────────────────────────────────

        public bool HasSave() => File.Exists(SavePath);

        public void ClearSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
            _cachedData = null;
        }

        /// <summary>Gather state from all systems and write to disk.</summary>
        public void Save()
        {
            GameData data = _cachedData ?? new GameData();

            // ── Player Stats ──
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                Player.PlayerStats stats = playerGO.GetComponent<Player.PlayerStats>();
                if (stats != null)
                {
                    data.currentHealth  = stats.CurrentHealth;
                    data.currentStamina = stats.CurrentStamina;
                    data.playerLevel    = stats.CurrentLevel;
                    data.playerXP       = stats.CurrentXP;
                }

                data.posX = playerGO.transform.position.x;
                data.posY = playerGO.transform.position.y;
                data.posZ = playerGO.transform.position.z;
                data.rotY = playerGO.transform.eulerAngles.y;
            }

            // ── Inventory ──
            if (InventorySystem.Instance != null)
                data.inventorySlots = InventorySystem.Instance.GetSaveData();

            // ── Quests ──
            if (QuestSystem.Instance != null)
            {
                data.activeQuestIds    = QuestSystem.Instance.GetActiveQuestIds();
                data.completedQuestIds = new System.Collections.Generic.List<string>(QuestSystem.Instance.CompletedQuestIds);
            }

            // ── Settings ──
            data.masterVolume      = PlayerPrefs.GetFloat("MasterVolume", 1f);
            data.musicVolume       = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            data.sfxVolume         = PlayerPrefs.GetFloat("SFXVolume", 1f);
            data.qualityIndex      = PlayerPrefs.GetInt("QualityIndex", 1);
            data.touchSensitivity  = PlayerPrefs.GetFloat("TouchSensitivity", 3f);

            // ── Meta ──
            data.saveDateTime    = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.playtimeSeconds += (int)(Time.realtimeSinceStartup - _sessionStart);
            _sessionStart = Time.realtimeSinceStartup;

            // Write to disk
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
            _cachedData = data;

            Debug.Log($"[SaveSystem] Game saved to: {SavePath}");
        }

        /// <summary>Read from disk and apply state to all systems.</summary>
        public void Load()
        {
            if (!HasSave())
            {
                Debug.LogWarning("[SaveSystem] No save file found.");
                return;
            }

            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            _cachedData = data;

            // ── Player Stats ──
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                Player.PlayerStats stats = playerGO.GetComponent<Player.PlayerStats>();
                stats?.LoadStats(data.currentHealth, data.currentStamina, data.playerLevel, data.playerXP);

                // Restore position
                UnityEngine.CharacterController cc = playerGO.GetComponent<UnityEngine.CharacterController>();
                if (cc != null) cc.enabled = false;
                playerGO.transform.SetPositionAndRotation(
                    new Vector3(data.posX, data.posY, data.posZ),
                    Quaternion.Euler(0f, data.rotY, 0f));
                if (cc != null) cc.enabled = true;
            }

            // ── Inventory ──
            InventorySystem.Instance?.LoadData(data.inventorySlots, allItemDefinitions);

            // ── Quests ──
            QuestSystem.Instance?.LoadData(data.activeQuestIds, data.completedQuestIds);

            // ── Settings ──
            PlayerPrefs.SetFloat("MasterVolume",     data.masterVolume);
            PlayerPrefs.SetFloat("MusicVolume",      data.musicVolume);
            PlayerPrefs.SetFloat("SFXVolume",        data.sfxVolume);
            PlayerPrefs.SetInt  ("QualityIndex",     data.qualityIndex);
            PlayerPrefs.SetFloat("TouchSensitivity", data.touchSensitivity);
            AudioManager.Instance?.ApplyVolumes();

            Debug.Log($"[SaveSystem] Game loaded from: {SavePath}");
        }
    }
}
