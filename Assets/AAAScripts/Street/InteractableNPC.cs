// ==========================================
// Title:       InteractableNPC.cs
// Description: Interactable jaywalking NPC
// Author:      Sun Shuqi (10274096K)
// Date:        3 / August (edited on 13 August)
// ==========================================
using UnityEngine;

public class InteractableNPC : InteractableObject
{
    public override void OnInteract(PlayerInteraction player)
    {
        // Show panel (now takes the DialogueLine[] array + this object as source)
        player.ShowDetailPanel(dialogueLines, this);

        // Stop NPC movement
        GetComponent<NPCJaywalking>()?.StopForInteraction();

        // Play interaction animation
        GetComponent<NPCInteractFsm>()?.PlayInteractAnimation();

        if (isQuestInteraction && !completeQuestOnDialogueEnd)
        {
            TryCompleteQuestNow();
        }
    }

    private void TryCompleteQuestNow()
    {
        if (QuestManager.Instance != null &&
            QuestManager.Instance.currentQuest == questNumber)
        {
            QuestManager.Instance.CompleteQuest();
        }
    }
}