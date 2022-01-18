using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    // Want this clase to be a lower level dependency, at the core of our gameplay so that's why we're defining it in Core
    public class ActionScheduler : MonoBehaviour
    {
        IAction CurrentAction; // Keep a reference to the current ongoing action

        public void StartAction(IAction Action)
        {
            // Because we inherit from Monobehavior in both Mover and Fighter, we can take Monobehavior as a parameter and then pass Mover or Fighter TO the Monobehavior...
            // ... and have Mover or Fighter work as a Monobehavior, but still do their functions(?)
            if (CurrentAction == Action) return; // If the CurrentAction is already equal to the Action we're passing in we don't need to set it at all
            if (CurrentAction != null)
            {
                CurrentAction.Cancel(); // Call the CURRENTLY RUNNING ACTION, AKA the currently running component/script's version of Cancel
                // Call that components implementation of Cancel
            }
            CurrentAction = Action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    }

}