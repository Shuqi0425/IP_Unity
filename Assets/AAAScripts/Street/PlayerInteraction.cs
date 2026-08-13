// ==========================================
// Title:       PlayerInteraction.cs
// Description: First-person Raycasting system for player interaction.
//              Handles multi-line dialogue (press E to advance) and
//              branching choices (click a button to pick).
// Author:      Sun Shuqi (10274096K)
// Date:        31 / July (edited on 13 August)
// ==========================================

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

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

    [Header("Dialogue Settings")]
    [Tooltip("Additional prompt to show when the dialogue is not the last line, e.g., '(Press E to continue)'")]
    [SerializeField] private string continuePrompt = "";

    [Tooltip("Additional prompt to show when the dialogue is the last line, e.g., '(Press E to close)'. Leave empty to not show.")]
    [SerializeField] private string endPrompt = "";

    [Header("Choice UI References")]
    [Tooltip("Choice panel, hidden by default, shown when a multiple-choice question appears")]
    [SerializeField] private GameObject choicePanel;

    [Tooltip("option buttons, it is recommended to leave enough for the maximum number of options (e.g., 4), unused ones will be automatically hidden")]
    [SerializeField] private Button[] choiceButtons;

    [Tooltip("The text components corresponding to each button, in the same order as choiceButtons")]
    [SerializeField] private TextMeshProUGUI[] choiceButtonTexts;

    [Header("Cursor Settings")]
    [Tooltip("Whether to automatically unlock and show the mouse when the dialogue/choice panel is open (usually needed for first-person view, otherwise the buttons cannot be clicked)")]
    [SerializeField] private bool unlockCursorDuringDialogue = true;

    [Tooltip("Drag the Starter Assets Inputs component attached to the Player here. If left empty, it will automatically try to find it in the parent object. " +
             "Purpose: Pause mouse look when the dialogue panel is open, and resume it when closed, to prevent the camera from moving while clicking buttons.")]
    [SerializeField] private StarterAssetsInputs starterAssetsInput;

    [Header("Debug")]
    [Tooltip("draw debug rays in the Scene view")]
    [SerializeField] private bool showDebugRay = true;

    Camera playerCamera;
    InteractableObject currentTarget;
    private GameObject lastInteractedNPC;

    // ---- dialogue ----
    private DialogueLine[] currentDialogueLines;
    private int currentLineIndex;
    private InteractableObject currentDialogueSource;

    // ---- choice response transition state (after selecting an option -> view response -> press E to actually jump) ----
    private bool waitingForResponseContinue = false;
    private int pendingJumpIndex = -1;

    // ---- mouse state before opening the panel, to restore when closing the panel ----
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    // ---- whether the cursor is currently frozen due to the choice panel being open ----
    private bool isCursorFrozenForChoices = false;

    /// <summary>
    /// Triggered when the dialogue panel is closed (either normally or by pressing Esc to skip).
    /// For external scripts to subscribe, for example, to return control to the player after the scene opening briefing is finished.
    /// </summary>
    public System.Action OnDialogueClosed;

    void Awake()
    {
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("[PlayerInteraction] There is no camera tagged 'MainCamera' in the scene!");
        }

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);

        // if` the StarterAssetsInput is not assigned in the Inspector, try to find it in the parent object 
        if (starterAssetsInput == null)
        {
            starterAssetsInput = GetComponentInParent<StarterAssetsInputs>();
        }
    }

    void Update()
    {
        // when the info panel is open, we don't do raycasting, only handle E/Esc for dialogue advancement or closing
        if (infoPanel != null && infoPanel.activeSelf)
        {
            bool showingChoices = choicePanel != null && choicePanel.activeSelf;

            // shwoing choices: don't allow E to advance dialogue, only allow Esc to close the panel
            if (showingChoices)
            {
                return;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                AdvanceDialogue();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseDetailPanel();
            }
            return;
        }

        // regular raycasting for interaction
        UpdateInteractionTarget();

        if (currentTarget != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnInteract();
        }
    }

    void UpdateInteractionTarget()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                ClearCurrentTargets();
                return;
            }
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Vector3 rayEndPoint = ray.origin + (ray.direction * interactDistance);

        bool didHit = Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer);

        if (showDebugRay)
        {
            if (didHit)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
            else
            {
                Debug.DrawLine(ray.origin, rayEndPoint, Color.red);
            }
        }

        if (didHit)
        {
            SetCurrentTargetsFromHit(hit.collider);
        }
        else
        {
            ClearCurrentTargets();
        }
    }

    void SetCurrentTargetsFromHit(Collider hitCollider)
    {
        InteractableObject newTarget = null;

        if (hitCollider.CompareTag("Interactable"))
        {
            newTarget = hitCollider.GetComponentInParent<InteractableObject>();
        }

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;

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

    void ClearCurrentTargets()
    {
        currentTarget = null;

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void OnInteract()
    {
        if (currentTarget != null)
        {
            currentTarget.OnInteract(this);
        }
    }

    /// <summary>
    /// open teh dialogue panel and display the given dialogue lines, starting from the first line.
    /// </summary>
    public void ShowDetailPanel(DialogueLine[] dialogueLines, InteractableObject source = null)
    {
        ClearCurrentTargets();
        lastInteractedNPC = source != null ? source.gameObject : null;
        currentDialogueSource = source;

        currentDialogueLines = (dialogueLines != null && dialogueLines.Length > 0)
            ? dialogueLines
            : new DialogueLine[] { new DialogueLine { text = "..." } };

        currentLineIndex = 0;
        waitingForResponseContinue = false;
        pendingJumpIndex = -1;

        if (infoPanel != null)
        {
            infoPanel.SetActive(true);
        }

        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (currentDialogueLines == null ||
            currentLineIndex < 0 ||
            currentLineIndex >= currentDialogueLines.Length)
        {
            return;
        }

        DialogueLine line = currentDialogueLines[currentLineIndex];

        if (line.hasChoices && line.choices != null && line.choices.Length > 0)
        {
            //  a choice line: show text + options
            if (infoPanelText != null)
            {
                infoPanelText.text = line.text;
            }
            ShowChoices(line.choices);
        }
        else
        {
            // simple   text line: show text, hide options
            HideChoices();

            bool isLastLine = currentLineIndex >= currentDialogueLines.Length - 1;
            string suffix = isLastLine ? endPrompt : continuePrompt;

            if (infoPanelText != null)
            {
                infoPanelText.text = string.IsNullOrEmpty(suffix) ? line.text : $"{line.text}\n{suffix}";
            }
        }
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        if (choicePanel == null || choiceButtons == null) return;

        choicePanel.SetActive(true);

        // ---- only unlock the mouse and freeze the camera when the choice panel is shown ----
        SetCursorFrozenForChoices(true);

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            if (i < choices.Length)
            {
                int choiceIndex = i; 
                choiceButtons[i].gameObject.SetActive(true);

                if (choiceButtonTexts != null && i < choiceButtonTexts.Length && choiceButtonTexts[i] != null)
                {
                    choiceButtonTexts[i].text = choices[i].choiceText;
                }

                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideChoices()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        // ---- only unlock the mouse and unfreeze the camera when the choice panel is hidden ----
        SetCursorFrozenForChoices(false);
    }

    /// <summary>
    /// Called when the player clicks a choice button (bound to the button's onClick, passing the index through a lambda)
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        DialogueLine currentLine = currentDialogueLines[currentLineIndex];

        if (currentLine.choices == null || choiceIndex < 0 || choiceIndex >= currentLine.choices.Length)
        {
            return;
        }

        DialogueChoice choice = currentLine.choices[choiceIndex];
        int targetIndex = (choice.nextLineIndex >= 0) ? choice.nextLineIndex : currentLineIndex + 1;

        HideChoices();

        // if the choice has a response text, show it and wait for the player to press E to continue; otherwise, jump directly to the target line
        if (!string.IsNullOrEmpty(choice.responseText))
        {
            if (infoPanelText != null)
            {
                infoPanelText.text = string.IsNullOrEmpty(continuePrompt)
                    ? choice.responseText
                    : $"{choice.responseText}\n{continuePrompt}";
            }

            pendingJumpIndex = targetIndex;
            waitingForResponseContinue = true;
        }
        else
        {
            // if there's no response text, jump directly to the target line
            JumpToLine(targetIndex);
        }
    }

    private void JumpToLine(int index)
    {
        if (currentDialogueLines == null || index < 0 || index >= currentDialogueLines.Length)
        {
            CloseDetailPanel();
            return;
        }

        currentLineIndex = index;
        DisplayCurrentLine();
    }

    /// <summary>
    /// pressed E to advance the dialogue, either to the next line or to the target line after a choice response
    /// </summary>
    private void AdvanceDialogue()
    {
        if (waitingForResponseContinue)
        {
            waitingForResponseContinue = false;
            int target = pendingJumpIndex;
            pendingJumpIndex = -1;
            JumpToLine(target);
            return;
        }

        currentLineIndex++;

        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Length)
        {
            CloseDetailPanel();
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    /// <summary>
    /// close the dialogue panel, hide choices, and reset state. This can be called either when the dialogue naturally ends or when the player presses Esc to skip.
    /// </summary>
    public void CloseDetailPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        HideChoices();

        // inform the dialogue source that the dialogue has finished, so it can perform any necessary cleanup or state changes
        if (currentDialogueSource != null)
        {
            currentDialogueSource.OnDialogueFinished();
        }

        if (lastInteractedNPC != null)
        {
            lastInteractedNPC.GetComponent<NPCJaywalking>()?.LeaveAfterDialogue();
            lastInteractedNPC = null;
        }

        currentDialogueLines = null;
        currentDialogueSource = null;
        currentLineIndex = 0;
        waitingForResponseContinue = false;
        pendingJumpIndex = -1;

        OnDialogueClosed?.Invoke();
    }

    /// <summary>
    /// called when the choice panel is shown/hidden. During this time, the mouse is unlocked and the camera is frozen.
    /// Normal dialogue progression (without choices) is not affected, and the camera can move freely.
    /// If you don't want this behavior, uncheck unlockCursorDuringDialogue in the Inspector.
    /// </summary>
    private void SetCursorFrozenForChoices(bool freeze)
    {
        if (!unlockCursorDuringDialogue) return;

        if (freeze)
        {
            if (isCursorFrozenForChoices) return; // freeze only once, don't repeat

            // remember the previous cursor state so we can restore it later
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (starterAssetsInput != null)
            {
                starterAssetsInput.cursorInputForLook = false;
                starterAssetsInput.look = Vector2.zero; // clear any existing look input to prevent camera from moving when unlocking cursor
            }

            isCursorFrozenForChoices = true;
        }
        else
        {
            if (!isCursorFrozenForChoices) return;

            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;

            if (starterAssetsInput != null)
            {
                starterAssetsInput.cursorInputForLook = true;
            }

            isCursorFrozenForChoices = false;
        }
    }
}