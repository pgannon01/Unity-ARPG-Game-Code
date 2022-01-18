using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Abilities
{
    public abstract class TargetingStrategy : ScriptableObject 
    {
        public abstract void StartTargeting(AbilityData data, Action finished);
        // Abstract classes can't be implemented and can, instead, be implemented by children, or classes inheriting from this class
        // Also, class has to be abstract as well
        // Need this abstract class to be the catchall for all types of abilities that may implement Targeting differently
    }
}
