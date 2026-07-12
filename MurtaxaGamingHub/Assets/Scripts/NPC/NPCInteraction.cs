// ============================================================
//  NPCInteraction.cs
//  Place in: Assets/Scripts/NPC/
//  Handles player proximity detection and interaction trigger.
//  Works with NPCDialogue and QuestSystem.
// ============================================================
using UnityEngine;
using MurtaxaGaming.Utils;
using MurtaxaGaming.Systems;

namespace MurtaxaGaming.NPC
{
    public class NPCInteraction : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Interaction Settings")]
        [SerializeField] private float  interactRange   = 2.5f;
        [SerializeField] private string npcName         = "Villager";
        [SerializeField] private string interactPrompt  = "Press [E] to talk";

        [Header("Quest")]
        [SerializeField] private bool   hasQuest        = false;
        [SerializeField] private string questId         = "";

        // ── Components ────────────────────────────────────────────
        private NPCDialogue  _dialogue;
        private Transform    _player;

        // ── Runtime State ────────────────────────────────────────
        private bool _playerInRange = false;
        private bool _isInteracting = false;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            _dialogue = GetComponent<NPCDialogue>();
        }

        private void Start()
        {
            // Cache player reference
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }

        private void Update()
        {
            CheckProximity();

            if (_playerInRange && Input.GetKeyDown(KeyCode.E) && !_isInteracting)
                StartInteraction();
        }

        // ── Private ───────────────────────────────────────────────

        private void CheckProximity()
        {
            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            bool inRange = dist <= interactRange;

            if (inRange != _playerInRange)
            {
                _playerInRange = inRange;

                if (_playerInRange)
                {
                    // Face the player
                    Vector3 look = (_player.position - transform.position);
                    look.y = 0f;
                    if (look.sqrMagnitude > 0.01f)
                        transform.rotation = Quaternion.LookRotation(look);

                    GameEvents.TriggerInteractionPromptChanged($"[{npcName}] {interactPrompt}");
                }
                else
                {
                    GameEvents.TriggerInteractionPromptChanged("");
                }
            }
        }

        private void StartInteraction()
        {
            _isInteracting = true;

            // Face player
            if (_player != null)
            {
                Vector3 look = (_player.position - transform.position);
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(look);
            }

            // Play dialogue
            if (_dialogue != null)
                _dialogue.StartDialogue(OnDialogueFinished);
            else
                OnDialogueFinished();

            GameEvents.TriggerPlaySFX("NPCGreet");
            GameEvents.TriggerInteractionPromptChanged("");
        }

        private void OnDialogueFinished()
        {
            _isInteracting = false;

            // Offer quest if available
            if (hasQuest && !string.IsNullOrEmpty(questId))
                QuestSystem.Instance?.OfferQuest(questId);

            if (_playerInRange)
                GameEvents.TriggerInteractionPromptChanged($"[{npcName}] {interactPrompt}");
        }

        // ── Gizmos ────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
