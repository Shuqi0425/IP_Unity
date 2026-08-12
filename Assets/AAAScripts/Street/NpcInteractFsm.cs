// ==========================================
// Title:       NPCInteractFsm.cs
// Description: Plays an interaction animation when player presses E on this NPC
// ==========================================
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCInteractFsm : MonoBehaviour
{
    [Header("Animation settings")]
    [SerializeField] private string triggerName = "Interact"; 

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // called from InteractableNPC.OnInteract
    public void PlayInteractAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}