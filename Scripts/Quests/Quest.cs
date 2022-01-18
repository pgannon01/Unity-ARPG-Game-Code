using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Quests
{    
    [CreateAssetMenu(fileName = "Quest", menuName = "ARPGProject/Quest", order = 0)]
    public class Quest : ScriptableObject 
    {  
        [SerializeField] List<Objective> objectives = new List<Objective>();
        [SerializeField] List<Reward> rewards = new List<Reward>();

        [System.Serializable]
        public class Reward
        {
            [Min(1)] // Shouldn't have less than 1 for rewards so we'll always reward at least 1 item
            public int number; // of rewards
            public InventoryItem item; // The item we may give
        }

        [System.Serializable]
        public class Objective
        {
            public string reference;
            public string description; // Will be displayed in the UI
        }

        public string GetTitle()
        {
            return name;
        }

        public int GetObjectiveCount()
        {
            return objectives.Count;
        }

        public IEnumerable<Objective> GetObjectives()
        {
            return objectives;
        }

        public IEnumerable<Reward> GetRewards()
        {
            return rewards;
        }

        public bool HasObjective(string objectiveRef)
        {
            foreach (Quest.Objective objective in objectives)
            {
                if (objective.reference == objectiveRef)
                {
                    return true;
                }
            }
            return false;
        }

        public static Quest GetByName(string questName)
        {
            // Get a Quest SO by giving it that Quest's name
            foreach(Quest quest in Resources.LoadAll<Quest>(""))
            {
                if (quest.name == questName)
                {
                    return quest;
                }
            }
            return null;
        }
    }
}
