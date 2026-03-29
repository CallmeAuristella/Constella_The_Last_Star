using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int totalDeaths;

    public int totalNodesCollected;

    public bool starCollectorUnlocked;
    public bool skillIssueUnlocked;

    public List<StageNodeData> bestNodesPerStage;
    public List<int> unlockedAchievements;

    public List<int> completedStages;
}

[Serializable]
public class StageNodeData
{
    public int stageIndex;
    public int bestNodes;
}