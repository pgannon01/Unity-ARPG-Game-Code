using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using UnityEngine;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)] // set the min and max range for our characters levels
        [SerializeField] int StartingLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null; // Point to our scriptable object
        [SerializeField] GameObject LevelUpParticle = null;
        [SerializeField] bool ShouldUseModifiers = false;

        public event Action onLevelUp;

        LazyValue<int> currentLevel; // Used for being able to tell what level we are so we can inform the player when they level up
        // Setting it to 0 so we can initialize it as it actually should be, via our xp or our starting level

        Experience experience;

        private void Awake() 
        {
            experience = GetComponent<Experience>();    
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start() 
        {
            currentLevel.ForceInit();
        }

        private void OnEnable() 
        {
            // Rules for OnEnable the same as Awake, can't use external functions because you can't be sure their state has been set up by then
            // Reason we're using OnEnable here is because it's best practice for registering callbacks
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable() 
        {
            // Similar to OnEnable, don't need this but is good habit to get into
            // If something disables BaseStats it could continue to get notifications from Experience when Experience is gained
            // So to prevent that from happening we'll remove our subscription to the event we set up
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel() 
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                LevelUpEffect();
                onLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(LevelUpParticle, transform);
        }

        public float GetStat(Stat stat) // Make this a general method so we can pass in whatever stat we need, health, experience, etc
        {
            return (GetBaseStat(stat) + GetAddititiveModifier(stat)) * 1 + GetPercentageModifier(stat)/100;
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        public int GetLevel()
        {
            if (currentLevel.value < 1)
            {
                currentLevel.value = CalculateLevel();
            }
            return currentLevel.value;
        }

        private float GetAddititiveModifier(Stat stat)
        {
            if (!ShouldUseModifiers) return 0;
            
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            if (!ShouldUseModifiers) return 0;

            float total = 0;
            foreach(IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private int CalculateLevel()
        {


            // Calculate our level from the players experience points
            // This will be replacing our StartingLevel int
            Experience experience = GetComponent<Experience>();
            if (experience == null) return StartingLevel;
            // When they are enemies we don't calculate what their level might be, as they don't have experience, so we just use the StartingLevel variable

            float currentXP = experience.GetExperienceAmount();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            // calling it penultimateLevel because max level is one more than that once you've achieved xp points

            for (int level = 1; level <= penultimateLevel; level++)
            {
                // If we're less than or equal to the penultimateLevel look up the stat
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }

            return penultimateLevel + 1; // return the final level

            // How above method works:
            /*
                We're getting the penultimate level by querying how many things are in our level array (In the Progression scriptable object in the editor)
                Basically checking how many levels we set inside that array

                Then we are going over all of those set levels from 1 up to the penultimate through the foor loop
                Then getting the experience we need to level up for those particular levels and checking whether we've recieved less experience than that
                If we have less experience, as in we don't have enough xp to level up, we stop and we're at that level, we don't level up
                However if we have the experience needed to move up a level, we level up
            */
        }
    }
}