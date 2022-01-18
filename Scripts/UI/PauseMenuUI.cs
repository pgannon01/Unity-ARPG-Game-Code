using System.Collections;
using System.Collections.Generic;
using RPG.Control;
using RPG.SceneManagement;
using UnityEngine;

namespace RPG.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        PlayerController player;

        private void Awake() // Don't use Start here, use Awake
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void OnEnable() 
        {
            if (player ==  null) return;
            // Using OnEnable and OnDisable to be able to know when the Menu is opened so we can pause the game and display the contents
            // rather than having to do something complicated

            // Pause the game
            Time.timeScale = 0; // timeScale = how quickly events happen to real time
            // By setting it to 0 we say they're not happening at all, they're paused
            // This automatically will update anything using deltaTime for us

            player.enabled = false; // Forbid the player from doing anything on screen while paused
        }

        private void OnDisable() 
        {
            if (player == null) return;

            Time.timeScale = 1; // Set timeScale back to normal

            player.enabled = true; // Re enable player control of the game
        }

        public void Save()
        {
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.Save();
        }

        public void SaveAndQuit()
        {
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            savingWrapper.Save();

            savingWrapper.LoadMenu();

            // Below is one option for handling quitting the game
            // Basically it says that if we're in the editor, when we hit quit in the pause
            // we'll just go back to the main menu scene
            // However, if we hit Quit during a fully built game, we'll fully quit the game
            // Need to decide what is better, probably add in a fourth option of "Quit to Menu"
        // #if UNITY_EDITOR
        //     savingWrapper.LoadMenu();
        // #else
        //     Application.Quit();
        // #endif

        }
    }
}
