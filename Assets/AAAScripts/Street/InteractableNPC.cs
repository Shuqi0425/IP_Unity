// ==========================================
// Title:       InteractableNPC.cs
// Description: Interactable jaywalking NPC
// Author:      Sun Shuqi (10274096K)
// Date:        3 / August (edited on 10 August)
// ==========================================

using UnityEngine;

public class InteractableNPC : InteractableObject
{
    public override void OnInteract(PlayerInteraction player)
    {
        // Show panel
        player.ShowDetailPanel(detailText, gameObject);

        // Stop NPC movement
        GetComponent<NPCJaywalking>()?.StopForInteraction();

        // Complete quest if this is the current quest target
        if (isQuestInteraction &&
            QuestManager.Instance != null &&
            QuestManager.Instance.currentQuest == questNumber)
        {
            QuestManager.Instance.CompleteQuest();
        }
    }
}