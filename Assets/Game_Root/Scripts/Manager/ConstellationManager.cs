using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstellationManager : MonoBehaviour {
    public static ConstellationManager Instance;

    [Header("Runtime Data")]
    private HashSet<int> collectedNodes = new HashSet<int>();

    [Header("UI References")]
    public List<ConstellationNodeUI> hudNodes;
    public List<ConstellationNodeUI> summaryNodes;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    // =========================
    // CORE SYSTEM
    // =========================

    public void OnStarCollected(int nodeID) {
        if (!collectedNodes.Contains(nodeID)) {
            collectedNodes.Add(nodeID);
            Debug.Log("[Constellation] Collected: " + nodeID);

            UpdateHUD(nodeID);
        }
    }

    public bool IsCollected(int nodeID) {
        return collectedNodes.Contains(nodeID);
    }

    public void ResetAll() {
        collectedNodes.Clear();

        foreach (var node in hudNodes) {
            if (node != null)
                node.ResetUI();
        }

        foreach (var node in summaryNodes) {
            if (node != null)
                node.ResetUI();
        }
    }

    public void ResetCollectedNodes() {
        collectedNodes.Clear();
    }

    // =========================
    // HUD SYSTEM
    // =========================

    private void UpdateHUD(int nodeID) {
        var node = hudNodes.Find(n => n != null && n.nodeID == nodeID);

        if (node != null) {
            node.SetActiveInstant();
        }
    }

    public void SyncHUD() {
        foreach (var node in hudNodes) {
            if (node == null) continue;

            node.SetGameplayMode();
            node.StopAllCoroutines();

            if (collectedNodes.Contains(node.nodeID))
                node.SetActiveInstant();
            else
                node.ResetUI();
        }
    }

    // =========================
    // SUMMARY SYSTEM
    // =========================

    public void SyncSummaryInstant() {
        foreach (var node in summaryNodes) {
            if (node == null) continue;

            if (collectedNodes.Contains(node.nodeID))
                node.SetActiveInstant();
            else
                node.ResetUI();
        }
    }

    public int GetCollectedCount() {
        return collectedNodes.Count;
    }
}