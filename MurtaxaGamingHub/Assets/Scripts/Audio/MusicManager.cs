// ============================================================
//  MusicManager.cs
//  Place in: Assets/Scripts/Audio/
//  Zone-based music switching with crossfade support.
//  Listens for ZoneChanged events and swaps music tracks.
// ============================================================
using System.Collections;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Audio
{
    [System.Serializable]
    public class ZoneMusic
    {
        public string    zoneName;     // Matches GameEvents.TriggerZoneChanged(zoneName)
        public AudioClip musicClip;
        public AudioClip ambientClip;
    }

    public class MusicManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Zone Music Map")]
        [SerializeField] private ZoneMusic[] zoneMusicMap;
        [SerializeField] private float       crossfadeDuration = 1.5f;

        [Header("Combat")]
        [SerializeField] private AudioClip combatMusic;

        // ── Runtime State ────────────────────────────────────────
        private AudioClip _currentZoneMusic;
        private bool      _inCombat = false;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnZoneChanged    += OnZoneChanged;
            GameEvents.OnCombatStarted  += OnCombatStarted;
            GameEvents.OnCombatEnded    += OnCombatEnded;
            GameEvents.OnMusicChanged   += OnMusicChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnZoneChanged    -= OnZoneChanged;
            GameEvents.OnCombatStarted  -= OnCombatStarted;
            GameEvents.OnCombatEnded    -= OnCombatEnded;
            GameEvents.OnMusicChanged   -= OnMusicChanged;
        }

        // ── Event Handlers ────────────────────────────────────────

        private void OnZoneChanged(string zoneName)
        {
            foreach (var zm in zoneMusicMap)
            {
                if (zm.zoneName == zoneName)
                {
                    _currentZoneMusic = zm.musicClip;
                    if (!_inCombat)
                        StartCoroutine(CrossfadeTo(zm.musicClip));
                    AudioManager.Instance?.PlayAmbient(zm.ambientClip);
                    return;
                }
            }
        }

        private void OnCombatStarted()
        {
            if (_inCombat) return;
            _inCombat = true;
            StartCoroutine(CrossfadeTo(combatMusic));
        }

        private void OnCombatEnded()
        {
            if (!_inCombat) return;
            _inCombat = false;
            StartCoroutine(CrossfadeTo(_currentZoneMusic));
        }

        private void OnMusicChanged(string trackName)
        {
            // Used for menu music etc.
        }

        // ── Crossfade ─────────────────────────────────────────────

        private IEnumerator CrossfadeTo(AudioClip newClip)
        {
            if (AudioManager.Instance == null) yield break;
            // Fade out handled by AudioManager — just swap after delay
            yield return new WaitForSeconds(crossfadeDuration * 0.5f);
            AudioManager.Instance.PlayMusic(newClip, crossfadeDuration);
        }
    }
}
