using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using RPG.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RPG.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        LazyValue<SavingWrapper> savingWrapper; // Need to use LazyValue to ensure it gets spawned before the PersistentObjectSpawner's Awake method
        // Otherwise no way to be sure that it'll be there first
        Button button;
        [SerializeField] TMP_InputField saveGameName;

        private void Awake() 
        {
            savingWrapper = new LazyValue<SavingWrapper>(GetSavingWrapper);
        }

        private SavingWrapper GetSavingWrapper()
        {
            return FindObjectOfType<SavingWrapper>(); // Should only be one in the scene
        }

        public void Continue()
        {
            savingWrapper.value.ContinueGame();
        }

        public void NewGame()
        {
            savingWrapper.value.NewGame(saveGameName.text);
        }

        public void QuitGame()
        {
            // Will only work in built games
            Application.Quit();
        }
    }
}
