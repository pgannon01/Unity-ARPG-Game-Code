using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Movement;
using RPG.Core;
using GameDevTV.Saving;
using RPG.Attributes;
using RPG.Stats;
using GameDevTV.Utils;
using GameDevTV.Inventories;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] float TimeBetweenAttacks = 1f; // Defaulting to once every second
        // This will eventually be a property of the weapon we have equipped, but setting this here now for testing and education purposes

        [SerializeField] Transform RightHandTransform = null;
        [SerializeField] Transform LeftHandTransform = null;
        [SerializeField] WeaponConfig DefaultWeapon = null;
        [SerializeField] float autoAttackRange = 4f;

        // Commented out as this was a lesson and not meant to be implemented
        // [SerializeField] string DefaultWeaponName = "Unarmed"; // This will be for using our Resources folder to automatically set our default weapon
        
        float TimeSinceLastAttack = Mathf.Infinity; // This is for figuring out how long it's been since we last attacked so we can re-trigger our attack

        Mover PlayerMover;
        Animator AnimatorComponent;

        // Transform Target; // The enemy we're targetting
        Health Target; 

        Equipment equipment;

        WeaponConfig CurrentWeaponConfig; // Spawn the player with a default weapon, their fists, but this will be used for when they pick up a new weapon
        LazyValue<Weapon> CurrentWeapon;

        // On Resources folder:
        // Unity gives us access to a special folder that has to be specifically named "Resources"
        // Whenever we put stuff in this folder, it will be loaded at runtime
        // Normally, only the things associated with our specific scenes would be loaded at runtime and anything not needed by the scene or the objects in it...
        // would not get loaded
        // However, you may need stuff that's not directly referenced by the scene
        // In our instance, we need to make sure the players weapons can transfer from scene to scene.
        // As such, we moved all of our weapon scriptable objects into a Resources folder, this way even scenes that don't have or directly reference these weapons...
        // can load them in and spawn them
        // Resources folders are great ways to ensure content and items can be moved across different scenes without worry of losing them

        private void Awake() 
        {
            // caching components here for later use, and to save on resources
            PlayerMover = GetComponent<Mover>();
            AnimatorComponent = GetComponent<Animator>();
            CurrentWeaponConfig = DefaultWeapon;
            CurrentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
            equipment = GetComponent<Equipment>();
            if (equipment)
            {
                equipment.equipmentUpdated += UpdateWeapon;
            }
        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(DefaultWeapon);
        }

        private void Start() 
        {
            CurrentWeapon.ForceInit();

            // Below code changed due to refactor, but keeping for lessons

            // Below code is for setting our default weapon via the Resources folder
            // We create a Weapon object and pass in our already existing Weapon scriptable object and tell it to look for our default weapons name
            // Because of the Resources folder, we can use the Resources command to call it's methods, in this instance Load
            // Load takes a string which is the resource we want to load (Note, the string can either be a specific FOLDER to load, or a specific FILE you want to load)
            // We can't just pass in DefaultWeaponName into Load() however, so we have to use angle brackets to tell it the type to load
            // By adding the angle brackets and the type, Load will look in the Resources folder for something that has the type you pass in...
            // ... and the name matching the string you pass in

            // Weapon weapon = Resources.Load<Weapon>(DefaultWeaponName);

            // If you wanted to look for a specific file in a specific folder you'd have to specify it, like: "Folder/File"
            // NOTE, can put Resources files in multiple places, and they're important for when you have multiple Resources folder to never have duplicate names
            // as the engine will load up all the Resources folders as one, so duplicate files could cause serious issues

            // Commenting out due to refactor
            // if(CurrentWeapon == null)
            // {
            //     // If we don't have a weapon, equip our default weapon, otherwise the saving system will have done it's job, given us a weapon, and we don't need to do this
            //     EquipWeapon(DefaultWeapon);
            // }
        }

        private void Update()
        {
            TimeSinceLastAttack += Time.deltaTime;
            // Without this to move we would have to hold down the mouse button all the time, because Stop would always be called until we have a target
            if(Target == null) return; 

            if (Target.GetIsDead()) 
            {
                // Find ourselves a new target in range so that we can auto attack the next closest enemy, rather than having to click on them
                Target = FindNewTargetInRange();
                if (Target == null) return;
            }

            if (!GetIsInRange(Target.transform)) // If we're not in range, move into range
            {
                PlayerMover.MoveTo(Target.transform.position, 1f);
            }
            else // If we are in range, stop moving and attack
            {
                PlayerMover.Cancel(); // Note, this is called every frame now

                // Setting our attack animation here so that we'll attack AFTER we finish moving
                AttackBehavior();
            }
        }

        private void AttackBehavior() // Extracting this to a method because this will do more than just an animation
        {
            // Turn towards the combat target
            transform.LookAt(Target.transform);
            if (TimeSinceLastAttack > TimeBetweenAttacks)
            {
                TriggerAttack();
                TimeSinceLastAttack = 0;
            }
        }

        private Health FindNewTargetInRange()
        {
            Health bestCandidate = null;
            float bestDistance = Mathf.Infinity;

            foreach (Health candidates in FindAllTargetsInRange())
            {
                float candidateDistance = Vector3.Distance(transform.position, candidates.transform.position);

                if (candidateDistance < bestDistance)
                {
                    bestCandidate = candidate;
                    bestDistance = candidateDistance;
                }
            }
            return bestCandidate; 
        }

        private IEnumerable<Health> FindAllTargetsInRange()
        {
            // Sphere cast around our current location, looking for anything that has a health component that we can attack
            RaycastHit[] raycastHits = Physics.SphereCastAll(transform.position, autoAttackRange, Vector3.up);

            foreach (RaycastHit hit in raycastHits)
            {
                Health health = hit.transfrom.GetComponent<Health>(); 
                if (health == null) continue;

                if (health.GetIsDead()) continue;

                if (health.gameObject = gameObject) continue; // this is us, don't want to set ourselves as a target

                yield return health;
            }
        }

        private void TriggerAttack()
        {
            // Reset this trigger anytime we attack to ensure we don't have the glitch where the animation will half play
            AnimatorComponent.ResetTrigger("StopAttack");
            // This will trigger the Hit() event
            AnimatorComponent.SetTrigger("Attack"); // Just like how in Mover we had to set the float for our speed and our movement animation...
            // ... we have to set the Trigger for our attack animation to trigger it on and off when we need it
            // So for movement we called it when we're moving, and here we're calling it when we're attacking
            // Also note, the parameters passed to any Animation function NEED TO BE SPELLED AS THEY ARE IN THE EDITOR, so in this case we created an Attack animation state...
            // ... in the animation editor and named it "Attack", and we have to call it EXACTLY as we named it
        }

        // Animation Event
        void Hit()
        {
            if (Target == null) return; // Ensuring this won't run if we don't have a target

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if (CurrentWeapon.value != null)
            {
                CurrentWeapon.value.OnHit(); // Will trigger or OnHit event in Weapon.cs
            }

            if (CurrentWeaponConfig.HasProjectile())
            {
                // Check if our weapon has a projectile, AKA is a ranged weapon
                CurrentWeaponConfig.LaunchProjectile(RightHandTransform, LeftHandTransform, Target, gameObject, damage); 
                // gameObject essentially means THIS, it means the game object that THIS component is attached to 
            }
            else
            {
                // Needed for a HitEvent for our attack animations
                // Don't even really need implementation in this, might have some later, but as it is this just needs to be created and defined so any attack animations...
                // ... we have won't cause errors
                
                Target.TakeDamage(gameObject, damage); 
                // Doing it in this Animation event will apply the damage only once our hit actually hits the enemy
            }
        }

        // Animation Event
        void Shoot()
        {
            Hit();
            // Need to do this as for some of our animations they'll be named Shoot and some will be named hit
            // So better to not repeat ourselves and just have the Hit do all the logic and have the different named events call Hit when THEY need to hit things
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < CurrentWeaponConfig.GetWeaponRange(); // transform.position gets the players position
        }

        public bool CanAttack(GameObject combatTarget)
        {
            // Check to see if the target we click on is dead, if it is ignore its capsule component so we won't be stuck trying to attack an overlapping capsule
            // Probably better to try and destroy the capsule component itself if possible, or just remove any and all collision
            if (combatTarget == null) { return false; }
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) && !GetIsInRange(combatTarget.transform)) { return false; }
            Health TargetToTest = combatTarget.GetComponent<Health>();
            return TargetToTest != null && !TargetToTest.GetIsDead();
        }

        // For the Player
        public void Attack(GameObject CombatTarget)
        {
            Target = CombatTarget.GetComponent<Health>(); // Set the combat target for our player
            GetComponent<ActionScheduler>().StartAction(this);
        }

        public void Cancel()
        {
            // Attacking should stop movement
            AnimatorComponent.SetTrigger("StopAttack");
            AnimatorComponent.ResetTrigger("Attack");
            Target = null;
            PlayerMover.Cancel();
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            CurrentWeaponConfig = weapon;
            CurrentWeapon.value = AttachWeapon(weapon);
        }

        public Health GetTarget()
        {
            return Target;
        }

        public Transform GetHandTransform(bool isRightHand) // Gives us access to the correct transform
        {
            if (isRightHand)
            {
                return RightHandTransform;
            }
            else
            {
                return LeftHandTransform;
            }
        }

        private void UpdateWeapon()
        {
            // Get the weapon from equipment & cast to WeaponConfig
            WeaponConfig weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
            if (weapon == null)
            {
                // What if no weapon?
                EquipWeapon(DefaultWeapon);
            }
            else
            {
                // Equip Weapon
                EquipWeapon(weapon);
            }
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(RightHandTransform, LeftHandTransform, animator);
        }

        public object CaptureState()
        {
            return CurrentWeaponConfig.name; // Shouldn't return null as that would return an error
            // COULD BE, but we're assuming it won't
        }

        public void RestoreState(object state)
        {
            // cast the state to a string and assign it to the variable weaponName, as we'll be passing in the CurrentWeapon.name in from CaptureState
            string weaponName = (string)state;
            // Load the Resources folder and have it look for the weapon we saved
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            // Equip that weapon
            EquipWeapon(weapon);
        }
    }
}