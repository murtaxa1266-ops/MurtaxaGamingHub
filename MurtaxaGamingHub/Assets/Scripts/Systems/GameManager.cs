// ============================================================
//  GameManager.cs
//  Place in: Assets/Scripts/Systems/
//  Central game state machine: MainMenu → Playing → Paused →
//  GameOver → Loading. Coordinates all major systems.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    public class GameManager : Singleton<GameManager>
    {
        // ── Game States ───────────────────────────────────────────
        public enum GameState { MainMenu, Playing, Paused, GameOver, Loading }

        // ── Inspector ────────────────────────────────────────────
        [Header("Scene Names")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string gameScene     = "GameWorld";

        [Header("Settings")]
        [SerializeField] private float gameOverDelay  = 2f; // seconds before showing Game Over UI

        // ── Runtime State ────────────────────────────────────────
        private GameState _state = GameState.MainMenu;
        public  GameState State  => _state;
        public  bool IsPlaying   => _state == GameState.Playing;

        // XP: received from GameEvents and forwarded to player
        private Player.PlayerStats _playerStats;

        // ── Unity Lifecycle ───────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60; // 60 FPS cap for Android
            QualitySettings.vSyncCount  = 0;  // Let targetFrameRate control it
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied += OnPlayerDied;
            GameEvents.OnEnemyDied  += OnEnemyDied;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= OnPlayerDied;
            GameEvents.OnEnemyDied  -= OnEnemyDied;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Start a new game from the beginning.</summary>
        public void NewGame()
        {
            SaveSystem.Instance?.ClearSave();
            LoadScene(gameScene);
        }

        /// <summary>Load the most recent save and continue.</summary>
        public void ContinueGame()
        {
            if (!SaveSystem.Instance.HasSave())
            {
                Debug.LogWarning("[GameManager] No save file found. Starting new game.");
                NewGame();
                return;
            }
            LoadScene(gameScene);
        }

        /// <summary>Load the main menu.</summary>
        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            LoadScene(mainMenuScene);
        }

        /// <summary>Toggle pause.</summary>
        public void TogglePause()
        {
            if (_state == GameState.Playing)  PauseGame();
            else if (_state == GameState.Paused) ResumeGame();
        }

        public void PauseGame()
        {
            _state = GameState.Paused;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            GameEvents.TriggerPlaySFX("PauseMenu");
            Debug.Log("[GameManager] Game paused.");
        }

        public void ResumeGame()
        {
            _state = GameState.Playing;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Debug.Log("[GameManager] Game resumed.");
        }

        /// <summary>Save game to disk.</summary>
        public void SaveGame()
        {
            SaveSystem.Instance?.Save();
            GameEvents.TriggerGameSaved();
            GameEvents.TriggerPlaySFX("SaveGame");
        }

        /// <summary>Quit the application.</summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting application.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ── Scene Management ──────────────────────────────────────

        private void LoadScene(string sceneName)
        {
            _state = GameState.Loading;
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                // Progress can be sent to loading screen here
                yield return null;
            }

            op.allowSceneActivation = true;
            yield return op;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == gameScene)
            {
                _state = GameState.Playing;
                Time.timeScale = 1f;

                // Cache player
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null)
                    _playerStats = playerGO.GetComponent<Player.PlayerStats>();

                // Load saved data if any
                if (SaveSystem.Instance?.HasSave() == true)
                {
                    SaveSystem.Instance.Load();
                    GameEvents.TriggerGameLoaded();
                }

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
            else if (scene.name == mainMenuScene)
            {
                _state = GameState.MainMenu;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }
        }

        // ── Event Handlers ────────────────────────────────────────

        private void OnPlayerDied()
        {
            if (_state != GameState.Playing) return;
            StartCoroutine(HandleGameOver());
        }

        private IEnumerator HandleGameOver()
        {
            yield return new WaitForSeconds(gameOverDelay);
            _state = GameState.GameOver;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            // UIManager will pick up the GameOver state
        }

        private void OnEnemyDied(GameObject enemy, Vector3 pos, int xp)
        {
            // Award XP to player
            _playerStats?.GainXP(xp);
        }

        // ── Pause Input ───────────────────────────────────────────
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && (_state == GameState.Playing || _state == GameState.Paused))
                TogglePause();
        }
    }
}
