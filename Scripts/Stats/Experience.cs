using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDevTV.Saving;

namespace RPG.Stats
{
    public class Experience : MonoBehaviour, ISaveable
    {
        // Keep track of our experience points
        [SerializeField] float ExperiencePoints = 0;

        // public delegate void ExperienceGainedDelegate(); 
        // Can declare a delegate like above, but if it's going to be void and have no arguments can just use Action (below)
        public event Action onExperienceGained; // so instead of using the Delegate's name you just use the name Action
        // When you make an event you make it so that the methods can't be overwritten by other classes

        private void Update() 
        {
            // This is for a debug function to test leveling up and, primarily, test that our shops can hold new items as we level
            if (Input.GetKey(KeyCode.E))
            {
                GainExperience(Time.deltaTime * 1000);
            }
        }

        public float GetExperienceAmount()
        {
            return ExperiencePoints;
        }

        // Reward Experience points
        public void GainExperience(float experience)
        {
            ExperiencePoints += experience;
            onExperienceGained(); // Calling the event will call everything in the delegate list
        }

        public object CaptureState()
        {
            // Save our XP
            return ExperiencePoints;
        }

        public void RestoreState(object state)
        {
            // cast our state to a float and set it to equal to our experience points to ensure that's what we load
            ExperiencePoints = (float)state;
        }
    }
}
