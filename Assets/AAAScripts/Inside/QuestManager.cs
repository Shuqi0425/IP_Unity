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
            currentQuest++;
            Debug.Log("Quest completed! Next Quest: " + currentQuest);
        }
        else
        {
            allQuestsCompleted = true;
            Debug.Log("All 5 quests completed!");
        }
    }
}