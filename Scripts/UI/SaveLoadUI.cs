using System.Collections;
using System.Collections.Generic;
using RPG.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class SaveLoadUI : MonoBehaviour
    {
        [SerializeField] Transform contentRoot;
        [SerializeField] GameObject saveButtonPrefab;

        private void OnEnable() // Triggered whenever the GameObject is reactivated
        {
            SavingWrapper savingWrapper = FindObjectOfType<SavingWrapper>();
            if (savingWrapper == null) return;

            foreach (Transform child in contentRoot)
            {
                Destroy(child.gameObject);
            }

            
            foreach (string save in savingWrapper.ListSaves())
            {
                GameObject buttonInstance = Instantiate(saveButtonPrefab, contentRoot);
                TMP_Text textComponent = buttonInstance.GetComponentInChildren<TMP_Text>();
                textComponent.text = save;
                Button button = buttonInstance.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => 
                {
                    savingWrapper.LoadGame(save);
                });
            }
        }
    }
}
