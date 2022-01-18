using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using RPG.Attributes;
using RPG.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Control
{
    public class Respawner : MonoBehaviour
    {
        [SerializeField] Transform respawnLocation;
        [SerializeField] float respawnDelay = 3;
        [SerializeField] float fadeTime = 0.2f;
        [SerializeField] float healthRegenPercentage = 20;
        [SerializeField] float enemyHealthRegenPercentage = 20;

        private Health health;

        private void Awake() 
        {
            health = GetComponent<Health>();
            health.onDie.AddListener(Respawn);
        }

        private void Start() 
        {
            if (health.GetIsDead())
            {
                Respawn();
            }
        }

        private void Respawn()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>(); 
            savingWrapper.Save(); // Save on player death

            yield return new WaitForSeconds(respawnDelay);

            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeTime);

            RespawnPlayer();
            ResetEnemies();

            yield return fader.FadeIn(fadeTime);
        }

        private void ResetEnemies()
        {
            foreach (AIController enemyControllers in FindObjectsOfType<AIController>())
            {
                Health enemyHealth = enemyControllers.GetComponent<Health>();
                if (enemyHealth && !health.GetIsDead())
                {
                    enemyControllers.Reset();
                    enemyHealth.Heal(enemyHealth.GetMaxHealthPoints() * enemyHealthRegenPercentage / 100);
                }
            }
        }

        private void RespawnPlayer()
        {
            // Calculate position delta
            Vector3 positionDelta = respawnLocation.position - transform.position; 
            // Calculates the difference between where the character is and where our respawn point is
            // This way the camera doesn't jump on respawn

            GetComponent<NavMeshAgent>().Warp(respawnLocation.position);
            health.Heal(health.GetMaxHealthPoints() * healthRegenPercentage / 100);
            ICinemachineCamera activeVirtualCamera = FindObjectOfType<CinemachineBrain>().ActiveVirtualCamera;
            if (activeVirtualCamera.Follow == transform)
            {
                // Warn the camera we're about to respawn so it doesn't jump away
                activeVirtualCamera.OnTargetObjectWarped(transform, positionDelta);
            }
        }
    }
}