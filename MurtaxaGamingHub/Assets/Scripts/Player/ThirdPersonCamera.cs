// ============================================================
//  ThirdPersonCamera.cs
//  Place in: Assets/Scripts/Player/
//  Smooth third-person camera with orbit, zoom, and collision.
//  Attach to a dedicated CameraRig GameObject (empty GO).
//  Drag Main Camera as child of CameraRig.
// ============================================================
using UnityEngine;

namespace MurtaxaGaming.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Target")]
        [SerializeField] private Transform target;           // Player transform
        [SerializeField] private Vector3   followOffset = new Vector3(0f, 1.6f, 0f); // shoulder height

        [Header("Orbit")]
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch =  60f;

        [Header("Distance")]
        [SerializeField] private float defaultDistance = 5f;
        [SerializeField] private float minDistance     = 1.5f;
        [SerializeField] private float maxDistance     = 10f;
        [SerializeField] private float zoomSpeed       = 4f;
        [SerializeField] private float zoomSmoothTime  = 0.2f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.1f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionRadius  = 0.3f;

        [Header("Mobile Touch")]
        [SerializeField] private bool  enableTouch      = true;
        [SerializeField] private float touchSensitivity = 0.15f;

        // ── Runtime State ────────────────────────────────────────
        private float   _yaw;           // horizontal rotation
        private float   _pitch;         // vertical rotation
        private float   _currentDist;
        private float   _targetDist;
        private float   _distVelocity;
        private Vector3 _posVelocity;

        private bool _isPaused;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _currentDist = defaultDistance;
            _targetDist  = defaultDistance;

            if (target == null)
                Debug.LogError("[ThirdPersonCamera] No target assigned! Drag the Player into 'Target'.");
        }

        private void Update()
        {
            if (_isPaused) return;
            HandleInput();
        }

        private void LateUpdate()
        {
            if (target == null || _isPaused) return;
            UpdateCamera();
        }

        // ── Input ─────────────────────────────────────────────────

        private void HandleInput()
        {
            // Mouse input
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw   += Input.GetAxis("Mouse X") * mouseSensitivity;
                _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            // Touch input (Android)
            if (enableTouch && Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    _yaw   += touch.deltaPosition.x * touchSensitivity;
                    _pitch -= touch.deltaPosition.y * touchSensitivity;
                    _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
                }
            }

            // Pinch to zoom (Android)
            if (enableTouch && Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                float prevMag = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
                float curMag  = (t0.position - t1.position).magnitude;
                float delta   = prevMag - curMag;

                _targetDist += delta * touchSensitivity * 0.1f;
                _targetDist  = Mathf.Clamp(_targetDist, minDistance, maxDistance);
            }

            // Scroll wheel zoom (PC)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDist -= scroll * zoomSpeed;
                _targetDist  = Mathf.Clamp(_targetDist, minDistance, maxDistance);
            }
        }

        // ── Camera Update ─────────────────────────────────────────

        private void UpdateCamera()
        {
            // Smooth zoom
            _currentDist = Mathf.SmoothDamp(_currentDist, _targetDist, ref _distVelocity, zoomSmoothTime);

            // Compute desired orbit rotation
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // Pivot point on the player
            Vector3 pivot = target.position + followOffset;

            // Desired camera position behind the player
            Vector3 desiredPos = pivot - rotation * Vector3.forward * _currentDist;

            // Collision: shorten distance if something is in the way
            float adjustedDist = CheckCollision(pivot, desiredPos, _currentDist);
            desiredPos = pivot - rotation * Vector3.forward * adjustedDist;

            // Smooth position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVelocity, positionSmoothTime);
            transform.LookAt(pivot);
        }

        // ── Collision ─────────────────────────────────────────────

        private float CheckCollision(Vector3 from, Vector3 to, float wantedDist)
        {
            RaycastHit hit;
            Vector3 dir = (to - from).normalized;
            if (Physics.SphereCast(from, collisionRadius, dir, out hit, wantedDist, collisionLayers))
                return Mathf.Clamp(hit.distance, minDistance, wantedDist);
            return wantedDist;
        }

        // ── Public API ────────────────────────────────────────────
        public void SetPaused(bool paused) => _isPaused = paused;
        public Quaternion GetRotation()    => Quaternion.Euler(0f, _yaw, 0f);
    }
}
