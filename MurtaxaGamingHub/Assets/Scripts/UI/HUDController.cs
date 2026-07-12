// ============================================================
//  HUDController.cs
//  Place in: Assets/Scripts/UI/
//  Drives HUD elements: health bar, stamina bar, XP bar, toast.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.UI
{
    public class HUDController : Singleton<HUDController>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Health Bar")]
        [SerializeField] private Slider      healthSlider;
        [SerializeField] private Image       healthFill;
        [SerializeField] private Gradient    healthGradient;   // red→yellow→green
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Stamina Bar")]
        [SerializeField] private Slider      staminaSlider;
        [SerializeField] private Image       staminaFill;
        [SerializeField] private CanvasGroup staminaGroup;     // fade out when full

        [Header("XP / Level")]
        [SerializeField] private Slider      xpSlider;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Zone Name")]
        [SerializeField] private TextMeshProUGUI zoneNameText;
        [SerializeField] private CanvasGroup     zoneNameGroup;
        [SerializeField] private float           zoneDisplayDuration = 3f;

        [Header("Toast Notification")]
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private CanvasGroup     toastGroup;
        [SerializeField] private float           toastDuration = 2.5f;

        [Header("Item Collected")]
        [SerializeField] private TextMeshProUGUI itemCollectedText;
        [SerializeField] private CanvasGroup     itemCollectedGroup;

        // ── Runtime ───────────────────────────────────────────────
        private Coroutine _toastCoroutine;
        private Coroutine _zoneCoroutine;
        private Coroutine _itemCoroutine;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged  += UpdateHealth;
            GameEvents.OnPlayerStaminaChanged += UpdateStamina;
            GameEvents.OnPlayerXPGained       += _ => { /* xp slider update elsewhere */ };
            GameEvents.OnPlayerLevelUp        += UpdateLevel;
            GameEvents.OnZoneChanged          += ShowZoneName;
            GameEvents.OnItemCollected        += ShowItemCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged  -= UpdateHealth;
            GameEvents.OnPlayerStaminaChanged -= UpdateStamina;
            GameEvents.OnPlayerLevelUp        -= UpdateLevel;
            GameEvents.OnZoneChanged          -= ShowZoneName;
            GameEvents.OnItemCollected        -= ShowItemCollected;
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Show a brief toast message on screen.</summary>
        public void ShowToast(string message)
        {
            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(ToastRoutine(message));
        }

        // ── Event Handlers ────────────────────────────────────────

        private void UpdateHealth(float cur, float max)
        {
            if (healthSlider == null) return;
            float pct = max > 0 ? cur / max : 0f;
            healthSlider.value = pct;
            if (healthFill != null && healthGradient != null)
                healthFill.color = healthGradient.Evaluate(pct);
            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
        }

        private void UpdateStamina(float cur, float max)
        {
            if (staminaSlider == null) return;
            float pct = max > 0 ? cur / max : 0f;
            staminaSlider.value = pct;

            // Fade stamina bar out when full
            if (staminaGroup != null)
                staminaGroup.alpha = pct < 0.99f ? 1f : 0f;
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null) levelText.text = $"Lv {level}";
        }

        private void ShowZoneName(string zoneName)
        {
            if (_zoneCoroutine != null) StopCoroutine(_zoneCoroutine);
            _zoneCoroutine = StartCoroutine(ZoneNameRoutine(zoneName));
        }

        private void ShowItemCollected(string itemName)
        {
            if (_itemCoroutine != null) StopCoroutine(_itemCoroutine);
            _itemCoroutine = StartCoroutine(ItemCollectedRoutine(itemName));
        }

        // ── Coroutines ────────────────────────────────────────────

        private IEnumerator ToastRoutine(string message)
        {
            if (toastText  != null) toastText.text  = message;
            if (toastGroup != null) toastGroup.alpha = 1f;
            yield return new WaitForSeconds(toastDuration);
            if (toastGroup != null)
            {
                float t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    toastGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                    yield return null;
                }
                toastGroup.alpha = 0f;
            }
        }

        private IEnumerator ZoneNameRoutine(string zoneName)
        {
            if (zoneNameText  != null) zoneNameText.text  = zoneName;
            if (zoneNameGroup != null) zoneNameGroup.alpha = 1f;
            yield return new WaitForSeconds(zoneDisplayDuration);
            if (zoneNameGroup != null)
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    zoneNameGroup.alpha = Mathf.Lerp(1f, 0f, t);
                    yield return null;
                }
                zoneNameGroup.alpha = 0f;
            }
        }

        private IEnumerator ItemCollectedRoutine(string itemName)
        {
            if (itemCollectedText  != null) itemCollectedText.text  = $"+ {itemName}";
            if (itemCollectedGroup != null) itemCollectedGroup.alpha = 1f;
            yield return new WaitForSeconds(2f);
            if (itemCollectedGroup != null)
            {
                float t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    itemCollectedGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                    yield return null;
                }
                itemCollectedGroup.alpha = 0f;
            }
        }
    }
}
