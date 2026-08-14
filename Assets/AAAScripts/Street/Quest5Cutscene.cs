// ==========================================
// Title:       Quest5Cutscene.cs
// Description: Plays a cutscene when Quest 5 is completed.
//              Camera switch + optional object movement + cleanup.
// ==========================================

using System.Collections;
using UnityEngine;
using StarterAssets;

public class Quest5Cutscene : MonoBehaviour
{
    [Header("Cutscene Camera")]
    [Tooltip("Disable the Camera component on this by default.")]
    public Camera cutsceneCamera;

    [Header("Objects to Animate (Optional)")]
    public Transform movingObject;
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    [Header("Objects to Disable")]
    [Tooltip("Drag any objects that should disappear at the end.")]
    public GameObject[] objectsToDisable;

    [Header("Player & UI")]
    public StarterAssetsInputs playerInputs;
    public GameObject playerUI;

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
        if (completedQuestNumber == 5 && !cutscenePlayed)
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

        // ---- OPTIONAL: MOVE AN OBJECT ----
        if (movingObject != null && waypoints != null && waypoints.Length > 0)
        {
            foreach (Transform wp in waypoints)
            {
                if (wp == null) continue;

                while (Vector3.Distance(movingObject.position, wp.position) > 0.2f)
                {
                    Vector3 dir = (wp.position - movingObject.position).normalized;
                    movingObject.position += dir * moveSpeed * Time.deltaTime;

                    if (dir.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(dir);
                        movingObject.rotation = Quaternion.Slerp(
                            movingObject.rotation, targetRot, rotateSpeed * Time.deltaTime);
                    }

                    if (cutsceneCamera != null)
                    {
                        cutsceneCamera.transform.LookAt(movingObject);
                    }

                    yield return null;
                }
            }
        }

        // ---- WAIT A BEAT ----
        yield return new WaitForSeconds(1.5f);

        // ---- SWITCH BACK TO MAIN CAMERA ----
        if (cutsceneCamera != null && mainCam != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
            mainCam.gameObject.SetActive(true);
        }

        // ---- DISABLE OBJECTS OFF-SCREEN ----
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }
        if (movingObject != null) movingObject.gameObject.SetActive(false);

        // ---- RESTORE PLAYER ----
        if (playerInputs != null) playerInputs.cursorInputForLook = true;
        if (playerUI != null) playerUI.SetActive(true);

        Debug.Log("[Quest5Cutscene] Cutscene complete!");
    }
}