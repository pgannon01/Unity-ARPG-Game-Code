using System.Collections;
using System.Collections.Generic;
using RPG.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.UI
{
    public class TraitRowUI : MonoBehaviour
    {
        [SerializeField] Trait trait;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] Button minusButton;
        [SerializeField] Button plusButton;

        TraitStore playerTraitStore = null;

        private void Start() 
        {
            playerTraitStore = GameObject.FindGameObjectWithTag("Player").GetComponent<TraitStore>();
            minusButton.onClick.AddListener(() => AllocatePoints(-1));
            plusButton.onClick.AddListener(() => AllocatePoints(+1));
        }

        private void Update() 
        {
            minusButton.interactable = playerTraitStore.CanAssignPoints(trait, -1); // Set it to be interactable when the value is greater than 0
            plusButton.interactable = playerTraitStore.CanAssignPoints(trait, +1);

            valueText.text = playerTraitStore.GetProposedPoints(trait).ToString();    
        }

        public void AllocatePoints(int points)
        {
            playerTraitStore.AssignPoints(trait, points);
        }

    }
}
