using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDevTV.Inventories;
using RPG.Stats;

namespace RPG.Inventories
{
    public class StatsEquipment : Equipment, IModifierProvider
    {
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            foreach (EquipLocation slot in GetAllPopulatedSlots()) // Return an enum of EquipLocation's
            {
                // Get the item at that slot, whatever it is and cast it to IModifierProvider
                IModifierProvider item = GetItemInSlot(slot) as IModifierProvider;
                // Not all things that are equippable will have IModifierProvider, just a standard piece of equipment won't have it
                if (item == null) continue; // Since not every equipment will have an IModifierProvider, we just skip over the things that don't

                foreach (float modifier in item.GetAdditiveModifiers(stat)) // Loop over all the modifiers
                {
                    // So by this point we're going through every individual slot and, now, getting each of their individual modifiers
                    // So if an equipment adds both health and strength, for instance, we'll need to be sure both are applied
                    yield return modifier;
                }
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            foreach (EquipLocation slot in GetAllPopulatedSlots())
            {
                IModifierProvider item = GetItemInSlot(slot) as IModifierProvider;
                if (item == null) continue;

                foreach (float modifier in item.GetPercentageModifiers(stat))
                {
                    yield return modifier;
                }
            }
        }
    }
}