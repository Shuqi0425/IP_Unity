// ==========================================
// Title:       QuestManager.cs
// Description: Manages quest progress across scenes.
// Date:        9 / August (edited on 14 August)
// ==========================================
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Progress")]
    [Tooltip("Current quest number: 1 - 5")]
    public int currentQuest = 1;

    [Tooltip("Indicates if all five quests are completed")]
    public bool allQuestsCompleted = false;

    // Event fired when ANY quest is completed. Passes the quest number that was just finished.
    public static event System.Action<int> OnQuestCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CompleteQuest()
    {
        if (currentQuest < 5)
        {
            int completedQuest = currentQuest;
            currentQuest++;
            Debug.Log("Quest completed! Next Quest: " + currentQuest);

            // inform other scripts that a quest has been completed
            OnQuestCompleted?.Invoke(completedQuest);
        }
        else
        {
            allQuestsCompleted = true;
            Debug.Log("All 5 quests completed!");

            // finish the last quest and inform other scripts
            OnQuestCompleted?.Invoke(5);
        }
    }
}