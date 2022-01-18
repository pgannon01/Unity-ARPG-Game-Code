using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class Fader : MonoBehaviour
    {
        CanvasGroup canvasGroup;
        Coroutine currentlyActiveFade = null;

        private void Awake() 
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void FadeOutImmediate()
        {
            canvasGroup.alpha = 1;
        }

        public Coroutine FadeOut(float time) // Making these public for future use, not sure why
        {
            // As it is, with how the coroutines work, we can encounter a bug where if we fade out, then try to fade back in, one of the two will never happen
            // That's cause we're trying to call another coroutine while one is already being called
            // So, we have to refactor things a bit in here

            return Fade(1, time);
        }

        public Coroutine FadeIn(float time)
        {
            return Fade(0, time);

            // Commenting out for refactor, but keeping for lessons
            // while (canvasGroup.alpha > 0)
            // {
            //     canvasGroup.alpha -= Time.deltaTime / time;

            //     yield return null;
            // }
        }

        public Coroutine Fade(float target, float time)
        {
            // Cancel running coroutines
            if (currentlyActiveFade != null)
            {
                StopCoroutine(currentlyActiveFade);
            }

            // Run fadeout coroutine (Replace the previously cancelled currentlyActiveFade with THIS coroutine)
            currentlyActiveFade = StartCoroutine(FadeRoutine(target, time));
            return currentlyActiveFade;
        }

        private IEnumerator FadeRoutine(float target, float time)
        {
            // Move this to here, from FadeOut, for refactoring, as was mentioned in FadeOut
            while (!Mathf.Approximately(canvasGroup.alpha, target)) // while alpha is not 1, update it to be one
            {
                // moving alpha towards 1
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.unscaledDeltaTime / time); // Increase the alpha a bit every frame for however long we set it
                // unscaledDeltaTime isn't affected by pause so we can fade out when paused
                // So using deltaTime and dividing it by the length of time we want to set for it to fade, it will increase the alpha by that amount until it reaches 1...
                // ... at the time we pass in
                // Basically, whatever time we pass in is how long it will take for the alpha to reach 1
                yield return null;
            }
            // while (canvasGroup.alpha < 1) // while alpha is not 1, update it to be one
            // Commented out for refactoring, but keeping for lesson
        }
    }
}
