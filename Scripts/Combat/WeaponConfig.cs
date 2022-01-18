using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Attributes;
using RPG.Stats;
using UnityEngine;

namespace RPG.Combat
{
    // This is a scriptable object

    // Definition of scriptable object:
    // A data container that you can use to save large amounts of data, independent of class instances
    // One of the main use cases for ScriptableObjects is to reduce your Project’s memory usage by avoiding copies of values
    // Every time you instantiate that Prefab, it will get its own copy of that data. 
    // Instead of using the method, and storing duplicated data, you can use a ScriptableObject to store the data and then access it by reference from all of the Prefabs. 
    // This means that there is one copy of the data in memory.
    // Just like MonoBehaviours, ScriptableObjects derive from the base Unity object but, unlike MonoBehaviours, you can not attach a ScriptableObject to a GameObject
    // Instead, you need to save them as Assets in your Project.
    
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : EquipableItem, IModifierProvider
    {
        // Putting in this scriptable object any information that may change depending on the weapon we're wielding
        // We'll always generally wield melee in our right hand, so that won't change, but what might change is the type of weapon
        [SerializeField] AnimatorOverrideController AnimatorOverride = null;

        [SerializeField] Weapon WeaponPrefab = null;

        [SerializeField] float WeaponRange = 2f;

        [SerializeField] float WeaponDamage = 5f;
        [SerializeField] float PercentageBonus = 0;

        [SerializeField] bool isRightHanded = true;

        [SerializeField] Projectile projectile = null; // Some weapons will have projectiles, some won't

        const string WeaponName = "Weapon"; // String reference so we can use this throughout the class

        public Weapon Spawn(Transform RightHandTransform, Transform LeftHandTransform, Animator animator)
        {
            // If we already have a weapon in our hands and run over a new one, destroy the old weapon so we only have the new one
            DestroyOldWeapon(RightHandTransform, LeftHandTransform);


            Weapon weapon = null;

            if (WeaponPrefab != null)
            {
                Transform HandTransform = GetTransform(RightHandTransform, LeftHandTransform);
                weapon =  Instantiate(WeaponPrefab, HandTransform);
                weapon.name = WeaponName;
            }

            // Casting
            // overrideController will be null if it's just the runtimeAnimatorController, NOT the AnimatorOverrideController
            // otherwise overrideController will have the value of the AnimatorOverrideController that is in the runtimeAnimatorController slot
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

            if (AnimatorOverride != null)
            {
                // Previously, there was a bug with our code where if you picked up a weapon, then picked up a second weapon that didn't have its...
                // ...Animator Override set, it would bug out and you'd use the previous weapon's animator override, which we obviously don't want               
                // The else statement below is meant to fix that
                animator.runtimeAnimatorController = AnimatorOverride;
            }
            else if(overrideController != null)
            {
                // if it's already an override, find its parent and put that in the runtimeAnimatorController's slot instead
                animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
            }

            return weapon;
        }

        private Transform GetTransform(Transform RightHandTransform, Transform LeftHandTransform)
        {
            // Get whether the weapon is in the right or left hand
            Transform HandTransform;
            if (isRightHanded) HandTransform = RightHandTransform;
            else HandTransform = LeftHandTransform;
            return HandTransform;
        }

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(Transform RightHand, Transform LeftHand, Health Target, GameObject Instigator, float CalculatedDamage)
        {
            // Actually create the Projectile in the game and set it's target to who we're shooting at
            Projectile projectileInstance = Instantiate(projectile, GetTransform(RightHand, LeftHand).position, Quaternion.identity);
            projectileInstance.SetTarget(Target, Instigator, CalculatedDamage);
        }

        public float GetWeaponRange()
        {
            return WeaponRange;
        }

        public float GetWeaponDamage()
        {
            return WeaponDamage;
        }

        public float GetPercentageBonus()
        {
            return PercentageBonus;
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return WeaponDamage;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return PercentageBonus;
            }
        }

        private void DestroyOldWeapon(Transform RightHand, Transform LeftHand)
        {
            Transform OldWeapon = RightHand.Find(WeaponName);
            if (OldWeapon == null)
            {
                // Checks to see if we have a weapon in our right hand. If there isn't one, assign it to our left hand
                OldWeapon = LeftHand.Find(WeaponName);
            }
            // Check to see if we have a weapon in our left hand. If not, then we don't have any weapons
            if (OldWeapon == null) return;

            OldWeapon.name = "DESTROYING"; // Make sure we don't have an issue when we pick up a new weapon where we have 2 weapons named "Weapon"
            // This way we'll change the name, so we won't accidentally destroy both weapons on pickup
            
            // If one of the above checks fail that means we have a weapon in our hand, and so we now have to destroy that old weapon
            Destroy(OldWeapon.gameObject);
        }
    }
}