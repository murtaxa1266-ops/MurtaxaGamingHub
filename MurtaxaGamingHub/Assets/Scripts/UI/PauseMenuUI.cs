// ============================================================
//  PauseMenuUI.cs
//  Place in: Assets/Scripts/UI/
//  Pause menu: resume, save, settings, main menu, quit.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using MurtaxaGaming.Systems;

namespace MurtaxaGaming.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Save Feedback")]
        [SerializeField] private TMPro.TextMeshProUGUI saveStatusText;
        private float _saveStatusTimer = 0f;

        private void Awake()
        {
            resumeButton  ?.onClick.AddListener(OnResume);
            saveButton    ?.onClick.AddListener(OnSave);
            settingsButton?.onClick.AddListener(OnSettings);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
            quitButton    ?.onClick.AddListener(OnQuit);
        }

        private void Update()
        {
            if (_saveStatusTimer > 0f)
            {
                _saveStatusTimer -= Time.unscaledDeltaTime;
                if (_saveStatusTimer <= 0f && saveStatusText != null)
                    saveStatusText.text = "";
            }
        }

        private void OnResume()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.ResumeGame();
        }

        private void OnSave()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.SaveGame();

            if (saveStatusText != null)
            {
                saveStatusText.text = "Game Saved!";
                _saveStatusTimer = 2f;
            }
        }

        private void OnSettings()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            UIManager.Instance?.ToggleSettings();
        }

        private void OnMainMenu()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.GoToMainMenu();
        }

        private void OnQuit()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.QuitGame();
        }
    }
}
