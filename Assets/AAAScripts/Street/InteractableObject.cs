// ==========================================
// Title:       Interactable.cs
// Description: Base class for all interactable objects in the game.
// Author:      Sun Shuqi (10274096K)
// Date:        31 / July (edited on 10 August)
// ==========================================

using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("UI notification setting")]
    [Tooltip("Short notice when raycasting point on")]
    public string promptMessage = "Press E to Check";

    [Header("Detail info")]
    [TextArea(3, 5)]
    public string detailText = "Content";

    [Header("Quest Settings")]
    [Tooltip("Enable this if interacting with this object completes a quest.")]
    public bool isQuestInteraction = false;

    [Tooltip("Which quest does this object complete?")]
    public int questNumber = 1;

    /// <summary>
    /// Trigger interact logic
    /// </summary>
    public virtual void OnInteract(PlayerInteraction player)
    {
        // Show detail panel
        player.ShowDetailPanel(detailText);

        // Complete quest if this is the current quest target
        if (isQuestInteraction &&
            QuestManager.Instance != null &&
            QuestManager.Instance.currentQuest == questNumber)
        {
            QuestManager.Instance.CompleteQuest();
        }
    }
}