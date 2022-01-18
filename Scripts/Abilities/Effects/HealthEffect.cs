using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Abilities.Effects
{
    [CreateAssetMenu(fileName = "Health Effect", menuName ="Abilities/Effects/Health Effect", order = 0)]
    public class HealthEffect : EffectStrategy
    {
        [SerializeField] float healthChange; // How much damage we're doing, or potentially how much healing we're doing

        public override void StartEffect(AbilityData data, Action finished)
        {
            foreach (GameObject target in data.GetTargets())
            {
                Health health = target.GetComponent<Health>();
                if (health)
                {
                    if (healthChange < 0) // if our healthChange variable is in the negative, we want to apply damage
                    {
                        health.TakeDamage(data.GetUser(), -healthChange);
                    }
                    else // else we want to heal the target
                    {
                        health.Heal(healthChange);
                    }
                }
            }
            finished();
        }
    }
}
