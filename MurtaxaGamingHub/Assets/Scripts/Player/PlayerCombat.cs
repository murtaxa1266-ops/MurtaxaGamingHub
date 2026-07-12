// ============================================================
//  PlayerCombat.cs
//  Place in: Assets/Scripts/Player/
//  Melee attack system with combo window, hit detection, and
//  stamina gating. Works alongside PlayerController & PlayerStats.
// ============================================================
using System.Collections;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Player
{
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Attack Settings")]
        [SerializeField] private float attackDamage     = 25f;
        [SerializeField] private float attackRange      = 1.5f;
        [SerializeField] private float attackAngle      = 120f;    // cone in front of player
        [SerializeField] private float attackCooldown   = 0.5f;
        [SerializeField] private float comboWindowTime  = 0.8f;    // seconds to press again for combo
        [SerializeField] private int   maxComboSteps    = 3;
        [SerializeField] private LayerMask enemyLayers;

        [Header("Attack Force")]
        [SerializeField] private float knockbackForce   = 5f;

        [Header("Stamina Cost")]
        [SerializeField] private float staminaPerAttack = 10f;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem hitParticle;  // Assign in inspector

        // ── Components ────────────────────────────────────────────
        private PlayerStats _stats;
        private Animator    _anim;

        // ── Runtime State ────────────────────────────────────────
        private int   _comboStep      = 0;
        private float _lastAttackTime = -10f;
        private bool  _inComboWindow  = false;
        private bool  _isAttacking    = false;
        private bool  _isDead         = false;

        // Animator parameter hashes
        private static readonly int _hashAttack1 = Animator.StringToHash("Attack1");
        private static readonly int _hashAttack2 = Animator.StringToHash("Attack2");
        private static readonly int _hashAttack3 = Animator.StringToHash("Attack3");

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _anim  = GetComponent<Animator>();
        }

        private void OnEnable()  => GameEvents.OnPlayerDied += OnDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= OnDied;

        private void Update()
        {
            if (_isDead || _isAttacking) return;

            if (Input.GetButtonDown("Fire1"))
                TryAttack();
        }

        // ── Attack Logic ──────────────────────────────────────────

        private void TryAttack()
        {
            // Check cooldown
            if (Time.time - _lastAttackTime < attackCooldown) return;

            // Check stamina
            if (!_stats.UseStamina(staminaPerAttack))
            {
                GameEvents.TriggerPlaySFX("StaminaEmpty");
                return;
            }

            // Combo logic
            if (_inComboWindow && _comboStep < maxComboSteps)
                _comboStep++;
            else
                _comboStep = 1;

            StartCoroutine(PerformAttack(_comboStep));
        }

        private IEnumerator PerformAttack(int step)
        {
            _isAttacking    = true;
            _inComboWindow  = false;
            _lastAttackTime = Time.time;

            // Trigger correct combo animation
            switch (step)
            {
                case 1: _anim.SetTrigger(_hashAttack1); GameEvents.TriggerPlaySFX("Attack1"); break;
                case 2: _anim.SetTrigger(_hashAttack2); GameEvents.TriggerPlaySFX("Attack2"); break;
                case 3: _anim.SetTrigger(_hashAttack3); GameEvents.TriggerPlaySFX("Attack3"); break;
            }

            // Wait half of cooldown before detecting hits (animation wind-up)
            yield return new WaitForSeconds(attackCooldown * 0.5f);

            DetectHits();

            // Open combo window in second half
            _isAttacking   = false;
            _inComboWindow = true;

            yield return new WaitForSeconds(comboWindowTime);
            _inComboWindow = false;
            _comboStep     = 0;
        }

        // ── Hit Detection ─────────────────────────────────────────

        private void DetectHits()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);
            bool hitAny = false;

            foreach (Collider col in hits)
            {
                // Check angle (cone in front)
                Vector3 toTarget = (col.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, toTarget);
                if (angle > attackAngle * 0.5f) continue;

                // Deal damage
                Enemy.EnemyStats enemyStats = col.GetComponent<Enemy.EnemyStats>();
                if (enemyStats != null)
                {
                    enemyStats.TakeDamage(attackDamage, transform);
                    hitAny = true;

                    // Knockback
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    rb?.AddForce(toTarget * knockbackForce, ForceMode.Impulse);

                    // Hit particle
                    if (hitParticle != null)
                        hitParticle.transform.position = col.ClosestPoint(transform.position);
                    hitParticle?.Play();
                }
            }

            GameEvents.TriggerPlaySFX(hitAny ? "HitConnect" : "HitMiss");
        }

        private void OnDied() => _isDead = true;

        // ── Gizmos ────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
