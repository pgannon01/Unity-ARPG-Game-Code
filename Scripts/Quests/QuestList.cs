using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using GameDevTV.Saving;
using RPG.Core;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        List<QuestStatus> statuses = new List<QuestStatus>();

        public event Action onUpdate;

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest))
            {
                return; // Ensure we can't pick up quests multiple times
            }
            // Add quests to our Quest journal
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        public void CompleteObjective(Quest quest, string objective)
        {
            QuestStatus status = GetQuestStatus(quest);
            status.CompleteObjective(objective);
            if (status.IsQuestComplete())
            {
                GiveQuestReward(quest);
            }
            if (onUpdate != null)
            {
                // Need this here as well to redraw the UI when we complete objectives
                onUpdate();
            }
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }

        private QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        private void GiveQuestReward(Quest quest)
        {
            foreach (Quest.Reward reward in quest.GetRewards())
            {
                bool onSuccess = GetComponent<Inventory>().AddToFirstEmptySlot(reward.item, reward.number); 
                // Get the players inventory so we can add their rewards to their inventory
                if (!onSuccess)
                {
                    // if no space in the inventory, drop the item on the ground
                    GetComponent<ItemDropper>().DropItem(reward.item, reward.number);
                }
            }
        }

        public object CaptureState()
        {
            List<object> state = new List<object>();
            foreach (QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            List<object> stateList = state as List<object>;
            if (stateList == null) return;
            
            statuses.Clear();
            foreach (object objectState in stateList)
            {
                statuses.Add(new QuestStatus(objectState));
            }
        }

        public bool? Evaluate(string predicate, string[] parameters)
        {
            switch (predicate)
            {
                case "HasQuest": 
                    return HasQuest(Quest.GetByName(parameters[0]));
                
                case "CompletedQuest":
                    return GetQuestStatus(Quest.GetByName(parameters[0])).IsQuestComplete();
            }

            return null;
        }
    }
}
