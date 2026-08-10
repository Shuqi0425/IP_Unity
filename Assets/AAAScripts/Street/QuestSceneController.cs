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

        // Hide all quest targets
        foreach (GameObject target in questTargets)
        {
            if (target != null)
            {
                target.SetActive(false);
            }
        }

        // Current Quest 1 = Array index 0
        // Current Quest 2 = Array index 1
        int index = QuestManager.Instance.currentQuest - 1;

        if (index >= 0 && index < questTargets.Length)
        {
            questTargets[index].SetActive(true);

            Debug.Log(
                "Current Quest: " +
                QuestManager.Instance.currentQuest
            );
        }
    }
}