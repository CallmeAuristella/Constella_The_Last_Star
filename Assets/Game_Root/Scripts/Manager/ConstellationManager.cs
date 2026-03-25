using UnityEngine;
using System.Collections.Generic;

public class ConstellationManager : MonoBehaviour
{
    public static ConstellationManager Instance;

    [Header("Runtime Data")]
    public List<string> collectedNodes = new List<string>();

    [Header("UI References")]
    public List<ConstellationNodeUI> hudNodes;
    public List<ConstellationNodeUI> summaryNodes;

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

            UpdateHUD(id); // 🔥 hanya HUD real-time
        }
    }

    public bool IsCollected(string id)
    {
        return collectedNodes.Contains(id);
    }

    public void ResetAll()
    {
        collectedNodes.Clear();

        // reset HUD
        foreach (var node in hudNodes)
        {
            if (node != null)
                node.ResetUI();
        }

        // reset Summary
        foreach (var node in summaryNodes)
        {
            if (node != null)
                node.ResetUI();
        }
    }

    public void ResetCollectedNodes()
    {
        collectedNodes.Clear();
    }

    // ======================================================
    // ⭐ HUD SYSTEM (REALTIME)
    // ======================================================

    private void UpdateHUD(string id)
    {
        var node = hudNodes.Find(n => n != null && n.nodeID == id);

        if (node != null)
        {
            node.SetActiveInstant();
        }
    }

    public void SyncHUD()
    {
        foreach (var node in hudNodes)
        {
            if (node == null) continue;

            // 🔥 SET MODE DULU
            node.SetGameplayMode();
            node.StopAllCoroutines();

            // 🔥 BARU UPDATE VISUAL
            if (collectedNodes.Contains(node.nodeID))
                node.SetActiveInstant();
            else
                node.ResetUI();
        }
    }

    // ======================================================
    // ⭐ SUMMARY SYSTEM (ANIMATION)
    // ======================================================

    public void SyncSummaryInstant()
    {
        foreach (var node in summaryNodes)
        {
            if (node == null) continue;

            if (collectedNodes.Contains(node.nodeID))
                node.SetActiveInstant();
            else
                node.ResetUI();
        }
    }

    public List<string> GetCollectedNodes()
    {
        return new List<string>(collectedNodes);
    }
}