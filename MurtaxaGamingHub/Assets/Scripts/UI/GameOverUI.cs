// ============================================================
//  GameOverUI.cs
//  Place in: Assets/Scripts/UI/
//  Game Over screen: retry, main menu, quit.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Systems;

namespace MurtaxaGaming.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI playtimeText;

        private void Awake()
        {
            retryButton   ?.onClick.AddListener(OnRetry);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
            quitButton    ?.onClick.AddListener(OnQuit);

            if (titleText != null) titleText.text = "YOU DIED";
        }

        private void OnEnable()
        {
            // Show playtime when panel activates
            if (playtimeText != null)
            {
                int total = (int)Time.time;
                playtimeText.text = $"Playtime: {total / 60}m {total % 60}s";
            }
        }

        private void OnRetry()
        {
            Utils.GameEvents.TriggerPlaySFX("UIClick");
            // Load last checkpoint / save
            GameManager.Instance?.ContinueGame();
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
