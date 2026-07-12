// ============================================================
//  PlayerStats.cs
//  Place in: Assets/Scripts/Player/
//  Manages player health, stamina, XP, and levelling.
// ============================================================
using System.Collections;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Player
{
    public class PlayerStats : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Health")]
        [SerializeField] private float maxHealth          = 100f;
        [SerializeField] private float healthRegenRate    = 2f;    // per second when out of combat
        [SerializeField] private float regenDelay         = 5f;    // seconds after last damage

        [Header("Stamina")]
        [SerializeField] private float maxStamina         = 100f;
        [SerializeField] private float staminaDrainRun    = 15f;   // per second while running
        [SerializeField] private float staminaDrainJump   = 20f;   // flat cost per jump
        [SerializeField] private float staminaDrainAttack = 10f;   // flat cost per attack
        [SerializeField] private float staminaRegenRate   = 20f;   // per second when idle
        [SerializeField] private float staminaRegenDelay  = 1f;    // seconds after last use

        [Header("XP / Level")]
        [SerializeField] private int   startLevel         = 1;
        [SerializeField] private int   xpPerLevel         = 100;   // XP needed to level up
        [SerializeField] private float statBoostPerLevel  = 0.1f;  // +10% stats per level

        // ── Runtime State ────────────────────────────────────────
        private float _currentHealth;
        private float _currentStamina;
        private int   _currentLevel;
        private int   _currentXP;

        private bool  _isInCombat;
        private float _lastDamageTime;
        private float _lastStaminaUseTime;

        // ── Properties (read-only to external systems) ────────────
        public float CurrentHealth  => _currentHealth;
        public float MaxHealth      => maxHealth * (1 + (_currentLevel - 1) * statBoostPerLevel);
        public float CurrentStamina => _currentStamina;
        public float MaxStamina     => maxStamina * (1 + (_currentLevel - 1) * statBoostPerLevel);
        public int   CurrentLevel   => _currentLevel;
        public int   CurrentXP      => _currentXP;
        public bool  IsDead         => _currentHealth <= 0f;
        public bool  HasStamina(float amount) => _currentStamina >= amount;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _currentLevel  = startLevel;
            _currentHealth = MaxHealth;
            _currentStamina= MaxStamina;
            _currentXP     = 0;
        }

        private void Start()
        {
            // Broadcast initial values to UI
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
        }

        private void Update()
        {
            HandleHealthRegen();
            HandleStaminaRegen();
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Apply damage to the player.</summary>
        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - amount);
            _lastDamageTime = Time.time;
            _isInCombat = true;

            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlaySFX("PlayerHurt");

            if (_currentHealth <= 0f)
                Die();
        }

        /// <summary>Heal the player by amount (clamped to MaxHealth).</summary>
        public void Heal(float amount)
        {
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlaySFX("Heal");
        }

        /// <summary>Consume stamina. Returns false if insufficient.</summary>
        public bool UseStamina(float amount)
        {
            if (_currentStamina < amount) return false;

            _currentStamina = Mathf.Max(0f, _currentStamina - amount);
            _lastStaminaUseTime = Time.time;
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
            return true;
        }

        /// <summary>Drain stamina continuously (e.g. while running). Returns false when empty.</summary>
        public bool DrainStamina(float rateMultiplier = 1f)
        {
            float drain = staminaDrainRun * rateMultiplier * Time.deltaTime;
            if (_currentStamina <= 0f) return false;

            _currentStamina = Mathf.Max(0f, _currentStamina - drain);
            _lastStaminaUseTime = Time.time;
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
            return _currentStamina > 0f;
        }

        /// <summary>Award XP and check for level up.</summary>
        public void GainXP(int amount)
        {
            _currentXP += amount;
            GameEvents.TriggerPlayerXPGained(amount);

            while (_currentXP >= xpPerLevel * _currentLevel)
            {
                _currentXP -= xpPerLevel * _currentLevel;
                LevelUp();
            }
        }

        /// <summary>Fully restore health and stamina (e.g. checkpoint).</summary>
        public void FullRestore()
        {
            _currentHealth  = MaxHealth;
            _currentStamina = MaxStamina;
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
        }

        /// <summary>Notify that combat has ended (enables health regen).</summary>
        public void ExitCombat() => _isInCombat = false;

        // ── Save / Load ───────────────────────────────────────────
        public void LoadStats(float hp, float stamina, int level, int xp)
        {
            _currentLevel   = level;
            _currentXP      = xp;
            _currentHealth  = Mathf.Min(hp, MaxHealth);
            _currentStamina = Mathf.Min(stamina, MaxStamina);
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
        }

        // ── Private ───────────────────────────────────────────────

        private void HandleHealthRegen()
        {
            if (IsDead || _isInCombat) return;
            if (Time.time - _lastDamageTime < regenDelay) return;
            if (_currentHealth >= MaxHealth) return;

            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + healthRegenRate * Time.deltaTime);
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
        }

        private void HandleStaminaRegen()
        {
            if (IsDead) return;
            if (Time.time - _lastStaminaUseTime < staminaRegenDelay) return;
            if (_currentStamina >= MaxStamina) return;

            _currentStamina = Mathf.Min(MaxStamina, _currentStamina + staminaRegenRate * Time.deltaTime);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
        }

        private void LevelUp()
        {
            _currentLevel++;
            // Restore to full on level up
            _currentHealth  = MaxHealth;
            _currentStamina = MaxStamina;

            GameEvents.TriggerPlayerLevelUp(_currentLevel);
            GameEvents.TriggerPlayerHealthChanged(_currentHealth, MaxHealth);
            GameEvents.TriggerPlayerStaminaChanged(_currentStamina, MaxStamina);
            GameEvents.TriggerPlaySFX("LevelUp");
            Debug.Log($"[PlayerStats] Level Up! Now level {_currentLevel}.");
        }

        private void Die()
        {
            Debug.Log("[PlayerStats] Player died.");
            GameEvents.TriggerPlayerDied();
            GameEvents.TriggerPlaySFX("PlayerDeath");
        }
    }
}
