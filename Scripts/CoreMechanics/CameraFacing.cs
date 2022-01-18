using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class CameraFacing : MonoBehaviour
    {
        // Purpose of this script is to keep our text above our players heads facing the camera

        private void LateUpdate() 
        {
            // Rotate the orientation of the text every frame
            transform.forward = Camera.main.transform.forward;
        }

    }
}
