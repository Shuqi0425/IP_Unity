// ==========================================
// Title:       PlayerNearDetector.cs
// Description: NPC detect player near and trigger animation
// Author:      Sun Shuqi (10274096K)
// Date:        12 / August
// ==========================================

using UnityEngine;

public class NPCPlayerNearDetector : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("PlayerNear", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            animator.SetBool("PlayerNear", false);
        }
    }
}