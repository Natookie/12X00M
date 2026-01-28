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
    
    [System.NonSerialized] private int highestValueAchieved = 0;
    [System.NonSerialized] private int roundWhenAssigned = -1;
    [System.NonSerialized] private int currentTarget = 0;

    public override void InitializeProgress(RoomStats roomStats){
        if(RoundManager.Instance == null || roomStats == null) return;

        roundWhenAssigned = RoundManager.Instance.CheckCurrentRound();
        int currentValue = GetCurrentStatValue(roomStats);
        highestValueAchieved = currentValue;
        currentTarget = highestValueAchieved + requiredValue;
    }

    public override bool IsCompleted(RoomStats roomStats){
        if(roomStats == null) return false;
        return GetCurrentStatValue(roomStats) >= currentTarget;
    }

    public override string GetDescription(RoomStats roomStats){
        if(roomStats == null) return $"[0/{requiredValue}] Task configuration error";
        
        int currentValue = GetCurrentStatValue(roomStats);
        int needed = currentTarget - currentValue;
        
        return $"Cumulative: [{currentValue}/{currentTarget}] Need {needed} more {statType} point{(needed != 1 ? "s" : "")} (Started Round {roundWhenAssigned})";
    }

    private int GetCurrentStatValue(RoomStats roomStats){
        return statType switch{
            RoomStatType.Charisma => roomStats.Charisma,
            RoomStatType.Comfort => roomStats.Comfort,
            RoomStatType.Functionality => roomStats.Functionality,
            _ => 0
        };
    }
}