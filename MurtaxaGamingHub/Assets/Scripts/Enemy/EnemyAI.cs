// ============================================================
//  EnemyAI.cs
//  Place in: Assets/Scripts/Enemy/
//  Finite-state machine: Patrol → Chase → Attack → Search
//  Requires: NavMeshAgent, EnemyStats, Animator on same GO.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(Animator))]
    public class EnemyAI : MonoBehaviour
    {
        // ── State Machine ─────────────────────────────────────────
        public enum State { Idle, Patrol, Chase, Attack, Search, Dead }

        // ── Inspector ────────────────────────────────────────────
        [Header("Detection")]
        [SerializeField] private float sightRange      = 12f;
        [SerializeField] private float sightAngle      = 110f;    // full cone angle
        [SerializeField] private float hearingRange    = 6f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float       patrolWaitTime  = 2f;
        [SerializeField] private float       patrolSpeed     = 2.5f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed      = 5f;
        [SerializeField] private float loseTargetTime  = 4f;     // seconds before giving up

        [Header("Attack")]
        [SerializeField] private float attackRange     = 1.8f;
        [SerializeField] private float attackDamage    = 15f;
        [SerializeField] private float attackCooldown  = 1.5f;

        [Header("Search")]
        [SerializeField] private float searchDuration  = 5f;
        [SerializeField] private float searchRadius    = 8f;

        // ── Components ────────────────────────────────────────────
        private NavMeshAgent _agent;
        private EnemyStats   _stats;
        private Animator     _anim;

        // ── Runtime State ────────────────────────────────────────
        private State   _currentState   = State.Patrol;
        private int     _patrolIndex    = 0;
        private bool    _waitingAtPoint = false;
        private float   _lastAttackTime = -10f;
        private float   _lostTargetTimer= 0f;
        private float   _searchTimer    = 0f;
        private Vector3 _lastKnownPos;
        private Transform _target;       // Player transform

        // Animator hashes
        private static readonly int _hashSpeed  = Animator.StringToHash("Speed");
        private static readonly int _hashAttack = Animator.StringToHash("Attack");
        private static readonly int _hashAlert  = Animator.StringToHash("Alert");

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _stats = GetComponent<EnemyStats>();
            _anim  = GetComponent<Animator>();

            _agent.speed = patrolSpeed;
        }

        private void Update()
        {
            if (_stats.IsDead) return;

            switch (_currentState)
            {
                case State.Idle:   UpdateIdle();   break;
                case State.Patrol: UpdatePatrol(); break;
                case State.Chase:  UpdateChase();  break;
                case State.Attack: UpdateAttack(); break;
                case State.Search: UpdateSearch(); break;
            }

            UpdateAnimator();
        }

        // ── State Updates ─────────────────────────────────────────

        private void UpdateIdle()
        {
            if (CanSeePlayer())
                TransitionTo(State.Chase);
            else if (patrolPoints.Length > 0)
                TransitionTo(State.Patrol);
        }

        private void UpdatePatrol()
        {
            if (CanSeePlayer() || CanHearPlayer())
            {
                TransitionTo(State.Chase);
                return;
            }

            if (patrolPoints.Length == 0) return;

            if (!_waitingAtPoint)
            {
                _agent.SetDestination(patrolPoints[_patrolIndex].position);

                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    _waitingAtPoint = true;
                    StartCoroutine(WaitAtPatrolPoint());
                }
            }
        }

        private void UpdateChase()
        {
            bool canSee = CanSeePlayer();

            if (canSee)
            {
                _lastKnownPos   = _target.position;
                _lostTargetTimer = 0f;
                _agent.SetDestination(_target.position);
                _agent.speed = chaseSpeed;

                // Switch to attack if in range
                float dist = Vector3.Distance(transform.position, _target.position);
                if (dist <= attackRange)
                    TransitionTo(State.Attack);
            }
            else
            {
                _lostTargetTimer += Time.deltaTime;
                if (_lostTargetTimer >= loseTargetTime)
                    TransitionTo(State.Search);
            }
        }

        private void UpdateAttack()
        {
            if (_target == null) { TransitionTo(State.Patrol); return; }

            float dist = Vector3.Distance(transform.position, _target.position);

            // Chase again if target escaped
            if (dist > attackRange * 1.5f)
            {
                TransitionTo(State.Chase);
                return;
            }

            // Face the target
            Vector3 lookDir = (_target.position - transform.position);
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), 300f * Time.deltaTime);

            // Attack when cooldown ready
            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                _lastAttackTime = Time.time;
                _anim.SetTrigger(_hashAttack);
                GameEvents.TriggerPlaySFX("EnemyAttack");

                // Damage the player (check range again)
                if (dist <= attackRange + 0.2f)
                {
                    Player.PlayerStats ps = _target.GetComponent<Player.PlayerStats>();
                    ps?.TakeDamage(attackDamage);
                }
            }
        }

        private void UpdateSearch()
        {
            _searchTimer += Time.deltaTime;

            if (CanSeePlayer())
            {
                TransitionTo(State.Chase);
                return;
            }

            if (_searchTimer >= searchDuration)
            {
                _target = null;
                TransitionTo(State.Patrol);
                return;
            }

            // Wander around last known position
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                Vector3 searchPos = _lastKnownPos + Random.insideUnitSphere * searchRadius;
                searchPos.y = _lastKnownPos.y;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(searchPos, out hit, searchRadius, NavMesh.AllAreas))
                    _agent.SetDestination(hit.position);
            }
        }

        // ── Transitions ───────────────────────────────────────────

        private void TransitionTo(State next)
        {
            _currentState = next;

            switch (next)
            {
                case State.Patrol:
                    _agent.isStopped = false;
                    _agent.speed = patrolSpeed;
                    _lostTargetTimer = 0f;
                    break;

                case State.Chase:
                    _agent.isStopped = false;
                    _agent.speed = chaseSpeed;
                    _anim.SetTrigger(_hashAlert);
                    GameEvents.TriggerPlaySFX("EnemyAlert");
                    GameEvents.TriggerCombatStarted();
                    break;

                case State.Attack:
                    _agent.isStopped = true;
                    break;

                case State.Search:
                    _agent.isStopped = false;
                    _agent.speed = patrolSpeed;
                    _searchTimer = 0f;
                    break;
            }
        }

        // ── Detection ─────────────────────────────────────────────

        private bool CanSeePlayer()
        {
            // Broad sphere check first (cheaper)
            Collider[] cols = Physics.OverlapSphere(transform.position, sightRange, playerLayer);
            if (cols.Length == 0) return false;

            Transform player = cols[0].transform;

            // Angle check
            Vector3 toPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, toPlayer);
            if (angle > sightAngle * 0.5f) return false;

            // Line-of-sight check
            float dist = Vector3.Distance(transform.position, player.position);
            if (Physics.Raycast(transform.position + Vector3.up, toPlayer, dist, obstacleLayer))
                return false;

            _target = player;
            return true;
        }

        private bool CanHearPlayer()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, hearingRange, playerLayer);
            if (cols.Length > 0)
            {
                _target = cols[0].transform;
                return true;
            }
            return false;
        }

        /// <summary>Called by EnemyStats when damaged — immediately alerts AI to attacker.</summary>
        public void AlertToTarget(Transform attacker)
        {
            _target = attacker;
            _lastKnownPos = attacker.position;
            if (_currentState != State.Attack && _currentState != State.Chase)
                TransitionTo(State.Chase);
        }

        // ── Helpers ───────────────────────────────────────────────

        private IEnumerator WaitAtPatrolPoint()
        {
            yield return new WaitForSeconds(patrolWaitTime);
            _patrolIndex  = (_patrolIndex + 1) % patrolPoints.Length;
            _waitingAtPoint = false;
        }

        private void UpdateAnimator()
        {
            _anim.SetFloat(_hashSpeed, _agent.velocity.magnitude);
        }

        // ── Gizmos ────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            // Sight range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
            // Hearing range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, hearingRange);
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
