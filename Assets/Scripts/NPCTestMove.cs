// ==========================================
// Title:       NPCPatrolPath.cs
// Description: NPC walks back and forth along a set of waypoints (e.g. an L-shaped road).
// Author:      Sun Shuqi (10274096K)
// Date:        1 / August
// ==========================================

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCPatrolPath : MonoBehaviour
{
    [Header("pathway point setings")]
    [SerializeField] private Transform[] waypoints;

    [Header("arrive threshold")]
    [SerializeField] private float arriveThreshold = 1.0f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private bool movingForward = true;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("[NPCPatrolPath] at least 2 waypoints are required!");
            return;
        }

        agent.stoppingDistance = 0f;
        GoToCurrentWaypoint();
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        float distanceToTarget = Vector3.Distance(transform.position, waypoints[currentIndex].position);

        if (!agent.pathPending && distanceToTarget <= arriveThreshold)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void GoToCurrentWaypoint()
    {
        agent.SetDestination(waypoints[currentIndex].position);
    }

    private void AdvanceToNextWaypoint()
    {
        if (movingForward)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                currentIndex = waypoints.Length - 2;
                movingForward = false;
            }
        }
        else
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = 1;
                movingForward = true;
            }
        }

        GoToCurrentWaypoint();
    }
}