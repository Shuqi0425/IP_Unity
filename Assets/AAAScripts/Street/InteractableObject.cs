// ==========================================
// Title:       InteractableObject.cs
// Description: Base class for all interactable objects in the game. Supports multi-line dialogue with branching choices.
// Author:      Sun Shuqi (10274096K)
// Date:        13 August
// ==========================================

using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("UI notification setting")]
    [Tooltip("Short notice when raycasting point on")]
    public string promptMessage = "Press E to Check";

    [Header("Detail info")]
    [Tooltip("Dialogue content, supports plain text and multiple-choice branching")]
    public DialogueLine[] dialogueLines = new DialogueLine[]
    {
        new DialogueLine { text = "Content", hasChoices = false }
    };

    [Header("Quest Settings")]
    [Tooltip("Enable this if interacting with this object completes a quest.")]
    public bool isQuestInteraction = false;

    [Tooltip("Which quest does this object complete?")]
    public int questNumber = 1;

    [Tooltip("Whether to complete the quest only after the dialogue has finished (uncheck to complete immediately upon interaction).")]
    public bool completeQuestOnDialogueEnd = true;

    [Header("Animation")]
    [Tooltip("Assign the NPC's Animator here to trigger a wave on interact.")]
    public Animator npcAnimator;

    [Tooltip("Name of the Animator trigger parameter (must match exactly!).")]
    public string waveTriggerName = "wave";

    /// <summary>
    /// Trigger interact logic
    /// </summary>
    public virtual void OnInteract(PlayerInteraction player)
    {
        // Trigger wave animation if animator is assigned
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger(waveTriggerName);
        }

        // Show detail panel, passing the whole dialogue array + this object as source
        player.ShowDetailPanel(dialogueLines, this);

        // if this interaction is related to a quest and the quest should be completed immediately, try to complete it now
        if (isQuestInteraction && !completeQuestOnDialogueEnd)
        {
            TryCompleteQuest();
        }
    }

    /// <summary>
    /// player calls this method when the dialogue finishes, to check if the quest should be completed
    /// </summary>
    public void OnDialogueFinished()
    {
        if (isQuestInteraction && completeQuestOnDialogueEnd)
        {
            TryCompleteQuest();
        }
    }

    private void TryCompleteQuest()
    {
        if (QuestManager.Instance != null &&
            QuestManager.Instance.currentQuest == questNumber)
        {
            QuestManager.Instance.CompleteQuest();
        }
    }
}
