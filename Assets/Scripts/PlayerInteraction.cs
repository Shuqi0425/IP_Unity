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
    [SerializeField] private float interactDistance = 3.0f;

    [Tooltip("Layer of interactable objects")]
    [SerializeField] private LayerMask interactableLayer;

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

    private Camera playerCamera;
    private InteractableObject currentTarget; // interactable object point out now

    private void Start()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("[PlayerInteraction] There is no camera tagged 'MainCamera' in the scene!");
        }

        // hide all UI at origin
        if (promptText != null) promptText.gameObject.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    private void Update()
    {
        // 1. If panel is on press E or esc to close
        if (infoPanel != null && infoPanel.activeSelf)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseDetailPanel();
            }
            return;
        }

        // 2. raycasting and renew UI
        PerformRaycast();

        // 3. press E to interact
        if (currentTarget != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentTarget.OnInteract(this);
        }
    }

    private void PerformRaycast()
    {
        if (playerCamera == null) return;

        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        if (showDebugRay)
        {
            Debug.DrawRay(rayOrigin, rayDirection * interactDistance, Color.red);
        }

        // detect the object by raycast
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, interactDistance, interactableLayer))
        {
            // print object hited in console window
            Debug.Log("ray hit the object: " + hit.collider.name);

            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                currentTarget = interactable;

                if (promptText != null)
                {
                    promptText.text = currentTarget.promptMessage;
                    promptText.gameObject.SetActive(true);
                }
                return;
            }
            else
            {
                Debug.LogWarning("hit object " + hit.collider.name + "without InteractableObject script on it!");
            }
        }

        // if raycast nothing or non-interactable,renew the status and hide hit-on announcement
        ClearTarget();
    }

    private void ClearTarget()
    {
        currentTarget = null;
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// open the detail info panel
    /// </summary>
    public void ShowDetailPanel(string content)
    {
        ClearTarget(); // hide hit-on announcemnet

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
    }
}