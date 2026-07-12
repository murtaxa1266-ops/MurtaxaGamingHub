// ============================================================
//  NPCDialogue.cs
//  Place in: Assets/Scripts/NPC/
//  Stores dialogue lines and drives DialogueUI.
//  Supports branching via dialogue nodes.
// ============================================================
using System;
using System.Collections.Generic;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.NPC
{
    [System.Serializable]
    public class DialogueNode
    {
        [TextArea(2, 5)]
        public string text;               // Line of dialogue
        public string speakerName;        // NPC name shown in UI
        public Sprite speakerPortrait;    // Optional portrait sprite
        public string[] choices;          // Player response options (empty = no choice, just continue)
        public int[]   nextNodeIndices;   // Which node each choice leads to (-1 = end)
    }

    public class NPCDialogue : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Dialogue Tree")]
        [SerializeField] private List<DialogueNode> nodes = new List<DialogueNode>();
        [SerializeField] private int startNodeIndex = 0;

        // ── Runtime State ────────────────────────────────────────
        private int      _currentNodeIndex;
        private Action   _onComplete;
        private bool     _isPlaying;

        // ── Public API ────────────────────────────────────────────

        /// <summary>Start dialogue from the configured start node.</summary>
        public void StartDialogue(Action onComplete = null)
        {
            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogWarning($"[NPCDialogue] No dialogue nodes on {gameObject.name}.");
                onComplete?.Invoke();
                return;
            }

            _onComplete       = onComplete;
            _currentNodeIndex = startNodeIndex;
            _isPlaying        = true;

            ShowCurrentNode();
        }

        /// <summary>Advance to next node or end dialogue.</summary>
        public void Advance(int choiceIndex = 0)
        {
            if (!_isPlaying) return;

            DialogueNode current = GetCurrentNode();
            if (current == null) { EndDialogue(); return; }

            // Determine next node
            int next = -1;
            if (current.nextNodeIndices != null && choiceIndex < current.nextNodeIndices.Length)
                next = current.nextNodeIndices[choiceIndex];

            if (next < 0 || next >= nodes.Count)
            {
                EndDialogue();
                return;
            }

            _currentNodeIndex = next;
            ShowCurrentNode();
        }

        // ── Private ───────────────────────────────────────────────

        private void ShowCurrentNode()
        {
            DialogueNode node = GetCurrentNode();
            if (node == null) { EndDialogue(); return; }

            GameEvents.TriggerPlaySFX("DialogueOpen");

            // Send node data to DialogueUI via event or direct call
            UI.DialogueUI.Instance?.ShowNode(node, Advance);
        }

        private void EndDialogue()
        {
            _isPlaying = false;
            UI.DialogueUI.Instance?.HideDialogue();
            GameEvents.TriggerPlaySFX("DialogueClose");
            _onComplete?.Invoke();
        }

        private DialogueNode GetCurrentNode()
        {
            if (_currentNodeIndex < 0 || _currentNodeIndex >= nodes.Count) return null;
            return nodes[_currentNodeIndex];
        }
    }
}
