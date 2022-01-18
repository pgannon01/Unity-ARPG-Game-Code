using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDevTV.Inventories;
using RPG.Stats;

namespace RPG.Inventories
{
    [CreateAssetMenu(menuName = ("RPG/Inventory/Equipable Item"))]
    public class StatsEquipableItem : EquipableItem, IModifierProvider
    {
        [SerializeField]
        Modifier[] additiveModifiers;
        [SerializeField]
        Modifier[] percentageModifiers;

        [System.Serializable]
        struct Modifier
        {
            public Stat stat;
            public float value;
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            foreach (Modifier modifier in additiveModifiers)
            {
                // This function basically goes through the list of modifiers we set in the additiveModifier (will be the same for percentage)
                // it will be going through the list of modifiers we've set and see if we've set a modifier for the stat we pass into this function
                // If we have, we'll modify it based on the value we set
                if (modifier.stat == stat)
                {
                    // check to see if the stat that was configured is the stat we're looking for(what we're passing in to the function)
                    yield return modifier.value;
                }
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            // This is the same as additiveModifiers, except obviously for percentageModifiers instead
            foreach (Modifier modifier in percentageModifiers)
            {
                if (modifier.stat == stat)
                {
                    yield return modifier.value;
                }
            }
        }
    }
}
