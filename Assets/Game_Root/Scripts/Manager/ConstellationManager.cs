using UnityEngine;
using System.Collections.Generic;

public class ConstellationManager : MonoBehaviour
{
    public static ConstellationManager Instance;

    [Header("Runtime Data")]
    public List<string> collectedNodes = new List<string>();

    [Header("UI References (HUD + Summary)")]
    public List<ConstellationNodeUI> uiNodes = new List<ConstellationNodeUI>();

    private void Awake()
    {
        // 🔥 SINGLETON PER SCENE
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        collectedNodes = new List<string>();
    }

    // ======================================================
    // ⭐ CORE SYSTEM
    // ======================================================

    public void OnStarCollected(string id)
    {
        if (!collectedNodes.Contains(id))
        {
            collectedNodes.Add(id);
            Debug.Log("[Constellation] Collected: " + id);

            UpdateUI(id);
        }
    }

    public bool IsCollected(string id)
    {
        return collectedNodes.Contains(id);
    }

    public void ResetAll()
    {
        collectedNodes.Clear();

        foreach (var node in uiNodes)
        {
            if (node != null)
                node.ResetUI();
        }
    }

    // ======================================================
    // ⭐ UI SYSTEM
    // ======================================================

    private void UpdateUI(string id)
    {
        var node = uiNodes.Find(n => n != null && n.nodeID == id);

        if (node != null)
        {
            node.SetActiveInstant(); // ✅ FIX
        }
    }

    public void SyncAllUI()
    {
        foreach (var node in uiNodes)
        {
            if (node == null) continue;

            if (collectedNodes.Contains(node.nodeID))
                node.SetActiveInstant(); // ✅ FIX
            else
                node.ResetUI();
        }
    }
}