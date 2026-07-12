// ============================================================
//  AudioManager.cs
//  Place in: Assets/Scripts/Audio/
//  Central audio hub. Plays SFX from pool, manages volumes.
//  Add AudioSource components to this GameObject for each channel.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Audio
{
    [System.Serializable]
    public class SoundEntry
    {
        public string   name;       // Key used in GameEvents (e.g. "PlayerHurt")
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.8f, 1.2f)] public float pitchMin = 0.95f;
        [Range(0.8f, 1.2f)] public float pitchMax = 1.05f;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;       // Short one-shot SFX
        [SerializeField] private AudioSource uiSource;        // UI click/transition sounds
        [SerializeField] private AudioSource musicSource;     // Background music (looping)
        [SerializeField] private AudioSource ambientSource;   // Ambient/environment loop

        [Header("Sound Library")]
        [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

        [Header("Volume Defaults")]
        [SerializeField] private float masterVolume  = 1f;
        [SerializeField] private float musicVolume   = 0.8f;
        [SerializeField] private float sfxVolume     = 1f;

        // ── Runtime ───────────────────────────────────────────────
        private Dictionary<string, SoundEntry> _soundLookup = new Dictionary<string, SoundEntry>();

        // ── Unity Lifecycle ───────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            BuildLookup();
            LoadVolumePrefs();
        }

        private void OnEnable()  => GameEvents.OnPlaySFX += PlaySFX;
        private void OnDisable() => GameEvents.OnPlaySFX -= PlaySFX;

        // ── Public API ────────────────────────────────────────────

        /// <summary>Play a named SFX by key.</summary>
        public void PlaySFX(string soundName)
        {
            if (!_soundLookup.TryGetValue(soundName, out SoundEntry entry))
            {
                // Don't error on missing sounds — just silently skip
                return;
            }

            if (entry.clip == null) return;

            AudioSource src = soundName.StartsWith("UI") ? uiSource : sfxSource;
            src.pitch  = Random.Range(entry.pitchMin, entry.pitchMax);
            src.volume = entry.volume * sfxVolume * masterVolume;
            src.PlayOneShot(entry.clip);
        }

        /// <summary>Play a music clip (loops). Pass null to stop music.</summary>
        public void PlayMusic(AudioClip clip, float fadeDuration = 1f)
        {
            if (musicSource.clip == clip) return;

            // Instant swap for now; crossfade can be added with coroutines
            musicSource.clip   = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop   = true;
            if (clip != null) musicSource.Play();
            else musicSource.Stop();
        }

        /// <summary>Play an ambient loop. Pass null to stop.</summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (ambientSource.clip == clip) return;
            ambientSource.clip  = clip;
            ambientSource.loop  = true;
            ambientSource.volume = sfxVolume * masterVolume * 0.4f;
            if (clip != null) ambientSource.Play();
            else ambientSource.Stop();
        }

        /// <summary>Set master volume (0-1). Persisted via PlayerPrefs.</summary>
        public void SetMasterVolume(float v)
        {
            masterVolume = Mathf.Clamp01(v);
            ApplyVolumes();
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            ApplyVolumes();
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        public void SetSFXVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            ApplyVolumes();
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        /// <summary>Called after loading settings from save file.</summary>
        public void ApplyVolumes()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume  = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolume    = PlayerPrefs.GetFloat("SFXVolume", 1f);

            musicSource.volume   = musicVolume * masterVolume;
            ambientSource.volume = sfxVolume * masterVolume * 0.4f;
        }

        // ── Private ───────────────────────────────────────────────
        private void BuildLookup()
        {
            _soundLookup.Clear();
            foreach (var entry in sounds)
                if (!string.IsNullOrEmpty(entry.name))
                    _soundLookup[entry.name] = entry;
        }

        private void LoadVolumePrefs()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", masterVolume);
            musicVolume  = PlayerPrefs.GetFloat("MusicVolume", musicVolume);
            sfxVolume    = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);
        }
    }
}
