// ==========================================
// Title:       DialogueData.cs
// Description: Data structures for branching dialogue (lines + choices).
// Author:      Sun Shuqi (10274096K)
// ==========================================

using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("按钮上显示的文字，比如 '因为容易被车撞'")]
    public string choiceText;

    [Tooltip("选完这个选项后，NPC说的回应（可留空，留空则直接跳转不显示回应）")]
    [TextArea(2, 4)]
    public string responseText;

    [Tooltip("选完之后跳到第几句(从0开始)。填-1代表按顺序往下一句走")]
    public int nextLineIndex = -1;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;

    [Tooltip("这句是否是二选一/多选一的问题")]
    public bool hasChoices = false;

    [Tooltip("选项列表，hasChoices为true时才会用到")]
    public DialogueChoice[] choices;
}
