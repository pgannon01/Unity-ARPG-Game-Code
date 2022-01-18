using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class PersistentObjectsSpawner : MonoBehaviour
    {
        [SerializeField]
        GameObject PersistentObjectPrefab;

        static bool HasSpawned = false; // A normal variable lives and dies with the class
        // But static classes stays with the application, rather than the instance of the class
        // So declaring this a static variable means it will persist throughout the game and always be able to tell if the Persistent Object Prefab has spawned or not
        // Generally this is a BAD idea, but have no other real option with unity

        private void Awake() 
        {
            // Instantiate the PersistentObject Prefab
            // How do we know if this is the first scene and haven't already created it in a previous scene?
            if (HasSpawned) return;

            SpawnPersistentObjects();
            
            HasSpawned = true;
        }

        private void SpawnPersistentObjects()
        {
            // Create the prefab and set it to don't destroy on load
            GameObject PersistenObject = Instantiate(PersistentObjectPrefab);
            DontDestroyOnLoad(PersistenObject);
        }
    }
}
