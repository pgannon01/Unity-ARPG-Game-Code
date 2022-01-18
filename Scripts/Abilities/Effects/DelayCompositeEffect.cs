using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Abilities.Effects
{
    [CreateAssetMenu(fileName = "Delayed Composite Effect", menuName = "Abilities/Effects/Delayed Composite", order = 0)]
    public class DelayCompositeEffect : EffectStrategy
    {
        [SerializeField] float delay;
        [SerializeField] EffectStrategy[] delayedEffects;
        [SerializeField] bool abortIfCancelled = false;

        public override void StartEffect(AbilityData data, Action finished)
        {
            data.StartCoroutine(DelayedEffect(data, finished));
        }

        private IEnumerator DelayedEffect(AbilityData data, Action finished)
        {
            yield return new WaitForSeconds(delay);
            if (abortIfCancelled && data.IsCancelled()) yield break;
            foreach (EffectStrategy effect in delayedEffects)
            {
                effect.StartEffect(data, finished);
            }
        }
    }
}
