using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine.EventSystems;
using GameDevTV.Inventories;

namespace RPG.Control // Creating a namespace so we can import this into other files later and use the functions in here
// Can set categories, like RPG.Player, RPG.Control, etc, and doing this so it doesn't conflict with already existing namespaces
// One thing to keep in mind is to ensure that you don't have too many dependencies and control the flow of logic and dependencies, so that, if you change something here...
// ... it'll only affect relevent child scripts that inherit from this, rather than potentially everything
{
    public class PlayerController : MonoBehaviour
    {
        Health HealthComponent;
        ActionStore actionStore;

        [System.Serializable]
        struct CursorMapping // Used for setting Unity's Cursor options
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float MaxNavMeshProjectionDistance = 1f;
        [SerializeField] float raycastRadius = 1f;
        [SerializeField] int numberOfAbilities = 6;

        bool isDraggingUI = false;

        private void Awake() 
        {
            HealthComponent = GetComponent<Health>();
            actionStore = GetComponent<ActionStore>();
        }

        private void Update()
        {
            if (InteractWithUI()) return;
            if (HealthComponent.GetIsDead())
            {
                SetCursor(CursorType.None); // Change this to a DEAD cursor in the Enum
                return; // If we're dead, don't do any of the following functions
            } 

            UseAbilities(); // Can potentially use an ability and move so we don't want to return out of this to stop movement

            if (InteractWithComponent()) return;  
            if (InteractWithMovement()) return;

            SetCursor(CursorType.None);
        }

        // As it currently stands, Raycast lists are NOT sorted when they're returned
        // So upon calling a Raycast list, there's not a guaranteed chance that what we'll get is what we hit first, as in what's closest to us
        // This function's purpose is to sort through our Raycast list and return the closest thing to us, which is what we need it to do
        RaycastHit[] RaycastAllSorted()
        {
            // Get all hits
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), raycastRadius);

            // sort by distance
            // build array of distances
            float[] distances = new float[hits.Length]; // creates a brand new array the same length as our RaycastHits array

            // Build up our distances array
            for (int i = 0; i < hits.Length; i++) // need the indices so we're using a FOR loop instead of a FOREACH
            {
                distances[i] = hits[i].distance;
                // hit value has a distance to the raycast 
                // and we'll put that distance into the distances array
                // so the Sort function knows what value it is supposed to be sorting relative to
            }

            // sort the hits
            Array.Sort(distances, hits);

            // return sorted list
            return hits;
        }

        private bool InteractWithUI()
        {
            if (Input.GetMouseButtonUp(0))
            {
                isDraggingUI = false;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isDraggingUI = true;
                }
                SetCursor(CursorType.UI);
                return true;
            }

            if (isDraggingUI)
            {
                return true;
            }
            
            return false;
        }

        private void UseAbilities()
        {
            for (int i = 0; i < numberOfAbilities; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    actionStore.Use(i, gameObject);
                }
            }
            
        }

        private bool InteractWithComponent()
        {
            // Raycast through the world, getting all the raycast hits, like what we did in InteractWithCombat
            // Go through all the hits, get the gameObjects and all the components on those objects that implement IRaycastable
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this)) // See if the raycastable can Handle the raycast in the loop
                    {
                        SetCursor(raycastable.GetCursorType()); // Using Combat by default
                        return true;
                    }
                }
            }
            return false; // There were no Raycastable objects through this particular array, and we don't set the cursor either
        }

        private bool InteractWithMovement() // refactored for future cursor affordance
        {
            // refactored this into a bool function so that, in the future, if we can't walk somewhere we can change the cursor so the player can know they can't go there

            // Ray Ray = GetMouseRay(); // Changed this to an inline function call down below in the HasHit bool
            //RaycastHit Hit;

            // Call a Raycast method
            //bool HasHit = Physics.Raycast(GetMouseRay(), out Hit); // Raycast returns a bool so we'll store it to see if it hits something
            // For the "out" keyword: We're passing in the Parameter Hit but also retrieving OUT and storing in the Hit variable where the raycast hit
            // So basically in our Hit variable it will, from this function, store the position the raycast hits
            // To put it even simpler, we're passing in the empty variable, setting it, and getting it back

            // Changed above code to below code
            // Essentially now we're going to use our RaycastNavMesh function to check and make sure that the player can only move on terrain that has a navmesh baked
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);

            if (hasHit)
            {
                if (!GetComponent<Mover>().CanMoveTo(target)) return false;

                // if (Input.GetMouseButtonDown(0)) // THIS will only return true during the individual FRAME the user clicks, so you can't click and hold to move
                if (Input.GetMouseButton(0)) // 0 is left click, 1 is right click, usually
                {
                    // GetComponent<NavMeshAgent>().destination = Hit.point;
                    // Changing the above for refactoring
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                    // Moved the above two functions to the PlayerController script to have it handle the overarching logic
                    // Basically, Raycasting will be needed to do combat, but previously Raycasting was only handled in the Mover script
                    // So, rather than write multiple pieces of the same code, we moved the Raycasting part, the part we'd need multiple different components to use...
                    // ... into this PlayerController, and kept movement where it was
                    // This will pull(?) movement, and other actions, from other scripts and do the Raycasting that needs to get done here, rather than have multiple scripts all
                    // doing Raycasting on their own

                    // This script will handle the Raycasting, call the relevant functions that need that Raycasting, and pass the Raycast TO those functions...
                    // ... whose scripts can then run using that Raycast we pass in
                }
                SetCursor(CursorType.Movement);
                return true;
            }
            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            // Purpose of this function is to make sure player can't walk anywhere we don't have a nav mesh set up
            // This way they won't be able to walk up, around, and to areas we don't want them to
            // Is going to return a bool and the target Vector3, hence the "out" parameter
            target = new Vector3(); // Setting this in the beginning in case we don't get a hit, because we need it

            // Raycast to terrain
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit) return false;

            // find nearest navmesh point
            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(hit.point, out navMeshHit, MaxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;

            // return true if we find that point
            target = navMeshHit.position;

            // // Below code is to have our NavMesh calculate the distance of the path and disallow us from moving across the map
            // // Purpose is to ensure a safe path, and also one that doesn't send the player through who knows where, possibly aggroing enemies along the way
            // NavMeshPath path = new NavMeshPath(); // Have to do this because otherwise code below, CalculatePath, won't work
            // // Essentially, some reference types, such as NavMeshPath types, can be null, but we NEED it to have a value already stored for it to work
            // bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            // if (!hasPath) return false;
            // if (path.status != NavMeshPathStatus.PathComplete) return false;
            // if (GetPathLength(path) > MaxNavMeshPathLength) return false; // This will be for setting the max length we can click to travel
            // // If you don't want this, don't include this part

            return true;

        }

        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        public static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}


