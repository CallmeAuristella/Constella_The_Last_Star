using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class OrionPuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [Tooltip("Urutan Wajib: 1. Alnitak, 2. Alnilam, 3. Mintaka")]
    public List<StarNode> starSequence;

    [Header("Puzzle Events")]
    public UnityEvent OnPuzzleSolved; // Drag Portal/Reward kesini
    public UnityEvent OnPuzzleFailed; // Drag SFX Error kesini

    // Internal State
    private int _currentIndex = 0;
    private bool _isSolved = false;

    // --- CARA PAKAI: ---
    // Di Inspector setiap StarNode (Alnitak, Alnilam, Mintaka):
    // 1. Cari event 'On Node Ignited'.
    // 2. Drag GameObject 'OrionPuzzleManager' kesitu.
    // 3. Pilih Function: OrionPuzzleManager -> RegisterStarActivation (Dynamic StarNode).

    public void RegisterStarActivation(StarNode activatedStar)
    {
        // 1. Kalo udah kelar, gak usah diproses lagi
        if (_isSolved) return;

        // 2. Validasi Safety
        if (starSequence.Count == 0)
        {
            Debug.LogError("Tod! List Sequence di Inspector kosong! Isi dulu.");
            return;
        }

        // 3. LOGIKA CEK URUTAN
        // Kita cek apakah bintang yang baru diinjek ini sesuai sama urutan index sekarang?
        if (activatedStar == starSequence[_currentIndex])
        {
            // --- BENAR ---
            Debug.Log($"Puzzle Step {_currentIndex + 1} OK: {activatedStar.nodeID}");
            _currentIndex++; // Lanjut ke target berikutnya

            // Cek apakah ini bintang terakhir?
            if (_currentIndex >= starSequence.Count)
            {
                PuzzleCompleted();
            }
        }
        else
        {
            // --- SALAH URUTAN ---
            // Cek dulu: Apakah bintang ini ada di dalam list puzzle?
            // (Biar gak ngereset puzzle kalau pemain gak sengaja nabrak bintang minor lain yang gak ada hubungannya)
            if (starSequence.Contains(activatedStar))
            {
                Debug.Log($"SALAH URUTAN TOD! Harusnya {starSequence[_currentIndex].nodeID}, tapi malah {activatedStar.nodeID}. RESET!");
                ResetPuzzle();
            }
        }
    }

    private void PuzzleCompleted()
    {
        _isSolved = true;
        Debug.Log("--- ORION BELT ALIGNED (SOLVED) ---");
        OnPuzzleSolved.Invoke();
    }

    private void ResetPuzzle()
    {
        OnPuzzleFailed.Invoke(); // Bunyiin suara gagal

        // Reset Index
        _currentIndex = 0;

        // Matikan ulang SEMUA bintang yang ada di puzzle (termasuk yang bener)
        foreach (StarNode star in starSequence)
        {
            // Panggil fungsi ResetNode() yang baru kita tambah di StarNode.cs
            star.ResetNode();
        }
    }
}