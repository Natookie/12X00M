using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture Type Task", menuName = "Tasks/Furniture Type Count Task")]
public class FurnitureTypeCountTask : TaskData
{
    [Header("FURNITURE TYPE REQUIREMENTS")]
    public FurnitureType requiredType;
    public int requiredCount = 1;
    
    [System.NonSerialized] private int highestCountAchieved = 0;
    [System.NonSerialized] private int roundWhenAssigned = -1;
    [System.NonSerialized] private int currentTarget = 0;

    public override void InitializeProgress(RoomStats roomStats){
        if(RoundManager.Instance == null || roomStats == null || requiredType == FurnitureType.None) return;

        roundWhenAssigned = RoundManager.Instance.CheckCurrentRound();
        int currentCount = roomStats.GetFurnitureCountByType(requiredType);
        highestCountAchieved = currentCount;
        if(currentCount >= currentTarget) currentTarget = highestCountAchieved + requiredCount;
    }

    public override bool IsCompleted(RoomStats roomStats){
        if(roomStats == null || requiredType == FurnitureType.None) return false;
        return roomStats.GetFurnitureCountByType(requiredType) >= currentTarget;
    }

    public override string GetDescription(RoomStats roomStats){
        if(requiredType == FurnitureType.None) return $"[0/{requiredCount}] Task configuration error";

        string typeName = requiredType.ToString();
        if(roomStats == null) return $"[0/{requiredCount}] Place {requiredCount} {typeName}(s)";
        int currentCount = roomStats.GetFurnitureCountByType(requiredType);

        return $"Type: [{currentCount}/{currentTarget}] Need {currentTarget} total {typeName}(s) (Round {roundWhenAssigned})";
    }
}