// Old code, keeping for lesson:
// private bool InteractWithCombat()
// {
//     // Below will store a list of all the things that our raycast goes through
//     RaycastHit[] Hits = Physics.RaycastAll(GetMouseRay());
//     foreach (RaycastHit Hit in Hits)
//     {
//         // Check to see if the object the Raycast that's cast from our mouse click hits an enemy
//         // Everything inside that Raycast from our mouse will be stored in Hits, so we're checking each element of that array to see if an enemy is in there...
//         // can use transform or collider to get this, then check to see if it's the correct component, our enemy target
//         // CombatTarget Target = Hit.transform.GetComponent<CombatTarget>();
//         // if (Target == null) continue;

//         // if (!GetComponent<Fighter>().CanAttack(Target.gameObject)) continue; // continue basically just skips the rest of the body of this loop and move onto the next item 
//         // in the foreach loop so the foreach won't carry on with the rest of this code
//         // Basically checking if the item in the Hit array is a CombatTarget, if it is, continue the rest of this code, if not continue through the loop
//         // Also checking to make sure the target is alive, so we're making sure we're clicking on an actual target AND they're alive in order to attack them

//         // if (Input.GetMouseButton(0))
//         // {
//         //     // This will make it so that it will only run our attack function when we left click
//         //     // If we click, get our Fighter component and call the attack function
//         //     GetComponent<Fighter>().Attack(Target.gameObject);
//         // }

//         // return true; // Setting this here so even if we are hovering over an enemy, we don't want to do any movement
//         // later on down the line we want to provide some cursor affordance. Basically, wanna change the cursor from a regular movement cursor to a combat one
//         // From a gauntlet to a sword, essentially
//     }
//     return false; // Didn't find any combat targets to attack
// }
