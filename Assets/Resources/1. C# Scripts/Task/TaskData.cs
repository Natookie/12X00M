using UnityEngine;

public abstract class TaskData : ScriptableObject
{
    [Header("TASK INFO")]
    public string taskName;
    public string description;
    public int rewardCoins = 50;
    
    public abstract bool IsCompleted(RoomStats roomStats);
    public abstract string GetDescription(RoomStats roomStats);
}