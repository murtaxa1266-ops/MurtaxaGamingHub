// ============================================================
//  PlayerController.cs
//  Place in: Assets/Scripts/Player/
//  Handles: walk, run, jump, crouch, and root-motion movement.
//  Requires: CharacterController, PlayerStats, Animator on same GO.
//  Attach this script directly to your Player prefab root.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed    = 4f;
        [SerializeField] private float runSpeed     = 8f;
        [SerializeField] private float crouchSpeed  = 2f;
        [SerializeField] private float rotateSpeed  = 720f;  // degrees per second

        [Header("Jump & Gravity")]
        [SerializeField] private float jumpForce    = 7f;
        [SerializeField] private float gravity      = -20f;
        [SerializeField] private float groundDist   = 0.3f;
        [SerializeField] private LayerMask groundMask;

        [Header("Crouch")]
        [SerializeField] private float standHeight  = 1.8f;
        [SerializeField] private float crouchHeight = 0.9f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;  // Drag the Main Camera here

        // ── Components ────────────────────────────────────────────
        private CharacterController _cc;
        private PlayerStats         _stats;
        private Animator            _anim;

        // ── Runtime State ────────────────────────────────────────
        private Vector3 _velocity;         // Y velocity for gravity/jump
        private bool    _isGrounded;
        private bool    _isCrouching;
        private bool    _isRunning;
        private bool    _isMoving;
        private bool    _isDead;

        // Animator parameter hashes (cached for performance)
        private static readonly int _hashSpeed    = Animator.StringToHash("Speed");
        private static readonly int _hashIsGround = Animator.StringToHash("IsGrounded");
        private static readonly int _hashJump     = Animator.StringToHash("Jump");
        private static readonly int _hashCrouch   = Animator.StringToHash("IsCrouching");
        private static readonly int _hashDie      = Animator.StringToHash("Die");

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _cc    = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _anim  = GetComponent<Animator>();

            // Lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += OnDied;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= OnDied;
        }

        private void Update()
        {
            if (_isDead) return;

            CheckGround();
            HandleCrouch();
            HandleMovement();
            HandleJump();
            ApplyGravity();
            UpdateAnimator();
        }

        // ── Ground Check ──────────────────────────────────────────

        private void CheckGround()
        {
            // Sphere cast from bottom of CharacterController
            Vector3 spherePos = transform.position + Vector3.down * (_cc.height / 2f - _cc.radius);
            _isGrounded = Physics.CheckSphere(spherePos, groundDist, groundMask);

            if (_isGrounded && _velocity.y < 0f)
                _velocity.y = -2f; // keep grounded
        }

        // ── Movement ─────────────────────────────────────────────

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(h, 0f, v).normalized;
            _isMoving = inputDir.magnitude > 0.1f;

            // Determine running state (cannot run while crouching or out of stamina)
            bool wantsRun = Input.GetKey(KeyCode.LeftShift) && !_isCrouching;
            _isRunning = wantsRun && _isMoving;

            // Drain stamina when running; stop running if stamina depleted
            if (_isRunning)
                _isRunning = _stats.DrainStamina();

            float speed = _isCrouching ? crouchSpeed : (_isRunning ? runSpeed : walkSpeed);

            if (_isMoving)
            {
                // Move relative to camera direction
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight   = cameraTransform.right;
                camForward.y = 0f;
                camRight.y   = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
                _cc.Move(moveDir * speed * Time.deltaTime);

                // Smooth rotation toward movement direction
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

        // ── Jump ──────────────────────────────────────────────────

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && _isGrounded && !_isCrouching)
            {
                if (_stats.UseStamina(20f)) // stamina cost for jump
                {
                    _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                    _anim.SetTrigger(_hashJump);
                    GameEvents.TriggerPlaySFX("Jump");
                }
            }
        }

        // ── Gravity ───────────────────────────────────────────────

        private void ApplyGravity()
        {
            _velocity.y += gravity * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }

        // ── Crouch ────────────────────────────────────────────────

        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                _isCrouching = !_isCrouching;

                // Adjust CharacterController height for crouch
                _cc.height = _isCrouching ? crouchHeight : standHeight;
                _cc.center = new Vector3(0f, _cc.height / 2f, 0f);

                GameEvents.TriggerPlaySFX(_isCrouching ? "Crouch" : "StandUp");
            }
        }

        // ── Animator ─────────────────────────────────────────────

        private void UpdateAnimator()
        {
            // 0 = idle, 0.5 = walk, 1 = run
            float targetSpeed = _isMoving ? (_isRunning ? 1f : 0.5f) : 0f;
            float current = _anim.GetFloat(_hashSpeed);
            _anim.SetFloat(_hashSpeed, Mathf.Lerp(current, targetSpeed, Time.deltaTime * 10f));
            _anim.SetBool(_hashIsGround, _isGrounded);
            _anim.SetBool(_hashCrouch, _isCrouching);
        }

        // ── Event Handlers ────────────────────────────────────────

        private void OnDied()
        {
            _isDead = true;
            _anim.SetTrigger(_hashDie);

            // Release cursor for game-over UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── Gizmos ────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc == null) return;

            Gizmos.color = Color.green;
            Vector3 spherePos = transform.position + Vector3.down * (cc.height / 2f - cc.radius);
            Gizmos.DrawWireSphere(spherePos, groundDist);
        }
    }
}
