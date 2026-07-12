// ============================================================
//  QuestLogUI.cs
//  Place in: Assets/Scripts/UI/
//  Displays active quests and their objective progress.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MurtaxaGaming.Systems;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.UI
{
    public class QuestLogUI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("Quest List")]
        [SerializeField] private Transform          questListContainer;
        [SerializeField] private GameObject         questEntryPrefab;

        [Header("Detail Panel")]
        [SerializeField] private TextMeshProUGUI    questTitleText;
        [SerializeField] private TextMeshProUGUI    questDescText;
        [SerializeField] private Transform          objectiveContainer;
        [SerializeField] private GameObject         objectivePrefab;

        [Header("Controls")]
        [SerializeField] private Button             closeButton;
        [SerializeField] private KeyCode            toggleKey = KeyCode.L;

        // ── Runtime ───────────────────────────────────────────────
        private bool                 _isOpen = false;
        private List<GameObject>     _questEntries    = new List<GameObject>();
        private List<GameObject>     _objectiveItems  = new List<GameObject>();

        // ── Unity Lifecycle ───────────────────────────────────────
        private void Awake()
        {
            closeButton?.onClick.AddListener(() => Toggle(false));
        }

        private void OnEnable()
        {
            GameEvents.OnQuestAccepted        += _ => Refresh();
            GameEvents.OnQuestCompleted       += _ => Refresh();
            GameEvents.OnQuestObjectiveUpdated += (id, idx, prog) => Refresh();
        }

        private void OnDisable()
        {
            GameEvents.OnQuestAccepted        -= _ => Refresh();
            GameEvents.OnQuestCompleted       -= _ => Refresh();
            GameEvents.OnQuestObjectiveUpdated -= (id, idx, prog) => Refresh();
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle(!_isOpen);
        }

        // ── Public API ────────────────────────────────────────────
        public void Toggle(bool open)
        {
            _isOpen = open;
            gameObject.SetActive(open);
            GameEvents.TriggerQuestLogToggled(open);
            if (open) { Refresh(); GameEvents.TriggerPlaySFX("UIOpen"); }
            else GameEvents.TriggerPlaySFX("UIClose");
        }

        private void Refresh()
        {
            if (!_isOpen || QuestSystem.Instance == null) return;

            // Clear list
            foreach (var go in _questEntries) Destroy(go);
            _questEntries.Clear();

            var active = QuestSystem.Instance.ActiveQuests;

            foreach (var quest in active)
            {
                GameObject entry = Instantiate(questEntryPrefab, questListContainer);
                _questEntries.Add(entry);

                TextMeshProUGUI title = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (title != null) title.text = quest.title;

                // Progress bar
                Slider bar = entry.GetComponentInChildren<Slider>();
                if (bar != null && quest.objectives.Count > 0)
                {
                    int done = 0;
                    foreach (var obj in quest.objectives) if (obj.IsComplete) done++;
                    bar.value = (float)done / quest.objectives.Count;
                }

                // Click to show detail
                string qid = quest.questId;
                Button btn = entry.GetComponent<Button>();
                btn?.onClick.AddListener(() => ShowDetail(qid));
            }

            // Show first quest detail by default
            if (active.Count > 0) ShowDetail(active[0].questId);
        }

        private void ShowDetail(string questId)
        {
            var quest = QuestSystem.Instance?.ActiveQuests;
            if (quest == null) return;

            QuestData data = null;
            foreach (var q in quest) if (q.questId == questId) { data = q; break; }
            if (data == null) return;

            if (questTitleText != null) questTitleText.text = data.title;
            if (questDescText  != null) questDescText.text  = data.description;

            // Objectives
            foreach (var go in _objectiveItems) Destroy(go);
            _objectiveItems.Clear();

            foreach (var obj in data.objectives)
            {
                GameObject item = Instantiate(objectivePrefab, objectiveContainer);
                _objectiveItems.Add(item);

                TextMeshProUGUI txt = item.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = $"{(obj.IsComplete ? "✓" : "○")} {obj.description} ({obj.currentCount}/{obj.requiredCount})";
            }
        }
    }
}
