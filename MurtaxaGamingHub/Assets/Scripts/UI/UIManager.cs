// ============================================================
//  UIManager.cs
//  Place in: Assets/Scripts/UI/
//  Central UI controller. Holds references to all UI panels
//  and switches between them based on GameManager state.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;
using MurtaxaGaming.Systems;

namespace MurtaxaGaming.UI
{
    public class UIManager : Singleton<UIManager>
    {
        // ── Inspector — Panel References ──────────────────────────
        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject questLogPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("HUD Sub-Elements")]
        [SerializeField] private GameObject interactionPromptObject;
        [SerializeField] private TMPro.TextMeshProUGUI interactionPromptText;

        // ── Runtime State ────────────────────────────────────────
        private bool _inventoryOpen = false;
        private bool _questLogOpen  = false;
        private bool _mapOpen       = false;
        private bool _settingsOpen  = false;

        // ── Unity Lifecycle ───────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            // Start with all panels hidden
            HideAll();
        }

        private void OnEnable()
        {
            GameEvents.OnInventoryToggled        += OnInventoryToggled;
            GameEvents.OnQuestLogToggled         += OnQuestLogToggled;
            GameEvents.OnMapToggled              += OnMapToggled;
            GameEvents.OnInteractionPromptChanged+= OnInteractionPromptChanged;
            GameEvents.OnPlayerDied              += ShowGameOver;
            GameEvents.OnCheckpointReached       += OnCheckpointReached;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryToggled        -= OnInventoryToggled;
            GameEvents.OnQuestLogToggled         -= OnQuestLogToggled;
            GameEvents.OnMapToggled              -= OnMapToggled;
            GameEvents.OnInteractionPromptChanged-= OnInteractionPromptChanged;
            GameEvents.OnPlayerDied              -= ShowGameOver;
            GameEvents.OnCheckpointReached       -= OnCheckpointReached;
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            switch (GameManager.Instance.State)
            {
                case GameManager.GameState.MainMenu:
                    ShowOnly(mainMenuPanel);
                    break;

                case GameManager.GameState.Playing:
                    // HUD is always visible during play; other panels on demand
                    SetActive(hudPanel, true);
                    SetActive(mainMenuPanel, false);
                    SetActive(pauseMenuPanel, false);
                    break;

                case GameManager.GameState.Paused:
                    SetActive(pauseMenuPanel, true);
                    SetActive(hudPanel, true);     // keep HUD visible
                    break;

                case GameManager.GameState.Loading:
                    ShowOnly(loadingPanel);
                    break;
            }
        }

        // ── Public API ────────────────────────────────────────────

        public void ShowMainMenu()   => ShowOnly(mainMenuPanel);
        public void ShowLoading()    => ShowOnly(loadingPanel);
        public void ShowHUD()        => SetActive(hudPanel, true);

        public void ShowGameOver()
        {
            SetActive(gameOverPanel, true);
            SetActive(hudPanel, false);
        }

        public void ToggleSettings()
        {
            _settingsOpen = !_settingsOpen;
            SetActive(settingsPanel, _settingsOpen);
        }

        public void CloseSettings()
        {
            _settingsOpen = false;
            SetActive(settingsPanel, false);
        }

        // ── Event Handlers ────────────────────────────────────────

        private void OnInventoryToggled(bool open)
        {
            _inventoryOpen = open;
            SetActive(inventoryPanel, open);
            // Close others
            if (open) { _questLogOpen = false; _mapOpen = false; SetActive(questLogPanel, false); SetActive(mapPanel, false); }
        }

        private void OnQuestLogToggled(bool open)
        {
            _questLogOpen = open;
            SetActive(questLogPanel, open);
            if (open) { _inventoryOpen = false; _mapOpen = false; SetActive(inventoryPanel, false); SetActive(mapPanel, false); }
        }

        private void OnMapToggled(bool open)
        {
            _mapOpen = open;
            SetActive(mapPanel, open);
            if (open) { _inventoryOpen = false; _questLogOpen = false; SetActive(inventoryPanel, false); SetActive(questLogPanel, false); }
        }

        private void OnInteractionPromptChanged(string msg)
        {
            bool show = !string.IsNullOrEmpty(msg);
            if (interactionPromptObject != null) interactionPromptObject.SetActive(show);
            if (interactionPromptText   != null) interactionPromptText.text = msg;
        }

        private void OnCheckpointReached(string id)
        {
            // Show brief "Checkpoint Reached" toast (handled by HUDController)
            HUDController.Instance?.ShowToast("Checkpoint Reached");
        }

        // ── Helpers ───────────────────────────────────────────────

        private void HideAll()
        {
            SetActive(hudPanel, false);
            SetActive(mainMenuPanel, false);
            SetActive(pauseMenuPanel, false);
            SetActive(inventoryPanel, false);
            SetActive(questLogPanel, false);
            SetActive(mapPanel, false);
            SetActive(gameOverPanel, false);
            SetActive(settingsPanel, false);
            SetActive(dialoguePanel, false);
            SetActive(loadingPanel, false);
            if (interactionPromptObject != null) interactionPromptObject.SetActive(false);
        }

        private void ShowOnly(GameObject panel)
        {
            HideAll();
            SetActive(panel, true);
        }

        private void SetActive(GameObject go, bool state)
        {
            if (go != null && go.activeSelf != state) go.SetActive(state);
        }
    }
}
