using UnityEngine;

public class QuestObjectSpawner : MonoBehaviour
{
    public GameObject questObject1;
    public GameObject questObject2;
    public GameObject questObject3;
    public GameObject questObject4;
    public GameObject questObject5;

    private void Start()
    {
        // hide all quest objects at the start
        questObject1.SetActive(false);
        questObject2.SetActive(false);
        questObject3.SetActive(false);
        questObject4.SetActive(false);
        questObject5.SetActive(false);

        // show the quest object based on the current quest
        switch (QuestManager.Instance.currentQuest)
        {
            case 1:
                questObject1.SetActive(true);
                break;

            case 2:
                questObject2.SetActive(true);
                break;

            case 3:
                questObject3.SetActive(true);
                break;

            case 4:
                questObject4.SetActive(true);
                break;

            case 5:
                questObject5.SetActive(true);
                break;
        }
    }
}