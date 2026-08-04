// ==========================================
// Title:       DoubleDoorOpenScript.cs
// Description: Controls the one-way opening animation for double doors simultaneously.
// Author:      Sun Shuqi (10274096K)
// Date:        4 / August
// ==========================================

using UnityEngine;

/// <summary>
/// Controls the one-way opening animation for double sci-fi doors simultaneously.
/// </summary>
public class DoubleDoorOpenScript : MonoBehaviour
{
    [Header("Door Animators")]
    /// <summary>
    /// Reference to the left door's Animator component.
    /// </summary>
    public Animator leftDoorAnimator;

    /// <summary>
    /// Reference to the right door's Animator component.
    /// </summary>
    public Animator rightDoorAnimator;

    /// <summary>
    /// State flag tracking whether the double doors have already been opened.
    /// </summary>
    private bool hasOpened = false;

    /// <summary>
    /// Triggers when another collider enters the trigger volume.
    /// </summary>
    /// <param name="other">The collider entering the door's sensor zone.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is the Player and the doors haven't been opened yet
        if (other.CompareTag("Player") && !hasOpened)
        {
            OpenDoubleDoors();
        }
    }

    /// <summary>
    /// Synchronously triggers the opening animation on both doors and locks the state.
    /// </summary>
    private void OpenDoubleDoors()
    {
        if (leftDoorAnimator != null)
        {
            leftDoorAnimator.SetTrigger("OpenDoor");
        }

        if (rightDoorAnimator != null)
        {
            rightDoorAnimator.SetTrigger("OpenDoor");

            hasOpened = true;
            Debug.Log("¡¾System¡¿Player approached. Double doors opened simultaneously.");
        }
    }
}