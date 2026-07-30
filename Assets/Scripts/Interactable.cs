// ==========================================
// Title:       Interactable.cs
// Description: Base class for all interactable objects in the game.
// Author:      Sun Shuqi (10274096K)
// Date:        30 / July
// ==========================================

using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Base Settings")]
    [Tooltip("Name of the object or NPC")]
    public string objectName = "interactable";

    /// <summary>
    /// Virtual method (virtual): allows subclasses to override (override)
    /// </summary>
    public virtual void Interact()
    {
        Debug.Log($"[Base Interaction] Interacting with {objectName}.");
    }
}