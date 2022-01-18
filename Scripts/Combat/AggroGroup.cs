using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat
{
    public class AggroGroup : MonoBehaviour
    {
        // This script will be for combinging enemies into one large aggro group
        // So when one enemy aggros, they'll all aggro together
        [SerializeField] Fighter[] fighters; // The fighters we want to activate/deactivate
        [SerializeField] bool activateOnStart = false;

        private void Start() 
        {
            Activate(activateOnStart);
        }

        public void Activate(bool shouldActivate)
        {
            foreach (Fighter fighter in fighters)
            {
                CombatTarget target = fighter.GetComponent<CombatTarget>();
                if (target != null)
                {
                    target.enabled = shouldActivate;
                }
                fighter.enabled = shouldActivate;
            }
        }
    }
}
