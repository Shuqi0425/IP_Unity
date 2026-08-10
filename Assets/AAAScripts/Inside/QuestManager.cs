// ==========================================
// Title:       QuestManager.cs
// Description: Manages quest progress across scenes.
// Date:        9 / August
// ==========================================

using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Progress")]
    [Tooltip("Current quest number: 1 - 5")]
    public int currentQuest = 1;

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
            Debug.Log("All 5 quests completed!");
        }
    }
}