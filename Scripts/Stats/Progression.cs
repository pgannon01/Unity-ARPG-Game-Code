using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
    public class Progression : ScriptableObject
    {
        // Need to create this scriptable object for progression for our players and other NPC's stats and leveling progression to point to
        // This will hold all the data of how much health they'll have at certain levels, the experience required, etc
        // Should only need one because everything is going to point at this
        // Personally would make 2, one for player and one for enemies, and maybe different one for different enemies

        [SerializeField] ProgressionCharacterClass[] characterClasses = null;

        // Dictionary
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> lookupTable = null;
        // We know that in our dictionary our first key is our CharacterClass
        // We can then nest a second dictionary and use the key of the Stat we're looking up
        // finally we're looking for our level integer, so we can just have a float array to look up

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            // Dictionary Method
            BuildLookup();

            if (!lookupTable[characterClass].ContainsKey(stat))
            {
                return 0;
            }

            float[] levels = lookupTable[characterClass][stat]; // Call the table, look up by the passed in CharacterClass, stat

            if (levels.Length == 0)
            {
                return 0;
            }

            if (levels.Length < level) // if we don't have a level for the characterclass and/or stat we're trying to look up, return out of this method
            {
                return levels[levels.Length - 1];
            }

            return levels[level - 1];

            ////////////////// DEPRECATED CODE //////////////////////////////////
            // Now that we've expanded our stats into how they now work, we have two lists now, the characterClasses and the ProgressionStats list
            // So we're gonna need 2 for loops, one for each list
            // BUT, we need to optimize it further, as checking over EACH of the character classes AND all their stats would be VERY expensive later on
            // So we need to optimize it using Dictionaries
            // Want to use a Dictionary as a look up table, so we can pass in our class and the stat we're looking for and our level and get it to return what we need
            // But sadly we can't use dictionaries with scriptable objects, dicts are not serializable
            // Have to build our dict after the fact

            // foreach(ProgressionCharacterClass progressionClass in characterClasses)
            // {
            //     if (progressionClass.characterClass != characterClass) continue;

            //     foreach(ProgressionStat progressionStat in progressionClass.stats)
            //     {
            //         if (progressionStat.stat != stat) continue; // if it's not the stat that was passed in continue, because we're looking for a different stat

            //         if (progressionStat.levels.Length < level) continue;
            //         // Length of the array has to be >= the level
            //         // if the Length of the array is LESS than the level we should continue

            //         return progressionStat.levels[level - 1];
            //     }
            // }
            // return 0;
            // Commenting out above code to replace it with our dictionary method, but still keeping it for future looking up and for comments
        }

        // See how many levels there are to ensure we don't level up beyond the max amount of levels we have set
        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            BuildLookup(); // Do this to ensure it's built before doing this

            float[] levels = lookupTable[characterClass][stat];
            return levels.Length;
        }

        private void BuildLookup()
        {
            if (lookupTable != null) return; // only build our dictionary if it hasn't been built before

            lookupTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            // Core of our lookupTable is we're going to have to go through all the character classes in our list, put the key where it belongs...
            // then creating a new dictionary for our Stats

            foreach (ProgressionCharacterClass progressionClass in characterClasses)
            {
                // Two things we need: The key, which we have already (The ProgressionClass) and the dictionary we want to build, the Stat and float[] one
                var statLookupTable = new Dictionary<Stat, float[]>(); // Go over each of our classes and add a stat lookup table
                // (because dicts are such long types, we're just gonna use a general "VAR" type here)

                foreach (ProgressionStat progressionStat in progressionClass.stats)
                {
                    // Our above statLookupTable is in turn built by going over all the stats we have set in our Progression class
                    // Uses the particular stat as the key, with the levels as the variable we want to create
                    statLookupTable[progressionStat.stat] = progressionStat.levels;
                }

                lookupTable[progressionClass.characterClass] = statLookupTable;
            }
        }

        // We want a list of all the character classes in this SO
        // So we create a class within this class, which is below
        [System.Serializable]
        class ProgressionCharacterClass
        {
            // In here we can create arguments and fields and, above, we can create a SerializeField OF this class
            // This way we can change it in the editor as needed
            // So we put all the data for progression we want in here, and we'll also have access to tune and change things in the editor as we please
            public CharacterClass characterClass; // could also just label this SerializeField instead of using public
            public ProgressionStat[] stats;

            // Commenting out our list of health due to reworks, it's all going to be in the stats variable now
            // public float[] Health; 
            // Health will increase per level, so we make it an array of floats to allow us to have different levels of health
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels; // instead of a list of Health values, we're gonna have a list of levels
            // The way it works now is a little more complicated, but it helps a lot more.
            // Now we can have a list of a bunch of different stats, each with their own subset of lists to correspond to the level the characters are
            // So in the future we could also have Strength and Dexterity stats for example, and have them set for characters depending on their level
            // Also allows us to set the level of experience to be rewarded for defeat of certain enemies, based on the level of those enemies
        }
    }
}