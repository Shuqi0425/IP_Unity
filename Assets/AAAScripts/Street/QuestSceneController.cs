// ==========================================
// Title:       QuestSceneController.cs
// Description: control the quest scene, show the current quest target based on QuestManager's currentQuest
// Author:      Sun Shuqi (10274096K)
// Date:        13 August
// ==========================================

using UnityEngine;

public class QuestSceneController : MonoBehaviour
{
    [Header("Quest Targets")]
    [Tooltip("Quest targets from Quest 1 to Quest 5")]
    public GameObject[] questTargets;

    private void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager not found!");
            return;
        }

        RefreshQuestTargets();
    }

    /// <summary>
    /// Call this after a quest is completed to update which target is visible.
    /// </summary>
    public void RefreshQuestTargets()
    {
        // Hide all quest targets
        foreach (GameObject target in questTargets)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        // Current Quest 1 = Array index 0, Quest 4 = index 3
        int index = QuestManager.Instance.currentQuest - 1;

        if (index >= 0 && index < questTargets.Length)
        {
            questTargets[index].SetActive(true);
            Debug.Log("Current Quest: " + QuestManager.Instance.currentQuest);
        }
    }
}