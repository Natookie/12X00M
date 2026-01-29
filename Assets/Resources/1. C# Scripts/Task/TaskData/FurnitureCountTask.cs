using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture Count Task", menuName = "Tasks/Furniture Count Task")]
public class FurnitureCountTask : TaskData
{
    [Header("FURNITURE REQUIREMENTS")]
    public FurnitureData requiredFurniture;
    public int requiredCount = 1;
    
    [System.NonSerialized] private int highestCountAchieved = 0;
    [System.NonSerialized] private int roundWhenAssigned = -1;
    [System.NonSerialized] private int currentTarget = 0;

    public override void InitializeProgress(RoomStats roomStats){
        if(RoundManager.Instance == null || roomStats == null || requiredFurniture == null) return;

        roundWhenAssigned = RoundManager.Instance.CheckCurrentRound();
        int currentCount = roomStats.GetFurnitureCount(requiredFurniture);
        highestCountAchieved = currentCount;
        if(currentCount >= currentTarget) currentTarget = highestCountAchieved + requiredCount;
    }

    public override bool IsCompleted(RoomStats roomStats){
        if(roomStats == null || requiredFurniture == null) return false;
        return roomStats.GetFurnitureCount(requiredFurniture) >= currentTarget;
    }

    public override int GetTrustCost(RoomStats roomStats){
        if(roomStats == null) return 0;

        int trustCost = Mathf.Max(0, highestCountAchieved * 2);
        trustCost = Mathf.Clamp(trustCost, 0, 40);

        return trustCost;
    }

    public override void ResetCumulativeProgress(RoomStats roomStats){
        if(roomStats == null || requiredFurniture == null) return;
        
        highestCountAchieved = 0;
        currentTarget = highestCountAchieved + requiredCount;
    }

    public override string GetDescription(RoomStats roomStats){
        if(requiredFurniture == null) return $"[0/{requiredCount}] Task configuration error";
        string furnitureName = requiredFurniture.FurnitureName;

        if(roomStats == null) return $"[0/{requiredCount}] Place {requiredCount} {furnitureName}(s)";
        int currentCount = roomStats.GetFurnitureCount(requiredFurniture);

        return $"[{currentCount}/{currentTarget}] (Single) Need {currentTarget} total {furnitureName}(s)";
    }
}
