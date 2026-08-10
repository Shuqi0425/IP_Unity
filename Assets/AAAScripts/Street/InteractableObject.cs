// ==========================================
// Title:       Interactable.cs
// Description: Base class for all interactable objects in the game.
// Author:      Sun Shuqi (10274096K)
// Date:        31 / July
// ==========================================

using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("UI notification setting")]
    [Tooltip("Short notice when raycasting point on")]
    public string promptMessage = "Press E to Check";

    [Header("Detail info")]
    [TextArea(3, 5)]
    public string detailText = "Content";

    /// <summary>
    /// Trigger interact logic
    /// </summary>
    public virtual void OnInteract(PlayerInteraction player)
    {
        // triggered UI panel and show info
        player.ShowDetailPanel(detailText);
    }
}