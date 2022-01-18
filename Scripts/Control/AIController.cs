using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using RPG.Attributes;
using GameDevTV.Utils;
using System;
using UnityEngine.AI;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField]
        float ChaseDistance = 5f;

        [SerializeField]
        float SuspicionTime = 3f;

        [SerializeField]
        float AggroCooldownTime = 5f;

        [SerializeField]
        PatrolPath PatrolPath;

        [SerializeField]
        float WaypointTolerance = 1f;

        [SerializeField]
        float GuardWaitTime = 3f;

        [Range(0,1)] // Makes it so that our PatrolSpeedFraction below can ONLY be between 0 and 1
        [SerializeField]
        float PatrolSpeedFraction = 0.2f; // 20% of our maximum speed

        [SerializeField] float shoutDistance = 5f;

        Fighter FighterComponent;
        Health HealthComponent;
        GameObject Player;
        Mover MoverComponent;

        // This will be used to store the location a guard is supposed to be standing and guarding
        // This way, when he loses aggro of the player, he'll return to where he was last standing
        LazyValue<Vector3> GuardLocation;

        // Give the enemies some more human like abilities to chase after the player, even after they leave the aggro zone/lose sight
        // A normal guard/enemy wouldnt just give up, they'd chase and if they lost you continue looking for where you would've gone, search, and then give up
        // This is how we're going to, start to, accomplish that behavior
        float TimeSinceLastSawPlayer = Mathf.Infinity;
        int CurrentWaypointIndex = 0;
        float TimeAtWaypoint = Mathf.Infinity;
        float TimeSinceAggrevated = Mathf.Infinity;

        private void Awake() 
        {
            // Caching references here
            FighterComponent = GetComponent<Fighter>();
            HealthComponent = GetComponent<Health>();
            Player = GameObject.FindWithTag("Player");
            MoverComponent = GetComponent<Mover>();

            GuardLocation = new LazyValue<Vector3>(GetGuardPosition);
            GuardLocation.ForceInit();
        }

        public void Reset()
        {
            // Warp enemies back to starting positions upon player death, and also grant some health
            NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.Warp(GuardLocation.value);

            // Reset AI State
            TimeSinceLastSawPlayer = Mathf.Infinity;
            CurrentWaypointIndex = 0;
            TimeAtWaypoint = Mathf.Infinity;
            TimeSinceAggrevated = Mathf.Infinity;
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start() 
        {
        }

        private void Update()
        {
            if (HealthComponent.GetIsDead()) return; // If the enemy is dead, don't run the following code

            // calculate distance to player
            if (IsAggrevated() && FighterComponent.CanAttack(Player))
            {
                AttackBehavior();
            }
            else if (TimeSinceLastSawPlayer < SuspicionTime)
            {
                // Get hold of the Action Scheduler and cancel the current action
                SuspicionBehavior();
            }
            else
            {
                PatrolBehavior();
            }
            UpdateTimers();
        }

        public void Aggrevate()
        {
            TimeSinceAggrevated = 0;

        }

        private void UpdateTimers()
        {
            TimeAtWaypoint += Time.deltaTime;
            TimeSinceLastSawPlayer += Time.deltaTime;
            TimeSinceAggrevated += Time.deltaTime;
        }

        private void PatrolBehavior()
        {
            Vector3 NextPosition = GuardLocation.value;

            if (PatrolPath != null) // If we have a PatrolPath assigned to the enemy, have them patrolling. Otherwise, have them standing still and guarding
            {
                if (AtWaypoint())
                {
                    TimeAtWaypoint = 0;
                    CycleWaypoint(); // Advance to the next waypoint
                }
                NextPosition = GetCurrentWaypoint();
            }
            if (TimeAtWaypoint > GuardWaitTime) // Have the guard linger on a waypoint as he patrols
            {
                // If player is outside of attack range, cancel the AI's ability to attack and return to the set Guarding position
                MoverComponent.StartMoveAction(NextPosition, PatrolSpeedFraction);
                // Previously had it set to cancel the attack, but since StartMoveAction already does that, we only need StartMoveAction
            }
        }

        private bool AtWaypoint()
        {
            // Check if we're at the waypoint
            float DistanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return DistanceToWaypoint < WaypointTolerance;
        }

        private void CycleWaypoint()
        {
            CurrentWaypointIndex = PatrolPath.GetNextIndex(CurrentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return PatrolPath.GetWaypoint(CurrentWaypointIndex);
        }

        private void SuspicionBehavior()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehavior()
        {
            TimeSinceLastSawPlayer = 0;
            FighterComponent.Attack(Player);

            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            // Loop over all the hits
            foreach (RaycastHit hit in hits)
            {
                // find any enemy components
                AIController ai = hit.collider.GetComponent<AIController>();
                if (ai == null) continue;

                // aggrevate those enemies
                ai.Aggrevate();
            }
            
        }

        private bool IsAggrevated()
        {
            float DistanceToPlayer = Vector3.Distance(Player.transform.position, transform.position);
            // check if distance to player is within chase distance OR check if the aggro cooldown time is less than the time since last aggrod
            return DistanceToPlayer < ChaseDistance || TimeSinceAggrevated < AggroCooldownTime;
        }

        // Called by Unity!
        //private void OnDrawGizmos() // Gives us the ability to have Unity call this when it wants to draw Gizmos 
        private void OnDrawGizmosSelected()
        {
            // OnDrawGizmosSelected, will ONLY draw the Gizmos when that specific thing is selected
            // OnDrawGizmos method will always have the Gizmos drawn on the editor screen, whereas this Selected version will only draw them when that prefab is selected

            // Need to make some calls to the Gizmo API
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, ChaseDistance);
        }
    }
}
