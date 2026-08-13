// ==========================================
// Title:       InteractableObject.cs
// Description: Base class for all interactable objects in the game.
//              Supports multi-line dialogue with branching choices.
// Author:      Sun Shuqi (10274096K)
// Date:        31 / July (edited on 13 August)
// ==========================================

using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("UI notification setting")]
    [Tooltip("Short notice when raycasting point on")]
    public string promptMessage = "Press E to Check";

    [Header("Detail info")]
    [Tooltip("对话内容，支持纯文本和二选一/多选一分支")]
    public DialogueLine[] dialogueLines = new DialogueLine[]
    {
        new DialogueLine { text = "Content", hasChoices = false }
    };

    [Header("Quest Settings")]
    [Tooltip("Enable this if interacting with this object completes a quest.")]
    public bool isQuestInteraction = false;

    [Tooltip("Which quest does this object complete?")]
    public int questNumber = 1;

    [Tooltip("是否在对话全部结束后才完成任务（取消勾选则一开始互动就完成）")]
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

        // 如果任务不需要等对话说完，就直接在开场时完成
        if (isQuestInteraction && !completeQuestOnDialogueEnd)
        {
            TryCompleteQuest();
        }
    }

    /// <summary>
    /// 供PlayerInteraction在整段对话结束时调用
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
