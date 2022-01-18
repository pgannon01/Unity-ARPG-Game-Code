using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDevTV.Saving;
using UnityEngine.SceneManagement;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        // At the moment, only going to use one save file, so we'll just make it a constant here that we can load
        // From here we'll both save to and load from this default save game name
        private const string currentSaveKey = "currentSaveName";
        [SerializeField] float FadeInTime = 0.2f;
        [SerializeField] float fadeOutTime = 0.3f;
        [SerializeField] int firstLevelBuildIndex = 1;
        [SerializeField] int menuLevelBuild = 0;

        public void ContinueGame() 
        {
            if (!PlayerPrefs.HasKey(currentSaveKey)) return; // Deals with a case where we never created a new game
            if (!GetComponent<SavingSystem>().SaveFileExists(GetCurrentSave())) return; // If we didn't save a game, I think?
            StartCoroutine(LoadLastScene());
            // This is to avoid a bug where some of our Start() methods would happen before our loading is finished (For Awake which used to be here)
        }

        public void NewGame(string saveFileName)
        {
            if (String.IsNullOrEmpty(saveFileName)) return;
            SetCurrentSave(saveFileName);
            StartCoroutine(LoadFirstScene());
        }

        public void LoadGame(string saveFile)
        {
            SetCurrentSave(saveFile);
            ContinueGame();
        }

        public void LoadMenu()
        {
            StartCoroutine(LoadMenuScene());
        }

        private void SetCurrentSave(string saveFileName)
        {
            PlayerPrefs.SetString(currentSaveKey, saveFileName);
        }

        private string GetCurrentSave()
        {
            return PlayerPrefs.GetString(currentSaveKey);
        }

        private IEnumerator LoadFirstScene()
        {
            // Start is able to return IEnumerators, and can become it's own Coroutine
            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime); // When we load into the game, we want to load in via a blackscreen

            // On start of the game it will immediately try to load up the last saved scene
            yield return SceneManager.LoadSceneAsync(firstLevelBuildIndex);

            yield return fader.FadeIn(FadeInTime);
        }

        private IEnumerator LoadLastScene() 
        {
            // Start is able to return IEnumerators, and can become it's own Coroutine
            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime); // When we load into the game, we want to load in via a blackscreen

            // On start of the game it will immediately try to load up the last saved scene
            yield return GetComponent<SavingSystem>().LoadLastScene(GetCurrentSave());

            yield return fader.FadeIn(FadeInTime);
        }

        private IEnumerator LoadMenuScene()
        {
            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut(fadeOutTime);
            yield return SceneManager.LoadSceneAsync(menuLevelBuild);
            yield return fader.FadeIn(FadeInTime);
        }

        private void Update() 
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(GetCurrentSave());
        }

        public void Load()
        {
            // Call to saving system API and tell it to load
            GetComponent<SavingSystem>().Load(GetCurrentSave()); // LoadLastScene is a Coroutine so we need to call StartCoroutine
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(GetCurrentSave());
        }

        public IEnumerable<string> ListSaves()
        {
            return GetComponent<SavingSystem>().ListSaves();
        }
    }
}
