// ==========================================
// Title:       PlayerInteraction.cs
// Description: First-person Raycasting system for player interaction with evidence and NPCs using the E key.
// Author:      Sun Shuqi (10274096K)
// Date:        30 / July
// ==========================================

using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("maximum interaction distance")]
    [SerializeField] private float interactDistance = 3.0f;

    [Tooltip("Layer of interactable objects")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("Debug")]
    [Tooltip("draw debug rays in the Scene view")]
    [SerializeField] private bool showDebugRay = true;

    private Camera playerCamera;

    private void Start()
    {
        // get the main camera in the scene
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("[PlayerInteraction] There is no camera tagged 'MainCamera' in the scene!");
        }
    }

    private void Update()
    {
        // raycast to detect interactable objects in front of the player
        PerformRaycast();

        // try to interact when the player presses the E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    /// <summary>
    /// uses a raycast to detect interactable objects in front of the player and optionally draws a debug ray in the Scene view.
    /// </summary>
    private void PerformRaycast()
    {
        if (playerCamera == null) return;

        // get the origin and direction of the ray from the camera's position and forward direction
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // draw a debug ray in the Scene view if enabled
        if (showDebugRay)
        {
            Debug.DrawRay(rayOrigin, rayDirection * interactDistance, Color.red);
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // raycast to detect interactable objects in front of the player
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hitInfo, interactDistance, interactableLayer))
        {
            // hit an object in the interactable layer, try to get the Interactable component
            Interactable interactableObject = hitInfo.collider.GetComponent<Interactable>();

            if (interactableObject != null)
            {
                // perform interaction
                interactableObject.Interact();
            }
            else
            {
                Debug.LogWarning($"[Interaction] Hit {hitInfo.collider.name}, but it does not have an Interactable component!");
            }
        }
    }
}