// ==========================================
// Title:       DialogueData.cs
// Description: Data structures for branching dialogue (lines + choices).
// Author:      Sun Shuqi (10274096K)
// ==========================================

using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    [Tooltip("show the text on the button, e.g., 'Because it's easy to get hit by a car'")]
    public string choiceText;

    [Tooltip("The NPC's response after this choice is selected (can be left empty, if empty it will directly jump without showing a response)")]
    [TextArea(2, 4)]
    public string responseText;

    [Tooltip("After selecting this choice, jump to which line (starting from 0). Fill -1 to proceed to the next line in order.")]
    public int nextLineIndex = -1;
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;

    [Tooltip("Whether this line is a multiple-choice question")]
    public bool hasChoices = false;

    [Tooltip("List of choices, used only if hasChoices is true")]
    public DialogueChoice[] choices;
}
