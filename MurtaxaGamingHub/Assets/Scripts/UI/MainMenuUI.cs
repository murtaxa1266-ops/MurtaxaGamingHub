// ============================================================
//  MainMenuUI.cs
//  Place in: Assets/Scripts/UI/
//  Drives the Main Menu panel buttons.
//  Attach to the MainMenu Canvas root.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Systems;

namespace MurtaxaGaming.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Version")]
        [SerializeField] private string gameVersion = "v1.0.0";

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            // Wire buttons
            newGameButton ?.onClick.AddListener(OnNewGame);
            continueButton?.onClick.AddListener(OnContinue);
            settingsButton?.onClick.AddListener(OnSettings);
            quitButton    ?.onClick.AddListener(OnQuit);

            if (versionText != null)
                versionText.text = gameVersion;

            if (titleText != null)
                titleText.text = "MURTAXA\nGAMING HUB";
        }

        private void Start()
        {
            // Grey out Continue if no save exists
            if (continueButton != null)
                continueButton.interactable = SaveSystem.Instance?.HasSave() ?? false;
        }

        // ── Button Handlers ───────────────────────────────────────

        private void OnNewGame()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.NewGame();
        }

        private void OnContinue()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.ContinueGame();
        }

        private void OnSettings()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            UIManager.Instance?.ToggleSettings();
        }

        private void OnQuit()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            GameManager.Instance?.QuitGame();
        }
    }
}
