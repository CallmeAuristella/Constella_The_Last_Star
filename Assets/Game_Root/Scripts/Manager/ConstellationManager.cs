using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConstellationManager : MonoBehaviour
{
    public static ConstellationManager Instance { get; private set; }

    [Header("UI Database")]
    public List<ConstellationNodeUI> uiNodes = new List<ConstellationNodeUI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        uiNodes.RemoveAll(node => node == null);
    }

    // --- FUNGSI UTAMA YANG DICARI ERROR TADI ---
    public void SetStarStatus(string nodeID, bool isActivated)
    {
        var node = uiNodes.FirstOrDefault(x => x != null && x.nodeID == nodeID);
        if (node != null)
        {
            if (isActivated) node.SetActive();
            else node.ResetUI();
        }
        else
        {
            Debug.LogWarning($"[HUD] ID '{nodeID}' tidak ditemukan di Master List!");
        }
    }

    // Fungsi tambahan untuk sinkronisasi StarNode lama
    public void OnStarCollected(string idCollected) => SetStarStatus(idCollected, true);
    public void OnStarReset(string idReset) => SetStarStatus(idReset, false);

    // Fungsi reset massal berdasarkan list ID
    public void ResetNodesByIDs(List<string> ids)
    {
        foreach (string id in ids)
        {
            SetStarStatus(id, false);
        }
    }
}