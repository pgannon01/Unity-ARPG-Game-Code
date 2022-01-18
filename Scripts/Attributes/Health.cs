using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;
using GameDevTV.Saving;
using RPG.Stats;
using RPG.Core;

namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70;
        [SerializeField] TakeDamageEvent takeDamage;
        public UnityEvent onDie;

        // [SerializeField] UnityEvent takeDamage; // Commented out to show how it's usually done, as above version was refactored
        // There's another way to subscribe to Unity Events, and it's by doing this!
        // With this, we can, in the editor, set and subscribe to events, and then, later in this script, set up an area where we will INVOKE those events

        // Problem with Editor Events is that to pass in values from code is quite convoluted. We need to do <> and pass in the type we want to pass in, float in our case
        // Then we need to create a subclass of this unity event like below:
        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float>
        {
            // Can leave empty for now because is irrelevant at the moment
        }

        // This will contain anything to do with health for characters
        // Putting this here instead of defining it on individual characters to avoid writing the same code over and over, because almost everything will need some form...
        // ... of Health, so we can make one parent script that can apply to all characters that need health
        LazyValue<float> CurrentHealth;
        // Setting this to a negative value to start off
        // Reason being because there's a bug where, upon reloading the game, the enemies might accidentally respawn because, in Start...
        // we reset the health points, which conflicts with RestoreState
        // So we set it to -1 here by default. Then we load in and if, when we load in, the enemy is dead and has no health, we won't reset the health points

        bool wasDeadLastFrame = false;

        private void Awake() 
        {
            CurrentHealth = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Start() 
        {
            CurrentHealth.ForceInit(); // If at this point we haven't initialized CurrentHealth, we will force it to Initialize here
            // Commented out because we moved this to Awake using the teachers script
            // if (CurrentHealth < 0)
            // {
            //     // Want to get the health not from THIS component, but from our stats
            //     CurrentHealth = GetComponent<BaseStats>().GetStat(Stat.Health); // Have the BaseStats get the health from the characters stats
            //     // Health will be decided via the characters level, hence why we get it from the stats and not from here
            // }
        }

        private void OnEnable() 
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
        }

        private void OnDisable() 
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
        }

        public bool GetIsDead()
        {
            return CurrentHealth.value <= 0;
        }

        public void TakeDamage(GameObject instigator, float Damage)
        {
            CurrentHealth.value = Mathf.Max(CurrentHealth.value - Damage, 0);
            
            // Above basically says to set CurrentHealth to whatever CurrentHealth - Damage returns, or if that's 0 or below, just return 0, won't ever go below 0
            if(GetIsDead())
            {
                onDie.Invoke();
                
                AwardExperience(instigator);
            }
            else
            {
                // If we don't die, spawn the damage text
                takeDamage.Invoke(Damage); // This will trigger all of the functions in our list in the SerializeableField Event we set up
            }
            UpdateState();
        }

        public void Heal(float healthToRestore)
        {
            CurrentHealth.value = Mathf.Min(CurrentHealth.value + healthToRestore, GetMaxHealthPoints());
            UpdateState();
        }

        public float GetHealthPoints()
        {
            return CurrentHealth.value;
        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public float GetPercentage() 
        {
            return 100 * (GetFraction());
        }

        public float GetFraction() // get a fraction between 0 and 1, mainly for health bar scale
        {
            return CurrentHealth.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public void UpdateState()
        {
            Animator animator = GetComponent<Animator>();
            // Set the dead trigger and cancel current actions if this is the first time we're going into the dead state
            if (!wasDeadLastFrame && GetIsDead())
            {
                animator.SetTrigger("Death");
                GetComponent<ActionScheduler>().CancelCurrentAction(); // Cancel any currently running action
            }

            if (wasDeadLastFrame && !GetIsDead())
            {
                // Respawn and reset the animator to bring the character back to life
                animator.Rebind(); // Rebind just resets the animator entirely and starts from scratch
            }

            wasDeadLastFrame = GetIsDead();
        }

        // Award the player the experience for killing the enemies
        private void AwardExperience(GameObject instigator)
        {
            
            Experience experience = instigator.GetComponent<Experience>(); // Check if the instigator we pass in HAS an experience component
            // NPC's won't have an Experience component, hence if this passes it'll be a player
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }

        private void RegenerateHealth()
        {
            // CurrentHealth = GetComponent<BaseStats>().GetStat(Stat.Health); // This way would be to fully regen health upon levelling up
            float regenHealthPoints = CurrentHealth.value = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
            CurrentHealth.value = Mathf.Max(CurrentHealth.value, regenHealthPoints); 
            // Above is a way to set it to a certain percentage of our health upon leveling
            // Basically it's checking how much health we have when we level up and then checking how much 70% of that would be (Or whatever number we set)
            // If our regen is higher than our CurrentHealth level, the regenHealthPoints health would be what we go to
            // Else, we'll just stay at our CurrentHealth value
        }

        public object CaptureState()
        {
            return CurrentHealth;
        }

        public void RestoreState(object state)
        {
            CurrentHealth.value = (float)state; // casting our state, as a float, to our CurrentHealth
            // This should save our health

            UpdateState();

            // Without this check, if something's dead and we load, they'll respawn again which we may or may not want
        }
    }
}
