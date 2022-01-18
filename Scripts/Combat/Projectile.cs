using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float ProjectileSpeed = 1f;
        [SerializeField] bool IsHoming = false;
        [SerializeField] GameObject HitEffect = null;
        [SerializeField] float MaxLifeTime = 10f;
        [SerializeField] GameObject[] DestroyOnHit = null; // This will give us the ability to destroy certain parts of our projectiles over time
        [SerializeField] float LifeAfterImpact = 2f;
        [SerializeField] UnityEvent onHit;

        float Damage = 0;
        Health Target = null;
        Vector3 targetPoint;
        GameObject Instigator = null;

        private void Start() 
        {
            // Get the projectile to look at our target
            // Note, if you put this in Update, it will constantly track the target, which you may or may not want for projectiles
            transform.LookAt(GetAimLocation());
            // With this, it'll look at the target once, then try to go to where they were when you first shot
            // So if the target moves, you might not hit them
        }

        private void Update() 
        {
            if (Target == null) return;

            if (Target != null && IsHoming && !Target.GetIsDead())
            {
                transform.LookAt(GetAimLocation());
            }
            transform.Translate(Vector3.forward * ProjectileSpeed * Time.deltaTime);
            // Vector3.forward = Manipulate a GameObject's position on the Z axis
            // Transform.forward moves the GameObject while also considering it's rotation, Vector3.forward does not
        }

        public void SetTarget(Health target, GameObject instigator, float damage)
        {
            SetTarget(instigator, damage, target);
        }

        public void SetTarget(Vector3 targetPoint, GameObject instigator, float damage)
        {
            SetTarget(instigator, damage, null, targetPoint);
        }

        public void SetTarget(GameObject instigator, float damage, Health target=null, Vector3 targetPoint=default)
        {
            // Assign the target that's being passed in to our instance variable Target
            this.Target = target; // "this" refers to THIS instance, THIS specific calling
            this.targetPoint = targetPoint;
            // So we can say this THIS instance's Target variable is equal to the target we're passing in, setting our target
            this.Damage = damage;
            this.Instigator = instigator;

            Destroy(gameObject, MaxLifeTime); // Destroy(THIS game object, after the set time we pass in)
        }

        // Aim at the center of mass of a target, not their feet
        private Vector3 GetAimLocation()
        {
            if (Target == null)
            {
                return targetPoint;
            }

            CapsuleCollider TargetCapsule = Target.GetComponent<CapsuleCollider>();
            if (TargetCapsule == null)
            {
                return Target.transform.position;
            }
            return Target.transform.position + Vector3.up * TargetCapsule.height / 2;
        }

        private void OnTriggerEnter(Collider other) 
        {
            Health health = other.GetComponent<Health>();
            if (Target != null && health != Target) return;
            // Our Target is of type Health, so when we're looking at what we collided with we can get the Health component and compare it to Target...
            // and check to see if they're the same. If they're not, return

            if(Target != null &&  Target.GetIsDead()) return; // If the target is dead, don't do damage and don't destroy the projectile

            if (health == null || health.GetIsDead()) return;

            if (other.gameObject == Instigator) return; // If we're colliding with ourselves

            health.TakeDamage(Instigator, Damage); // Apply damage to the target, which we set above in SetTarget

            ProjectileSpeed = 0; // This is to make sure we don't have penetration of our projectiles when they hit us
            // If you WANT penetration, set this to an if check centered around a Serialized bool, like with IsHoming

            onHit.Invoke();

            if(HitEffect != null)
            {
                // If we have a hit effect, create one on our hit event
                Instantiate(HitEffect, GetAimLocation(), transform.rotation);
            }

            foreach (GameObject toDestroy in DestroyOnHit)
            {
                // destroy certain parts of our gameObject first
                Destroy(toDestroy);
            }

            Destroy(gameObject, LifeAfterImpact); // REMEMBER, have to pass in gameObject, not THIS
        }
    }
}