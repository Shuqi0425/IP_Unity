// ==========================================
// Title:       SceneBriefingController.cs
// Description: Shows a scene-opening briefing as ONE single block of text
//              (no pagination) based on the current quest number, using its
//              own dedicated UI panel. Press E to close and return control
//              to the player.
// Author:      Sun Shuqi (10274096K)
// ==========================================

using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

[System.Serializable]
public class SceneBriefingEntry
{
    [Tooltip("The quest number for which this briefing should be shown. Set to -1 to show regardless of the current quest (acts as a fallback).")]
    public int questNumber = -1;

    [Tooltip("The briefing text to display for this quest number")]
    [TextArea(4, 12)]
    public string briefingText;
}

public class SceneBriefingController : MonoBehaviour
{
    [Header("Briefing UI（独立于NPC对话的Panel）")]
    [Tooltip("The panel dedicated to the briefing, hidden by default")]
    [SerializeField] private GameObject briefingPanel;

    [Tooltip("The TextMeshProUGUI component that displays the briefing text")]
    [SerializeField] private TMPro.TextMeshProUGUI briefingText;

    [Tooltip("Additional prompt to show at the end of the text, e.g., '(Press E to close)'. Leave empty to not show.")]
    [SerializeField] private string closePrompt = "";

    [Header("References")]
    [Tooltip("The PlayerInteraction component on the Player in the scene. Leave empty to auto-find. It will be temporarily disabled during the briefing to prevent conflicts with NPC interactions.")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Tooltip("The StarterAssetsInputs component on the Player in the scene. Leave empty to auto-find (used to freeze movement/camera)")]
    [SerializeField] private StarterAssetsInputs starterAssetsInput;

    [Header("Briefing Data")]
    [Tooltip("Configure the briefing for each scene by Quest number. When entering the scene, it will look for a matching questNumber. If none is found, no briefing will be shown.")]
    public SceneBriefingEntry[] briefings;

    [Header("Behaviour")]
    [Tooltip("Whether to freeze player movement/camera during the briefing")]
    [SerializeField] private bool freezePlayerDuringBriefing = true;

    [Tooltip("Whether to play the briefing only the first time this scene is loaded (use a static flag to prevent repeated triggers during this run). If unchecked, the briefing will play every time the scene is entered.")]
    [SerializeField] private bool playOnlyOnce = false;

    private bool hasPlayedThisSession = false;
    private bool isShowing = false;

    private void Awake()
    {
        if (briefingPanel != null) briefingPanel.SetActive(false);
    }

    private void Start()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (starterAssetsInput == null && freezePlayerDuringBriefing)
        {
            starterAssetsInput = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("[SceneBriefingController] QuestManager not found! 无法判断该播放哪段开场白。");
            return;
        }

        if (playOnlyOnce && hasPlayedThisSession)
        {
            return;
        }

        SceneBriefingEntry match = FindMatchingBriefing(QuestManager.Instance.currentQuest);

        if (match == null || string.IsNullOrEmpty(match.briefingText))
        {
            return; // no briefing to show for this quest number
        }

        ShowBriefing(match.briefingText);
    }

    private void Update()
    {
        if (!isShowing) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CloseBriefing();
        }
    }

    private SceneBriefingEntry FindMatchingBriefing(int currentQuest)
    {
        SceneBriefingEntry fallback = null;

        foreach (SceneBriefingEntry entry in briefings)
        {
            if (entry.questNumber == currentQuest) return entry;
            if (entry.questNumber == -1) fallback = entry;
        }

        return fallback;
    }

    private void ShowBriefing(string text)
    {
        isShowing = true;
        hasPlayedThisSession = true;

        if (briefingPanel != null) briefingPanel.SetActive(true);

        if (briefingText != null)
        {
            briefingText.text = string.IsNullOrEmpty(closePrompt) ? text : $"{text}\n{closePrompt}";
        }

        // 暂时禁用NPC互动，避免E键被两边同时响应
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        // 冻结玩家移动/视角
        if (freezePlayerDuringBriefing && starterAssetsInput != null)
        {
            starterAssetsInput.enabled = false;
        }
    }

    private void CloseBriefing()
    {
        isShowing = false;

        if (briefingPanel != null) briefingPanel.SetActive(false);

        if (freezePlayerDuringBriefing && starterAssetsInput != null)
        {
            starterAssetsInput.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }
    }
}