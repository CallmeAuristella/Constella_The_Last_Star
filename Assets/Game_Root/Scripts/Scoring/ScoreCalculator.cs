using UnityEngine;

public static class ScoreCalculator
{
    public static RunEvaluation EvaluateRun(
        int stageIndex,
        int baseScore,
        float time,
        int nodesCollected,
        int totalNodes,
        int deathCount
    )
    {
        RunEvaluation eval = new RunEvaluation();

        eval.completionTime = time;
        eval.nodesCollected = nodesCollected;

        int final = 0;

        // =====================
        // BASE SCORE
        // =====================
        final += baseScore;

        eval.breakdown.Add(new BreakdownEntry
        {
            label = "Base Score",
            value = baseScore,
            isBonus = false
        });

        // =====================
        // TIME BONUS
        // =====================
        TimeTier tier = GetTimeTier(time);
        int timeBonus = GetTimeBonus(stageIndex, tier);

        if (timeBonus > 0)
        {
            final += timeBonus;

            eval.breakdown.Add(new BreakdownEntry
            {
                label = $"Time Bonus ({tier})",
                value = timeBonus,
                isBonus = true
            });

            // 🔥 RUN BONUS (bukan achievement)
            eval.runBonuses.Add(new RunBonusEntry
            {
                type = ConvertTierToBonus(tier),
                value = timeBonus
            });
        }

        // =====================
        // NODE BONUS
        // =====================
        int nodeBonus = GetNodeBonus(stageIndex, nodesCollected);

        if (nodeBonus > 0)
        {
            final += nodeBonus;

            eval.breakdown.Add(new BreakdownEntry
            {
                label = "Node Bonus",
                value = nodeBonus,
                isBonus = true
            });

            eval.runBonuses.Add(new RunBonusEntry
            {
                type = RunBonusType.NodeBonus,
                value = nodeBonus
            });
        }

        // PERFECT NODE
        bool perfect = nodesCollected >= totalNodes;

        int perfectBonus = GetPerfectNodeBonus(stageIndex, perfect);

        if (perfectBonus > 0)
        {
            final += perfectBonus;

            eval.breakdown.Add(new BreakdownEntry
            {
                label = "Perfect Nodes",
                value = perfectBonus,
                isBonus = true
            });
        }

        // =====================
        // NO DEATH BONUS
        // =====================
        bool noDeath = deathCount == 0;

        int noDeathBonus = GetNoDeathBonus(stageIndex, noDeath);

        if (noDeathBonus > 0)
        {
            final += noDeathBonus;

            eval.breakdown.Add(new BreakdownEntry
            {
                label = "No Death Bonus",
                value = noDeathBonus,
                isBonus = true
            });

            eval.runBonuses.Add(new RunBonusEntry
            {
                type = RunBonusType.NoDeathBonus,
                value = noDeathBonus
            });
        }

        eval.finalScore = final;

        // =====================
        // ACHIEVEMENTS (ONLY REAL ONES)
        // =====================
        if (noDeath)
        {
            if (!GameManager.Instance.unlockedAchievements.Contains(AchievementType.NoDeath_Run))
            {
                Debug.Log("ADD ACHIEVEMENT: NO DEATH");

                GameManager.Instance.UnlockAchievement(AchievementType.NoDeath_Run);

                eval.achievements.Add(new AchievementEntry
                {
                    type = AchievementType.NoDeath_Run,
                    achieved = true
                });
            }
        }
        if (tier == TimeTier.Gold && noDeath)
        {
            if (!GameManager.Instance.unlockedAchievements.Contains(AchievementType.GodSpeed))
            {
                GameManager.Instance.UnlockAchievement(AchievementType.GodSpeed);

                eval.achievements.Add(new AchievementEntry
                {
                    type = AchievementType.GodSpeed,
                    achieved = true
                });
            }
        }

        // ⚠️ INI JANGAN DI SINI (GLOBAL)
        // AllNodes_AllStages → harus dicek di GameManager / SaveSystem
        // GodSpeed → optional global unlock

        return eval;
    }

    // =====================
    // HELPERS
    // =====================

    static TimeTier GetTimeTier(float time)
    {
        if (time <= 120f) return TimeTier.Gold;
        if (time <= 240f) return TimeTier.Silver;
        if (time <= 300f) return TimeTier.Bronze;

        return TimeTier.None;
    }

    static RunBonusType ConvertTierToBonus(TimeTier tier)
    {
        switch (tier)
        {
            case TimeTier.Gold: return RunBonusType.FastGold;
            case TimeTier.Silver: return RunBonusType.FastSilver;
            case TimeTier.Bronze: return RunBonusType.FastBronze;
        }

        return RunBonusType.NodeBonus; // fallback (harusnya ga kepake)
    }

    static int GetTimeBonus(int stageIndex, TimeTier tier)
    {
        switch (stageIndex)
        {
            case 1:
                if (tier == TimeTier.Gold) return 450;
                if (tier == TimeTier.Silver) return 300;
                if (tier == TimeTier.Bronze) return 150;
                break;

            case 2:
            case 3:
                if (tier == TimeTier.Gold) return 800;
                if (tier == TimeTier.Silver) return 500;
                if (tier == TimeTier.Bronze) return 300;
                break;
        }

        return 0;
    }

    static int GetNodeBonus(int stageIndex, int nodes)
    {
        int perNode = (stageIndex == 1) ? 45 : 50;
        return nodes * perNode;
    }

    static int GetPerfectNodeBonus(int stageIndex, bool perfect)
    {
        if (!perfect) return 0;
        return (stageIndex == 1) ? 300 : 600;
    }

    static int GetNoDeathBonus(int stageIndex, bool noDeath)
    {
        if (!noDeath) return 0;
        return (stageIndex == 1) ? 300 : 600;
    }
}