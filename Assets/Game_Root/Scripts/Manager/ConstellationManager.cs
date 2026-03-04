using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConstellationManager : MonoBehaviour
{
    public static ConstellationManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Masukkan semua Node UI (bintang di UI) ke sini. Kosong/None akan dihapus otomatis.")]
    public List<ConstellationNodeUI> uiNodes = new List<ConstellationNodeUI>();

    private void Awake()
    {
        // 1. SETUP SINGLETON YANG AMAN
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; // Hentikan eksekusi kalau ini adalah duplikat
        }

        // 2. FITUR ANTI-CRASH: Bersihkan list dari slot 'None' bawaan Inspector
        int emptySlotsRemoved = uiNodes.RemoveAll(node => node == null);
        if (emptySlotsRemoved > 0)
        {
            Debug.LogWarning($"[ConstellationManager] Ditemukan {emptySlotsRemoved} slot kosong di Inspector. Sudah dibersihkan otomatis.");
        }
    }

    // --- FUNGSI UTAMA ---

    public void OnStarCollected(string idCollected)
    {
        // LINQ Aman: Cek apakah 'x' tidak null sebelum membaca 'nodeID'
        var targetNode = uiNodes.FirstOrDefault(x => x != null && x.nodeID == idCollected);

        if (targetNode != null)
        {
            targetNode.SetActive();
            // Debug dimatikan/dikecilkan porsinya biar Console lo gak spam pas main
            // Debug.Log($"[HUD] Node {idCollected} menyala."); 
        }
        else
        {
            Debug.LogWarning($"[HUD] Node UI dengan ID '{idCollected}' tidak ditemukan! Cek Inspector atau Typo.");
        }
    }

    public void OnStarReset(string idReset)
    {
        var targetNode = uiNodes.FirstOrDefault(x => x != null && x.nodeID == idReset);

        if (targetNode != null)
        {
            targetNode.ResetUI();
        }
    }

    public bool IsNodeCollected(string id)
    {
        var node = uiNodes.FirstOrDefault(x => x != null && x.nodeID == id);

        // Langsung return nilai boolean dari kondisi (jika node ada DAN statusnya true)
        return node != null && node.isActivated;
    }
}