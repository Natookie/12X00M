using UnityEngine;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    [Header("TASK CONFIGURATION")]
    [SerializeField] private List<TaskData> allTasks;
    [SerializeField] private RoomStats roomStats;
    [SerializeField] private int maxActiveTasks = 2;

    private List<TaskData> activeTasks = new List<TaskData>();

    public System.Action<List<TaskData>> OnTasksUpdated;

    void Start(){
        AssignRandomTasks();
    }

    public void AssignRandomTasks(){
        activeTasks.Clear();
        
        List<TaskData> availableTasks = new List<TaskData>(allTasks);
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int count = Mathf.Min(maxActiveTasks, availableTasks.Count);
        for(int i = 0; i < count; i++) activeTasks.Add(availableTasks[i]);
        
        OnTasksUpdated?.Invoke(activeTasks);
        Debug.Log($"Assigned {activeTasks.Count} tasks");
    }

    public List<TaskData> GetActiveTasks() => activeTasks;
    public bool IsTaskCompleted(TaskData task) => task.IsCompleted(roomStats);
    public string GetTaskDescription(TaskData task) => task.GetDescription(roomStats);
}