// ============================================================
//  GameEvents.cs
//  Place in: Assets/Scripts/Utils/
//  Centralized event system to decouple game systems.
//  Any script can subscribe/unsubscribe without direct references.
// ============================================================
using System;
using UnityEngine;

namespace MurtaxaGaming.Utils
{
    public static class GameEvents
    {
        // ── Player Events ────────────────────────────────────────
        /// <summary>Fired when player health changes. (currentHP, maxHP)</summary>
        public static event Action<float, float> OnPlayerHealthChanged;
        /// <summary>Fired when player stamina changes. (currentStamina, maxStamina)</summary>
        public static event Action<float, float> OnPlayerStaminaChanged;
        /// <summary>Fired when player dies.</summary>
        public static event Action OnPlayerDied;
        /// <summary>Fired when player collects an item. (itemName)</summary>
        public static event Action<string> OnItemCollected;
        /// <summary>Fired when player levels up. (newLevel)</summary>
        public static event Action<int> OnPlayerLevelUp;
        /// <summary>Fired when player gains XP. (amount)</summary>
        public static event Action<int> OnPlayerXPGained;

        // ── Combat Events ────────────────────────────────────────
        /// <summary>Fired when an enemy dies. (enemyGO, position, xpReward)</summary>
        public static event Action<GameObject, Vector3, int> OnEnemyDied;
        /// <summary>Fired when combat starts.</summary>
        public static event Action OnCombatStarted;
        /// <summary>Fired when combat ends.</summary>
        public static event Action OnCombatEnded;

        // ── Quest Events ─────────────────────────────────────────
        /// <summary>Fired when a quest is accepted. (questId)</summary>
        public static event Action<string> OnQuestAccepted;
        /// <summary>Fired when a quest objective is updated. (questId, objectiveIndex, progress)</summary>
        public static event Action<string, int, int> OnQuestObjectiveUpdated;
        /// <summary>Fired when a quest is completed. (questId)</summary>
        public static event Action<string> OnQuestCompleted;

        // ── World Events ─────────────────────────────────────────
        /// <summary>Fired when a checkpoint is reached. (checkpointId)</summary>
        public static event Action<string> OnCheckpointReached;
        /// <summary>Fired when a scene/zone changes. (zoneName)</summary>
        public static event Action<string> OnZoneChanged;
        /// <summary>Fired when game is saved.</summary>
        public static event Action OnGameSaved;
        /// <summary>Fired when game is loaded.</summary>
        public static event Action OnGameLoaded;

        // ── UI Events ────────────────────────────────────────────
        /// <summary>Fired to open/close inventory. (isOpen)</summary>
        public static event Action<bool> OnInventoryToggled;
        /// <summary>Fired to open/close quest log. (isOpen)</summary>
        public static event Action<bool> OnQuestLogToggled;
        /// <summary>Fired to open/close map. (isOpen)</summary>
        public static event Action<bool> OnMapToggled;
        /// <summary>Fired to show/hide interaction prompt. (message)</summary>
        public static event Action<string> OnInteractionPromptChanged;

        // ── Audio Events ─────────────────────────────────────────
        /// <summary>Fired to play a named SFX. (sfxName)</summary>
        public static event Action<string> OnPlaySFX;
        /// <summary>Fired to change background music. (trackName)</summary>
        public static event Action<string> OnMusicChanged;

        // ── Invoke Helpers (safe null-check) ─────────────────────
        public static void TriggerPlayerHealthChanged(float cur, float max)    => OnPlayerHealthChanged?.Invoke(cur, max);
        public static void TriggerPlayerStaminaChanged(float cur, float max)   => OnPlayerStaminaChanged?.Invoke(cur, max);
        public static void TriggerPlayerDied()                                  => OnPlayerDied?.Invoke();
        public static void TriggerItemCollected(string name)                   => OnItemCollected?.Invoke(name);
        public static void TriggerPlayerLevelUp(int lvl)                       => OnPlayerLevelUp?.Invoke(lvl);
        public static void TriggerPlayerXPGained(int xp)                       => OnPlayerXPGained?.Invoke(xp);

        public static void TriggerEnemyDied(GameObject go, Vector3 pos, int xp) => OnEnemyDied?.Invoke(go, pos, xp);
        public static void TriggerCombatStarted()                               => OnCombatStarted?.Invoke();
        public static void TriggerCombatEnded()                                 => OnCombatEnded?.Invoke();

        public static void TriggerQuestAccepted(string id)                     => OnQuestAccepted?.Invoke(id);
        public static void TriggerQuestObjectiveUpdated(string id, int idx, int prog) => OnQuestObjectiveUpdated?.Invoke(id, idx, prog);
        public static void TriggerQuestCompleted(string id)                    => OnQuestCompleted?.Invoke(id);

        public static void TriggerCheckpointReached(string id)                 => OnCheckpointReached?.Invoke(id);
        public static void TriggerZoneChanged(string zone)                     => OnZoneChanged?.Invoke(zone);
        public static void TriggerGameSaved()                                   => OnGameSaved?.Invoke();
        public static void TriggerGameLoaded()                                  => OnGameLoaded?.Invoke();

        public static void TriggerInventoryToggled(bool open)                  => OnInventoryToggled?.Invoke(open);
        public static void TriggerQuestLogToggled(bool open)                   => OnQuestLogToggled?.Invoke(open);
        public static void TriggerMapToggled(bool open)                        => OnMapToggled?.Invoke(open);
        public static void TriggerInteractionPromptChanged(string msg)         => OnInteractionPromptChanged?.Invoke(msg);

        public static void TriggerPlaySFX(string sfx)                         => OnPlaySFX?.Invoke(sfx);
        public static void TriggerMusicChanged(string track)                   => OnMusicChanged?.Invoke(track);

        /// <summary>
        /// Call on scene load to clear all event subscriptions and prevent stale references.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnPlayerHealthChanged  = null; OnPlayerStaminaChanged = null; OnPlayerDied       = null;
            OnItemCollected        = null; OnPlayerLevelUp        = null; OnPlayerXPGained   = null;
            OnEnemyDied            = null; OnCombatStarted        = null; OnCombatEnded      = null;
            OnQuestAccepted        = null; OnQuestObjectiveUpdated= null; OnQuestCompleted   = null;
            OnCheckpointReached    = null; OnZoneChanged          = null; OnGameSaved        = null;
            OnGameLoaded           = null; OnInventoryToggled     = null; OnQuestLogToggled  = null;
            OnMapToggled           = null; OnInteractionPromptChanged = null;
            OnPlaySFX              = null; OnMusicChanged         = null;
        }
    }
}
