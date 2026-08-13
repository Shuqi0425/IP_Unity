// ==========================================
// Title:       PlayerInteraction.cs
// Description: First-person Raycasting system for player interaction.Handles multi-line dialogue (press E to advance) and branching choices (click a button to pick).
// Author:      Sun Shuqi (10274096K)
// Date:        13 August
// ==========================================

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    [Header("Dialogue Settings")]
    [Tooltip("Additional prompt to show when the dialogue is not the last line, e.g., '(Press E to continue)'")]
    [SerializeField] private string continuePrompt = "";

    [Tooltip("Additional prompt to show when the dialogue is the last line, e.g., '(Press E to close)'. Leave empty to not show.")]
    [SerializeField] private string endPrompt = "";

    [Header("Choice UI References")]
    [Tooltip("Choice panel, hidden by default, shown when a multiple-choice question appears")]
    [SerializeField] private GameObject choicePanel;

    [Tooltip("Choice buttons, it's recommended to have enough for the maximum number of choices (e.g., 4). Unused buttons will be automatically hidden.")]
    [SerializeField] private Button[] choiceButtons;

    [Tooltip("The text components corresponding to each button, in the same order as choiceButtons")]
    [SerializeField] private TextMeshProUGUI[] choiceButtonTexts;

    [Header("Debug")]
    [Tooltip("draw debug rays in the Scene view")]
    [SerializeField] private bool showDebugRay = true;

    Camera playerCamera;
    InteractableObject currentTarget;
    private GameObject lastInteractedNPC;

    // ---- 对话进度状态 ----
    private DialogueLine[] currentDialogueLines;
    private int currentLineIndex;
    private InteractableObject currentDialogueSource;

    // ---- 选项回应过渡状态（选完选项 -> 先看回应 -> 按E才真正跳转）----
    private bool waitingForResponseContinue = false;
    private int pendingJumpIndex = -1;

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
    }

    void Update()
    {
        // 对话面板打开时
        if (infoPanel != null && infoPanel.activeSelf)
        {
            bool showingChoices = choicePanel != null && choicePanel.activeSelf;

            // 显示选项时，E/Esc都不处理，只等玩家点按钮
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

        // 平时的射线检测逻辑
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

        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green);
            SetCurrentTargetsFromHit(hit.collider);
        }
        else
        {
            Debug.DrawLine(ray.origin, rayEndPoint, Color.red);
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
    /// 打开对话面板，从第0句开始播放
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
            // 这是一个选项句：只显示问题文字，不加continue/end提示（因为要等玩家点按钮）
            if (infoPanelText != null)
            {
                infoPanelText.text = line.text;
            }
            ShowChoices(line.choices);
        }
        else
        {
            // 普通句：显示文字 + continue/end提示
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

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            if (i < choices.Length)
            {
                int choiceIndex = i; // 闭包坑，必须存一份局部变量
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
    }

    /// <summary>
    /// 玩家点击了某个选项按钮时调用（绑定在按钮的onClick上，通过匿名方法传入下标）
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

        // 如果这个选项有回应文字，先显示回应，等玩家按E再跳转
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
            // 没有回应文字，直接跳转
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
    /// 按E推进对话：
    /// - 如果刚看完选项回应，这次E会真正跳转到目标句
    /// - 否则正常往下一句走，走到最后一句之后关闭面板
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
    /// 关闭对话面板（对话正常说完 / 按Esc强制跳过 都会走这里）
    /// </summary>
    public void CloseDetailPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }

        HideChoices();

        // notice the source that the dialogue has finished, so it can trigger quest completion if needed
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
    }
}
