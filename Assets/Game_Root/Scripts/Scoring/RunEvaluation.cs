using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RunEvaluation
{
    // ===== BASE =====
    public int baseScore;

    // ===== TIME =====
    public float completionTime;
    public int timeBonus;
    public TimeTier timeTier;

    // ===== NODE =====
    public int nodesCollected;
    public int nodeScore;
    public bool allNodesCollected;
    public int completionBonus;

    // ===== DEATH =====
    public int deathCount;
    public int noDeathBonus;
    public bool noDeathAchieved;

    // ===== FINAL =====
    public int finalScore;

    //===== ARRAY LIST =====
    public List<ScoreEntry> breakdown = new List<ScoreEntry>();
}

public enum TimeTier
{
    None,
    Bronze,
    Silver,
    Gold
}

[System.Serializable]
public class ScoreEntry
{
    public string label;
    public int value;
}