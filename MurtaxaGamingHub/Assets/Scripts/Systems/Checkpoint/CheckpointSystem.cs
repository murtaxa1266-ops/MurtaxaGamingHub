// ============================================================
//  CheckpointSystem.cs
//  Place in: Assets/Scripts/Systems/Checkpoint/
//  Manages world checkpoints (campfires, shrines, etc.).
//  Attach to individual checkpoint GameObjects in the scene.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    /// <summary>
    /// Place on each checkpoint object in the world.
    /// Tag the checkpoint GameObject as "Checkpoint".
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Identity")]
        [SerializeField] private string checkpointId;   // Unique ID per checkpoint
        [SerializeField] private string displayName = "Campfire";

        [Header("Respawn")]
        [SerializeField] private Transform respawnPoint; // Where player spawns on respawn

        [Header("Visual")]
        [SerializeField] private ParticleSystem activateEffect;
        [SerializeField] private Renderer[]      glowRenderers;
        [SerializeField] private Color           activeColor   = Color.yellow;
        [SerializeField] private Color           inactiveColor = Color.gray;

        private bool _isActive = false;

        public string CheckpointId  => checkpointId;
        public Transform RespawnPoint => respawnPoint != null ? respawnPoint : transform;

        private void Start()
        {
            SetVisualState(_isActive);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (!_isActive)
                Activate();
            else
            {
                // Already active — heal & save
                GameEvents.TriggerPlaySFX("CheckpointRest");
                CheckpointManager.Instance?.RestAtCheckpoint(checkpointId);
            }
        }

        private void Activate()
        {
            _isActive = true;
            SetVisualState(true);
            activateEffect?.Play();

            CheckpointManager.Instance?.RegisterCheckpoint(this);
            GameEvents.TriggerCheckpointReached(checkpointId);
            GameEvents.TriggerPlaySFX("CheckpointActivate");

            Debug.Log($"[Checkpoint] Activated: {displayName} ({checkpointId})");
        }

        private void SetVisualState(bool active)
        {
            Color c = active ? activeColor : inactiveColor;
            if (glowRenderers != null)
                foreach (Renderer r in glowRenderers)
                    r.material.color = c;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  CheckpointManager — global manager (singleton)
    // ──────────────────────────────────────────────────────────

    public class CheckpointManager : Singleton<CheckpointManager>
    {
        private Checkpoint        _lastCheckpoint;
        private Player.PlayerStats _playerStats;

        private void Start()
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                _playerStats = playerGO.GetComponent<Player.PlayerStats>();
        }

        private void OnEnable()  => GameEvents.OnPlayerDied += RespawnAtLastCheckpoint;
        private void OnDisable() => GameEvents.OnPlayerDied -= RespawnAtLastCheckpoint;

        /// <summary>Register a checkpoint as the new last activated.</summary>
        public void RegisterCheckpoint(Checkpoint cp)
        {
            _lastCheckpoint = cp;
            // Auto-save at each checkpoint
            SaveSystem.Instance?.Save();
        }

        /// <summary>Full restore + auto-save at this checkpoint.</summary>
        public void RestAtCheckpoint(string checkpointId)
        {
            _playerStats?.FullRestore();
            SaveSystem.Instance?.Save();
            GameEvents.TriggerPlaySFX("CheckpointRest");
        }

        /// <summary>Called when player dies — respawn at last checkpoint.</summary>
        private void RespawnAtLastCheckpoint()
        {
            if (_lastCheckpoint == null)
            {
                Debug.LogWarning("[CheckpointManager] No checkpoint registered. Loading last save.");
                GameManager.Instance?.ContinueGame();
                return;
            }

            // Teleport player to respawn point
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                UnityEngine.CharacterController cc = playerGO.GetComponent<UnityEngine.CharacterController>();
                if (cc != null) cc.enabled = false;
                playerGO.transform.position = _lastCheckpoint.RespawnPoint.position;
                if (cc != null) cc.enabled = true;
            }

            _playerStats?.FullRestore();
            GameManager.Instance?.ResumeGame();
        }
    }
}
