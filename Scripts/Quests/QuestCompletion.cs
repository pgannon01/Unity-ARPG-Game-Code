using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestCompletion : MonoBehaviour
    {
        [SerializeField] Quest quest; // The quest we're working on
        [SerializeField] string objective; // The individual objectives we're working on

        public void CompleteObjective()
        {
            QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>(); // Get a hold of the players QuestList
            questList.CompleteObjective(quest, objective); // Complete the objectives
        }
    }
}
