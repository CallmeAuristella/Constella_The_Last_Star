using System.Collections.Generic;

public class RunEvaluation
{
    public List<BreakdownEntry> breakdown = new List<BreakdownEntry>();
    public List<AchievementEntry> achievements = new List<AchievementEntry>();
    public List<RunBonusEntry> runBonuses = new List<RunBonusEntry>();

    public int finalScore;

    // 🔥 INI YANG MISSING (BIKIN ERROR LO)
    public float completionTime;
    public int nodesCollected;
}