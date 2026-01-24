using UnityEngine;

public enum RoomStatType
{
    Charisma,
    Comfort,
    Functionality
}

[CreateAssetMenu(fileName = "New Stat Threshold Task", menuName = "Tasks/Stat Threshold Task")]
public class StatThresholdTask : TaskData
{
    [Header("STAT REQUIREMENTS")]
    public RoomStatType statType;
    public int requiredValue = 10;

    public override bool IsCompleted(RoomStats roomStats){
        int currentValue = statType switch{
            RoomStatType.Charisma => roomStats.Charisma,
            RoomStatType.Comfort => roomStats.Comfort,
            RoomStatType.Functionality => roomStats.Functionality,
            _ => 0
        };
        return currentValue >= requiredValue;
    }

    public override string GetDescription(RoomStats roomStats){
        int currentValue = statType switch{
            RoomStatType.Charisma => roomStats.Charisma,
            RoomStatType.Comfort => roomStats.Comfort,
            RoomStatType.Functionality => roomStats.Functionality,
            _ => 0
        };
        return $"[{currentValue}/{requiredValue}] Have {requiredValue} {statType} point{(requiredValue > 1 ? "s" : "")} in the room";
    }
}