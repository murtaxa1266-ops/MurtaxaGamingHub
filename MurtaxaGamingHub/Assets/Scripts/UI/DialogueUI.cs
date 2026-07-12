// ============================================================
//  DialogueUI.cs
//  Place in: Assets/Scripts/UI/
//  Renders NPC dialogue nodes: text, portrait, choices.
// ============================================================
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.NPC;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.UI
{
    public class DialogueUI : Singleton<DialogueUI>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Layout")]
        [SerializeField] private GameObject      panel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private Image           speakerPortrait;
        [SerializeField] private TextMeshProUGUI dialogueBodyText;
        [SerializeField] private float           typewriterSpeed  = 40f; // chars per second

        [Header("Choices")]
        [SerializeField] private Transform   choiceContainer;
        [SerializeField] private GameObject  choiceButtonPrefab;  // Prefab: Button + TMP_Text

        [Header("Continue Prompt")]
        [SerializeField] private GameObject continuePrompt;   // "▼ Press E to continue"

        // ── Runtime ───────────────────────────────────────────────
        private Action<int> _onChoice;
        private Coroutine   _typewriter;
        private bool        _skipTyping;
        private bool        _waitingForInput;

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void Update()
        {
            if (!_waitingForInput) return;

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (_skipTyping)
                {
                    // Already typing — skip to end
                    _skipTyping = false;
                }
                else
                {
                    // Advance (no choices)
                    _onChoice?.Invoke(0);
                }
            }
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Show a dialogue node. onChoice called with index when player selects.</summary>
        public void ShowNode(DialogueNode node, Action<int> onChoice)
        {
            _onChoice = onChoice;
            _waitingForInput = false;

            if (panel != null && !panel.activeSelf) panel.SetActive(true);

            // Speaker
            if (speakerNameText != null) speakerNameText.text = node.speakerName;
            if (speakerPortrait != null)
            {
                speakerPortrait.gameObject.SetActive(node.speakerPortrait != null);
                speakerPortrait.sprite = node.speakerPortrait;
            }

            // Clear choices
            ClearChoices();
            if (continuePrompt != null) continuePrompt.SetActive(false);

            // Typewriter effect
            if (_typewriter != null) StopCoroutine(_typewriter);
            _typewriter = StartCoroutine(TypeText(node.text, () =>
            {
                // Text done — show choices or continue prompt
                if (node.choices != null && node.choices.Length > 0)
                    SpawnChoices(node.choices);
                else
                {
                    if (continuePrompt != null) continuePrompt.SetActive(true);
                    _waitingForInput = true;
                }
            }));
        }

        /// <summary>Hide the dialogue panel.</summary>
        public void HideDialogue()
        {
            if (_typewriter != null) StopCoroutine(_typewriter);
            if (panel != null) panel.SetActive(false);
            ClearChoices();
        }

        // ── Private ───────────────────────────────────────────────

        private IEnumerator TypeText(string text, Action onComplete)
        {
            if (dialogueBodyText == null) { onComplete?.Invoke(); yield break; }

            dialogueBodyText.text = "";
            _skipTyping = true;
            _waitingForInput = false;

            foreach (char c in text)
            {
                if (!_skipTyping)
                {
                    dialogueBodyText.text += c;
                    yield return new WaitForSeconds(1f / typewriterSpeed);
                }
                else
                {
                    // Skip mode — just dump the rest
                    dialogueBodyText.text = text;
                    _skipTyping = false;
                    break;
                }
            }

            onComplete?.Invoke();
        }

        private void SpawnChoices(string[] choices)
        {
            if (choiceContainer == null || choiceButtonPrefab == null) return;

            for (int i = 0; i < choices.Length; i++)
            {
                int index = i;
                GameObject go = Instantiate(choiceButtonPrefab, choiceContainer);
                TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = choices[i];
                Button btn = go.GetComponent<Button>();
                btn?.onClick.AddListener(() =>
                {
                    GameEvents.TriggerPlaySFX("UIClick");
                    _onChoice?.Invoke(index);
                });
            }
        }

        private void ClearChoices()
        {
            if (choiceContainer == null) return;
            foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        }
    }
}
