// ============================================================
//  WorldZone.cs
//  Place in: Assets/Scripts/World/
//  Trigger-based zone detector. When the player enters, fires
//  ZoneChanged event and switches music/ambient to match.
//  Add a Box Collider (Is Trigger = true) to this GameObject.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.World
{
    public class WorldZone : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Zone Settings")]
        [SerializeField] private string zoneName    = "Forest";
        [SerializeField] private Color  gizmoColor  = new Color(0f, 1f, 0.3f, 0.2f);

        [Header("Music Override (optional)")]
        [SerializeField] private AudioClip zoneMusic;
        [SerializeField] private AudioClip zoneAmbient;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            GameEvents.TriggerZoneChanged(zoneName);
            Debug.Log($"[WorldZone] Entered: {zoneName}");

            // Music/ambient override handled by MusicManager via ZoneChanged event
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            BoxCollider bc = GetComponent<BoxCollider>();
            if (bc != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(bc.center, bc.size);
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
                Gizmos.DrawWireCube(bc.center, bc.size);
            }
            else
            {
                Gizmos.DrawSphere(transform.position, 5f);
            }
        }
    }
}
