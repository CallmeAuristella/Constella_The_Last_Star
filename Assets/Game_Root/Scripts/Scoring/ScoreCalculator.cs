using UnityEngine;

public static class ScoreCalculator
{
    public static RunEvaluation EvaluateRun(
        int stageIndex,
        int baseScore,
        float time,
        int nodesCollected,
        int totalNodesInStage,
        int deathCount
    )
    {
        RunEvaluation eval = new RunEvaluation();

        // ===== BASE =====
        eval.baseScore = baseScore;
        eval.completionTime = time;

        // ===== TIME BONUS =====
        CalculateTimeBonus(stageIndex, time, eval);

        // ===== NODE BONUS =====
        eval.nodesCollected = nodesCollected;
        eval.nodeScore = nodesCollected * 50;

        eval.allNodesCollected = (nodesCollected >= totalNodesInStage);
        eval.completionBonus = eval.allNodesCollected ? GetCompletionBonus(stageIndex) : 0;

        // ===== DEATH BONUS =====
        eval.deathCount = deathCount;
        eval.noDeathAchieved = (deathCount == 0);
        eval.noDeathBonus = eval.noDeathAchieved ? GetNoDeathBonus(stageIndex) : 0;

        // ===== FINAL SCORE =====
        eval.finalScore =
            eval.baseScore +
            eval.timeBonus +
            eval.nodeScore +
            eval.completionBonus +
            eval.noDeathBonus;

        // =========================
        // BREAKDOWN (TARUH DI SINI)
        // =========================

        eval.breakdown.Clear();

        eval.breakdown.Add(new ScoreEntry { label = "Base Score", value = eval.baseScore });

        if (eval.timeBonus > 0)
        {
            eval.breakdown.Add(new ScoreEntry
            {
                label = $"Time Bonus ({eval.timeTier})",
                value = eval.timeBonus
            });
        }

        if (eval.nodeScore > 0)
        {
            eval.breakdown.Add(new ScoreEntry
            {
                label = "Node Score",
                value = eval.nodeScore
            });
        }

        if (eval.completionBonus > 0)
        {
            eval.breakdown.Add(new ScoreEntry
            {
                label = "Completion Bonus",
                value = eval.completionBonus
            });
        }

        if (eval.noDeathBonus > 0)
        {
            eval.breakdown.Add(new ScoreEntry
            {
                label = "No Death Bonus",
                value = eval.noDeathBonus
            });
        }

        return eval;

    }

    // =========================

    static void CalculateTimeBonus(int stageIndex, float time, RunEvaluation eval)
    {
        switch (stageIndex)
        {
            case 1:
                if (time <= 120f)
                {
                    eval.timeTier = TimeTier.Gold;
                    eval.timeBonus = 450;
                }
                else if (time <= 180f)
                {
                    eval.timeTier = TimeTier.Silver;
                    eval.timeBonus = 300;
                }
                else if (time <= 240f)
                {
                    eval.timeTier = TimeTier.Bronze;
                    eval.timeBonus = 150;
                }
                break;

            case 2:
                if (time <= 180f)
                {
                    eval.timeTier = TimeTier.Gold;
                    eval.timeBonus = 800;
                }
                else if (time <= 240f)
                {
                    eval.timeTier = TimeTier.Silver;
                    eval.timeBonus = 500;
                }
                else if (time <= 300f)
                {
                    eval.timeTier = TimeTier.Bronze;
                    eval.timeBonus = 250;
                }
                break;

            case 3:
                if (time <= 180f)
                {
                    eval.timeTier = TimeTier.Gold;
                    eval.timeBonus = 900; // ambil upper bound biar rewarding
                }
                else if (time <= 240f)
                {
                    eval.timeTier = TimeTier.Silver;
                    eval.timeBonus = 600;
                }
                else if (time <= 300f)
                {
                    eval.timeTier = TimeTier.Bronze;
                    eval.timeBonus = 300;
                }
                break;


        }
    }

    static int GetCompletionBonus(int stageIndex)
    {
        switch (stageIndex)
        {
            case 1: return 400;
            case 2: return 600;
            case 3: return 700;
        }
        return 0;
    }

    static int GetNoDeathBonus(int stageIndex)
    {
        switch (stageIndex)
        {
            case 1: return 300;
            case 2:return 600;
            case 3: return 700;
        }
        return 0;
    }
}