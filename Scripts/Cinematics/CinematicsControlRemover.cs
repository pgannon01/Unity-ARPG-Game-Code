using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using RPG.Core;
using RPG.Control;

namespace RPG.Cinematics
{
    public class CinematicsControlRemover : MonoBehaviour
    {
        GameObject Player;

        private void Awake() 
        {
            Player = GameObject.FindWithTag("Player");
        }

        private void OnEnable() 
        {
            GetComponent<PlayableDirector>().played += DisableControl;
            GetComponent<PlayableDirector>().stopped += EnableControl;
        }

        private void OnDisable() 
        {
            GetComponent<PlayableDirector>().played -= DisableControl;
            GetComponent<PlayableDirector>().stopped -= EnableControl;    
        }

        void DisableControl(PlayableDirector pd)
        {
            Player.GetComponent<ActionScheduler>().CancelCurrentAction();
            Player.GetComponent<PlayerController>().enabled = false; // Disable player input
        }

        void EnableControl(PlayableDirector pd)
        {
            Player.GetComponent<PlayerController>().enabled = true; // Re-enable player input
        }
    }

}