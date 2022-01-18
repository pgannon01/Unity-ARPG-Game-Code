using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Attributes
{
    public class HealthDisplay : MonoBehaviour
    {
        // Display the health as a percentage on the UI
        Health health;

        private void Awake() 
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>(); // Get the player's health component    
        }

        private void Update() 
        {
            // Update Health UI text
            GetComponent<Text>().text = String.Format("{0:0}/{1:0}", health.GetHealthPoints(), health.GetMaxHealthPoints());
            // Above is getting the UI's Text component and then saying...
            // take the first thing on the right, our percentage, and put it in place of our curly braces with the 0 in it
            // The :0 after the first zero says to round the number to the input decimal places, in this case 0 decimal places, so only whole numbers
            // If you wanted more decimal places, you'd put: {0:0.0}
        }
    }
}
