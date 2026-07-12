// ============================================================
//  QuestData.cs
//  Place in: Assets/Scripts/Systems/Quest/
//  ScriptableObject that defines a single quest.
//  Create via: Assets > Create > MurtaxaGaming > Quest
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace MurtaxaGaming.Systems
{
    public enum QuestState { Available, Active, Completed, Failed }

    [System.Serializable]
    public class QuestObjective
    {
        public string   description;      // "Kill 5 Wolves"
        public string   targetId;         // e.g. enemy tag or item id used for tracking
        public int      requiredCount;    // how many needed
        [System.NonSerialized]
        public int      currentCount;     // runtime progress (not serialized in SO)
        public bool     IsComplete => currentCount >= requiredCount;
    }

    [System.Serializable]
    public class QuestReward
    {
        public int            xpAmount;
        public int            goldAmount;
        public InventoryItem  itemReward;
        public int            itemQuantity = 1;
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "MurtaxaGaming/Quest", order = 1)]
    public class QuestData : ScriptableObject
    {
        [Header("Identity")]
        public string         questId;
        public string         title;
        [TextArea(2, 5)]
        public string         description;
        public string         npcGiverId;   // NPC that gives this quest

        [Header("Objectives")]
        public List<QuestObjective> objectives = new List<QuestObjective>();

        [Header("Rewards")]
        public QuestReward    reward;

        [Header("Chain")]
        public string         nextQuestId;  // Quest to auto-unlock on completion (optional)

        // Runtime state (not stored in ScriptableObject directly — QuestSystem manages it)
        [System.NonSerialized] public QuestState state = QuestState.Available;

        /// <summary>Reset runtime state (called when quest system loads).</summary>
        public void ResetRuntime()
        {
            state = QuestState.Available;
            foreach (var obj in objectives)
                obj.currentCount = 0;
        }

        /// <summary>Returns true if all objectives are complete.</summary>
        public bool AllObjectivesComplete()
        {
            foreach (var obj in objectives)
                if (!obj.IsComplete) return false;
            return true;
        }
    }
}
