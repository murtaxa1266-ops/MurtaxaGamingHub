// ============================================================
//  QuestSystem.cs
//  Place in: Assets/Scripts/Systems/Quest/
//  Tracks active quests, objective progress, and completion.
// ============================================================
using System.Collections.Generic;
using UnityEngine;
using MurtaxaGaming.Utils;

namespace MurtaxaGaming.Systems
{
    public class QuestSystem : Singleton<QuestSystem>
    {
        // ── Inspector ────────────────────────────────────────────
        [Header("All Quests in the Game")]
        [SerializeField] private QuestData[] allQuests;

        // ── Runtime State ────────────────────────────────────────
        private Dictionary<string, QuestData> _questLookup  = new Dictionary<string, QuestData>();
        private List<QuestData>               _activeQuests = new List<QuestData>();
        private List<string>                  _completedIds = new List<string>();

        public IReadOnlyList<QuestData> ActiveQuests    => _activeQuests.AsReadOnly();
        public IReadOnlyList<string>    CompletedQuestIds => _completedIds.AsReadOnly();

        // ── Unity Lifecycle ───────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            InitializeLookup();
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyDied  += OnEnemyDied;
            GameEvents.OnItemCollected += OnItemCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied  -= OnEnemyDied;
            GameEvents.OnItemCollected -= OnItemCollected;
        }

        // ── Public API ────────────────────────────────────────────

        /// <summary>Offer a quest to the player (called by NPCInteraction).</summary>
        public void OfferQuest(string questId)
        {
            if (!_questLookup.TryGetValue(questId, out QuestData quest)) return;
            if (quest.state != QuestState.Available) return;

            // For now, auto-accept. Could show UI choice here.
            AcceptQuest(questId);
        }

        /// <summary>Accept a quest by ID.</summary>
        public bool AcceptQuest(string questId)
        {
            if (!_questLookup.TryGetValue(questId, out QuestData quest)) return false;
            if (quest.state != QuestState.Available) return false;

            quest.state = QuestState.Active;
            _activeQuests.Add(quest);

            GameEvents.TriggerQuestAccepted(questId);
            GameEvents.TriggerPlaySFX("QuestAccepted");
            Debug.Log($"[QuestSystem] Quest accepted: {quest.title}");
            return true;
        }

        /// <summary>Manually advance an objective (e.g. from world triggers).</summary>
        public void UpdateObjective(string questId, string targetId, int amount = 1)
        {
            if (!_questLookup.TryGetValue(questId, out QuestData quest)) return;
            if (quest.state != QuestState.Active) return;

            for (int i = 0; i < quest.objectives.Count; i++)
            {
                QuestObjective obj = quest.objectives[i];
                if (obj.targetId == targetId && !obj.IsComplete)
                {
                    obj.currentCount = Mathf.Min(obj.currentCount + amount, obj.requiredCount);
                    GameEvents.TriggerQuestObjectiveUpdated(questId, i, obj.currentCount);
                    Debug.Log($"[QuestSystem] Objective '{obj.description}' progress: {obj.currentCount}/{obj.requiredCount}");
                    break;
                }
            }

            if (quest.AllObjectivesComplete())
                CompleteQuest(questId);
        }

        /// <summary>Returns true if a quest is in the given state.</summary>
        public bool IsQuestInState(string questId, QuestState state)
        {
            return _questLookup.TryGetValue(questId, out QuestData q) && q.state == state;
        }

        // ── Save / Load ───────────────────────────────────────────

        public List<string> GetActiveQuestIds()
        {
            var ids = new List<string>();
            foreach (var q in _activeQuests) ids.Add(q.questId);
            return ids;
        }

        public void LoadData(List<string> activeIds, List<string> completedIds)
        {
            // Reset all quests first
            foreach (var q in _questLookup.Values) q.ResetRuntime();
            _activeQuests.Clear();
            _completedIds.Clear();

            foreach (string id in completedIds)
            {
                if (_questLookup.TryGetValue(id, out QuestData q))
                {
                    q.state = QuestState.Completed;
                    _completedIds.Add(id);
                }
            }
            foreach (string id in activeIds)
            {
                if (_questLookup.TryGetValue(id, out QuestData q))
                {
                    q.state = QuestState.Active;
                    _activeQuests.Add(q);
                }
            }
        }

        // ── Private ───────────────────────────────────────────────

        private void InitializeLookup()
        {
            _questLookup.Clear();
            if (allQuests == null) return;
            foreach (var quest in allQuests)
            {
                if (quest == null) continue;
                quest.ResetRuntime();
                _questLookup[quest.questId] = quest;
            }
        }

        private void CompleteQuest(string questId)
        {
            if (!_questLookup.TryGetValue(questId, out QuestData quest)) return;

            quest.state = QuestState.Completed;
            _activeQuests.Remove(quest);
            _completedIds.Add(questId);

            // Grant rewards
            GrantReward(quest.reward);

            GameEvents.TriggerQuestCompleted(questId);
            GameEvents.TriggerPlaySFX("QuestComplete");
            Debug.Log($"[QuestSystem] Quest completed: {quest.title}");

            // Auto-start chained quest
            if (!string.IsNullOrEmpty(quest.nextQuestId))
                AcceptQuest(quest.nextQuestId);
        }

        private void GrantReward(QuestReward reward)
        {
            if (reward == null) return;

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                Player.PlayerStats stats = playerGO.GetComponent<Player.PlayerStats>();
                stats?.GainXP(reward.xpAmount);
            }

            if (reward.itemReward != null)
                InventorySystem.Instance?.AddItem(reward.itemReward, reward.itemQuantity);
        }

        private void OnEnemyDied(GameObject enemy, Vector3 pos, int xp)
        {
            // Check all active quests for "kill" objectives
            string tag = enemy.tag;
            foreach (var quest in _activeQuests)
                UpdateObjective(quest.questId, tag, 1);
        }

        private void OnItemCollected(string itemName)
        {
            foreach (var quest in _activeQuests)
                UpdateObjective(quest.questId, itemName, 1);
        }
    }
}
