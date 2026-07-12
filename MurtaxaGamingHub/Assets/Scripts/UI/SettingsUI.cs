// ============================================================
//  SettingsUI.cs
//  Place in: Assets/Scripts/UI/
//  Volume sliders, graphics quality, touch sensitivity.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Audio;

namespace MurtaxaGaming.UI
{
    public class SettingsUI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics")]
        [SerializeField] private Slider qualitySlider;           // 0=Low, 1=Med, 2=High
        [SerializeField] private TextMeshProUGUI qualityLabel;

        [Header("Controls")]
        [SerializeField] private Slider touchSensitivitySlider;

        [Header("Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        private static readonly string[] QualityLabels = { "Low", "Medium", "High" };

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            applyButton?.onClick.AddListener(OnApply);
            closeButton?.onClick.AddListener(OnClose);

            masterVolumeSlider    ?.onValueChanged.AddListener(v => AudioManager.Instance?.SetMasterVolume(v));
            musicVolumeSlider     ?.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
            sfxVolumeSlider       ?.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
            qualitySlider         ?.onValueChanged.AddListener(OnQualityChanged);
        }

        private void OnEnable() => LoadCurrentSettings();

        // ── Private ───────────────────────────────────────────────

        private void LoadCurrentSettings()
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value   = PlayerPrefs.GetFloat("MasterVolume",     1f);
            if (musicVolumeSlider  != null) musicVolumeSlider.value    = PlayerPrefs.GetFloat("MusicVolume",      0.8f);
            if (sfxVolumeSlider    != null) sfxVolumeSlider.value      = PlayerPrefs.GetFloat("SFXVolume",        1f);
            if (qualitySlider      != null)
            {
                qualitySlider.value = PlayerPrefs.GetInt("QualityIndex", 1);
                UpdateQualityLabel((int)qualitySlider.value);
            }
            if (touchSensitivitySlider != null)
                touchSensitivitySlider.value = PlayerPrefs.GetFloat("TouchSensitivity", 3f);
        }

        private void OnQualityChanged(float val)
        {
            int index = Mathf.RoundToInt(val);
            QualitySettings.SetQualityLevel(index, true);
            UpdateQualityLabel(index);
        }

        private void UpdateQualityLabel(int index)
        {
            if (qualityLabel != null && index >= 0 && index < QualityLabels.Length)
                qualityLabel.text = QualityLabels[index];
        }

        private void OnApply()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");

            // Persist settings
            PlayerPrefs.SetFloat("MasterVolume",     masterVolumeSlider?.value    ?? 1f);
            PlayerPrefs.SetFloat("MusicVolume",      musicVolumeSlider?.value     ?? 0.8f);
            PlayerPrefs.SetFloat("SFXVolume",        sfxVolumeSlider?.value       ?? 1f);
            PlayerPrefs.SetInt  ("QualityIndex",     Mathf.RoundToInt(qualitySlider?.value ?? 1f));
            PlayerPrefs.SetFloat("TouchSensitivity", touchSensitivitySlider?.value ?? 3f);
            PlayerPrefs.Save();

            AudioManager.Instance?.ApplyVolumes();
            UIManager.Instance?.CloseSettings();
        }

        private void OnClose()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            UIManager.Instance?.CloseSettings();
        }
    }
}
