using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.UI.DamageText
{
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText damageTextPrefab = null;

        public void Spawn(float damageAmount)
        {
            DamageText instance = Instantiate<DamageText>(damageTextPrefab, transform); // Instantiate a DamageText component
            // (Have it be from what we set in the serialized field, apply it to THIS transform)
            instance.SetValue(damageAmount);
        }
    }
}
