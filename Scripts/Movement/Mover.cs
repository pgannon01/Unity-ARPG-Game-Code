using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RPG.Attributes;
using GameDevTV.Saving;
using RPG.Core;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable // Can't inherit from more than one class but CAN inherit from as many interfaces as you like
    {
        [SerializeField] 
        Transform target;

        [SerializeField] 
        float MaxSpeed = 6f;

        [SerializeField] float MaxNavMeshPathLength = 40f;


        NavMeshAgent NavMeshAgent;
        Health HealthComponent;

        private void Awake() 
        {
            HealthComponent = GetComponent<Health>();
            NavMeshAgent = GetComponent<NavMeshAgent>();    
        }

        // Ray LastRay; // The last ray that we shoot at the screen for raycasting

        // Update is called once per frame
        void Update()
        {
            // Commented out because this was debug stuff to teach about how raycasts worked and to show how to click on the screen and get a direction
            // by drawing a Ray to where we clicked on the terrain from the camera
            // Debug.DrawRay(LastRay.origin, LastRay.direction * 100); // Draw a line from the camera to where we clicked
            // LastRay = Camera.main.ScreenPointToRay(Input.mousePosition); // Get the camera, named main, and get the Ray we sent from our mouse

            NavMeshAgent.enabled = !HealthComponent.GetIsDead(); // If something dies, remove it's navmeshagent

            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 Destination, float SpeedFraction)
        {
            // This function is meant for allowing us to click away while in combat and leave combat so we don't keep chasing the enemy
            // This will need to be refactored later!
            // Below is now a circular dependency, this script depends on fighter and fighter depends on this, DON'T DO THIS!
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(Destination, SpeedFraction);
        }

        public bool CanMoveTo(Vector3 destination)
        {
            // Below code is to have our NavMesh calculate the distance of the path and disallow us from moving across the map
            // Purpose is to ensure a safe path, and also one that doesn't send the player through who knows where, possibly aggroing enemies along the way
            NavMeshPath path = new NavMeshPath(); // Have to do this because otherwise code below, CalculatePath, won't work
            // Essentially, some reference types, such as NavMeshPath types, can be null, but we NEED it to have a value already stored for it to work
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if (!hasPath) return false;
            if (path.status != NavMeshPathStatus.PathComplete) return false;
            if (GetPathLength(path) > MaxNavMeshPathLength) return false; // This will be for setting the max length we can click to travel
            // If you don't want this, don't include this part

            return true;
        }

        public void MoveTo(Vector3 Destination, float SpeedFraction)
        {
            NavMeshAgent.destination = Destination;
            NavMeshAgent.speed = MaxSpeed * Mathf.Clamp01(SpeedFraction); // Mathf.Clamp01 makes it so that whatever value is passed in HAS to be between 0 and 1, not higher or lower
            NavMeshAgent.isStopped = false;
        }

        public void Cancel()
        {
            // Since this is an interface function, we need to ensure it has the same return type, takes the same parameters, etc. of the defined interface
            // This Cancel should stop attacking
            NavMeshAgent.isStopped = true;
        }

        private void UpdateAnimator()
        {
            Vector3 Velocity = NavMeshAgent.velocity; // Get the vector, the speed and the direction we're attempting to travel, from the NavMeshAgent
            Vector3 LocalVelocity = transform.InverseTransformDirection(Velocity); // transforms a direction from world space to local space
            float Speed = LocalVelocity.z; // All we need to know is the Z speed to get our forward direction, we're not travelling to the side or backwards
            GetComponent<Animator>().SetFloat("ForwardSpeed", Speed); // This will allow us to actually set the speed for our animator

            // Why convert from global(world space) to local space?
            // When we're creating our velocity, that we're storing in Velocity, we're grabbing the global velocity, which may change at a particular rate to show us where we...
            // are in the world, which isn't useful for our animator, our animator just needs to now whether we're moving or not and at what speed 
            // So InverseTransformDirection converts that to essentially just tell us that we're moving in a direction and the units we're moving at 
            // This way we can make it meaningful for our animator, and useful, can't use global values from a local point of view to tell the animator what we need to do
        }

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0; // What we'll return at the end, setting it in the beginning

            if (path.corners.Length < 2) return total; // path has less than 2 corners, meaning we can't calculate its distance so we have to return it

            for (int i = 0; i < path.corners.Length - 1; i++) // Have to do Length - 1 to ensure we don't overshoot our distance, hence our previous if check
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            // print(total); // Debug statement here if you need it

            return total;
        }

        // Below is another way to save large amounts of data instead of a dictionary
        // Going to use this and comment out the dictionary method of doing it
        [System.Serializable] // This makes sure that all the public fields in the struct are serializable
        struct MoverSaveData
        {
            public SerializableVector3 position;
            public SerializableVector3 rotation;
        }

        public object CaptureState()
        {
            // DICTIONARY METHOD BELOW
            // Instead of just returning ONE piece of data, we can instead return a dictionary full of objects, so we can store things like the position and rotation, below
            // Or in the future, xp, money, weapons, etc.
            // Dictionary<string, object> data = new Dictionary<string, object>();
            // data["position"] = new SerializableVector3(transform.position); // Set the character's position to the key "position"
            // data["rotation"] = new SerializableVector3(transform.eulerAngles); // Set the character's rotation to the key "rotation"
            // return data; // Need to return this specifically to get it to work
            // Also for the loading, we'll need to cast this and for that we'd need the return type specifically

            // Generally two ways to cast an object, both of which I'll make examples of below (we're going to use one of them)
            // Examples are based around what we're doing in this method, so they'll be relevant, and should both work
            // SerializableVector3 x = (SerializableVector3)state;
            // If the state we pass into our X variable is NOT a SerializableVector3, this will thrown an Exception error

            // SerializableVector3 x = state as SerializableVector3;
            // If the state we pass int our X variable is NOT a SerializableVector3, this will return null

            // STRUCT METHOD BELOW
            MoverSaveData data = new MoverSaveData();
            data.position = new SerializableVector3(transform.position);
            data.rotation = new SerializableVector3(transform.eulerAngles);
            return data;
        }

        public void RestoreState(object state)
        {
            // DICTIONARY METHOD BELOW
            // Dictionary<string, object> data = (Dictionary<string, object>)state; // Casting our state AS a SerializableVector3 to the variable Position
            // GetComponent<NavMeshAgent>().enabled = false;
            // transform.position = ((SerializableVector3)data["position"]).ToVector(); // when we load, set our position back to where we were when we saved
            // transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();
            // GetComponent<NavMeshAgent>().enabled = true;
            // Have to disable and reenable our NavMeshAgent when we're setting our position because otherwise it could cause problems and put us on an offset from where...
            // ... we saved, causing potentially big issues

            // STRUCT METHOD BELOW
            MoverSaveData data = (MoverSaveData)state; // Casting our state AS a SerializableVector3 to the variable Position
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = data.position.ToVector();
            transform.eulerAngles = data.rotation.ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
    }
}
