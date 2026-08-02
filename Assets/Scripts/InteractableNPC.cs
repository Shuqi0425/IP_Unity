// ==========================================
// Title:       InteractableNPC.cs
// Description: Interactable jaywalking NPC
// Author:      Sun Shuqi (10274096K)
// Date:        3 / August
// ==========================================
using UnityEngine;

public class InteractableNPC : InteractableObject
{
    public override void OnInteract(PlayerInteraction player)
    {
        player.ShowDetailPanel(detailText, gameObject);
        GetComponent<NPCJaywalking>()?.StopForInteraction();
    }
}