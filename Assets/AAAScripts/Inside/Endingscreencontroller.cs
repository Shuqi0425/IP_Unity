// ==========================================
// Title:       EndingScreenController.cs
// Description: Shows a one-time ending screen when the player returns to
//              the Inside scene AFTER completing all 5 quests
//              (QuestManager.allQuestsCompleted == true). Uses its own
//              dedicated UI panel (separate from dialogue/briefing panels).
// ==========================================

using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class EndingScreenController : MonoBehaviour
{
    [Header("Ending UI（独立的结束画面Panel）")]
    [Tooltip("结束画面专用的面板，平时隐藏")]
    [SerializeField] private GameObject endingPanel;

    [Tooltip("显示结束文字的Text组件")]
    [SerializeField] private TMPro.TextMeshProUGUI endingText;

    [Tooltip("结束画面要显示的全文，直接一次性显示，不分页")]
    [TextArea(6, 20)]
    [SerializeField]
    private string endingContent =
        "You: I have learnt so many things from these short 5 days! I've learnt that\n\n" +
        "- Jaywalking is dangerous even if it's convenient, and many people still do it. We need to put an end to it, especially with how normalised it is, with young adults to literal children doing it.\n\n" +
        "- We must always practise special awareness when on the road, and ensure our transportation vehicle is not faulty before using it. Wearing protective gear is also very much recommended, as it can literally save your life in critical cases.\n\n" +
        "- We must stay focused on the road, especially when traversing dangerous parts. Having awareness and knowing what is going on around you is a huge help, and can even save you from life or death cases, especially as a pedestrian.\n\n" +
        "You: Thank you for going on this journey, and I hope you learnt something important from this. Most importantly, I hope you had fun playing.";

    [Header("References")]
    [Tooltip("场景里的PlayerInteraction组件，留空会自动查找。显示结束画面期间会暂时禁用它")]
    [SerializeField] private PlayerInteraction playerInteraction;

    [Tooltip("场景里Player身上的StarterAssetsInputs，留空会自动查找（用于冻结移动/视角）")]
    [SerializeField] private StarterAssetsInputs starterAssetsInput;

    [Header("Ending Effects")]
    [Tooltip("按E关闭结束画面后要播放的粒子特效（比如庆祝的礼花/光效）。拖入场景里对应的Particle System物体")]
    [SerializeField] private ParticleSystem endingParticles;

    [Header("Behaviour")]
    [Tooltip("结束画面显示期间是否冻结玩家移动/视角")]
    [SerializeField] private bool freezePlayerDuringEnding = true;

    [Tooltip("结束画面是否允许按E关闭。如果希望它是永久性的结局画面（比如后面就没别的可玩了），取消勾选")]
    [SerializeField] private bool allowCloseWithE = true;

    private bool isShowing = false;

    private void Awake()
    {
        if (endingPanel != null) endingPanel.SetActive(false);
    }

    private void Start()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        if (starterAssetsInput == null && freezePlayerDuringEnding)
        {
            starterAssetsInput = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogError("[EndingScreenController] QuestManager not found!");
            return;
        }

        // 只有当五个Quest全部完成，才显示结束画面；否则正常玩游戏，什么都不做
        if (QuestManager.Instance.allQuestsCompleted)
        {
            ShowEnding();
        }
    }

    private void Update()
    {
        if (!isShowing || !allowCloseWithE) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            CloseEnding();
        }
    }

    private void ShowEnding()
    {
        isShowing = true;

        if (endingPanel != null) endingPanel.SetActive(true);

        if (endingText != null)
        {
            string suffix = allowCloseWithE ? "\n\npress E to continue" : "";
            endingText.text = endingContent + suffix;
        }

        // temporarily disable NPC interaction to avoid E key conflicts
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        // temporarily freeze player movement/view
        if (freezePlayerDuringEnding && starterAssetsInput != null)
        {
            starterAssetsInput.enabled = false;
        }

        // unlock the cursor to allow interaction with UI buttons (e.g., "Restart", "Quit")
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseEnding()
    {
        isShowing = false;

        if (endingPanel != null) endingPanel.SetActive(false);

        // ---- newly added: play ending particles ----
        if (endingParticles != null)
        {
            endingParticles.gameObject.SetActive(true); // in case the object is initially hidden
            endingParticles.Play();
        }

        if (freezePlayerDuringEnding && starterAssetsInput != null)
        {
            starterAssetsInput.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}