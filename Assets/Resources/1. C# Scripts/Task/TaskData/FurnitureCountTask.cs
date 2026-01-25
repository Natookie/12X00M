using UnityEngine;

[CreateAssetMenu(fileName = "New Furniture Count Task", menuName = "Tasks/Furniture Count Task")]
public class FurnitureCountTask : TaskData
{
    [Header("FURNITURE REQUIREMENTS")]
    public FurnitureData requiredFurniture;
    public int requiredCount = 1;

    public override bool IsCompleted(RoomStats roomStats){
        if(requiredFurniture == null) return false;
        
        int currentCount = roomStats.GetFurnitureCount(requiredFurniture);
        return currentCount >= requiredCount;
    }

    public override string GetDescription(RoomStats roomStats){
        int current = roomStats.GetFurnitureCount(requiredFurniture);
        return $"[{current}/{requiredCount}] Have {requiredCount} {requiredFurniture.FurnitureName}(s) in the room";
    }
}