using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace RPG.Attributes
{
    public class ManaDisplay : MonoBehaviour
    {
        // Display the mana as a percentage on the UI
        Mana mana;

        private void Awake()
        {
            mana = GameObject.FindWithTag("Player").GetComponent<Mana>(); // Get the player's health component    
        }

        private void Update()
        {
            // Update Mana UI text
            GetComponent<Text>().text = String.Format("{0:0}/{1:0}", mana.GetMana(), mana.GetMaxMana());
        }
    }
}
