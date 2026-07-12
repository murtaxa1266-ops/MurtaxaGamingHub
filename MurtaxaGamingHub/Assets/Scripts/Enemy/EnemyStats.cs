// ============================================================
//  EnemyStats.cs
//  Place in: Assets/Scripts/Enemy/
//  Enemy health, damage intake, death, and reward drops.
// ============================================================
using System.Collections;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Enemy
{
    public class EnemyStats : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Stats")]
        [SerializeField] private float maxHealth     = 60f;
        [SerializeField] private float currentHealth;

        [Header("Rewards")]
        [SerializeField] private int   xpReward      = 30;
        [SerializeField] private GameObject[] dropPrefabs;    // Items to drop on death
        [SerializeField] [Range(0f, 1f)] private float dropChance = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject damageNumberPrefab;  // Optional floating damage text
        [SerializeField] private Renderer   bodyRenderer;
        [SerializeField] private Color      hitFlashColor = Color.red;
        [SerializeField] private float      hitFlashDuration = 0.15f;

        // ── Runtime State ────────────────────────────────────────
        private bool        _isDead;
        private EnemyAI     _ai;
        private Animator    _anim;
        private Color       _originalColor;

        private static readonly int _hashDie = Animator.StringToHash("Die");

        public bool IsDead => _isDead;
        public float HealthPercent => currentHealth / maxHealth;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            currentHealth = maxHealth;
            _ai   = GetComponent<EnemyAI>();
            _anim = GetComponent<Animator>();

            if (bodyRenderer != null)
                _originalColor = bodyRenderer.material.color;
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Apply damage from attacker. attacker may be null.</summary>
        public void TakeDamage(float amount, Transform attacker = null)
        {
            if (_isDead) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            // Alert AI to attacker
            if (attacker != null && _ai != null)
                _ai.AlertToTarget(attacker);

            StartCoroutine(HitFlash());

            if (currentHealth <= 0f)
                Die();
        }

        /// <summary>Instantly kill the enemy.</summary>
        public void InstantKill()
        {
            if (_isDead) return;
            currentHealth = 0f;
            Die();
        }

        // ── Private ───────────────────────────────────────────────

        private void Die()
        {
            _isDead = true;

            // Disable AI
            if (_ai != null) _ai.enabled = false;

            // Play death animation
            if (_anim != null) _anim.SetTrigger(_hashDie);

            // Fire global event
            GameEvents.TriggerEnemyDied(gameObject, transform.position, xpReward);
            GameEvents.TriggerPlaySFX("EnemyDeath");

            // Drop items
            TryDropLoot();

            // Destroy after animation plays
            Destroy(gameObject, 3f);
        }

        private void TryDropLoot()
        {
            if (dropPrefabs == null || dropPrefabs.Length == 0) return;

            if (Random.value <= dropChance)
            {
                GameObject drop = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
                if (drop != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                    Instantiate(drop, spawnPos, Quaternion.identity);
                }
            }
        }

        private IEnumerator HitFlash()
        {
            if (bodyRenderer == null) yield break;

            bodyRenderer.material.color = hitFlashColor;
            yield return new WaitForSeconds(hitFlashDuration);
            if (bodyRenderer != null)
                bodyRenderer.material.color = _originalColor;
        }
    }
}
