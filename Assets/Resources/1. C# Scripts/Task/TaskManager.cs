using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TaskManager : MonoBehaviour
{
    [Header("TASK CONFIGURATION")]
    [SerializeField] private List<TaskData> allTasks;
    [SerializeField] private RoomStats roomStats;
    [SerializeField] private int maxMainTasks = 3;  //1 to 4
    [SerializeField] private int maxExtraTasks = 2; //0 to 2
    
    [Header("REFERENCES")]
    [SerializeField] private TaskUI taskUI;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private int refreshCost = 50;
    [SerializeField] private int completeReward = 100;

    private List<TaskData> mainTasks = new List<TaskData>();
    private List<TaskData> extraTasks = new List<TaskData>();

    public System.Action OnTasksUpdated;

    void Start(){
        AssignRandomTasks();
    }

    public void OnFurniturePlaced(){
        if(taskUI != null) taskUI.UpdateUI();
    }

    public void AssignRandomTasks(){
        mainTasks.Clear();
        extraTasks.Clear();
        
        List<TaskData> availableTasks = new List<TaskData>(allTasks);
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int mainCount = Mathf.Min(maxMainTasks, availableTasks.Count);
        for(int i = 0; i < mainCount; i++) mainTasks.Add(availableTasks[i]);
        
        if(availableTasks.Count > mainCount){
            int extraCount = Mathf.Min(maxExtraTasks, availableTasks.Count - mainCount);
            for(int i = mainCount; i < mainCount + extraCount; i++){
                extraTasks.Add(availableTasks[i]);
            }
        }
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
        
        Debug.Log($"Assigned {mainTasks.Count} main tasks and {extraTasks.Count} extra tasks");
    }

    public void RefreshMainTasks(){
        if(moneyManager != null && !moneyManager.CanAfford(refreshCost)){
            Debug.Log("Not enough money to refresh main tasks");
            return;
        }
        if(moneyManager != null) moneyManager.AddMoney(-refreshCost);
        
        RefreshTaskList(mainTasks, maxMainTasks);
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
        
        Debug.Log("Refreshed main tasks");
    }

    public void RefreshExtraTasks(){
        if(moneyManager != null && !moneyManager.CanAfford(refreshCost)){
            Debug.Log("Not enough money to refresh extra tasks");
            return;
        }
        if(moneyManager != null) moneyManager.AddMoney(-refreshCost);
        
        RefreshTaskList(extraTasks, maxExtraTasks);
        
        OnTasksUpdated?.Invoke();
        if(taskUI != null) taskUI.UpdateUI();
        
        Debug.Log("Refreshed extra tasks");
    }

    void RefreshTaskList(List<TaskData> taskList, int maxTasks){
        taskList.Clear();
        
        List<TaskData> availableTasks = new List<TaskData>();
        foreach(TaskData task in allTasks){
            if(!mainTasks.Contains(task) && !extraTasks.Contains(task)){
                availableTasks.Add(task);
            }
        }
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int count = Mathf.Min(maxTasks, availableTasks.Count);
        for(int i = 0; i < count; i++) taskList.Add(availableTasks[i]);
    }

    public void CheckCompletedTask(){
        bool completedAny = false;
        
        for(int i = mainTasks.Count - 1; i >= 0; i--){
            if(IsTaskCompleted(mainTasks[i])){
                CompleteTask(mainTasks[i], true);
                mainTasks.RemoveAt(i);
                completedAny = true;
            }
        }
        
        for(int i = extraTasks.Count - 1; i >= 0; i--){
            if(IsTaskCompleted(extraTasks[i])){
                CompleteTask(extraTasks[i], false);
                extraTasks.RemoveAt(i);
                completedAny = true;
            }
        }
        
        if(completedAny){
            OnTasksUpdated?.Invoke();
            if(taskUI != null) taskUI.UpdateUI();
            
            AutoRefillEmptySlots();
        }
    }

    void CompleteTask(TaskData task, bool isMainTask){
        if(moneyManager != null){
            int reward = completeReward * (isMainTask ? 2 : 1);
            moneyManager.AddMoney(reward);
            Debug.Log($"Completed task: {task.name}. Reward: ${reward}");
        }
    }

    void AutoRefillEmptySlots(){
        if(mainTasks.Count < maxMainTasks){
            int needed = maxMainTasks - mainTasks.Count;
            FillEmptySlots(mainTasks, needed);
        }
        
        if(extraTasks.Count < maxExtraTasks){
            int needed = maxExtraTasks - extraTasks.Count;
            FillEmptySlots(extraTasks, needed);
        }
        
        if(taskUI != null) taskUI.UpdateUI();
    }

    void FillEmptySlots(List<TaskData> taskList, int count){
        List<TaskData> availableTasks = new List<TaskData>();
        foreach(TaskData task in allTasks){
            if(!mainTasks.Contains(task) && !extraTasks.Contains(task)){
                availableTasks.Add(task);
            }
        }
        
        for(int i = 0; i < availableTasks.Count; i++){
            int randomIndex = Random.Range(i, availableTasks.Count);
            (availableTasks[i], availableTasks[randomIndex]) = (availableTasks[randomIndex], availableTasks[i]);
        }
        
        int addCount = Mathf.Min(count, availableTasks.Count);
        for(int i = 0; i < addCount; i++) taskList.Add(availableTasks[i]);
    }

    public List<TaskData> GetMainTasks() => mainTasks;
    public List<TaskData> GetExtraTasks() => extraTasks;
    public bool IsTaskCompleted(TaskData task) => task.IsCompleted(roomStats);
    public string GetTaskDescription(TaskData task) => task.GetDescription(roomStats);
    public int GetRefreshCost() => refreshCost;

    #region AUTO POPULATE
    #if UNITY_EDITOR
    [ContextMenu("Populate All Tasks")]
    public void PopulateAllTasksFromResources(){
        allTasks.Clear();
        
        string[] traitPaths = Directory.GetFiles("Assets/Resources/5. Task Data/Trait", "*.asset", SearchOption.AllDirectories);
        string[] furniturePaths = Directory.GetFiles("Assets/Resources/5. Task Data/Furniture Count", "*.asset", SearchOption.AllDirectories);
        
        Debug.Log($"Found {traitPaths.Length} trait tasks and {furniturePaths.Length} furniture tasks");
        foreach(string path in traitPaths){
            TaskData task = AssetDatabase.LoadAssetAtPath<TaskData>(path);
            if(task != null) allTasks.Add(task);
        }
        
        foreach(string path in furniturePaths){
            TaskData task = AssetDatabase.LoadAssetAtPath<TaskData>(path);
            if(task != null) allTasks.Add(task);
        }
        
        Debug.Log($"Total tasks loaded: {allTasks.Count}");
        EditorUtility.SetDirty(this);
    }

    [ContextMenu("Clear All Tasks")]
    public void ClearAllTasks(){
        allTasks.Clear();
        Debug.Log("Cleared all tasks");
        EditorUtility.SetDirty(this);
    }
    #endif
    #endregion
}