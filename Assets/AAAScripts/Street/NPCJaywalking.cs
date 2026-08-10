// ==========================================
// Title:       NPCJaywalking.cs
// Description: NPC random Jaywalking
// Author:      Sun Shuqi (10274096K)
// Date:        3 / August
// ==========================================

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCJaywalking : MonoBehaviour
{
    [Header("moving settings")]
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float wanderRadius = 15f;

    [Header("leaving settings")]
    [SerializeField] private Transform leavePoint;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float waitTimer;
    private bool isWaiting;
    private bool isStoppedForInteraction; // if stopped by player interaction
    private bool isLeaving; // if leaving
    private int walkableOnlyMask; // only include Walkable, exclude Crosswalk

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        // Important: The agent is still allowed to walk on all areas (including Crosswalk) while moving, but we exclude Crosswalk when selecting a target point
        int crosswalkBit = 1 << NavMesh.GetAreaFromName("crossWalk");
        walkableOnlyMask = NavMesh.AllAreas & ~crosswalkBit;
        PickNewDestination();
    }

    private void Update()
    {
        // skip random walking logic after interaction of player
        if (isStoppedForInteraction) return;

        if (isLeaving)
        {
            UpdateLeaving();
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) PickNewDestination();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void PickNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        // use NavMesh.SamplePosition to find a valid point on the NavMesh, excluding Crosswalk areas
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, walkableOnlyMask))
        {
            agent.SetDestination(hit.position);
        }
        isWaiting = false;
    }

    // triggered when player press E
    public void StopForInteraction()
    {
        isStoppedForInteraction = true;
        agent.isStopped = true;
        agent.ResetPath(); // stop immediately and clean the path
    }

    // leave after conversation when converssation finished
    public void LeaveAfterDialogue()
    {
        isStoppedForInteraction = false;

        if (leavePoint != null)
        {
            // gone after reach the missiong point
            isLeaving = true;
            agent.isStopped = false;
            agent.SetDestination(leavePoint.position);
        }
        else
        {
            // GONE
            gameObject.SetActive(false);
        }
    }

    private void UpdateLeaving()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            gameObject.SetActive(false); // Gone when reach the leavepoint
        }
    }
}