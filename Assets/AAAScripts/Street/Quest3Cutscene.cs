// ==========================================
// Title:       Quest3Cutscene.cs
// Description: Plays a cutscene when Quest 3 is completed:
//              - Ambulance drives down the road via waypoints
//              - People on the floor disappear
//              - Player camera restored
// ==========================================

using System.Collections;
using UnityEngine;
using StarterAssets;

public class Quest3Cutscene : MonoBehaviour
{
    [Header("Ambulance")]
    public Transform ambulance;
    public Transform[] waypoints;
    public float moveSpeed = 8f;
    public float rotateSpeed = 5f;

    [Header("People on Floor")]
    public GameObject[] peopleOnFloor;
    public float peopleDisappearDelay = 3f;

    [Header("Player & UI")]
    public StarterAssetsInputs playerInputs;
    public GameObject playerUI;

    [Header("Optional")]
    public AudioSource ambulanceSiren;

    [Header("Cutscene Camera")]
    public Camera cutsceneCamera;
    public bool trackAmbulance = true;

    private bool cutscenePlayed = false;

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.OnQuestCompleted -= HandleQuestCompleted;
        }
    }

    private void HandleQuestCompleted(int completedQuestNumber)
    {
        if (completedQuestNumber == 3 && !cutscenePlayed)
        {
            cutscenePlayed = true;
            StartCoroutine(PlayCutscene());
        }
    }

    IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(0.5f);

        Camera mainCam = Camera.main;

        // ---- SWITCH TO CUTSCENE CAMERA ----
        if (cutsceneCamera != null && mainCam != null)
        {
            mainCam.gameObject.SetActive(false);
            cutsceneCamera.gameObject.SetActive(true);
        }

        // ---- LOCK PLAYER ----
        if (playerInputs != null)
        {
            playerInputs.cursorInputForLook = false;
            playerInputs.move = Vector2.zero;
            playerInputs.look = Vector2.zero;
            playerInputs.sprint = false;
            playerInputs.jump = false;
        }
        if (playerUI != null) playerUI.SetActive(false);
        if (ambulanceSiren != null) ambulanceSiren.Play();

        // ---- MOVE AMBULANCE ----
        if (ambulance != null && waypoints != null && waypoints.Length > 0)
        {
            foreach (Transform wp in waypoints)
            {
                if (wp == null) continue;

                while (Vector3.Distance(ambulance.position, wp.position) > 0.2f)
                {
                    Vector3 dir = (wp.position - ambulance.position).normalized;
                    ambulance.position += dir * moveSpeed * Time.deltaTime;

                    if (dir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        ambulance.rotation = Quaternion.Slerp(
                            ambulance.rotation, targetRot, rotateSpeed * Time.deltaTime);
                    }

                    if (trackAmbulance && cutsceneCamera != null)
                    {
                        cutsceneCamera.transform.LookAt(ambulance);
                    }

                    yield return null;
                }
            }
        }

        // ---- WAIT A BEAT AT THE END OF THE ROAD ----
        yield return new WaitForSeconds(peopleDisappearDelay);

        // ---- SWITCH CAMERA BACK TO PLAYER FIRST ----
        if (cutsceneCamera != null && mainCam != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(true);
        }

        // ---- DISABLE EVERYTHING OFF-SCREEN ----
        foreach (GameObject person in peopleOnFloor)
        {
            if (person != null) person.SetActive(false);
        }
        if (ambulance != null)
        {
            ambulance.gameObject.SetActive(false);
        }

        // ---- RESTORE PLAYER ----
        if (playerInputs != null) playerInputs.cursorInputForLook = true;
        if (playerUI != null) playerUI.SetActive(true);

        Debug.Log("[Quest3Cutscene] Cutscene finished. Go back to the office.");
    }
}