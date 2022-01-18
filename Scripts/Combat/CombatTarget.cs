using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Attributes;
using RPG.Control;

namespace RPG.Combat
{
    [RequireComponent(typeof(Health))] // This says that whenever we place a CombatTarget script on a character it will automatically place a Health component on the...
    // ... prefab as well, we won't ever really need a CombatTarget that doesn't have health
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        // For the enemies our player will target
        public bool HandleRaycast(PlayerController callingController)
        {
            if (!enabled) return false;
            if (!callingController.GetComponent<Fighter>().CanAttack(gameObject))
            {
                // If the callingController can't attack us for some reason, then return false
                return false;
            }

            if (Input.GetMouseButton(0))
            {
                // This will make it so that it will only run our attack function when we left click
                // If we click, get our Fighter component and call the attack function
                callingController.GetComponent<Fighter>().Attack(gameObject);
            }

            return true; // Setting this here so even if we are hovering over an enemy, we don't want to do any movement
        }
    }
}