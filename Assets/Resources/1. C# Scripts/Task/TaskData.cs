using UnityEngine;

public abstract class TaskData : ScriptableObject
{
    [Header("TASK INFO")]
    public string taskName;
    public string description;
    public int rewardCoins = 50;
    
    [System.NonSerialized] public int startingCount = 0;
    [System.NonSerialized] public bool isInitialized = false;
    
    public abstract bool IsCompleted(RoomStats roomStats);
    public abstract string GetDescription(RoomStats roomStats);
    
    public virtual void InitializeProgress(RoomStats roomStats){
        isInitialized = true;
    }
    
    public virtual int GetCurrentProgress(RoomStats roomStats){
        return 0;
    }
}