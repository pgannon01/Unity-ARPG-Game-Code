using System.Collections.Generic;

namespace RPG.Stats
{
    public interface IModifierProvider
    {
        IEnumerable<float> GetAdditiveModifiers(Stat stat); // return a list of additive modifiers for a particular stat
        // Want to ise IEnumerable instead of Enumerator here so we can do a foreach loop
        // Otherwise the Enumerable is exactly the same and works the exact same, with yield returns and such

        IEnumerable<float> GetPercentageModifiers(Stat stat);
        // This is to give us an added percentage modifier for our damage
        // So, say the PC is wearing rings or amulets or something that give a percent bonus to damage
        // Or say a weapon has it's base damage, but also a percentage modifier alongside it for bonus damage
        // This is what that'll be for
    }
}