using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core // Naming this Core, after the folder it's in, CoreMechanics
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target; // type Transform because we need to know the position, the transform, of the camera attached to the player


        // Update is called once per frame
        void LateUpdate()
        {
            // Setting camera to LateUpdate instead of Update to ensure the player is moving BEFORE the camera trys to follow them
            transform.position = target.position; // assigning it to follow the player, player is transform.position, camera is target.position
        }
    }

}