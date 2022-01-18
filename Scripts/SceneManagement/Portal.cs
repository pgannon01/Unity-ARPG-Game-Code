using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Control;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E, F
        }

        [SerializeField]
        int SceneToLoad = -1; // Set the INDEX of the scene we're going to load
        // Basically, what the index of the next level we want to load is
        // -1 will give us errors until we actually change it in the editor, and set up the scene to have an index
        // Need to remember to go to build settings and put the levels you want in the game in the Scenes In Build box
        [SerializeField]
        Transform SpawnPoint;
        [SerializeField]
        DestinationIdentifier Destination; 
        [SerializeField]
        float FadeOutTime = 0.5f;
        [SerializeField]
        float FadeInTime = 1f;
        [SerializeField]
        float FadeWaitTime = 0.5f;

        private void OnTriggerEnter(Collider other) 
        {
            if(other.gameObject.tag == "Player")
            {
                StartCoroutine(Transition()); // Will run the code up until the first "yield return" call and then let Unity decide when to run again
            }
        }

        // Following Coroutine meant to be able to transfer knowledge between levels
        // For now, just transferring where we were standing when we changed level
        private IEnumerator Transition()
        {
            if (SceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }

            // When running a Coroutine, the Coroutine destroys the top most object it calls
            // So in this case any time load a new level, without the DontDestroyOnLoad function, it would destroy the portal before anything else could happen
            // We may not want that and may want other things to be loaded alongside it, or something else, so we make sure it's not destroyed as soon as it loads...
            // ... and then destroy it at the end of the function
            DontDestroyOnLoad(gameObject);

            // Get our Fader component
            Fader fader = FindObjectOfType<Fader>(); // Find our Fader component

            // Checkpoint SAVE our current level
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();

            // Remove player control
            PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>(); // Always check for no circular dependencies!!!
            playerController.enabled = false;

            // start fade out sequence
            yield return fader.FadeOut(FadeOutTime); // REMEMBER, COROUTINES NEED YIELD RETURN TO RUN
            // If you don't put yield return in front of this method, it WILL NOT RUN

            wrapper.Save();

            // Trigger loading new level
            yield return SceneManager.LoadSceneAsync(SceneToLoad);
            // Remove control, again (A new player is spawned so we have to remove it again to ensure they don't accidentally get control back)
            // REMEMBER! Since this is a NEW Player in a NEW scene, we have to find the Player AGAIN, hence why the new variable here
            PlayerController newPlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            newPlayerController.enabled = false;

            // Checkpoint LOAD our current level
            wrapper.Load();
            
            // Get a hold of the other portal
            Portal OtherPortal = GetOtherPortal();
            UpdatePlayer(OtherPortal);

            wrapper.Save(); // Save once we go through the portal
            // This way when we load a checkpoint save, it won't load us into a portal transition area

            // Wait for our set time faded out to ensure everything's loaded correctly
            yield return new WaitForSeconds(FadeWaitTime);

            // Fade back in to our new level
            yield return fader.FadeIn(FadeInTime);

            // Restore player control
            newPlayerController.enabled = true;

            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject Player = GameObject.FindWithTag("Player");
            Player.GetComponent<NavMeshAgent>().enabled = false;
            Player.GetComponent<NavMeshAgent>().Warp(otherPortal.SpawnPoint.position);
            Player.transform.rotation = otherPortal.SpawnPoint.rotation;
            Player.GetComponent<NavMeshAgent>().enabled = true;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue; // If the portal is the one in our current scene, skip over it
                if (portal.Destination != Destination) continue; // Only going to return the portal if it has the right destination and it's not us

                return portal;
            }
            return null; // Can't find a portal
        }
    }
}