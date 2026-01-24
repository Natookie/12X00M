using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture Type Task", menuName = "Tasks/Furniture Type Count Task")]
public class FurnitureTypeCountTask : TaskData
{
    [Header("FURNITURE TYPE REQUIREMENTS")]
    public FurnitureType requiredType;
    public int requiredCount = 1;

    public override bool IsCompleted(RoomStats roomStats){
        if (requiredType == FurnitureType.None) return false;
        return roomStats.GetFurnitureCountByType(requiredType) >= requiredCount;
    }

    public override string GetDescription(RoomStats roomStats){
        int current = roomStats.GetFurnitureCountByType(requiredType);
        string typeName = requiredType.ToString();
        return $"[{current}/{requiredCount}] Have {requiredCount} {typeName}(s) in the room";
    }
}