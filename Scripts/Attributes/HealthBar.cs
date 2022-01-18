using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health healthComponent = null;
        [SerializeField] RectTransform Foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        private void Update() 
        {
            if (Mathf.Approximately(healthComponent.GetFraction(), 0)
            || Mathf.Approximately(healthComponent.GetFraction(), 1))
            {
                // If our fraction is approximately equal to 0 or 1, don't show the health bar
                // Basically if the bar is full or empty, don't show it
                rootCanvas.enabled = false;
                return;
            }

            rootCanvas.enabled = true;
            Foreground.localScale = new Vector3(healthComponent.GetFraction(), 1, 1); // x, y, z
        }
    }
}