// ============================================================
//  FootstepAudio.cs
//  Place in: Assets/Scripts/Audio/
//  Plays footstep sounds based on terrain/surface type and
//  player movement state. Attach to the Player GameObject.
// ============================================================
using UnityEngine;

namespace MurtaxaGaming.Audio
{
    [System.Serializable]
    public class SurfaceFootstep
    {
        public string   surfaceTag;       // e.g. "Grass", "Stone", "Wood", "Water"
        public AudioClip[] stepClips;
        [Range(0f, 1f)] public float volume = 0.6f;
    }

    [RequireComponent(typeof(CharacterController))]
    public class FootstepAudio : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Surface Sounds")]
        [SerializeField] private SurfaceFootstep[] surfaces;
        [SerializeField] private SurfaceFootstep   defaultSurface; // fallback

        [Header("Timing")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval  = 0.3f;

        [Header("Layers")]
        [SerializeField] private LayerMask groundLayer;

        // ── Components ────────────────────────────────────────────
        private CharacterController _cc;
        private AudioSource         _src;

        // ── Runtime State ────────────────────────────────────────
        private float _stepTimer    = 0f;
        private bool  _wasGrounded  = false;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _cc  = GetComponent<CharacterController>();
            _src = gameObject.AddComponent<AudioSource>();
            _src.spatialBlend = 1f;  // 3D audio
            _src.rolloffMode  = AudioRolloffMode.Linear;
            _src.maxDistance  = 15f;
        }

        private void Update()
        {
            bool grounded = _cc.isGrounded;
            bool moving   = _cc.velocity.magnitude > 0.5f;
            bool running  = _cc.velocity.magnitude > 5f;

            // Landing sound
            if (grounded && !_wasGrounded)
            {
                PlayStep(GetSurfaceAtFeet());
                _stepTimer = 0f;
            }
            _wasGrounded = grounded;

            if (!grounded || !moving) { _stepTimer = 0f; return; }

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep(GetSurfaceAtFeet());
                _stepTimer = running ? runStepInterval : walkStepInterval;
            }
        }

        // ── Private ───────────────────────────────────────────────

        private void PlayStep(SurfaceFootstep surface)
        {
            if (surface == null || surface.stepClips == null || surface.stepClips.Length == 0) return;

            AudioClip clip = surface.stepClips[Random.Range(0, surface.stepClips.Length)];
            if (clip == null) return;

            _src.volume = surface.volume;
            _src.pitch  = Random.Range(0.9f, 1.1f);
            _src.PlayOneShot(clip);
        }

        private SurfaceFootstep GetSurfaceAtFeet()
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            if (Physics.Raycast(origin, Vector3.down, out hit, 0.4f, groundLayer))
            {
                string tag = hit.collider.tag;
                if (surfaces != null)
                    foreach (var s in surfaces)
                        if (s.surfaceTag == tag) return s;
            }

            return defaultSurface;
        }
    }
}
