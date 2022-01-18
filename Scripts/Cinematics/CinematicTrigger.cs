using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPG.Cinematics
{
    public class CinematicTrigger : MonoBehaviour
    {
        bool IsTriggered = false;

        private void OnTriggerEnter(Collider other) 
        {
            if(!IsTriggered && other.gameObject.tag == "Player")
            {
                GetComponent<PlayableDirector>().Play();
                IsTriggered = true;
            }
        }
    }
}