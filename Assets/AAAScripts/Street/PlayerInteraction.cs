// ==========================================
// Title:       PlayerInteraction.cs
// Description: First-person Raycasting system for player interaction with UI feedback.
// Author:      Sun Shuqi (10274096K)
// Date:        31 / July
// ==========================================

using UnityEngine; 
using UnityEngine.InputSystem; 
using TMPro; 

public class PlayerInteraction : MonoBehaviour
{

    [Header("Raycast Settings")]
    [Tooltip("maximum interaction distance")]
    [SerializeField] private float interactDistance = 3.0f; // Sets max interaction ray distance.

    [Tooltip("Layer of interactable objects")]
    [SerializeField] private LayerMask interactableLayer; // Selects which layers the ray can hit.

    [Header("UI References (Canvas)")]
    [Tooltip("Press E to Interact Text")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Tooltip("Panel")]
    [SerializeField] private GameObject infoPanel;

    [Tooltip("InfoPanel")]
    [SerializeField] private TextMeshProUGUI infoPanelText;

    [Header("Debug")]
    [Tooltip("draw debug rays in the Scene view")]
    [SerializeField] private bool showDebugRay = true;

    Camera playerCamera; // Caches the camera used for aiming.

    InteractableObject currentTarget; // Tracks the interactable currently aimed at
    private GameObject lastInteractedNPC; // record which NPC triggered conversation

    void Awake()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("[PlayerInteraction] There is no camera tagged 'MainCamera' in the scene!");
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    void Update()
    {
        // if panel is open, only listen for close input, skip raycast this frame
        if (infoPanel != null && infoPanel.activeSelf)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseDetailPanel();
            }
            return;
        }

        UpdateInteractionTarget(); // Refreshes the currently aimed interactable.

        if (currentTarget != null && Keyboard.current.eKey.wasPressedThisFrame) // Checks interact input.
        {
            OnInteract(); // Runs when the interact input is triggered.
        }
    }

    void UpdateInteractionTarget() // Performs the raycast target check.
    {
        if (playerCamera == null) // Checks whether camera cache is missing.
        { 
            playerCamera = Camera.main; // Tries to re-fetch the main camera.
            if (playerCamera == null) // Checks if camera is still unavailable.
            { 
                ClearCurrentTargets(); // Clears all tracked targets.
                return;
            }
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Builds a forward ray from the camera.
        Vector3 rayEndPoint = ray.origin + (ray.direction * interactDistance); // Computes the max-distance end point of the ray.

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer)) // Casts the ray against valid colliders.
        { 
            Debug.DrawLine(ray.origin, hit.point, Color.green); // Draws the hit ray in green.
            SetCurrentTargetsFromHit(hit.collider); // Updates targets from the hit collider.
        }
        else // Handles the case where nothing was hit.
        {
            Debug.DrawLine(ray.origin, rayEndPoint, Color.red); // Draws the full ray in red when no hit occurs.
            ClearCurrentTargets();
        } 
    }

    void SetCurrentTargetsFromHit(Collider hitCollider) // Derives interactable reference from a hit.
    {
        InteractableObject newTarget = null; // Prepares a fresh interactable target.

        if (hitCollider.CompareTag("Interactable")) // Checks if the hit object is tagged as Interactable.
        {
            newTarget = hitCollider.GetComponentInParent<InteractableObject>(); // Gets InteractableObject script from hit hierarchy.
        }

        if (newTarget != currentTarget) // Only refresh UI when the target actually changed.
        {
            currentTarget = newTarget; // Commits the interactable target.

            if (currentTarget != null && promptText != null)
            {
                promptText.text = currentTarget.promptMessage;
                promptText.gameObject.SetActive(true);
            }
            else if (currentTarget == null && promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }

    void ClearCurrentTargets() // Resets currently tracked interaction target.
    {
        currentTarget = null; // Clears interactable target.

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void OnInteract() // Runs when the interact input is triggered.
    {
        if (currentTarget != null) // Checks for an interactable target.
        {
            currentTarget.OnInteract(this); // Calls the target's own interact logic (shows detail panel).
        }
    }

    /// <summary>
    /// open the detail info panel
    /// </summary>
    public void ShowDetailPanel(string content, GameObject sourceNPC = null)
    {
        ClearCurrentTargets(); // hide hit-on announcement
        lastInteractedNPC = sourceNPC;

        if (infoPanelText != null)
        {
            infoPanelText.text = content;
        }

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }
    }

    /// <summary>
    /// close info panel
    /// </summary>
    public void CloseDetailPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        if (lastInteractedNPC != null)
        {
            lastInteractedNPC.GetComponent<NPCJaywalking>()?.LeaveAfterDialogue();
            lastInteractedNPC = null;
        }
    }
} 