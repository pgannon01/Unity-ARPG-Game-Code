using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Control
{
    public class PatrolPath : MonoBehaviour
    {
        const float WaypointGizmoRadius = 0.3f;

        private void OnDrawGizmos() // Draw the gizmos for the patrol path
        {
            for (int i = 0; i < transform.childCount; i++) // This foor loop will go from 0 to the amount of children this GameObject has, -1
            {
                int j = GetNextIndex(i);
                Gizmos.DrawSphere(GetWaypoint(i), WaypointGizmoRadius);
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }
        }

        public int GetNextIndex(int i)
        {
            if (i + 1 >= transform.childCount)
            {
                return 0;
            }
            return i + 1;
        }

        public Vector3 GetWaypoint(int i)
        {
            return transform.GetChild(i).position;
        }
    }
}
